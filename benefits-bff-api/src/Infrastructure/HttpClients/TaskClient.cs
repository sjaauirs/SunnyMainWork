using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sunny.Benefits.Bff.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Common.Core.Helpers;

namespace Sunny.Benefits.Bff.Infrastructure.HttpClients
{
    public class TaskClient : BaseClient, ITaskClient
    {
        public TaskClient(IConfiguration configuration, ILogger<TaskClient> logger) :
            base(configuration.GetSection("TaskAPI").Value ?? throw new ArgumentNullException(nameof(configuration)), logger)
        {

        }
    }
}
