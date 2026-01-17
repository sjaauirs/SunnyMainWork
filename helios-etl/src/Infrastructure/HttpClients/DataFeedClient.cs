using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.ETL.Common.Helpers;
using SunnyRewards.Helios.ETL.Infrastructure.HttpClients.Interfaces;

namespace SunnyRewards.Helios.ETL.Infrastructure.HttpClients
{
    public class DataFeedClient : BaseClient, IDataFeedClient
    {
        public DataFeedClient(IConfiguration configuration, ILogger<DataFeedClient> logger) :
            base(configuration.GetSection("DatafeedApi").Value ?? throw new ArgumentNullException(nameof(configuration)), logger)
        {

        }
    }
}
