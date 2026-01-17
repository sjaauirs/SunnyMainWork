using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Admin.Core.Domain.Dtos
{
    public class ImageSearchResponseDto:BaseResponseDto
    {
        public List<string> lables { get; set; } = new List<string>();
        public List<string> logos { get; set; } = new List<string>();
        public string text { get; set; }
    }
}
