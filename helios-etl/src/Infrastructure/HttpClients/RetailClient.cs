using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.ETL.Common.Helpers;
using SunnyRewards.Helios.ETL.Infrastructure.HttpClients.Interfaces;

namespace SunnyRewards.Helios.ETL.Infrastructure.HttpClients
{
    public class RetailClient : BaseClient, IRetailClient
    {
        public RetailClient(IConfiguration configuration, ILogger<RetailClient> logger) :
            base(configuration.GetSection("RetailApi").Value ?? throw new ArgumentNullException(nameof(configuration)), logger)
        {

        }
    }
}