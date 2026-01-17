using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.User.Core.Domain.Dtos
{
    public class ConsumerAttributesResponseDto : BaseResponseDto
    {
        public List<ConsumerDto> Consumers { get; set; } = new List<ConsumerDto>();
    }
}
