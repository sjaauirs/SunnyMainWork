using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sunny.Benefits.Bff.Infrastructure.Helpers
{
    public class LivelinessService : IHealthCheckPublisher
    {
        private readonly ILogger _logger;

        public LivelinessService(ILogger<LivelinessService> logger)
        {
            _logger = logger;
        }

        public System.Threading.Tasks.Task PublishAsync(HealthReport report, CancellationToken cancellationToken)
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
            return System.Threading.Tasks.Task.CompletedTask;
        }
    }
}
