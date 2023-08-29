using Microsoft.Extensions.Logging;
using SF.DataGeneration.BLL.BackgroundProcessing;
using SF.DataGeneration.Models.BackgroundJob.DocumentSending;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SF.DataGeneration.BLL.Services.BackgroundServices.SendingDocumentsWithoutTagging
{
    public interface IDocumentSendingQueueManager
    {
        Task QueueDocumentSendingJob(DocumentSendingBackgroundJob jobItem);
    }


    public class DocumentSendingQueueManager : IDocumentSendingQueueManager
    {
        private readonly BackgroundChannel<DocumentSendingBackgroundJob> _backgroundChannel;
        private readonly ILogger<DocumentSendingQueueManager> _logger;

        public DocumentSendingQueueManager(BackgroundChannel<DocumentSendingBackgroundJob> backgroundChannel,
                                           ILogger<DocumentSendingQueueManager> logger)
        {
            _backgroundChannel = backgroundChannel;
            _logger = logger;
        }

        public async Task QueueDocumentSendingJob(DocumentSendingBackgroundJob jobItem)
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
