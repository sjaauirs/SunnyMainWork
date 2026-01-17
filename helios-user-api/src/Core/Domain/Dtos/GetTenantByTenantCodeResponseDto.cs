using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.User.Core.Domain.Dtos
{
    public class GetTenantByTenantCodeResponseDto : BaseDto
    {
        public long TenantId { get; set; }
        public long SponsorId { get; set; }
        public string? TenantCode { get; set; }
        public int PlanYear { get; set; }
        public DateTime PeriodStartTs { get; set; }
        public string? PartnerCode { get; set; }
        public bool RecommendedTask { get; set; }
        public string? TenantAttribute { get; set; }
        public string? redemption_vendor_name_0 { get; set; }
        public string? redemption_vendor_partner_id_0 { get; set; }
        public string? ApiKey { get; set; }
        public bool SelfReport { get; set; }
        public bool DirectLogin { get; set; }
        public bool EnableServerLogin { get; set; }
        public string? TenantName { get; set; }
    }
}
