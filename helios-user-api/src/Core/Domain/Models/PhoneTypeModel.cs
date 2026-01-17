using SunnyRewards.Helios.Common.Core.Domain.Models;

namespace SunnyRewards.Helios.User.Core.Domain.Models
{
    public class PhoneTypeModel : BaseModel
    {
        public virtual long PhoneTypeId { get; set; }
        public virtual string? PhoneTypeCode { get; set; }
        public virtual string? PhoneTypeName { get; set; }
        public virtual string? Description { get; set; }
    }
}
