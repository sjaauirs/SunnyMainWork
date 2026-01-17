using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sunny.Benefits.Bff.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Common.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sunny.Benefits.Bff.Infrastructure.HttpClients
{
    public class TenantClient : BaseClient, ITenantClient
    {
        public TenantClient(IConfiguration configuration, ILogger<TenantClient> logger) :
           base(configuration.GetSection("TenantAPI").Value ?? throw new ArgumentNullException(nameof(configuration)), logger)
        {

        }
    }
}
