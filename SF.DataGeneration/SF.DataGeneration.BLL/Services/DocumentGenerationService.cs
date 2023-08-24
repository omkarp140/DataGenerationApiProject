using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OfficeOpenXml;
using SF.DataGeneration.BLL.Helpers;
using SF.DataGeneration.BLL.Interfaces;
using SF.DataGeneration.Models.Dto.Document;
using SF.DataGeneration.Models.Enum;
using SF.DataGeneration.Models.StudioApiModels.RequestDto;
using SF.DataGeneration.Models.StudioApiModels.ResponseDto;
using System.Diagnostics;
using System.Globalization;

namespace SF.DataGeneration.BLL.Services
{
    public class DocumentGenerationService : IDocumentGenerationService
    {
        private readonly ILogger<DocumentGenerationService> _logger;
        private readonly IDocumentbotStudioApiService _documentbotStudioApiService;
        private Guid _greetingIntentId;

        public DocumentGenerationService(ILogger<DocumentGenerationService> logger,
                                         IDocumentbotStudioApiService documentbotStudioApiService)
        {
            _logger = logger;
            _documentbotStudioApiService = documentbotStudioApiService;
            License.LicenseKey = "";
        }

        public async Task GenerateDocumentsWithExcelData(DocumentGenerationUserInputDto request, StudioEnvironment environment)
        {
            var successfullyTaggedDocumentIds = new List<Guid>();
            var sw = Stopwatch.StartNew();

            try
            {
                await _documentbotStudioApiService.SetupHttpClientAuthorizationHeaderAndApiUrl(request, environment);

                var entitiesFromBot = await _documentbotStudioApiService.GetDocumentbotEntitiesFromStudio();

                // Read the Excel file and fetch entity indices
                var worksheet = await ReadExcelWorksheet(request.ExcelFilePath);
                _greetingIntentId = request.GreetingIntentId;

                var entities = await UpdateExcelindicesForEntities(entitiesFromBot, worksheet);

                // Generate and send documents based on Excel data
                successfullyTaggedDocumentIds = await GenerateAndSendDocuments(worksheet, request, entities);
            }
            catch (Exception ex)
            {
                // In case of any failure, log the error and proceed to mark successfully tagged documents as completed.
                _logger.LogError(ex, "An error occurred while generating documents.");
            }
            finally
            {
                // Mark the successfully tagged documents as completed
                await MarkDocumentsAsCompleted(successfullyTaggedDocumentIds);
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

        private async Task<List<Guid>> GenerateAndSendDocuments(ExcelWorksheet worksheet, DocumentGenerationUserInputDto request, List<EntityHelperDto> entities)
        {
            var successfullyTaggedDocumentIds = new List<Guid>();

            for(int row = 2; row <= worksheet.Dimension.Rows; row++)
            {
                try
                {
                    using var PDFDocument = PdfDocument.FromFile(worksheet.Cells[row, worksheet.Dimension.Columns].Value?.ToString());
                    string AllText = TextHelperService.CleanTextExtractedFromPdf(PDFDocument.ExtractAllText());

                    var textReplacementList = new List<TextReplacementHelperDto>();
                    for (int col = 1; col < worksheet.Dimension.Columns; col++)
                    {
                        textReplacementList.Add(new TextReplacementHelperDto
                        {
                            EntityId = entities.Find(e => e.ExcelIndex == col).Id,
                            OldText = worksheet.Cells[row, col].Value.ToString(),
                            EntityType = entities.Find(e => e.ExcelIndex == col).Name
                        });
                    }
                    for (int i = 1; i <= request.NoOfDocumentsToCreate; i++)
                    {
                        var sw1 = Stopwatch.StartNew();
                        var documentTaggingResult = await RenderDocumentSendToBotAndUpdateTagging(textReplacementList, AllText, $"{request.DocumentNamePrefix}_{row - 1}_DocTest_{i}.pdf");
                        if (documentTaggingResult.TaggingApiResponseStatusCode)
                        {
                            successfullyTaggedDocumentIds.Add(documentTaggingResult.DocumentId);
                            sw1.Stop();
                            _logger.LogInformation($"\n\n{request.DocumentNamePrefix}_{row - 1}_DocTest_{i}.pdf - sent to bot and tagged - In {sw1.Elapsed.Seconds} Seconds.\n\n");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"An error occurred while processing document at RowIndex: {row}");
                }
            }
            return successfullyTaggedDocumentIds;
        }

        private async Task<DocumentTaggingResultDto> RenderDocumentSendToBotAndUpdateTagging(List<TextReplacementHelperDto> textReplacementList, string documentText, string documentName)
        {
            foreach(var entity in textReplacementList)
            {
                entity.NewText = TextHelperService.GenerateReadbleRandomData(entity.EntityType);
                documentText = documentText.Replace(entity.OldText, entity.NewText);
            }

            var renderer = new ChromePdfRenderer();
            var pdf = renderer.RenderHtmlAsPdf(documentText);

            await using var ms = new MemoryStream(pdf.BinaryData);
            var botResponse = await _documentbotStudioApiService.SendDocumentToBotInStudio(pdf.BinaryData, documentName);
            var documentDetails = await GetSingleDocumentDetails(documentName);

            var taggingResult = await UpdateTaggingOnDocument(textReplacementList,documentDetails.Result);
            return new DocumentTaggingResultDto()
            {
                DocumentId = documentDetails.Result.Id,
                TaggingApiResponseStatusCode = taggingResult
            };            
        }        

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

        private async Task<bool> UpdateTaggingOnDocument(List<TextReplacementHelperDto> textReplacementList, DocumentDetails documentDetails)
        {
            var entities = new List<DocumentEntityTaggedReadDto>();
            var intents = new List<DocumentIntentTaggedReadDto>();

            var intentWordIds = new List<int>() { 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 };

            string intentValue = "";

            foreach (var wordId in intentWordIds)
            {
                intentValue = intentValue + documentDetails.DocumentJson.Pages[0].WordLevel[wordId].Text + documentDetails.DocumentJson.Pages[0].WordLevel[wordId].Space;
            }

            intents.Add(new DocumentIntentTaggedReadDto()
            {
                IntentId = _greetingIntentId,
                WordIds = intentWordIds,
                TaggedAuthor = 1,
                DocumentId = documentDetails.Id,
                Value = intentValue
            });

            foreach (var item in textReplacementList)
            {
                var wordIds = new List<int>();

                if (Enum.TryParse(item.EntityType, out RandomDataType dataType))
                {
                    switch (dataType)
                    {
                        case RandomDataType.Name:
                            wordIds = new List<int>() { 5 };
                            break;

                        case RandomDataType.EmailId:
                            wordIds = new List<int>() { 73, 74, 75, 76, 77, 78, 79 };
                            break;

                        case RandomDataType.Address:
                            wordIds = new List<int>() { 155, 156, 157, 158, 159, 160, 161, 162, 163, 164, 165 };
                            break;

                        case RandomDataType.Date:
                            wordIds = new List<int>() { 65, 66, 67, 68, 69 };
                            break;

                        case RandomDataType.UserId:
                            wordIds = new List<int>() { 83 };
                            break;

                        case RandomDataType.MobileNo:
                            wordIds = new List<int>() { 58, 59, 60 };
                            break;

                        case RandomDataType.Designation:
                            wordIds = new List<int>() { 169, 170 };
                            break;
                    }
                }

                string entityValue = "";

                foreach (var wordId in wordIds)
                {
                    entityValue = entityValue + documentDetails.DocumentJson.Pages[0].WordLevel[wordId].Text + documentDetails.DocumentJson.Pages[0].WordLevel[wordId].Space;
                }

                entities.Add(new DocumentEntityTaggedReadDto
                {
                    EntityId = item.EntityId,
                    WordIds = wordIds,
                    Value = entityValue,
                    TaggedAuthor = 1,
                    DocumentId = documentDetails.Id,
                });
            }

            var updateDocumentDetailsRequest = new UpdateDocumentDetailsDto()
            {
                DocumentTypeId = documentDetails.TaggedData.DocumentTypeLink[0].DocumentTypeId,
                DocumentTaggedDto = new DocumentTaggedDto()
                {
                    EntitiesTagged = entities,
                    IntentsTagged = intents,
                    DocumentTypeLink = documentDetails.TaggedData.DocumentTypeLink
                }
            };

            return await _documentbotStudioApiService.UpdateDocumentTaggingInStudio(JsonConvert.SerializeObject(updateDocumentDetailsRequest), documentDetails.Id);
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
    }
}
