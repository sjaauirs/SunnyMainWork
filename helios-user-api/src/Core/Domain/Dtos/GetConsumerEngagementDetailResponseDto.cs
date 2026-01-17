using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.User.Core.Domain.Dtos
{
    public class GetConsumerEngagementDetailResponseDto : BaseResponseDto
    {
        public bool HasEngagement { get; set; } = false;
        public string ConsumerCode { get; set; } = string.Empty;
    }
}
