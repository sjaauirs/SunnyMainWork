using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sunny.Benefits.Bff.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Common.Core.Helpers;
using System;

namespace Sunny.Benefits.Bff.Infrastructure.HttpClients
{
    public class CmsClient : BaseClient, ICmsClient
    {
        private readonly ILogger<CmsClient> _logger;
        private static int _instanceCount = 0;
        private readonly int _instanceId;

        public CmsClient(IConfiguration configuration, ILogger<CmsClient> logger) :
            base(configuration.GetSection("CmsAPI").Value ?? throw new ArgumentNullException(nameof(configuration)), logger)
        {
            _logger = logger;
            _instanceCount++;
            _instanceId = _instanceCount;
            
            // Log when a new CmsClient instance is created - helps identify connection pooling issues
            _logger.LogInformation(
                "[CONNECTION-POOL-DIAG] CmsClient instance #{InstanceId} created (Total instances: {TotalInstances}), HttpClient HashCode: {HttpClientHashCode}",
                _instanceId, _instanceCount, this.GetHashCode());
        }
    }
}
