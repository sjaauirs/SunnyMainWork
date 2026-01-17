using System.ComponentModel.DataAnnotations;

namespace SunnyRewards.Helios.Tenant.Core.Domain.Dtos
{
    public class UpdateTenantDto
    {
        [Required]
        public string TenantCode { get; set; } = null!;
        [Required]
        public int PlanYear { get; set; }
        public DateTime PeriodStartTs { get; set; }
        public DateTime PeriodEndTs { get; set; }
        [Required]
        public string PartnerCode { get; set; } = null!;
        public bool RecommendedTask { get; set; }
        [Required]
        public string TenantAttribute { get; set; } = null!;
        [Required]
        public string redemption_vendor_name_0 { get; set; } = null!;
        [Required]
        public string redemption_vendor_partner_id_0 { get; set; } = null!;
        public string? ApiKey { get; set; }
        public bool SelfReport { get; set; }
        public bool DirectLogin { get; set; }
        public bool EnableServerLogin { get; set; }
        [Required]
        public string TenantName { get; set; } = null!;
        public string? EncKeyId { get; set; }
        public string? UpdateUser { get; set; } = string.Empty;
        public string? AuthConfig { get; set; }

    }
}
