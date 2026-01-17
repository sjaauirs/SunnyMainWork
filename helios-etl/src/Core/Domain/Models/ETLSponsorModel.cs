using SunnyRewards.Helios.ETL.Common.Domain.Models;

namespace SunnyRewards.Helios.ETL.Core.Domain.Models
{
    public class ETLSponsorModel : BaseModel
    {
        public virtual long? SponsorId { get; set; }
        public virtual long? CustomerId { get; set; }
        public virtual string? SponsorCode { get; set; }
        public virtual string? SponsorName { get; set; }
        public virtual string? SponsorDescription { get; set; }
    }
}