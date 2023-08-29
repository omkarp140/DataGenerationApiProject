using SF.DataGeneration.Models.BackgroundJob.Common;
using System.Threading.Channels;

namespace SF.DataGeneration.BLL.BackgroundProcessing
{
    public sealed class BackgroundChannel<T> where T : BackgroundJobItem
    {
        private readonly Channel<T> _channel;
        private const int MaxWorkItemsInChannel = 100;

        public BackgroundChannel()
        {
            var options = new BoundedChannelOptions(MaxWorkItemsInChannel)
            {
                FullMode = BoundedChannelFullMode.Wait
            };
            _channel = Channel.CreateBounded<T>(options);
        }

        public async Task<bool> QueueJob(T jobItem, CancellationToken ct = default)
        {
            while(await _channel.Writer.WaitToWriteAsync(ct) && !ct.IsCancellationRequested)
            {
                if (_channel.Writer.TryWrite(jobItem))
                {
                    return true;
                }
            }
            return false;
        }

        public IAsyncEnumerable<T> ReadAllAsync(CancellationToken ct)
        {
            var workItem = _channel.Reader.ReadAllAsync(ct);
            return workItem;
        }
    }
}
