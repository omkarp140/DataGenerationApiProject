using Grpc.Core.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SF.DataGeneration.BLL.Interfaces;
using SF.DataGeneration.Models.Dto.Document;
using SF.DataGeneration.Models.Enum;
using SF.DataGeneration.Models.Settings;
using SF.DataGeneration.Models.StudioApiModels.ResponseDto;
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

            HttpResponseMessage response = _httpClient.GetAsync(url).GetAwaiter().GetResult();

            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                entityResponse = JsonConvert.DeserializeObject<EntityResponseDto>(responseContent);
            }
            else
            {
                Console.WriteLine("Error: " + response.StatusCode);
            }
            return entityResponse.Result.Records;
        }

        public async Task<bool> SendDocumentToBotInStudio(byte[] file, string fileNameWithExtension)
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
                    string responseContent = await response.Content.ReadAsStringAsync();
                }
                else
                {
                    Console.WriteLine("Request failed with status code: " + response.StatusCode);
                }

                return response.IsSuccessStatusCode;
            }
        }

        public async Task<DocumentSearchResponseDto> SearchForDocumentId(string requestBody)
        {
            string url = $"{_documentbotapibaseurl}/Document/search";
            var searchResponse = new DocumentSearchResponseDto();

            using var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
            var response = _httpClient.PostAsync(url, content).GetAwaiter().GetResult();

            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                searchResponse = JsonConvert.DeserializeObject<DocumentSearchResponseDto>(responseContent);
            }
            else
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Error: " + responseContent);
            }
            return searchResponse;
        }

        public async Task<DocumentDetailsResponseDto> GetDocumentDetailsFromStudio(Guid documentId)
        {
            string url = $"{_documentbotapibaseurl}/Document/{documentId}";

            var response = _httpClient.GetAsync(url).GetAwaiter().GetResult();
            var result = new DocumentDetailsResponseDto();

            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                result = JsonConvert.DeserializeObject<DocumentDetailsResponseDto>(responseContent);
            }
            else
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Error: " + response.StatusCode);
            }
            return result;
        }

        public async Task UpdateDocumentTaggingInStudio(string request, Guid documentId)
        {
            string url = $"{_documentbotapibaseurl}/Document/{documentId}/details";

            using var requestBody = new StringContent(request);
            requestBody.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var response = _httpClient.PutAsync(url, requestBody).GetAwaiter().GetResult();

            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
            }
            else
            {
                Console.WriteLine("Error: " + response.StatusCode);
            }
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

            var response = _httpClient.PutAsync(url, content).GetAwaiter().GetResult();

            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                return true;
            }
            else
            {
                Console.WriteLine("Error: " + response.StatusCode);
                return false;
            }
        }
    }
}
