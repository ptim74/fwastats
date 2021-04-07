using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FWAStatsWeb.Services
{
    public class HostedWebSubmitService : HostedService
    {
        private WeightSubmitService submitService;
        public HostedWebSubmitService(WeightSubmitService submitService)
        {
            this.submitService = submitService;
        }
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await submitService.ProcessQueue(cancellationToken);
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }
    }
}
