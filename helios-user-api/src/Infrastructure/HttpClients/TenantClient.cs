using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Common.Core.Helpers;
using SunnyRewards.Helios.User.Infrastructure.HttpClients.Interfaces;

namespace SunnyRewards.Helios.User.Infrastructure.HttpClients
{
    public class TenantClient : BaseClient, ITenantClient
    {
        public TenantClient(IConfiguration configuration, ILogger<TenantClient> baseLogger) :
           base(configuration.GetSection("TenantAPI").Value ?? throw new ArgumentNullException(nameof(configuration)),baseLogger)
        {

        }
    }
}
