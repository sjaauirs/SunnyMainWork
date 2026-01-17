using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.User.Core.Domain.Dtos
{
    public class GetConsumerEngagementDetailRequestDto
    {
        public DateTime EngagementFrom { get; set; }
        public DateTime EngagementUntil { get; set; }
        public string ConsumerCode { get; set; } = string.Empty;
    }
}
