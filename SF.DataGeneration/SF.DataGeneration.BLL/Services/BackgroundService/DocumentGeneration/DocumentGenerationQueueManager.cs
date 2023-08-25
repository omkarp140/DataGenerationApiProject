using Microsoft.Extensions.Logging;
using SF.DataGeneration.BLL.BackgroundServices;
using SF.DataGeneration.Models.BackgroundJob;

namespace SF.DataGeneration.BLL.Services.BackgroundService.DocumentGeneration
{
    public interface IDocumentGenerationQueueManager
    {
        Task QueueDocumentGenerationJob(DocumentGenerationJobItem jobItem);
    }
    public class DocumentGenerationQueueManager : IDocumentGenerationQueueManager
    {
        private readonly BackgroundTaskQueue<DocumentGenerationJobItem> _taskQueue;
        private readonly ILogger<DocumentGenerationQueueManager> _logger;

        public DocumentGenerationQueueManager(BackgroundTaskQueue<DocumentGenerationJobItem> taskQueue,
                                       ILogger<DocumentGenerationQueueManager> logger)
        {
            _taskQueue = taskQueue;
            _logger = logger;
        }

        public async Task QueueDocumentGenerationJob(DocumentGenerationJobItem jobItem)
        {
            try
            {
                jobItem.Id = Guid.NewGuid();
                await _taskQueue.QueueJob(jobItem);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error adding JobItem - {jobItem.Id} of type : {Enum.GetName(jobItem.TaskType)}\n Exception: {ex.Message}");
            }
        }
    }
}
