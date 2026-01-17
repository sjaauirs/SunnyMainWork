using SunnyRewards.Helios.Common.Core.Domain.Models;

namespace SunnyRewards.Helios.User.Core.Domain.Models
{
    public class AddressTypeModel : BaseModel
    {
        public virtual long AddressTypeId { get; set; }
        public virtual string? AddressTypeCode { get; set; }
        public virtual string? AddressTypeName { get; set; }
        public virtual string? Description { get; set; }
    }
}
