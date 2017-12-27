using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LWFStatsWeb.Services
{
    public enum WeightSubmitStatus
    {
        None,
        Queued,
        Running,
    }

    public interface IWeightSubmitService
    {
        void Queue(string tag);
        WeightSubmitStatus Status(string tag);
    }

    public class WeightSubmitService : HostedService, IWeightSubmitService
    {
        private ConcurrentQueue<string> q = new ConcurrentQueue<string>();
        private string currentTag = string.Empty;

        /*
        public WeightSubmitService()
        {
            q = new ConcurrentQueue<string>();
        }*/

        public void Queue(string tag)
        {
            q.Enqueue(tag);
        }

        public bool IsRunning(string tag)
        {
            if (currentTag != null && tag.Equals(currentTag, StringComparison.OrdinalIgnoreCase))
                return true;
            return false;
        }

        public WeightSubmitStatus Status(string tag)
        {
            if (currentTag != null && currentTag.Equals(tag, StringComparison.OrdinalIgnoreCase))
                return WeightSubmitStatus.Running;

            foreach(var t in q)
            {
                if (t.Equals(tag, StringComparison.OrdinalIgnoreCase))
                    return WeightSubmitStatus.Queued;
            }

            return WeightSubmitStatus.None;
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
            /*
            while(q.TryDequeue(out currentTag))
            {
                
            }
            */
            throw new NotImplementedException();
        }
    }
}
