using SunnyRewards.Helios.Common.Core.Domain.Models;

namespace SunnyRewards.Helios.Task.Core.Domain.Models
{
    public class TermsOfServiceModel : BaseModel
    {
        public virtual long TermsOfServiceId { get; set; }
        public virtual string? TermsOfServiceText { get; set; }
        public virtual string? LanguageCode { get; set; }
        public virtual string TermsOfServiceCode { get; set; } = null!;
        
    }
}
