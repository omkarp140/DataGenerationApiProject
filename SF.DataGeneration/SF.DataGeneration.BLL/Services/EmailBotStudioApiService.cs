using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Polly;
using SF.DataGeneration.BLL.Interfaces;
using SF.DataGeneration.Models.Dto.Email;
using SF.DataGeneration.Models.Enum;
using SF.DataGeneration.Models.Settings;
using SF.DataGeneration.Models.StudioApiModels.RequestDto;
using System.Net;
using System.Text;

namespace SF.DataGeneration.BLL.Services
{
    public class EmailBotStudioApiService : IEmailBotStudioApiService
    {
        private readonly HttpClient _httpClient;
        private readonly StudioApiBaseUrl _studioApiBaseUrl;
        private readonly ILogger<EmailBotStudioApiService> _logger;
        private string _emailbotApiBaseUrl;
        private Guid _emailbotId;
        private string _externalApiUrl;
        private string _apiKey;

        public EmailBotStudioApiService(HttpClient httpClient,
                                        IOptions<StudioApiBaseUrl> studioApiBaseUrl,
                                        ILogger<EmailBotStudioApiService> logger)
        {
            _httpClient = httpClient;
            _studioApiBaseUrl = studioApiBaseUrl.Value;
            _logger = logger;
        }

        public async Task SetupHttpClientAuthorizationHeaderAndApiUrl(EmailDataGenerationUserInputDto request, StudioEnvironment environment)
        {
            _httpClient.DefaultRequestHeaders.Add("accept", "application/json");
            _httpClient.DefaultRequestHeaders.Add("authorization", request.AccessToken);
            _emailbotId = request.EmailbotId;
            _externalApiUrl = request.ExternalApiEndpoint;
            _apiKey = request.ApiKey;

            switch (environment)
            {
                case StudioEnvironment.Dev:
                    _emailbotApiBaseUrl = $"{_studioApiBaseUrl.Dev}/emailbot/{_emailbotId}";
                    break;

                case StudioEnvironment.QA:
                    _emailbotApiBaseUrl = $"{_studioApiBaseUrl.QA}/emailbot/{_emailbotId}";
                    break;

                case StudioEnvironment.Staging:
                    _emailbotApiBaseUrl = $"{_studioApiBaseUrl.Staging}/emailbot/{_emailbotId}";
                    break;
            }
        }

        public async Task<bool> SendEmailToBotInStudio(EmailBodyDto email)
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

                    var jsonStr = JsonConvert.SerializeObject(email);
                    var content = new StringContent(jsonStr, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync(_externalApiUrl, content);
                    //var response = client.PostAsync(_externalApiUrl, content).GetAwaiter().GetResult();

                    if (response.IsSuccessStatusCode)
                    {
                        _logger.LogInformation($"Email - '{email.Subject}' successfully sent to bot");
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
    }
}
