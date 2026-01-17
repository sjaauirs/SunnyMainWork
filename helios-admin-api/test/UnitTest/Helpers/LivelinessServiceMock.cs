using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Moq;
using SunnyRewards.Helios.Admin.Infrastructure.Helpers;
using Xunit;

namespace SunnyRewards.Helios.Admin.UnitTest.Helpers
{
    public class LivelinessServiceMock
    {
        private readonly Mock<ILogger<LivelinessService>> _logger;
        private readonly Mock<Dictionary<string, HealthReportEntry>> _keyValuePairs;
        private readonly LivelinessService _livelinessService;
        public LivelinessServiceMock()
        {
            _logger = new Mock<ILogger<LivelinessService>>();
            _keyValuePairs = new Mock<Dictionary<string, HealthReportEntry>>();
            _livelinessService = new LivelinessService(_logger.Object);
        }

        [Fact]
        public void Check_Health_Report_Is_Healthy()
        {
            var healthReport = new HealthReport(_keyValuePairs.Object, HealthStatus.Healthy, TimeSpan.Zero);
            _livelinessService.PublishAsync(healthReport, CancellationToken.None);
            Assert.True(healthReport.Status.ToString() == "Healthy");
            Assert.True(healthReport.TotalDuration == TimeSpan.Zero);
        }

        [Fact]
        public void Check_Health_Report_Is_UnHealthy()
        {
            var healthReport = new HealthReport(_keyValuePairs.Object, HealthStatus.Unhealthy, TimeSpan.Zero);
            _livelinessService.PublishAsync(healthReport, CancellationToken.None);
            Assert.True(healthReport.Status.ToString() == "Unhealthy");
            Assert.True(healthReport.TotalDuration == TimeSpan.Zero);
        }
    }
}
