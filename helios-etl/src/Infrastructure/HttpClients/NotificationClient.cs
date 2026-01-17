using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.ETL.Common.Helpers;
using SunnyRewards.Helios.ETL.Infrastructure.HttpClients.Interfaces;

namespace SunnyRewards.Helios.ETL.Infrastructure.HttpClients
{
    public class NotificationClient : BaseClient, INotificationClient
    {
        public NotificationClient(IConfiguration configuration, ILogger<NotificationClient> logger) :
            base(configuration.GetSection("NotificationAPI").Value ?? throw new ArgumentNullException(nameof(configuration)), logger)
        {

        }
    }
}
