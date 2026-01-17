using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Common.Core.Helpers;

namespace SunnyRewards.Helios.Admin.Infrastructure.HttpClients
{
    /// <summary>
    /// 
    /// </summary>
    public class UserClient : BaseClient, IUserClient
    {
        public UserClient(IConfiguration configuration, ILogger<UserClient> logger) :
            base(configuration.GetSection("UserAPI").Value ?? throw new ArgumentNullException(nameof(configuration)), logger)
        {
        }
    }
}
