using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Common.Core.Helpers;

namespace SunnyRewards.Helios.Admin.Infrastructure.HttpClients
{
    public class HealthClient : BaseClient, IHealthClient
    {
        public HealthClient(IConfiguration configuration, ILogger<HealthClient> logger) :
            base(configuration.GetSection("HealthAPI").Value ?? throw new ArgumentNullException(nameof(configuration)), logger)
        {
        }
    }
}
