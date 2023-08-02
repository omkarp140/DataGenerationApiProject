using SF.DataGeneration.Models.StudioApiModels.ResponseDto;

namespace SF.DataGeneration.Models.StudioApiModels.RequestDto
{
    public class UpdateDocumentDetailsDto
    {
        public Guid? DocumentTypeId { get; set; }
        public DocumentTaggedDto DocumentTaggedDto { get; set; }
    }
}
