using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Zhenway.BatchRequestAggregrators
{
    internal sealed class Pool
    {
        private readonly SemaphoreSlim _semaphore;

        private readonly ConcurrentQueue<Func<Task>> _queue = new ConcurrentQueue<Func<Task>>();

        public Pool(int maxConcurrency)
        {
            _semaphore = new SemaphoreSlim(maxConcurrency);
        }

        public async void Queue(Func<Task> func)
        {
            _queue.Enqueue(func);
            if (_semaphore.Wait(0))
            {
                try
                {
                    await HandleQueue();
                }
                finally
                {
                    _semaphore.Release();
                }
            }
        }

        private async Task HandleQueue()
        {
            Func<Task> next;
            while (_queue.TryDequeue(out next))
            {
                try
                {
                    await next();
                }
                catch
                {
                    // silent
                }
            }
        }
    }
}
