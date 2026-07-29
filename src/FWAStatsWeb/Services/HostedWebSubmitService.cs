namespace FWAStatsWeb.Services;

public class HostedWebSubmitService : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(1);

    private readonly WeightSubmitService submitService;
    private readonly ILogger<HostedWebSubmitService> logger;

    public HostedWebSubmitService(
        WeightSubmitService submitService,
        ILogger<HostedWebSubmitService> logger)
    {
        this.submitService = submitService;
        this.logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(PollInterval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await submitService.ProcessQueue();
            }
            catch (Exception ex)
            {
                // ProcessQueue calls out to the submit script, so a transient failure is
                // expected. Letting it escape would stop the host, since BackgroundService
                // defaults to BackgroundServiceExceptionBehavior.StopHost.
                logger.LogError(ex, "Weight submit queue processing failed; continuing");
            }
        }
    }
}
