using SunnyRewards.Helios.Common.Core.Domain.Models;

namespace SunnyRewards.Helios.Tenant.Core.Domain.Models
{
    public class TenantModel : BaseModel
    {
        public virtual long TenantId { get; set; }
        public virtual long SponsorId { get; set; }
        public virtual string? TenantCode { get; set; }
        public virtual int PlanYear { get; set; }
        public virtual DateTime PeriodStartTs { get; set; }
        public virtual DateTime PeriodEndTs { get; set; }
        public virtual string? PartnerCode { get; set; }
        public virtual bool RecommendedTask { get; set; }
        public virtual string? TenantAttribute { get; set; }
        public virtual string? redemption_vendor_name_0 { get; set; }
        public virtual string? redemption_vendor_partner_id_0 { get; set; }
        public virtual string? ApiKey { get; set; }
        public virtual bool Selfreport { get; set; }
        public virtual bool DirectLogin { get; set; }
        public virtual bool EnableServerLogin { get; set; }
        public virtual string TenantName { get; set; }
        public virtual string? EncKeyId { get; set; }
        public virtual string? TenantOption { get; set; }
        public virtual string? AuthConfig { get; set; }
        public virtual string? UtcTimeOffset{ get; set; }
        public virtual bool DstEnabled { get; set; }
    }
}
