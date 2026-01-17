using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Common.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Admin.Infrastructure.HttpClients
{
    public class SweepstakesClient : BaseClient, ISweepstakesClient
    {
        public SweepstakesClient(IConfiguration configuration, ILogger<SweepstakesClient> logger) :
           base(configuration.GetSection("SweepstakesAPI").Value ?? throw new ArgumentNullException(nameof(configuration)), logger)
        {
        }
    }
}
