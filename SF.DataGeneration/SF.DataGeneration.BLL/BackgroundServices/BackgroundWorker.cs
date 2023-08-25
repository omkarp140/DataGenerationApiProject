using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SF.DataGeneration.Models.BackgroundJob;

namespace SF.DataGeneration.BLL.BackgroundServices
{
    public abstract class BackgroundWorker<T> : BackgroundService where T : BackgroundJobItem
    {
        private readonly BackgroundTaskQueue<T> _taskQueue;
        private readonly ILogger<BackgroundWorker<T>> _logger;

        public BackgroundWorker(BackgroundTaskQueue<T> taskQueue,
                                  ILogger<BackgroundWorker<T>> logger)
        {
            _taskQueue = taskQueue;
            _logger = logger;
        }

        protected abstract Task<bool> ProcessJobAsync(T item, CancellationToken ct);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Queue Hosted service is running.");

            await foreach (var item in _taskQueue.ReadAllAsync(stoppingToken))
            {
                await ProcessJob(item);
            }
        }

        public virtual async Task ProcessJob(T jobItem)
        {
            var ct = new CancellationTokenSource();
            if (jobItem.TimeOut.TotalSeconds > 0)
            {
                ct.CancelAfter(jobItem.TimeOut);
            }

            var logMsg = $"Job - {jobItem.Id} of type : {Enum.GetName(jobItem.TaskType)} ";
            var success = await ProcessJobAsync(jobItem, ct.Token);
            _logger.LogInformation(logMsg + (success ? "processed successfully" : "failed to process"));
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Queue service is stopping.");
            await base.StopAsync(cancellationToken);
        }
    }
}
