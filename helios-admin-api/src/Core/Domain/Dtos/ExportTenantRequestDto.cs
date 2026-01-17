using System.ComponentModel.DataAnnotations;

namespace SunnyRewards.Helios.Admin.Core.Domain.Dtos
{
    public class ExportTenantRequestDto
    {
        [Required]
        public string TenantCode { get; set; } = null!;

        [Required]
        public string[] ExportOptions { get; set; } = null!;
    }
}
