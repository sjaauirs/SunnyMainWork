using SunnyRewards.Helios.Common.Core.Domain.Models;

namespace SunnyRewards.Helios.ETL.Core.Domain.Models
{
    public class ETLPersonAddressModel : BaseModel
    {
        public virtual long PersonAddressId { get; set; }
        public virtual long AddressTypeId { get; set; }
        public virtual long PersonId { get; set; }
        public virtual string? AddressLabel { get; set; }
        public virtual string? Line1 { get; set; }
        public virtual string? Line2 { get; set; }
        public virtual string? City { get; set; }
        public virtual string? State { get; set; }
        public virtual string? PostalCode { get; set; }
        public virtual string? Region { get; set; }
        public virtual string? CountryCode { get; set; }
        public virtual string? Country { get; set; }
        public virtual string? Source { get; set; }
        public virtual bool IsPrimary { get; set; }
    }
}
