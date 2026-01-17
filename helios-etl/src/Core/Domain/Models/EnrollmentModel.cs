
using SunnyRewards.Helios.ETL.Common.Domain.Models;

namespace SunnyRewards.Helios.ETL.Core.Domain.Models
{
    public class EnrollmentModel : BaseModel
    {
        public virtual int? EnrollmentInId { get; set; }
        public virtual string? MemNbr { get; set; }
        public virtual DateTime EnrStart { get; set; }
        public virtual DateTime EnrEnd { get; set; }
        public virtual string? BenMedical { get; set; }
        public virtual string? BenDent { get; set; }
        public virtual string? BenRx { get; set; }
        public virtual string? BenMhInp { get; set; }
        public virtual string? BenMhInt { get; set; }
        public virtual string? BenMhAmb { get; set; }
        public virtual string? BenCdInp { get; set; }
        public virtual string? BenCdInt { get; set; }
        public virtual string? BenCdAmb { get; set; }
        public virtual string? BenHospice { get; set; }
        public virtual string? BenDis { get; set; }
        public virtual string? EnrSubscriberNum { get; set; }
        public virtual string? HpEmployee { get; set; }
        public virtual string? EmpNbr { get; set; }
        public virtual int? MedEligCatId { get; set; }
        public virtual int? ProductId { get; set; }
        public virtual int? ProductId2 { get; set; }
        public virtual string? CoverageIndicator { get; set; }
        public virtual string? PbpNbr { get; set; }
        public virtual int? SnpType { get; set; }
        public virtual string? AmpNbr { get; set; }
        public virtual string? CinNumber { get; set; }
    }
}
