using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class UpdateSubtaskResponseDto : BaseResponseDto
    {
        public double AdditionalAmount { get; set; }
    }
}
