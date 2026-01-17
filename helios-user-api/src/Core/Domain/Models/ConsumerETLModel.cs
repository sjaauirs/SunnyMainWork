using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using SunnyRewards.Helios.Common.Core.Domain.Models;

namespace SunnyRewards.Helios.User.Core.Domain.Models
{
    public class ConsumerETLModel : BaseModel
    {

        public virtual long ConsumerId { get; set; }
        public virtual long PersonId { get; set; }
        public virtual string? TenantCode { get; set; }
        public virtual string? ConsumerCode { get; set; }
        public virtual DateTime RegistrationTs { get; set; }
        public virtual DateTime EligibleStartTs { get; set; }
        public virtual DateTime EligibleEndTs { get; set; }
        public virtual string? MemberNbr { get; set; }
        public virtual string? SubscriberMemberNbr { get; set; }
        public virtual PersonModel? Person { get; set; }
        public virtual string? RegionCode { get; set; }
        public virtual string? SubsciberMemberNbrPrefix { get; set; }
        public virtual string? MemberNbrPrefix { get; set; }
        public virtual string? PlanId { get; set; }
        public virtual string? SubgroupId { get; set; }
        public virtual string? PlanType { get; set; }
        public virtual string? MemberId { get; set; }
        public virtual string? MemberType { get; set; }
        public virtual bool IsSSOUser { get; set; }
        public virtual string? SubscriptionStatusJson { get; set; }

        public virtual string? ConsumerAttribute { get; set; }

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
