using SunnyRewards.Helios.ETL.Common.Mappings;
using SunnyRewards.Helios.ETL.Core.Domain.Models;

namespace SunnyRewards.Helios.ETL.Infrastructure.Mappings
{
    public class ETLConsumerModelMap : BaseMapping<ETLConsumerModel>
    {
        public ETLConsumerModelMap()
        {
            Schema("huser");
            Table("consumer");

            Id(x => x.ConsumerId).Column("consumer_id").GeneratedBy.Identity();
            Map(x => x.PersonId).Column("person_id");
            Map(x => x.UpdateTs).Column("update_ts");
            Map(x => x.CreateTs).Column("create_ts");
            Map(x => x.UpdateUser).Column("update_user");
            Map(x => x.CreateUser).Column("create_user");
            Map(x => x.DeleteNbr).Column("delete_nbr");
            Map(x => x.ConsumerCode).Column("consumer_code");
            Map(x => x.TenantCode).Column("tenant_code");
            Map(x => x.Eligible).Column("eligible");
            Map(x => x.EligibleEndTs).Column("eligible_end_ts");
            Map(x => x.EligibleStartTs).Column("eligible_start_ts");
            Map(x => x.RegistrationTs).Column("registration_ts");
            Map(x => x.Registered).Column("registered");
            Map(x => x.MemberNbr).Column("mem_nbr");
            Map(x => x.SubscriberMemberNbr).Column("subscriber_mem_nbr");
            Map(x => x.ConsumerAttribute).Column("consumer_attr").CustomSqlType("jsonb").CustomType<StringAsJsonb>();
            Map(x => x.AnonymousCode).Column("anonymous_code");
            Map(x => x.EnrollmentStatus).Column("enrollment_status");
            Map(x => x.EnrollmentStatusSource).Column("enrollment_status_source");
            Map(x => x.IsSSOUser).Column("is_sso_user");
            Map(x => x.Auth0UserName).Column("auth0_user_name");
            Map(x => x.RegionCode).Column("region_code");
            Map(x => x.SubsciberMemberNbrPrefix).Column("subscriber_mem_nbr_prefix");
            Map(x => x.MemberNbrPrefix).Column("mem_nbr_prefix");
            Map(x => x.PlanId).Column("plan_id");
            Map(x => x.SubgroupId).Column("subgroup_id");
            Map(x => x.PlanType).Column("plan_type");
            Map(x => x.MemberId).Column("member_id").Nullable();
            Map(x => x.OnBoardingState).Column("onboarding_state");
            Map(x => x.MemberType).Column("member_type").Nullable();
            Map(x => x.AgreementStatus).Column("agreement_status");
            Map(x => x.AgreementFileName).Column("agreement_file_name").Nullable().CustomSqlType("jsonb").CustomType<StringAsJsonb>();

        }
    }
}