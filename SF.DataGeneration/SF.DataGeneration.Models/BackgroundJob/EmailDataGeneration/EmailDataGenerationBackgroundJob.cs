using SF.DataGeneration.Models.BackgroundJob.Common;
using SF.DataGeneration.Models.BackgroundJob.Enum;
using SF.DataGeneration.Models.Dto.Email;
using SF.DataGeneration.Models.Enum;

namespace SF.DataGeneration.Models.BackgroundJob.EmailDataGeneration
{
    public class EmailDataGenerationBackgroundJob : BackgroundJobItem
    {
        public override TaskTypeEnum TaskType => TaskTypeEnum.EmailDataGeneration;
        public EmailDataGenerationUserInputDto Request { get; set; }
        public StudioEnvironment Environment { get; set; }
    }
}
