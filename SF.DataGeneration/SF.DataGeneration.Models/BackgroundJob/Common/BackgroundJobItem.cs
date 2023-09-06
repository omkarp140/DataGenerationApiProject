using SF.DataGeneration.Models.BackgroundJob.Enum;

namespace SF.DataGeneration.Models.BackgroundJob.Common
{
    public abstract class BackgroundJobItem
    {
        public Guid Id { get; set; }
        public abstract TaskTypeEnum TaskType { get; }
        public TimeSpan TimeOut { get; set; } = new TimeSpan(0, 10, 0);
    }
}
