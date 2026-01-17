using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sunny.Benefits.Bff.Core.Domain.Dtos
{
    public class TokenResponse :BaseResponseDto
    {
        public string? access_token { get; set; }
        public string? scope { get; set; }
        public string? expires_in { get; set; }
        public string? token_type { get; set; }
    }
}
