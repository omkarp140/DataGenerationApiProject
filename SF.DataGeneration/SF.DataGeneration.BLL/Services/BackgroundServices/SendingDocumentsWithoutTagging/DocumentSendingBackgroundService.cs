using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SF.DataGeneration.BLL.BackgroundProcessing;
using SF.DataGeneration.BLL.Interfaces;
using SF.DataGeneration.Models.BackgroundJob.DocumentSending;

namespace SF.DataGeneration.BLL.Services.BackgroundServices.SendingDocumentsWithoutTagging
{
    public class DocumentSendingBackgroundService : BackgroundWorker<DocumentSendingBackgroundJob>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DocumentSendingBackgroundService> _logger;

        public DocumentSendingBackgroundService(IServiceProvider serviceProvider,
                                                ILogger<DocumentSendingBackgroundService> logger,
                                                BackgroundChannel<DocumentSendingBackgroundJob> backgroundChannel) : base(backgroundChannel, logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task<bool> ProcessJobAsync(DocumentSendingBackgroundJob jobItem, CancellationToken ct)
        {
            using(var scope = _serviceProvider.CreateScope())
            {
                _logger.LogInformation("Document sending without tagging started");
                var _documentSendingService = scope.ServiceProvider.GetRequiredService<IDocumentGenerationService>();
                await _documentSendingService.SendDocumentsToBotWithoutTagging(jobItem.Request, jobItem.Environment);
                _logger.LogInformation("Document sending completed successfully");
                return true;
            }
        }
    }
}
