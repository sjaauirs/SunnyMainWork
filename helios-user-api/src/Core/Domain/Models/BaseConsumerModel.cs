using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SunnyRewards.Helios.Common.Core.Domain.Models;

namespace SunnyRewards.Helios.User.Core.Domain.Models
{
    public class BaseConsumerModel : BaseModel
    {
        public virtual long ConsumerId { get; set; }
        public virtual long PersonId { get; set; }
        public virtual string? TenantCode { get; set; }
        public virtual string? ConsumerCode { get; set; }
        public virtual DateTime RegistrationTs { get; set; }
        public virtual DateTime EligibleStartTs { get; set; }
        public virtual DateTime EligibleEndTs { get; set; }
        public virtual bool Registered { get; set; }
        public virtual bool Eligible { get; set; }
        public virtual string? MemberNbr { get; set; }
        public virtual string? SubscriberMemberNbr { get; set; }
        public virtual string? ConsumerAttribute { get; set; }
        public virtual PersonModel? Person { get; set; }
        public virtual string? AnonymousCode { get; set; }
        public virtual string? EnrollmentStatus { get; set; }
        public virtual string? EnrollmentStatusSource { get; set; }
        public virtual string? OnBoardingState { get; set; }
        public virtual string? AgreementStatus { get; set; }
        public virtual string? RegionCode { get; set; }
        public virtual string? SubsciberMemberNbrPrefix { get; set; }
        public virtual string? MemberNbrPrefix { get; set; }
        public virtual string? PlanId { get; set; }
        public virtual string? SubgroupId { get; set; }
        public virtual string? PlanType { get; set; }
        public virtual string? AgreementFileName { get; set; }
        public virtual bool IsSSOUser { get; set; }
        public virtual string? Auth0UserName { get; set; }
        public virtual string? MemberId { get; set; }
        public virtual string? MemberType { get; set; }
        public virtual string? SubscriptionStatusJson { get; set; }


        public virtual JObject Attr
        {
            get
            {
                if (!string.IsNullOrEmpty(ConsumerAttribute))
                    return JsonConvert.DeserializeObject<JObject>(ConsumerAttribute) ?? new JObject();

                return new JObject();
            }
        }

        public virtual bool HasAttr(string name)
        {
            return Attr.ContainsKey(name);
        }
    }
}