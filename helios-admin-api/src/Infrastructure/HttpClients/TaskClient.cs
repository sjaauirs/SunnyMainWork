using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Common.Core.Helpers;

namespace SunnyRewards.Helios.Admin.Infrastructure.HttpClients
{
    /// <summary>
    /// 
    /// </summary>
    public class TaskClient : BaseClient, ITaskClient
    {
        public TaskClient(IConfiguration configuration, ILogger<TaskClient> logger) :
            base(configuration.GetSection("TaskAPI").Value ?? throw new ArgumentNullException(nameof(configuration)), logger)
        {
        }
    }
}
