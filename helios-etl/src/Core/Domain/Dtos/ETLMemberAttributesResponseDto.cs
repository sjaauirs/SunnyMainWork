using SunnyRewards.Helios.ETL.Common.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos
{
    public class ETLMemberAttributesResponseDto : BaseResponseDto
    {
        public List<ConsumerDto> Consumer { get; set; } = new List<ConsumerDto>();
    }
}
