using SF.DataGeneration.Models.Dto.Document;
using SF.DataGeneration.Models.Enum;
using SF.DataGeneration.Models.StudioApiModels.ResponseDto;

namespace SF.DataGeneration.BLL.Interfaces
{
    public interface IDocumentbotStudioApiService
    {
        Task SetupHttpClientAuthorizationHeaderAndApiUrl(DocumentGenerationUserInputDto accessToken, StudioEnvironment environment);
        Task<List<EntityHelperDto>> GetDocumentbotEntitiesFromStudio();
        Task<bool> SendDocumentToBotInStudio(byte[] file, string fileNameWithExtension);
        Task<DocumentSearchResponseDto> SearchForDocumentId(string requestBody);
        Task<DocumentDetailsResponseDto> GetDocumentDetailsFromStudio(Guid documentId);
        Task<bool> UpdateDocumentTaggingInStudio(string request, Guid documentId);        
        Task<bool> UpdateDocumentStatusAsCompletedInStudio(IEnumerable<Guid> documentIds);
    }
}
