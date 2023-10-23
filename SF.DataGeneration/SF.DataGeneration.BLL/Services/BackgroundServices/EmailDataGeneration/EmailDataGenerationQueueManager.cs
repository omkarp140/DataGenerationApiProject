using Microsoft.Extensions.Logging;
using SF.DataGeneration.BLL.BackgroundProcessing;
using SF.DataGeneration.Models.BackgroundJob.EmailDataGeneration;

namespace SF.DataGeneration.BLL.Services.BackgroundServices.EmailDataGeneration
{
    public interface IEmailDataGenerationQueueManager
    {
        Task QueueEmailDataGenerationJob(EmailDataGenerationBackgroundJob jobItem);
    }

    public class EmailDataGenerationQueueManager : IEmailDataGenerationQueueManager
    {
        private readonly BackgroundChannel<EmailDataGenerationBackgroundJob> _backgroundChannel;
        private readonly ILogger<EmailDataGenerationQueueManager> _logger;

        public EmailDataGenerationQueueManager(BackgroundChannel<EmailDataGenerationBackgroundJob> backgroundChannel,
                                               ILogger<EmailDataGenerationQueueManager> logger)
        {
            _backgroundChannel = backgroundChannel;
            _logger = logger;
        }
        public async Task QueueEmailDataGenerationJob(EmailDataGenerationBackgroundJob jobItem)
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
