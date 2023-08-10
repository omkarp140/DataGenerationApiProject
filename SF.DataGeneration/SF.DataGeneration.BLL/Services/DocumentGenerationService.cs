using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OfficeOpenXml;
using SF.DataGeneration.BLL.Helpers;
using SF.DataGeneration.BLL.Interfaces;
using SF.DataGeneration.Models.Dto.Document;
using SF.DataGeneration.Models.Enum;
using SF.DataGeneration.Models.StudioApiModels.Other;
using SF.DataGeneration.Models.StudioApiModels.RequestDto;
using SF.DataGeneration.Models.StudioApiModels.ResponseDto;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.Net.Http.Headers;

namespace SF.DataGeneration.BLL.Services
{
    public class DocumentGenerationService : IDocumentGenerationService
    {
        private readonly ILogger<DocumentGenerationService> _logger;
        private readonly IDocumentbotStudioApiService _documentbotStudioApiService;
        private readonly ChromePdfRenderer _pdfRenderer;

        public DocumentGenerationService(ILogger<DocumentGenerationService> logger,
                                         IDocumentbotStudioApiService documentbotStudioApiService)
        {
            _logger = logger;
            _documentbotStudioApiService = documentbotStudioApiService;
            _pdfRenderer = new ChromePdfRenderer();
            License.LicenseKey = "IRONPDF.SIMPLIFAIAS.IRO230620.1722.98102-13A32F9432-DQTKC3VXVVK7K-6ZSYIRKXLXU3-SDI2XULXMSAI-337NJMOR2GW3-DYGQTHTSM77G-H2R7HH-LQZH3GPILE6UEA-IRONPDF.DOTNET.UNLIMITED.5YR-WZ6MO5.RENEW.SUPPORT.18.JUN.2028";
        }

        public async Task GenerateDocumentsWithExcelData(DocumentGenerationUserInputDto request, StudioEnvironment environment)
        {
            var successfullyTaggedDocumentIds = new List<Guid>();
            var sw = Stopwatch.StartNew();

            try
            {
                await _documentbotStudioApiService.SetupHttpClientAuthorizationHeaderAndApiUrl(request, environment);

                // Read the Excel file and fetch entity indices
                var worksheet = await ReadExcelWorksheet(request.ExcelFilePath);

                var entitiesFromBot = await _documentbotStudioApiService.GetDocumentbotEntitiesFromStudio();                

                var entities = await UpdateExcelindicesForEntities(entitiesFromBot, worksheet);

                //Generate annotation setup for entities
                //await GenerateAnnotationSetupForEntities(entities);

                // Generate and send documents based on Excel data
                //successfullyTaggedDocumentIds = await GenerateAndSendDocuments(worksheet, request, entities);

                var documents = await GenerateDocuments(worksheet, request, entities);
                successfullyTaggedDocumentIds = await SendDocumentsToStudioAndUpdateTagging(documents);
            }
            catch (Exception ex)
            {
                // In case of any failure, log the error and proceed to mark successfully tagged documents as completed.
                _logger.LogError(ex, "An error occurred while generating documents.");
            }
            finally
            {
                // Mark the successfully tagged documents as completed
                //await MarkDocumentsAsCompleted(successfullyTaggedDocumentIds);
                sw.Stop();
                _logger.LogInformation($"{request.NoOfDocumentsToCreate} documents sent & tagged on bot in - {sw.Elapsed.Minutes} minutes.");
            }
        }

        private async Task<ExcelWorksheet> ReadExcelWorksheet(string excelFilePath)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            var excelPackage = new ExcelPackage(new FileInfo(excelFilePath));
            return excelPackage.Workbook.Worksheets[0];
        }

        private async Task<List<EntityHelperDto>> UpdateExcelindicesForEntities(List<EntityHelperDto> entitiesFromBot, ExcelWorksheet worksheet)
        {
            for (int colIndex = 1; colIndex < worksheet.Dimension.Columns; colIndex++)
            {
                entitiesFromBot.Find(e => e.Name.ToLower() == worksheet.Cells[1, colIndex].Value?.ToString().ToLower())!.ExcelIndex = colIndex;
            }
            return entitiesFromBot;
        }

        private async Task<ConcurrentBag<NewDocument>> GenerateDocuments(ExcelWorksheet worksheet, DocumentGenerationUserInputDto request, List<EntityHelperDto> entities)
        {
            var newDocuments = new ConcurrentBag<NewDocument>();
            for (int row = 2; row <= worksheet.Dimension.Rows; row++)
            {
                using var PDFDocument = PdfDocument.FromFile(worksheet.Cells[row, worksheet.Dimension.Columns].Value?.ToString());
                string AllText = TextHelperService.CleanTextExtractedFromPdf(PDFDocument.ExtractAllText());

                var textReplacementList = new ConcurrentBag<TextReplacementHelperDto>();
                for (int col = 1; col < worksheet.Dimension.Columns; col++)
                {
                    textReplacementList.Add(new TextReplacementHelperDto
                    {
                        EntityId = entities.Find(e => e.ExcelIndex == col).Id,
                        OldText = worksheet.Cells[row, col].Value.ToString()
                    });
                }

                for (int i = 1; i <= request.NoOfDocumentsToCreate; i++)
                {
                    var newDocument = await RenderDocuments(textReplacementList, AllText, $"{request.DocumentNamePrefix}_{row - 1}_DocTest_{i}.pdf");
                    newDocuments.Add(newDocument);
                }
            }
            return newDocuments;
        }

        private async Task<NewDocument> RenderDocuments(ConcurrentBag<TextReplacementHelperDto> textReplacementList, string documentText, string documentName)
        {
            foreach(var entity in textReplacementList)
            {
                entity.NewText = TextHelperService.GenerateRandomString(entity.OldText);
                documentText = documentText.Replace(entity.OldText, entity.NewText);
            }
            var pdf = _pdfRenderer.RenderHtmlAsPdf(documentText);

            return new NewDocument()
            {
                BinaryData = pdf.BinaryData,
                FileName = documentName,
                TextReplacementList = textReplacementList
            };
        }

        private async Task<List<Guid>> SendDocumentsToStudioAndUpdateTagging(ConcurrentBag<NewDocument> documents)
        {
            var successfullyTaggedDocumentIds = new ConcurrentBag<Guid>();

            ParallelOptions options = new() { MaxDegreeOfParallelism = 3 };
            await Parallel.ForEachAsync(documents, options, async (document, ct) =>
            {
                var botResponse = await _documentbotStudioApiService.SendDocumentToBotInStudio(document.BinaryData, document.FileName);
                var documentDetails = await GetSingleDocumentDetails(document.FileName);
                document.DocumentDetails = documentDetails.Result;

                var taggingResult = await UpdateTaggingOnDocument(document);
                if (taggingResult)
                {
                    successfullyTaggedDocumentIds.Add(documentDetails.Result.Id);
                }
            });

            return successfullyTaggedDocumentIds.ToList();
        }


        //private async Task<List<Guid>> GenerateAndSendDocuments(ExcelWorksheet worksheet, DocumentGenerationUserInputDto request, List<EntityHelperDto> entities)
        //{
        //    var successfullyTaggedDocumentIds = new List<Guid>();

        //    for(int row = 2; row <= worksheet.Dimension.Rows; row++)
        //    {
        //        using var PDFDocument = PdfDocument.FromFile(worksheet.Cells[row, worksheet.Dimension.Columns].Value?.ToString());
        //        string AllText = TextHelperService.CleanTextExtractedFromPdf(PDFDocument.ExtractAllText());

        //        var textReplacementList = new List<TextReplacementHelperDto>();
        //        for (int col = 1; col < worksheet.Dimension.Columns; col++)
        //        {
        //            textReplacementList.Add(new TextReplacementHelperDto
        //            {
        //                EntityId = entities.Find(e => e.ExcelIndex == col).Id,
        //                OldText = worksheet.Cells[row, col].Value.ToString()
        //            });
        //        }
        //        for (int i = 1; i <= request.NoOfDocumentsToCreate; i++)
        //        {
        //            var sw1 = Stopwatch.StartNew();
        //            var documentTaggingResult = await RenderDocumentSendToBotAndUpdateTagging(textReplacementList, AllText, $"{request.DocumentNamePrefix}_{row - 1}_DocTest_{i}.pdf");
        //            if (documentTaggingResult.TaggingApiResponseStatusCode)
        //            {
        //                successfullyTaggedDocumentIds.Add(documentTaggingResult.DocumentId);
        //                sw1.Stop();
        //                _logger.LogInformation($"\n\n{request.DocumentNamePrefix}_{row - 1}_DocTest_{i}.pdf - sent to bot and tagged - In {sw1.Elapsed.Seconds} Seconds.\n\n");
        //            }                   
        //        }
        //    }
        //    return successfullyTaggedDocumentIds;
        //}

        //private async Task<DocumentTaggingResultDto> RenderDocumentSendToBotAndUpdateTagging(List<TextReplacementHelperDto> textReplacementList, string documentText, string documentName)
        //{
        //    foreach(var entity in textReplacementList)
        //    {
        //        entity.NewText = TextHelperService.GenerateRandomString(entity.OldText);
        //        documentText = documentText.Replace(entity.OldText, entity.NewText);
        //    }
        //    var pdf = _pdfRenderer.RenderHtmlAsPdf(documentText);

        //    await using var ms = new MemoryStream(pdf.BinaryData);
        //    var botResponse = await _documentbotStudioApiService.SendDocumentToBotInStudio(pdf.BinaryData, documentName);
        //    var documentDetails = await GetSingleDocumentDetails(documentName);

        //    var taggingResult = await UpdateTaggingOnDocument(textReplacementList,documentDetails.Result);
        //    return new DocumentTaggingResultDto()
        //    {
        //        DocumentId = documentDetails.Result.Id,
        //        TaggingApiResponseStatusCode = taggingResult
        //    };            
        //}        

        private async Task<DocumentDetailsResponseDto> GetSingleDocumentDetails(string documentName)
        {
            var documentSearchRequestDto = new GetDocumentRequestDto()
            {
                SearchText = documentName,
                StartDate = DateTime.ParseExact("1999-12-31T18:30:00.000Z", "yyyy-MM-dd'T'HH:mm:ss.fff'Z'", CultureInfo.InvariantCulture),
                EndDate = DateTime.UtcNow,
                PageNumber = 1,
                PageSize = 10
            };

            var searchResult = await _documentbotStudioApiService.SearchForDocumentId(JsonConvert.SerializeObject(documentSearchRequestDto));
            return await _documentbotStudioApiService.GetDocumentDetailsFromStudio(searchResult.Result.Records[0].Id);
        }

        private async Task<bool> UpdateTaggingOnDocument(NewDocument newDocument)
        {
            var entities = new ConcurrentBag<DocumentEntityTaggedReadDto>();
            var intents = new ConcurrentBag<DocumentIntentTaggedReadDto>();

            foreach(var item in newDocument.TextReplacementList)
            {
                var wordIds = new List<int>();

                wordIds.Add(newDocument.DocumentDetails.DocumentJson.Pages[0].WordLevel.Find(w => w.Text == item.NewText).WordId);
                var word = newDocument.DocumentDetails.DocumentJson.Pages[0].WordLevel.Find(w => w.Text == item.NewText);

                entities.Add(new DocumentEntityTaggedReadDto
                {
                    EntityId = item.EntityId,
                    WordIds = wordIds,
                    Value = word.Text + word.Space,
                    TaggedAuthor = 0,
                    DocumentId = newDocument.DocumentDetails.Id,
                });
            }

            var updateDocumentDetailsRequest = new UpdateDocumentDetailsDto()
            {
                DocumentTypeId = newDocument.DocumentDetails.TaggedData.DocumentTypeLink[0].DocumentTypeId,
                DocumentTaggedDto = new DocumentTaggedDto()
                {
                    EntitiesTagged = entities.ToList(),
                    IntentsTagged = intents.ToList(),
                    DocumentTypeLink = newDocument.DocumentDetails.TaggedData.DocumentTypeLink
                }
            };

            return await _documentbotStudioApiService.UpdateDocumentTaggingInStudio(JsonConvert.SerializeObject(updateDocumentDetailsRequest), newDocument.DocumentDetails.Id);
        }

        private async Task MarkDocumentsAsCompleted(List<Guid> documentIds)
        {
            int batchSize = 200;
            int totalBatches = (int)Math.Ceiling((double)documentIds.Count / batchSize);
            for (int batchNumber = 0; batchNumber < totalBatches; batchNumber++)
            {
                var batchDocumentIds = documentIds.Skip(batchNumber * batchSize).Take(batchSize).ToList();
                var result = await _documentbotStudioApiService.UpdateDocumentStatusAsCompletedInStudio(batchDocumentIds);
            }
        }

        public async Task CreateAnnontationSetup(StudioEnvironment environment, Guid documentbotId, string accessToken)
        {
            var shortKeys = new DocumentbotShortkeys();
            await _documentbotStudioApiService.SetupHttpClientAuthorizationHeaderAndApiUrl(new DocumentGenerationUserInputDto
            {
                AccessToken = accessToken,
                DocumentbotId = documentbotId
            }, environment);

            var entitiesFromBot = await _documentbotStudioApiService.GetDocumentbotEntitiesFromStudio();
            foreach(var entity in entitiesFromBot)
            {
                shortKeys.EntityShortkeys.Add(new ShortKeyConfig
                {
                    Id = entity.Id,
                    BackgroundColour = TextHelperService.GenerateRandomColorCode(),
                    ForegroundColour = "#000",
                    ShortKey = null
                });
            }
            await _documentbotStudioApiService.UpdateDocumentbotAnnotationShortkeys(shortKeys);
        }
    }
}
