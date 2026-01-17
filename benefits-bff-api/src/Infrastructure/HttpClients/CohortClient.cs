using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sunny.Benefits.Bff.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Common.Core.Helpers;
using System;

namespace Sunny.Benefits.Bff.Infrastructure.HttpClients
{
    public class CohortClient : BaseClient, ICohortClient
    {
        private readonly ILogger<CohortClient> _logger;
        private static int _instanceCount = 0;
        private readonly int _instanceId;

        public CohortClient(IConfiguration configuration, ILogger<CohortClient> logger) :
            base(configuration.GetSection("CohortAPI").Value ?? throw new ArgumentNullException(nameof(configuration)), logger)
        {
            _logger = logger;
            _instanceCount++;
            _instanceId = _instanceCount;
            
            // Log when a new CohortClient instance is created - helps identify connection pooling issues
            _logger.LogInformation(
                "[CONNECTION-POOL-DIAG] CohortClient instance #{InstanceId} created (Total instances: {TotalInstances}), HttpClient HashCode: {HttpClientHashCode}",
                _instanceId, _instanceCount, this.GetHashCode());
        }
    }
}
