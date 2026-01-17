using SunnyRewards.Helios.ETL.Common.Mappings;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.ETL.Infrastructure.Mappings
{
    public class ETLMemberImportFileDataMap : BaseMapping<ETLMemberImportFileDataModel>
    {
        public ETLMemberImportFileDataMap()
        {
            Schema("etl");
            Table("member_import_file_data");
            Id(x => x.MemberImportFileDataId).Column("member_import_file_data_id").GeneratedBy.Identity();
            Map(x => x.MemberImportFileId).Column("member_import_file_id");
            Map(x => x.RecordNumber).Column("record_number");
            Map(x => x.RawDataJson).Column("raw_data_json").CustomSqlType("jsonb").CustomType<StringAsJsonb>();
            Map(x => x.CreateTs).Column("create_ts");
            Map(x => x.UpdateTs).Column("update_ts");
            Map(x => x.CreateUser).Column("create_user");
            Map(x => x.UpdateUser).Column("update_user");
            Map(x => x.DeleteNbr).Column("delete_nbr");
            Map(x => x.MemberId).Column("member_id");
            Map(x => x.MemberType).Column("member_type");
            Map(x => x.LastName).Column("last_name");
            Map(x => x.FirstName).Column("first_name");
            Map(x => x.Gender).Column("gender");
            Map(x => x.Age).Column("age");
            Map(x => x.Dob).Column("dob");
            Map(x => x.Email).Column("email");
            Map(x => x.City).Column("city");
            Map(x => x.Country).Column("country");
            Map(x => x.PostalCode).Column("postal_code");
            Map(x => x.MobilePhone).Column("mobile_phone");
            Map(x => x.EmpOrDep).Column("emp_or_dep");
            Map(x => x.MemNbr).Column("mem_nbr");
            Map(x => x.SubscriberMemNbr).Column("subscriber_mem_nbr");
            Map(x => x.EligibilityStart).Column("eligibility_start");
            Map(x => x.EligibilityEnd).Column("eligibility_end");
            Map(x => x.MailingAddressLine1).Column("mailing_address_line1");
            Map(x => x.MailingAddressLine2).Column("mailing_address_line2");
            Map(x => x.MailingState).Column("mailing_state");
            Map(x => x.MailingCountryCode).Column("mailing_country_code");
            Map(x => x.HomePhoneNumber).Column("home_phone_number");
            Map(x => x.Action).Column("action");
            Map(x => x.PartnerCode).Column("partner_code");
            Map(x => x.MiddleName).Column("middle_name");
            Map(x => x.HomeAddressLine1).Column("home_address_line1");
            Map(x => x.HomeAddressLine2).Column("home_address_line2");
            Map(x => x.HomeState).Column("home_state");
            Map(x => x.HomeCity).Column("home_city");
            Map(x => x.HomePostalCode).Column("home_postal_code");
            Map(x => x.LanguageCode).Column("language_code");
            Map(x => x.RegionCode).Column("region_code");
            Map(x => x.SubscriberMemNbrPrefix).Column("subscriber_mem_nbr_prefix");
            Map(x => x.MemNbrPrefix).Column("mem_nbr_prefix");
            Map(x => x.PlanId).Column("plan_id");
            Map(x => x.PlanType).Column("plan_type");
            Map(x => x.SubgroupId).Column("subgroup_id");
            Map(x => x.IsSsoUser).Column("is_sso_user");
            Map(x => x.PersonUniqueIdentifier).Column("person_unique_identifier");
            Map(x => x.RecordProcessingStatus).Column("record_processing_status");
        }
    }
}
