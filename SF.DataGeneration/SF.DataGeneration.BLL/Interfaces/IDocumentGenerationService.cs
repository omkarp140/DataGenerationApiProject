using SF.DataGeneration.Models.Dto.Document;
using SF.DataGeneration.Models.Enum;

namespace SF.DataGeneration.BLL.Interfaces
{
    public interface IDocumentGenerationService
    {
        Task GenerateDocumentsWithExcelData(DocumentGenerationUserInputDto request, StudioEnvironment environment);
        Task SendDocumentsToBotWithoutTagging(DocumentGenerationUserInputDto request, StudioEnvironment environment);
    }
}
