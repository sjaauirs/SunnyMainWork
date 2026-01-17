using SunnyRewards.Helios.ETL.Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos.Json
{
    public class JobResultDetails
    {
        public List<string> Files { get; set; } = new List<string>();
        public int RecordsReceived { get; set; }
        public int RecordsProcessed { get; set; }
        public int RecordsSuccessCount { get; set; }
        public int RecordsErrorCount { get; set; }
    }

    public class ETLConsumerWalletAggregate
    {
        public ETLConsumerModel Consumer { get; set; }
        public ETLWalletModel Wallet { get; set; }
        public ETLPersonModel Person { get; set; }
        public ETLConsumerAccountModel? ConsumerAccount { get; set; }
    }

}
