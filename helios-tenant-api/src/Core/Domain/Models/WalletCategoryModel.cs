using SunnyRewards.Helios.Common.Core.Domain.Models;

namespace SunnyRewards.Helios.Tenant.Core.Domain.Models
{
    public class WalletCategoryModel : BaseModel
    {
        public override int Id { get; set; }
        public virtual string TenantCode { get; set; } = string.Empty;
        public virtual long WalletTypeId { get; set; }
        public virtual string WalletTypeCode { get; set; } = string.Empty;
        public virtual int CategoryFk { get; set; }
        public virtual string? ConfigJson { get; set; }
    }
}
