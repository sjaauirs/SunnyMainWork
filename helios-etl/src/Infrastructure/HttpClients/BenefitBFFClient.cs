using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.ETL.Common.Helpers;
using SunnyRewards.Helios.ETL.Infrastructure.HttpClients.Interfaces;

namespace SunnyRewards.Helios.ETL.Infrastructure.HttpClients
{
    public class BenefitBFFClient : BaseClient, IBenefitBFFClient
    {
        public BenefitBFFClient(IConfiguration configuration, ILogger<BenefitBFFClient> logger) :
            base(configuration.GetSection("BenefitAPI").Value ?? throw new ArgumentNullException(nameof(configuration)), logger)
        {

        }
    }
}
