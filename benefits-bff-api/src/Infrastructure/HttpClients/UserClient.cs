using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sunny.Benefits.Bff.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Common.Core.Helpers;

namespace Sunny.Benefits.Bff.Infrastructure.Repositories
{
    public class UserClient : BaseClient, IUserClient
    {
        public UserClient(IConfiguration configuration, ILogger<UserClient> logger) :
            base(configuration.GetSection("UserAPI").Value ?? throw new ArgumentNullException(nameof(configuration)), logger)
        {

        }
    }
}
