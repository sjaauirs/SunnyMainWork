using System.ComponentModel.DataAnnotations;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class TenantTaskCategoryRequestDto
    {
        public long TenantTaskCategoryId { get; set; }
        [Required]
        public long TaskCategoryId { get; set; }
        [Required]
        public string? TenantCode { get; set; }
        public string? ResourceJson { get; set; }
        [Required]
        public string? CreateUser { get; set; }
    }
}
