using SunnyRewards.Helios.ETL.Common.Domain.Models;

namespace SunnyRewards.Helios.ETL.Core.Domain.Models
{
    public class ETLTenantSweepstakesModel : BaseModel
    {
        public virtual long TenantSweepstakesId { get; set; }
        public virtual long SweepstakesId { get; set; }
        public virtual string? TenantCode { get; set; }
    }
}