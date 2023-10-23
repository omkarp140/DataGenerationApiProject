using SF.DataGeneration.Models.Dto.Document;
using SF.DataGeneration.Models.Dto.Email;
using SF.DataGeneration.Models.Enum;
using SF.DataGeneration.Models.StudioApiModels.RequestDto;

namespace SF.DataGeneration.BLL.Interfaces
{
    public interface IEmailBotStudioApiService
    {
        Task SetupHttpClientAuthorizationHeaderAndApiUrl(EmailDataGenerationUserInputDto request, StudioEnvironment environment);
        Task<bool> SendEmailToBotInStudio(EmailBodyDto email);
    }
}
