using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LWFStatsWeb.Services
{
    public interface IWeightSubmitService
    {
        void Queue(string tag);
        bool IsRunning(string tag);
        bool IsQueued(string tag);
    }

    public class WeightSubmitService : HostedService, IWeightSubmitService
    {
        private ConcurrentQueue<string> q = new ConcurrentQueue<string>();
        private ConcurrentQueue<int> qi = new ConcurrentQueue<int>();
        private string currentTag = string.Empty;

        public void Queue(string tag)
        {
            q.Enqueue(tag);
        }

        public bool IsRunning(string tag)
        {
            return tag != null && currentTag != null && tag.Equals(currentTag);
        }

        public bool IsQueued(string tag)
        {
            return tag != null && q != null && q.Contains(tag);
        }

        public int QueueSize()
        {
            return q == null ? 0 : q.Count;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await RunAsync(cancellationToken);
                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
            }
        }

        protected async Task RunAsync(CancellationToken cancellationToken)
        {
            while(q.TryDequeue(out string tag))
            {
                currentTag = tag;
            }
            currentTag = string.Empty;
            throw new NotImplementedException();
        }
    }
}
