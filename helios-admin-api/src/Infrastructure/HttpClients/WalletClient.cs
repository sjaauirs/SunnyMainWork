using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Common.Core.Helpers;

namespace SunnyRewards.Helios.Admin.Infrastructure.HttpClients
{
    /// <summary>
    /// 
    /// </summary>
    public class WalletClient : BaseClient, IWalletClient
    {
        public WalletClient(IConfiguration configuration, ILogger<WalletClient> logger) :
            base(configuration.GetSection("WalletAPI").Value ?? throw new ArgumentNullException(nameof(configuration)), logger)
        {
        }
    }
}
