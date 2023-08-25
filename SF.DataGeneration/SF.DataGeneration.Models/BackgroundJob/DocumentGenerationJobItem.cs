using SF.DataGeneration.Models.Dto.Document;
using SF.DataGeneration.Models.Enum;

namespace SF.DataGeneration.Models.BackgroundJob
{
    public class DocumentGenerationJobItem : BackgroundJobItem
    {
        public override TaskTypeEnum TaskType => TaskTypeEnum.DocumentGeneration;
        public DocumentGenerationUserInputDto Request { get; set; }
        public StudioEnvironment Environment { get; set; }
    }
}
