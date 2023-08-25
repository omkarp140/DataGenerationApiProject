using Microsoft.Extensions.Logging;
using SF.DataGeneration.BLL.BackgroundProcessing;
using SF.DataGeneration.Models.BackgroundJob.DocumentGeneration;

namespace SF.DataGeneration.BLL.Services.BackgroundServices.DocumentGeneration
{
    public interface IDocumentGenerationQueueManager
    {
        Task QueueDocumentGenerationJob(DocumentGenerationBackgroundJob jobItem);
    }

    public class DocumentGenerationQueueManager : IDocumentGenerationQueueManager
    {
        private readonly BackgroundChannel<DocumentGenerationBackgroundJob> _backgroundChannel;
        private readonly ILogger<DocumentGenerationQueueManager> _logger;

        public DocumentGenerationQueueManager(BackgroundChannel<DocumentGenerationBackgroundJob> backgroundChannel,
                                              ILogger<DocumentGenerationQueueManager> logger)
        {
            _backgroundChannel = backgroundChannel;
            _logger = logger;
        }

        public async Task QueueDocumentGenerationJob(DocumentGenerationBackgroundJob jobItem)
        {
            try
            {
                jobItem.Id = Guid.NewGuid();
                await _backgroundChannel.QueueJob(jobItem);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error adding JobItem = {jobItem.Id} of type : {Enum.GetName(jobItem.TaskType)}\n Exception: {ex.Message}");
            }
        }
    }
}
