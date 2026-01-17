using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sunny.Benefits.Bff.Core.Domain.Dtos
{
    public class FaqSectionResponseDto : BaseResponseDto
    {       
        public string? HtmlContentUrl { get; set; }
        public List<FaqSectionDto>? FaqSections { get; set; }
    }
}
