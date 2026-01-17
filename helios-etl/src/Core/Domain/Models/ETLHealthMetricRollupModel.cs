using Newtonsoft.Json;
using SunnyRewards.Helios.ETL.Common.Domain.Models;

namespace SunnyRewards.Helios.ETL.Core.Domain.Models
{
    public class ETLHealthMetricRollupModel : BaseModel
    {
        public virtual long HealthMetricRollupId { get; set; }
        public virtual long RollupPeriodTypeId { get; set; }
        public virtual string? TenantCode { get; set; }
        public virtual string? ConsumerCode { get; set; }
        public virtual DateTime RollupPeriodStartTs { get; set; }
        public virtual DateTime RollupPeriodEndTs { get; set; }
        public virtual string? RollupDataJson { get; set; }

        /// <summary>
        /// Deserialize the RollupDataJson to RollupData object
        /// </summary>
        public virtual RollupData? RollupData
        {
            get => RollupData != null ? JsonConvert.DeserializeObject<RollupData>(RollupDataJson) : null;
            set => RollupDataJson = value != null ? JsonConvert.SerializeObject(value) : null;
        }
    }

    public class RollupData
    {
        public virtual int TotalSteps { get; set; }
        public virtual int HeartRateAvg { get; set; }
        public virtual int HeartRateDenom { get; set; }
        public virtual int AverageHeartRate { get; set; }
        public virtual int TotalDistanceMiles { get; set; }
    }
}
