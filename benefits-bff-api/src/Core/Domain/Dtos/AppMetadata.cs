using Newtonsoft.Json;

namespace Sunny.Benefits.Bff.Core.Domain.Dtos
{
    public class AppMetadata
    {
        public string consumerCode { get; set; }
        public string env { get; set; }
        public string role { get; set; }
        public string tenantCode { get; set; }
        public SunnyRewadAppMetaData HttpsWwwSunnyrewardsComAppMetadata { get; set; }
    }

    public class SunnyRewadAppMetaData
    {
        public string consumerCode { get; set; }
        public string env { get; set; }
        public string role { get; set; }
        public string tenantCode { get; set; }
    }

    public class Identity
    {
        public string user_id { get; set; }
        public string provider { get; set; }
        public string connection { get; set; }
        public bool isSocial { get; set; }
    }

    public class UserUpdateResponseDto
    {
        public DateTime created_at { get; set; }
        public string email { get; set; }
        public bool email_verified { get; set; }
        public List<Identity> identities { get; set; }
        public string name { get; set; }
        public string nickname { get; set; }
        public string picture { get; set; }
        public DateTime updated_at { get; set; }
        public string user_id { get; set; }
        public UserMetadata user_metadata { get; set; }
        public AppMetadata app_metadata { get; set; }
        public string last_ip { get; set; }
        public DateTime last_login { get; set; }
        public int logins_count { get; set; }
    }

    public class UserMetadata
    {
        public string member_nbr { get; set; }
    }


}
