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
using System.Diagnostics;
using System.Globalization;

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
                await MarkDocumentsAsCompleted(successfullyTaggedDocumentIds, 2000);
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
                using var PDFDocument = PdfDocument.FromFile(worksheet.Cells[row, worksheet.Dimension.Columns].Value?.ToString());
                string AllText = TextHelperService.CleanTextExtractedFromPdf(PDFDocument.ExtractAllText());

                var textReplacementList = new List<TextReplacementHelperDto>();
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
            return successfullyTaggedDocumentIds;
        }

        private async Task<DocumentTaggingResultDto> RenderDocumentSendToBotAndUpdateTagging(List<TextReplacementHelperDto> textReplacementList, string documentText, string documentName)
        {
            foreach(var entity in textReplacementList)
            {
                entity.NewText = TextHelperService.GenerateRandomString(entity.OldText);
                documentText = documentText.Replace(entity.OldText, entity.NewText);
            }
            var pdf = _pdfRenderer.RenderHtmlAsPdf(documentText);

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

            foreach(var item in textReplacementList)
            {
                var wordIds = new List<int>();

                wordIds.Add(documentDetails.DocumentJson.Pages[0].WordLevel.Find(w => w.Text == item.NewText).WordId);
                var word = documentDetails.DocumentJson.Pages[0].WordLevel.Find(w => w.Text == item.NewText);

                entities.Add(new DocumentEntityTaggedReadDto
                {
                    EntityId = item.EntityId,
                    WordIds = wordIds,
                    Value = word.Text + word.Space,
                    TaggedAuthor = 0,
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

        private async Task MarkDocumentsAsCompleted(List<Guid> documentIds, int batchSize)
        {
            int totalBatches = (int)Math.Ceiling((double)documentIds.Count / batchSize);
            for (int batchNumber = 0; batchNumber < totalBatches; batchNumber++)
            {
                var batchDocumentIds = documentIds.Skip(batchNumber * batchSize).Take(batchSize).ToList();
                var result = await _documentbotStudioApiService.UpdateDocumentStatusAsCompletedInStudio(batchDocumentIds);
            }
        }

        public async Task CreateAnnontationSetup(StudioEnvironment environment, Guid documentbotId, string accessToken)
        {
            var random = new Random();
            var shortKeys = new DocumentbotShortkeys();
            await _documentbotStudioApiService.SetupHttpClientAuthorizationHeaderAndApiUrl(new DocumentGenerationUserInputDto
            {
                AccessToken = accessToken,
                DocumentbotId = documentbotId
            }, environment);

            var documentTypes = await _documentbotStudioApiService.GetDocumentTypesFromStudio();
            documentTypes.RemoveAll(x => x.Type != 0); //0 is enum for Ordinary document type ItemGenericTypeEnum in studio
            var entitiesFromBot = await _documentbotStudioApiService.GetDocumentbotEntitiesFromStudio();
            var intentsFromBot = await _documentbotStudioApiService.GetDocumentbotIntentsFromStudio();

            var alphanumericCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var shortKeysOptions = alphanumericCharacters.ToList();

            foreach (var documentType in documentTypes)
            {
                var shortkeyTemp = shortKeysOptions[random.Next(0, shortKeysOptions.Count())];
                shortKeysOptions.Remove(shortkeyTemp);
                shortKeys.DocumentTypeShortkeys.Add(new ShortKeyConfig
                {
                    Id = documentType.Id,
                    BackgroundColour = TextHelperService.GenerateRandomColorCode(),
                    ForegroundColour = "#000",
                    ShortKey = shortkeyTemp
                });
            }

            foreach (var entity in entitiesFromBot)
            {
                var shortkeyTemp = shortKeysOptions[random.Next(0, shortKeysOptions.Count())];
                shortKeysOptions.Remove(shortkeyTemp);
                shortKeys.EntityShortkeys.Add(new ShortKeyConfig
                {
                    Id = entity.Id,
                    BackgroundColour = TextHelperService.GenerateRandomColorCode(),
                    ForegroundColour = "#000",
                    ShortKey = shortkeyTemp
                });
            }

            foreach (var intent in intentsFromBot)
            {
                var shortkeyTemp = shortKeysOptions[random.Next(0, shortKeysOptions.Count())];
                shortKeysOptions.Remove(shortkeyTemp);
                shortKeys.IntentShortkeys.Add(new ShortKeyConfig
                {
                    Id = intent.Id,
                    BackgroundColour = TextHelperService.GenerateRandomColorCode(),
                    ForegroundColour = "#000",
                    ShortKey = shortkeyTemp
                });
            }
            await _documentbotStudioApiService.UpdateDocumentbotAnnotationShortkeys(shortKeys);
        }

        public async Task MarkSyncedDocumentsAsCompleted(StudioEnvironment environment, Guid documentbotId, string accessToken, string searchText)
        {
            await _documentbotStudioApiService.SetupHttpClientAuthorizationHeaderAndApiUrl(new DocumentGenerationUserInputDto
            {
                AccessToken = accessToken,
                DocumentbotId = documentbotId
            }, environment);

            var documentSearchRequestDto = new GetDocumentRequestDto()
            {
                SearchText = searchText,
                StartDate = DateTime.ParseExact("1999-12-31T18:30:00.000Z", "yyyy-MM-dd'T'HH:mm:ss.fff'Z'", CultureInfo.InvariantCulture),
                EndDate = DateTime.UtcNow,
                PageNumber = 1,
                PageSize = int.MaxValue
            };

            var searchResult = await _documentbotStudioApiService.SearchForDocumentId(JsonConvert.SerializeObject(documentSearchRequestDto));
            var documentIds = searchResult.Result.Records.Select(d => d.Id).ToList();
            await MarkDocumentsAsCompleted(documentIds, 2000);
        }
    }
}
