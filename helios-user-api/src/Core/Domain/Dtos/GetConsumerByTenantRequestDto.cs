using System.ComponentModel.DataAnnotations;

namespace SunnyRewards.Helios.User.Core.Domain.Dtos
{
    public class GetConsumerByTenantRequestDto
    {
        [Required(ErrorMessage = "TenantCode is Required")]
        public string? TenantCode { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Page number must be greater than 0.")]
        public int PageNumber { get; set; } = 1; 

        [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100.")]
        public int PageSize { get; set; } = 10;  
        public string? SearchTerm { get; set; }  
    }
}
