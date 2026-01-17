using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Common.Core.Helpers;
using SunnyRewards.Helios.User.Infrastructure.HttpClients.Interfaces;

namespace SunnyRewards.Helios.User.Infrastructure.HttpClients
{
    public class AdminClient : BaseClient, IAdminClient
    {
        public AdminClient(IConfiguration configuration, ILogger<AdminClient> baseLogger) :
           base(configuration.GetSection("AdminAPI").Value ?? throw new ArgumentNullException(nameof(configuration)), baseLogger)
        {

        }
    }
}
