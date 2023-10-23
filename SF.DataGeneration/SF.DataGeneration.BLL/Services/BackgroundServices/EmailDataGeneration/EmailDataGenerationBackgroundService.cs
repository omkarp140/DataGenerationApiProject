using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SF.DataGeneration.BLL.BackgroundProcessing;
using SF.DataGeneration.BLL.Interfaces;
using SF.DataGeneration.Models.BackgroundJob.EmailDataGeneration;

namespace SF.DataGeneration.BLL.Services.BackgroundServices.EmailDataGeneration
{
    public class EmailDataGenerationBackgroundService : BackgroundWorker<EmailDataGenerationBackgroundJob>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<EmailDataGenerationBackgroundService> _logger;

        public EmailDataGenerationBackgroundService(BackgroundChannel<EmailDataGenerationBackgroundJob> backgroundChannel,
                                                   ILogger<EmailDataGenerationBackgroundService> logger,
                                                   IServiceProvider serviceProvider) : base(backgroundChannel, logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }
        protected override async Task<bool> ProcessJobAsync(EmailDataGenerationBackgroundJob item, CancellationToken ct)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                _logger.LogInformation("Email data generation started");
                var _emailDataGenerationService = scope.ServiceProvider.GetRequiredService<IEmailDataGenerationService>();
                await _emailDataGenerationService.SyncBulkEmailsOnEmailBot(item.Request, item.Environment);
                _logger.LogInformation("Email data generated successfully");
                return true;
            }
        }
    }
}
