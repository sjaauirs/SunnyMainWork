using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.ETL.Core.Domain.Models
{
    public class ETLConsumerTaskRewardModel
    {
        public ETLConsumerTaskModel ConsumerTask { get; set; }
        public ETLTaskRewardModel TaskReward { get; set; }
    }
}
