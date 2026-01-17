using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Admin.Core.Domain.Dtos
{
    public class UpdateTenantTaskRewardScriptRequestDto
    {
      
        [Required]
        public required string TenantTaskRewardScriptCode { get; set; }
        public string? TenantCode { get; set; }
        public string? TaskRewardCode { get; set; }
        public string? ScriptType { get; set; }
        public string? ScriptCode { get; set; }
       
    }
}
