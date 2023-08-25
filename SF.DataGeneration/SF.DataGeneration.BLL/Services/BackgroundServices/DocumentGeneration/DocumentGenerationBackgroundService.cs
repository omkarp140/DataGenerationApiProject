using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SF.DataGeneration.BLL.BackgroundProcessing;
using SF.DataGeneration.BLL.Interfaces;
using SF.DataGeneration.Models.BackgroundJob.DocumentGeneration;

namespace SF.DataGeneration.BLL.Services.BackgroundServices.DocumentGeneration
{
    public class DocumentGenerationBackgroundService : BackgroundWorker<DocumentGenerationBackgroundJob>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DocumentGenerationBackgroundService> _logger;

        public DocumentGenerationBackgroundService(BackgroundChannel<DocumentGenerationBackgroundJob> backgroundChannel,
                                                   ILogger<DocumentGenerationBackgroundService> logger,
                                                   IServiceProvider serviceProvider) : base(backgroundChannel, logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task<bool> ProcessJobAsync(DocumentGenerationBackgroundJob jobItem, CancellationToken ct)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                _logger.LogInformation("Document generation started");
                var _documentGenerationService = scope.ServiceProvider.GetRequiredService<IDocumentGenerationService>();
                await _documentGenerationService.GenerateDocumentsWithExcelData(jobItem.Request, jobItem.Environment);
                _logger.LogInformation("Documents generated successfully");
                return true;
            }
        }
    }
}
