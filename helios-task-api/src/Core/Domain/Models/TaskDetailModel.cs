using SunnyRewards.Helios.Common.Core.Domain.Models;

namespace SunnyRewards.Helios.Task.Core.Domain.Models
{
    public class TaskDetailModel : BaseModel
    {
        public virtual long TaskId { get; set; }
        public virtual long TaskDetailId { get; set; }
        public virtual long TermsOfServiceId { get; set; }
        public virtual string? TaskHeader { get; set; }
        public virtual string? TaskDescription { get; set; }
        public virtual string? LanguageCode { get; set; }
        public virtual string? TenantCode { get; set; }
        public virtual string? TaskCtaButtonText { get; set; }  
    }
}
