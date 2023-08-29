using System.Collections.Concurrent;

namespace SF.DataGeneration.Models.Dto.Document
{
    public class TextReplacementHelperDto
    {
        public Guid EntityId { get; set; }
        public string OldText { get; set; }
        public string NewText { get; set; }
        public string EntityType { get; set; }
    }

    public class TextReplacementTempDto
    {
        public string OldText { get; set; }
        public string NewText { get; set; }
        public string EntityType { get; set; }

    }

    public class NewDocumentTempDto
    {
        public byte[] BinaryData { get; set; }
        public string FileName { get; set; }
    }
}
