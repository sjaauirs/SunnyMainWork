using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Admin.Core.Domain.Dtos
{
    public class ScriptDto : BaseDto
    {

        public long ScriptId { get; set; }
        public string? ScriptCode { get; set; }
        public string? ScriptName { get; set; }
        public string? ScriptDescription { get; set; }
        public string? ScriptJson { get; set; }
        public string? ScriptSource { get; set; }
      


    }
    public class ScriptResponseDto : BaseResponseDto
    {

        public IList<ScriptDto> scripts { get; set; }

    }
}
