using System.ComponentModel.DataAnnotations;

namespace SunnyRewards.Helios.User.Core.Domain.Dtos
{
    public class GetConsumerByConsumerCodes
    {
        [Required(ErrorMessage = "TenantCode is Required")]
        public string? TenantCode { get; set; }
        public List<string> ConsumerCodes { get; set; } = new();
    }
}
