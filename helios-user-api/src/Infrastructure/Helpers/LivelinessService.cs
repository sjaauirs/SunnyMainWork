using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace SunnyRewards.Helios.User.Infrastructure.Helpers
{
    public class LivelinessService : IHealthCheckPublisher
    {
        private readonly ILogger _logger;

        public LivelinessService(ILogger<LivelinessService> logger)
        {
            _logger = logger;
        }

        public Task PublishAsync(HealthReport report, CancellationToken cancellationToken)
        {
            if (report.Status == HealthStatus.Healthy)
            {
                _logger.LogInformation("{Timestamp} Liveliness Probe Status: {Result}",
                    DateTime.UtcNow, report.Status);
            }
            else
            {
                _logger.LogError("{Timestamp} Liveliness Probe Status: {Result}",
                    DateTime.UtcNow, report.Status);
            }
            cancellationToken.ThrowIfCancellationRequested();
            return Task.CompletedTask;
        }
    }
}