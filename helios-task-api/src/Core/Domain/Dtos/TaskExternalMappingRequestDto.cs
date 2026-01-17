using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class TaskExternalMappingRequestDto
    {
        public long TaskExternalMappingId { get; set; }
        [Required]
        public required string? TenantCode { get; set; }
        [Required]

        public required string? TaskThirdPartyCode { get; set; }
        [Required]

        public required string? TaskExternalCode { get; set; }
        [Required]

        public required string? CreateUser { get; set; }
    }
}
