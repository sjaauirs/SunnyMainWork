using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.User.Core.Domain.Dtos
{
    public class ConsumerLoginDateResponseDto : BaseResponseDto
    {
        public string? ConsumerCode { get; set; }
        public DateTime? LoginTs { get; set; }    
        public long ConsumerId { get; set; }    
    }
}
