using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sunny.Benefits.Bff.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Common.Core.Helpers;

namespace Sunny.Benefits.Bff.Infrastructure.HttpClients
{
    public class FisClient : BaseClient, IFisClient
    {
        public FisClient(IConfiguration configuration, ILogger<FisClient> logger) :
            base(configuration.GetSection("FisAPI").Value ?? throw new ArgumentNullException(nameof(configuration)), logger)
        {

        }
    }
}
