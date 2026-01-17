using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class GetAdventureResponseDto:BaseResponseDto
    {
        public IList<AdventureDto> Adventures { get; set; } = new List<AdventureDto>();
    }
}
