using SunnyRewards.Helios.ETL.Common.Mappings;
using SunnyRewards.Helios.ETL.Core.Domain.Models;

namespace SunnyRewards.Helios.ETL.Infrastructure.Mappings
{
    public class EnrollmentMap : BaseMapping<EnrollmentModel>
    {
        public EnrollmentMap()
        {
            Schema("eligibility");
            Table("enrollment_in");

            Id(x => x.EnrollmentInId).Column("enrollment_in_id").GeneratedBy.Identity();
            Map(x => x.MemNbr).Column("mem_nbr");
            Map(x => x.EnrStart).Column("enr_start");
            Map(x => x.EnrEnd).Column("enr_end");
            Map(x => x.BenMedical).Column("ben_medical");
            Map(x => x.BenDent).Column("ben_dent");
            Map(x => x.BenRx).Column("ben_rx");
            Map(x => x.BenMhInp).Column("ben_mh_inp");
            Map(x => x.BenMhInt).Column("ben_mh_int");
            Map(x => x.BenMhAmb).Column("ben_mh_amb");
            Map(x => x.BenCdInp).Column("ben_cd_inp");
            Map(x => x.BenCdInt).Column("ben_cd_int");
            Map(x => x.BenCdAmb).Column("ben_cd_amb");
            Map(x => x.BenHospice).Column("ben_hospice");
            Map(x => x.BenDis).Column("ben_dis");
            Map(x => x.EnrSubscriberNum).Column("enr_subscriber_num");
            Map(x => x.HpEmployee).Column("hp_employee");
            Map(x => x.EmpNbr).Column("emp_nbr");
            Map(x => x.MedEligCatId).Column("med_elig_cat_id");
            Map(x => x.ProductId).Column("product_id");
            Map(x => x.ProductId2).Column("product_id_2");
            Map(x => x.CoverageIndicator).Column("coverage_indicator");
            Map(x => x.PbpNbr).Column("pbp_nbr");
            Map(x => x.SnpType).Column("snp_type");
            Map(x => x.AmpNbr).Column("amp_nbr");
            Map(x => x.CinNumber).Column("cin_number");
            Map(x => x.DeleteNbr).Column("delete_nbr");
        }
    }
}
