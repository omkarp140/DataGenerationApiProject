using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Polly;
using SF.DataGeneration.BLL.Interfaces;
using SF.DataGeneration.Models.Dto.Document;
using SF.DataGeneration.Models.Enum;
using SF.DataGeneration.Models.Settings;
using SF.DataGeneration.Models.StudioApiModels.ResponseDto;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace SF.DataGeneration.BLL.Services
{
    public class DocumentbotStudioApiService : IDocumentbotStudioApiService
    {
        private readonly HttpClient _httpClient;
        private readonly StudioApiBaseUrl _studioApiBaseUrl;
        private readonly ILogger<DocumentbotStudioApiService> _logger;
        private string _documentbotapibaseurl;
        private Guid _documentbotId;
        private string _externalApiUrl;
        private string _apiKey;        

        public DocumentbotStudioApiService(IOptions<StudioApiBaseUrl> studioApiBaseUrl,
                                           HttpClient httpClient,
                                           ILogger<DocumentbotStudioApiService> logger)
        {
            _studioApiBaseUrl = studioApiBaseUrl.Value;
            _httpClient = httpClient;
            _logger = logger;
        }

        private async Task<HttpResponseMessage> SendHttpRequestAsync(string url, HttpMethod method, HttpContent content = null)
        {
            var result = new HttpResponseMessage();
            var retryPolicy = Policy
                                .Handle<HttpRequestException>((e) => e.StatusCode != HttpStatusCode.BadRequest && e.StatusCode != HttpStatusCode.Unauthorized)
                                .Or<Exception>()
                                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(3, retryAttempt)));

            await retryPolicy.ExecuteAsync(async () =>
            {
                var request = new HttpRequestMessage(method, url);

                if (content != null)
                {
                    request.Content = content;
                }

                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    result = response;
                }
            });
            return result;
        }

        public async Task SetupHttpClientAuthorizationHeaderAndApiUrl(DocumentGenerationUserInputDto req, StudioEnvironment environment)
        {
            _httpClient.DefaultRequestHeaders.Add("accept", "application/json");
            _httpClient.DefaultRequestHeaders.Add("authorization", req.AccessToken);
            _documentbotId = req.DocumentbotId;
            _externalApiUrl = req.ExternalApiEndpoint;
            _apiKey = req.ApiKey;

            switch (environment)
            {
                case StudioEnvironment.Dev:
                    _documentbotapibaseurl = $"{_studioApiBaseUrl.Dev}/documentbot/{_documentbotId}";
                    break;

                case StudioEnvironment.QA:
                    _documentbotapibaseurl = $"{_studioApiBaseUrl.QA}/documentbot/{_documentbotId}";
                    break;

                case StudioEnvironment.Staging:
                    _documentbotapibaseurl = $"{_studioApiBaseUrl.Staging}/documentbot/{_documentbotId}";
                    break;
            }
        }

        public async Task<List<EntityHelperDto>> GetDocumentbotEntitiesFromStudio()
        {
            string url = $"{_documentbotapibaseurl}/entity/GetDocumentbotEntities?pageNumber=1&pageSize=100&name=&value=0&documentbotId={_documentbotId}";
            var entityResponse = new EntityResponseDto();

            HttpResponseMessage response = await SendHttpRequestAsync(url, HttpMethod.Get);

            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                entityResponse = JsonConvert.DeserializeObject<EntityResponseDto>(responseContent);
                _logger.LogInformation("Entites successfully fetched from bot");
            }
            else
            {
                _logger.LogError($"Error: {response.StatusCode} \n Description: {await response.Content.ReadAsStringAsync()}");
            }

            return entityResponse.Result.Records;
        }

        public async Task<bool> SendDocumentToBotInStudio(byte[] file, string fileNameWithExtension)
        {
            var result = false;
            var retryPolicy = Policy
                                 .Handle<HttpRequestException>((e) => e.StatusCode != HttpStatusCode.BadRequest && e.StatusCode != HttpStatusCode.Unauthorized)
                                 .Or<Exception>()
                                 .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(3, retryAttempt)));

            await retryPolicy.ExecuteAsync(async () =>
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("x-api-key", _apiKey);

                    using var formData = new MultipartFormDataContent();
                    using var fileContent = new StreamContent(new MemoryStream(file));

                    formData.Add(fileContent, "files", fileNameWithExtension);
                    var response = client.PostAsync(_externalApiUrl, formData).GetAwaiter().GetResult();

                    if (response.IsSuccessStatusCode)
                    {
                        _logger.LogInformation($"{fileNameWithExtension} successfully sent to bot");
                        result = true;
                    }
                    else
                    {
                        result = false;
                    }
                }
            });
            return result;
        }

        public async Task<DocumentSearchResponseDto> SearchForDocumentId(string requestBody)
        {
            string url = $"{_documentbotapibaseurl}/Document/search";
            var searchResponse = new DocumentSearchResponseDto();

            using var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
            var response = await SendHttpRequestAsync(url, HttpMethod.Post, content);

            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                searchResponse = JsonConvert.DeserializeObject<DocumentSearchResponseDto>(responseContent);
                _logger.LogInformation("Document search successfully executed");
            }
            else
            {
                _logger.LogError($"Error: {response.StatusCode} \n Description: {await response.Content.ReadAsStringAsync()}");
            }

            return searchResponse;
        }

        public async Task<DocumentDetailsResponseDto> GetDocumentDetailsFromStudio(Guid documentId)
        {
            string url = $"{_documentbotapibaseurl}/Document/{documentId}";

            var response = await SendHttpRequestAsync(url, HttpMethod.Get);
            var result = new DocumentDetailsResponseDto();

            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                result = JsonConvert.DeserializeObject<DocumentDetailsResponseDto>(responseContent);
                _logger.LogInformation("Document details successfully fetched from bot");
            }
            else
            {
                _logger.LogError($"Error: {response.StatusCode} \n Description: {await response.Content.ReadAsStringAsync()}");
            }

            return result;
        }

        public async Task<bool> UpdateDocumentTaggingInStudio(string request, Guid documentId)
        {
            string url = $"{_documentbotapibaseurl}/Document/{documentId}/details";

            using var requestBody = new StringContent(request);
            requestBody.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var response = await SendHttpRequestAsync(url, HttpMethod.Put, requestBody);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Document tagging successfully updated");
            }
            else
            {
                _logger.LogError($"Error: {response.StatusCode} \n Description: {await response.Content.ReadAsStringAsync()}");
            }
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateDocumentStatusAsCompletedInStudio(IEnumerable<Guid> documentIds)
        {
            string url = $"{_documentbotapibaseurl}/Document/status";

            var requestData = new
            {
                documentIds = documentIds,
                documentStatus = 2
            };
            var jsonRequest = JsonConvert.SerializeObject(requestData);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            var response = await SendHttpRequestAsync(url, HttpMethod.Put, content);

            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                return true;
            }
            else
            {
                _logger.LogError($"Error: {response.StatusCode} \n Description: {await response.Content.ReadAsStringAsync()}");
                return false;
            }
        }
    }
}
