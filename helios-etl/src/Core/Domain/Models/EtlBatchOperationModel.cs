using SunnyRewards.Helios.ETL.Common.Domain.Models;

namespace SunnyRewards.Helios.ETL.Core.Domain.Models
{
    public class EtlBatchOperationModel : BaseModel
    {
        public virtual long BatchOperationId { get; set; }
        public virtual string BatchOperationCode { get; set; }
        public virtual string BatchOperationGroupCode { get; set; }
        public virtual string BatchAction{ get; set; }
        public virtual string action_description_json { get; set; } = String.Empty;
    }
}
