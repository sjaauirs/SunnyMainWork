using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sunny.Benefits.Bff.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Common.Core.Helpers;

namespace Sunny.Benefits.Bff.Infrastructure.HttpClients
{
    public class ValidicClient : BaseClient, IValidicClient
    {
        public ValidicClient(IConfiguration configuration, ILogger<ValidicClient> logger) :
            base(configuration.GetSection("ValidicAPI").Value ?? throw new ArgumentNullException(nameof(configuration)), logger)
        {
        }
    }
}
