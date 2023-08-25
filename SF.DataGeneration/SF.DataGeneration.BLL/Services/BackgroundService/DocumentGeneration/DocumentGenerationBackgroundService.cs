using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SF.DataGeneration.BLL.BackgroundServices;
using SF.DataGeneration.BLL.Interfaces;
using SF.DataGeneration.Models.BackgroundJob;

namespace SF.DataGeneration.BLL.Services.BackgroundService.DocumentGeneration
{
    public class DocumentGenerationBackgroundService : BackgroundWorker<DocumentGenerationJobItem>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DocumentGenerationBackgroundService> _logger;

        public DocumentGenerationBackgroundService(BackgroundTaskQueue<DocumentGenerationJobItem> taskQueue,
                                              ILogger<DocumentGenerationBackgroundService> logger,
                                              IServiceProvider serviceProvider) : base(taskQueue, logger)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task<bool> ProcessJobAsync(DocumentGenerationJobItem jobItem, CancellationToken ct)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                _logger.LogInformation("Ticket processing started");
                var svc = scope.ServiceProvider.GetRequiredService<IDocumentGenerationService>();
                await svc.GenerateDocumentsWithExcelData(jobItem.Request, jobItem.Environment);
                _logger.LogInformation("Tickets processed successfully");
                return true;
            }
        }
    }
}
