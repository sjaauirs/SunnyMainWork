using SunnyRewards.Helios.ETL.Common.Domain.Models;

namespace SunnyRewards.Helios.ETL.Core.Domain.Models
{
    public class ETLSweepstakesResultModel : BaseModel
    {
        public virtual long SweepstakesResultId { get; set; }
        public virtual long SweepstakesInstanceId { get; set; }
        public virtual string TenantCode { get; set; }
        public virtual string ConsumerCode { get; set; }
        public virtual string PrizeIdentifier { get; set; }
        public virtual DateTime ResultTs { get; set; }
        public virtual string? PrizeDescribeJson { get; set; }
        public virtual bool IsRewarded { get; set; }
    }
}
