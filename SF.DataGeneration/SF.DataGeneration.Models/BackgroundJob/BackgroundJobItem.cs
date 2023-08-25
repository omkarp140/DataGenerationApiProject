namespace SF.DataGeneration.Models.BackgroundJob
{
    public abstract class BackgroundJobItem
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public abstract TaskTypeEnum TaskType { get; }
        public TimeSpan TimeOut { get; set; } = new TimeSpan(0, 10, 0);
    }
    public enum TaskTypeEnum
    {
        DocumentGeneration = 1
    }
}
