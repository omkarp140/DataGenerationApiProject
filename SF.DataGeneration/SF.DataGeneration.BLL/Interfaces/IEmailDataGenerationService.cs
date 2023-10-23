using SF.DataGeneration.Models.Dto.Email;
using SF.DataGeneration.Models.Enum;

namespace SF.DataGeneration.BLL.Interfaces
{
    public interface IEmailDataGenerationService
    {
        Task SyncBulkEmailsOnEmailBot(EmailDataGenerationUserInputDto request, StudioEnvironment environment);
    }
}
