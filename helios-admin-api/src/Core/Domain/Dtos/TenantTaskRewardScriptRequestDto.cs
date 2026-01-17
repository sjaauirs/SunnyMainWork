using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Admin.Core.Domain.Dtos
{
    public class TenantTaskRewardScriptRequestDto
    {
        [Required]
        public required string TenantCode { get; set; }
        [Required]
        public required string TaskRewardCode { get; set; }
        [Required]
        public required string ScriptType { get; set; }
        [Required]
        public required string ScriptCode { get; set; }


    }
}
