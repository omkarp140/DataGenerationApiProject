using SF.DataGeneration.Models.Dto.Document;
using SF.DataGeneration.Models.Enum;

namespace SF.DataGeneration.BLL.Interfaces
{
    public interface IDocumentGenerationService
    {
        Task GenerateDocumentsWithExcelData(DocumentGenerationUserInputDto request, StudioEnvironment environment);
        Task CreateAnnontationSetup(StudioEnvironment environment, Guid documentbotId, string accessToken);
    }
}
