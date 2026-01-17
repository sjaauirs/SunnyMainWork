using SunnyRewards.Helios.ETL.Common.Domain.Models;

namespace SunnyRewards.Helios.ETL.Core.Domain.Models
{
    public class ETLTenantProgramConfigModel : BaseModel
    {
        public virtual long TenantProgramConfigId { get; set; }
        public virtual string? TenantProgramConfigCode { get; set; }
        public virtual string? TenantCode { get; set; }
        public virtual string? ClientId { get; set; }
        public virtual string? CompanyId { get; set; }
        public virtual string? SubprogramId { get; set; }
        public virtual string? PackageIdMapping { get; set; }
        public virtual string? DiscreteDataConfig { get; set; }
        public virtual DateTime CreatedAt { get; set; }
        public virtual DateTime UpdatedAt { get; set; }
        public virtual string? CreatedUser { get; set; }
        public virtual string? UpdatedUser { get; set; }
    }
}
