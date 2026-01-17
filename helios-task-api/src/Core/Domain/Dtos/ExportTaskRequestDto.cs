using System.ComponentModel.DataAnnotations;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class ExportTaskRequestDto
    {
        [Required]
        public required string TenantCode { get; set; }
    }
}
