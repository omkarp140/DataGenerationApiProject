using Newtonsoft.Json;

namespace SF.DataGeneration.Models.StudioApiModels.ResponseDto
{
    public class DocumentTypeHelperDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int Type { get; set; }
    }

    public class DocumentTypeResponseDto
    {
        [JsonProperty("result")]
        public DocumentTypeResult Result { get; set; }

    }

    public class DocumentTypeResult
    {
        [JsonProperty("records")]
        public List<DocumentTypeHelperDto> Records { get; set; }
    }
}
