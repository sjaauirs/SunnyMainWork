using System.ComponentModel.DataAnnotations;

namespace Sunny.Benefits.Bff.Core.Domain.Dtos
{
    public class FisSetEnrollNotificationsRequestDto
    {
        [Required]
        public string? TenantCode { get; set; }

        [Required]
        public string? ConsumerCode { get; set; }

        [Required]
        public string EnrolledNotifications { get; set; }
    }
}
