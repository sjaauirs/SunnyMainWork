using SunnyRewards.Helios.ETL.Common.Domain.Models;

namespace SunnyRewards.Helios.ETL.Core.Domain.Models
{
    public class ETLTaskDetailModel : BaseModel
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