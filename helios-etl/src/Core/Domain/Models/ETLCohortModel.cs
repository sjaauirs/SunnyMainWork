using SunnyRewards.Helios.ETL.Common.Domain.Models;

namespace SunnyRewards.Helios.ETL.Core.Domain.Models
{
    public class ETLCohortModel : BaseModel
    {
        public virtual long CohortId { get; set; }
        public virtual string? CohortCode { get; set; }
        public virtual string? CohortName { get; set; }
        public virtual string CohortDescription { get; set; } = string.Empty;
        public virtual long ParentCohortId { get; set; }
        public virtual string? CohortRule { get; set; }
        public virtual bool CohortEnabled { get; set; }
        public virtual bool IncludeInCohortingJob { get; set; }
    }
}
