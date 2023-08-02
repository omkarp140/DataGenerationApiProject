using SF.DataGeneration.Models.Dto.Document;
using SF.DataGeneration.Models.Enum;

namespace SF.DataGeneration.BLL.Interfaces
{
    public interface IDocumentGenerationService
    {
        Task GenerateDocumentsOnBot(DocumentGenerationUserInputDto request, StudioEnvironment environment);
    }
}
