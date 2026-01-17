using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class GetAdventureRequestDto
    {
        [Required]
        public string TenantCode { get; set; } = string.Empty;
    }
}
