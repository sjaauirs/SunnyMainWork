using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.ETL.Common.Helpers;
using SunnyRewards.Helios.ETL.Infrastructure.HttpClients.Interfaces;

namespace SunnyRewards.Helios.ETL.Infrastructure.HttpClients
{
    public class HealthClient : BaseClient, IHealthClient
    {
        public HealthClient(IConfiguration configuration, ILogger<HealthClient> logger) : 
            base(configuration.GetSection("HealthAPI").Value ?? throw new ArgumentNullException(nameof(configuration)), logger)
        {
        }
    }
}
