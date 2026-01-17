using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.User.Core.Domain.Dtos
{
    public class GetAllPhoneTypesResponseDto : BaseResponseDto
    {
        public IList<PhoneTypeDto>? PhoneTypesList { get; set; }
    }
}
