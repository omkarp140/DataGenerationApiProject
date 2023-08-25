using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SF.DataGeneration.Models.BackgroundJob.Common;

namespace SF.DataGeneration.BLL.BackgroundProcessing
{
    public abstract class BackgroundWorker<T> : BackgroundService where T : BackgroundJobItem
    {
        private readonly BackgroundChannel<T> _backgroundChannel;
        private readonly ILogger<BackgroundWorker<T>> _logger;

        public BackgroundWorker(BackgroundChannel<T> backgroundChannel,
                                ILogger<BackgroundWorker<T>> logger)
        {
            _backgroundChannel = backgroundChannel;
            _logger = logger;
        }

        protected abstract Task<bool> ProcessJobAsync(T item, CancellationToken ct);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Queue hosted service is running.");
            
            await foreach(var item in _backgroundChannel.ReadAllAsync(stoppingToken))
            {
                await ProcessJob(item);
            }
        }

        public virtual async Task ProcessJob(T jobItem)
        {
            var ct = new CancellationTokenSource();
            if(jobItem.TimeOut.TotalSeconds > 0)
            {
                ct.CancelAfter(jobItem.TimeOut);
            }

            var logMsg = $"Job - {jobItem.Id} of type: {Enum.GetName(jobItem.TaskType)}";
            var success = await ProcessJobAsync(jobItem, ct.Token);
            _logger.LogInformation(logMsg + (success ? " processed successfully" : " failed to process"));
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Queue service is stopping.");
            await base.StopAsync(cancellationToken);
        }
    }
}
