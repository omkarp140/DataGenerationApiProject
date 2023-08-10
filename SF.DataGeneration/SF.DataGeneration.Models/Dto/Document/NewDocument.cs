using SF.DataGeneration.Models.StudioApiModels.ResponseDto;
using System.Collections.Concurrent;

namespace SF.DataGeneration.Models.Dto.Document
{
    public class NewDocument
    {
        public byte[] BinaryData { get; set; }
        public string FileName { get; set; }
        public ConcurrentBag<TextReplacementHelperDto> TextReplacementList { get; set; }
        public DocumentDetails DocumentDetails { get; set; }
    }
}
