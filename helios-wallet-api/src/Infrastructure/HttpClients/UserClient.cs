using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Common.Core.Helpers;
using SunnyRewards.Helios.Wallet.Infrastructure.HttpClients.Interfaces;

namespace SunnyRewards.Helios.Wallet.Infrastructure.HttpClients
{
    public class UserClient : BaseClient, IUserClient
    {
        public UserClient(IConfiguration configuration, ILogger<UserClient> logger) :
           base(configuration.GetSection("UserAPI").Value ?? throw new ArgumentNullException(nameof(configuration)), logger)
        {

        }
    }
}
