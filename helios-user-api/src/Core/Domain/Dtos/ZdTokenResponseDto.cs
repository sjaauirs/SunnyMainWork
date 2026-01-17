using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.User.Core.Domain.Dtos
{
    public class ZdTokenResponseDto : BaseResponseDto
    {
        public string? Jwt { get; set; }

    }
}
