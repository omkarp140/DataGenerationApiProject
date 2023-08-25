using SF.DataGeneration.Models.Common;

namespace SF.DataGeneration.Models.Dto.Document
{
    public class DocumentGenerationUserInputDto: CommonUserInputDto
    {
        public Guid DocumentbotId { get; set; }
        public string ExcelFilePath { get; set; }
        public string DocumentNamePrefix { get; set; }
        public int NoOfDocumentsToCreate { get; set; }
        public Guid GreetingIntentId { get; set; }
    }
}
