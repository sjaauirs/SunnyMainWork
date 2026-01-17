using System.ComponentModel.DataAnnotations;

namespace SunnyRewards.Helios.User.Core.Domain.Dtos
{
    public class ConsumerFlowProgressRequestDto
    {
        [Required]
        public string TenantCode { get; set; } = null!;
        [Required]
        public string ConsumerCode { get; set; } = null!;
        public List<string> CohortCodes { get; set; } = new List<string>();
    }
}
