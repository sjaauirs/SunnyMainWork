using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sunny.Benefits.Bff.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Common.Core.Helpers;

namespace Sunny.Benefits.Bff.Infrastructure.HttpClients
{
    public class NotificationClient : BaseClient, INotificationClient
    {
        public NotificationClient(IConfiguration configuration, ILogger<NotificationClient> logger) :
           base(configuration.GetSection("NotificationAPI").Value ?? throw new ArgumentNullException(nameof(configuration)), logger)
        {

        }
    }
}
