using SunnyRewards.Helios.ETL.Common.Domain.Models;

namespace SunnyRewards.Helios.ETL.Core.Domain.Models
{
    public class ETLTenantAccountModel : BaseModel
    {
        public virtual long TenantAccountId { get; set; }
        public virtual string? TenantAccountCode { get; set; }
        public virtual string? TenantCode { get; set; }
        public virtual string? AccLoadConfig { get; set; }
        public virtual string? TenantConfigJson { get; set; }
        public virtual string? FundingConfigJson { get; set; }
        public virtual long? LastMonetaryTransactionId { get; set; }
    }
}
