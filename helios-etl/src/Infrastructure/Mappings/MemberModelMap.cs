using SunnyRewards.Helios.ETL.Common.Mappings;
using SunnyRewards.Helios.ETL.Core.Domain.Models;

namespace SunnyRewards.Helios.ETL.Infrastructure.Mappings
{
    public class MemberModelMap : BaseMapping<MemberModel>
    {
        public MemberModelMap()
        {
            Schema("eligibility");
            Table("member_in"); 

            Id(x => x.Id).Column("member_in_id").GeneratedBy.Identity();
            Map(x => x.MemNbr).Column("mem_nbr");
            Map(x => x.LastName).Column("mem_lname");
            Map(x => x.FirstName).Column("mem_fname");
            Map(x => x.MiddleName).Column("mem_mname");
            Map(x => x.DateOfBirth).Column("mem_dob");
            Map(x => x.DeceasedDate).Column("deceased_dt");
            Map(x => x.Gender).Column("mem_gender");
            Map(x => x.Address1).Column("mem_addr1");
            Map(x => x.Address2).Column("mem_addr2");
            Map(x => x.Address3).Column("mem_addr3");
            Map(x => x.City).Column("mem_city");
            Map(x => x.State).Column("mem_state");
            Map(x => x.ZipCode).Column("mem_zip");
            Map(x => x.Country).Column("mem_county");
            Map(x => x.PhoneNumber).Column("mem_phone");
            Map(x => x.Email).Column("mem_email");
            Map(x => x.SocialSecurityNumber).Column("mem_ssn");
            Map(x => x.HispOrigin).Column("mem_hisp_orig");
            Map(x => x.NeedInterpreter).Column("mem_need_interpreter");
            Map(x => x.RaceCode1).Column("race_code_1");
            Map(x => x.RaceCode2).Column("race_code_2");
            Map(x => x.RaceCode3).Column("race_code_3");
            Map(x => x.HicNumber).Column("hic_number");
            Map(x => x.MbiNumber).Column("mbi_number");
            Map(x => x.MrnNumber).Column("mrn_number");
            Map(x => x.guardLastName).Column("guard_lname");
            Map(x => x.guardFirstName).Column("guard_fname");
            Map(x => x.guardMiddleName).Column("guard_mname");
            Map(x => x.guardEmail).Column("guard_email");
            Map(x => x.SpokenLanguageSource).Column("spoken_language_source");
            Map(x => x.WrittenLanguageSource).Column("written_language_source");
            Map(x => x.OtherLanguageSource).Column("other_language_source");
            Map(x => x.SpokenLanguageId).Column("spoken_language_id");
            Map(x => x.WrittenLanguageId).Column("written_language_id");
            Map(x => x.OtherLanguageId).Column("other_language_id");
            Map(x => x.RaceSource).Column("race_source");
            Map(x => x.EthnicitySource).Column("ethnicity_source");
            Map(x => x.DeleteNbr).Column("delete_nbr");
        }
    }
}
