using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sunny.Benefits.Bff.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Common.Core.Helpers;

namespace Sunny.Benefits.Bff.Infrastructure.HttpClients
{
    public class WalletClient : BaseClient, IWalletClient
    {
        public WalletClient(IConfiguration configuration, ILogger<WalletClient> logger) :
            base(configuration.GetSection("WalletAPI").Value ?? throw new ArgumentNullException(nameof(configuration)), logger)
        { }
    }
}
