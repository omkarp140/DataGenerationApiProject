using SF.DataGeneration.Models.BackgroundJob;
using System.Threading.Channels;

namespace SF.DataGeneration.BLL.BackgroundServices
{
    public sealed class BackgroundTaskQueue<T> where T : BackgroundJobItem
    {
        private readonly Channel<T> _taskQueue;
        private const int MaxWorkItemInChannel = 100;

        public BackgroundTaskQueue()
        {
            var options = new BoundedChannelOptions(MaxWorkItemInChannel)
            {
                FullMode = BoundedChannelFullMode.Wait
            };
            _taskQueue = Channel.CreateBounded<T>(options);
        }

        public async Task<bool> QueueJob(T jobItem, CancellationToken ct = default)
        {
            while (await _taskQueue.Writer.WaitToWriteAsync(ct) && !ct.IsCancellationRequested)
            {
                if (_taskQueue.Writer.TryWrite(jobItem))
                {
                    return true;
                }
            }
            return false;
        }

        public IAsyncEnumerable<T> ReadAllAsync(CancellationToken ct)
        {
            var workItem = _taskQueue.Reader.ReadAllAsync(ct);
            return workItem;
        }
    }
}
