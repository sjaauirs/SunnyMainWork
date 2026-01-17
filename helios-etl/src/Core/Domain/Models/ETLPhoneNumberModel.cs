using SunnyRewards.Helios.Common.Core.Domain.Models;

namespace SunnyRewards.Helios.ETL.Core.Domain.Models
{
    public class ETLPhoneNumberModel : BaseModel
    {
        public virtual long PhoneNumberId { get; set; }
        public virtual long PersonId { get; set; }
        public virtual long PhoneTypeId { get; set; }
        public virtual string? PhoneNumberCode { get; set; }
        public virtual string? PhoneNumber { get; set; }
        public virtual bool IsPrimary { get; set; }
        public virtual bool IsVerified { get; set; }
        public virtual DateTime? VerifiedDate { get; set; }
        public virtual string? Source { get; set; }
    }
}
