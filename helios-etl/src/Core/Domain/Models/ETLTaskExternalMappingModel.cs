using SunnyRewards.Helios.ETL.Common.Domain.Models;

namespace SunnyRewards.Helios.ETL.Core.Domain.Models
{
    public class ETLTaskExternalMappingModel : BaseModel
    {
        public virtual long TaskExternalMappingId { get; set; }
        public virtual string? TenantCode { get; set; }
        public virtual string? TaskThirdPartyCode { get; set; }
        public virtual string? TaskExternalCode { get; set; }
       
    }
}
