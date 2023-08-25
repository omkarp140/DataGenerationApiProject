using SF.DataGeneration.Models.BackgroundJob.Common;
using SF.DataGeneration.Models.BackgroundJob.Enum;
using SF.DataGeneration.Models.Dto.Document;
using SF.DataGeneration.Models.Enum;

namespace SF.DataGeneration.Models.BackgroundJob.DocumentGeneration
{
    public class DocumentGenerationBackgroundJob : BackgroundJobItem
    {
        public override TaskTypeEnum TaskType => TaskTypeEnum.DocumentGeneration;
        public DocumentGenerationUserInputDto Request { get; set; }
        public StudioEnvironment Environment { get; set; }
    }
}
