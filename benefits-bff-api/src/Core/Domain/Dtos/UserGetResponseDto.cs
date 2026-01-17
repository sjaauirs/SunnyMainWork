using Newtonsoft.Json;
using Sunny.Benefits.Bff.Core.Constants;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Dtos;

namespace Sunny.Benefits.Bff.Core.Domain.Dtos
{
    public class UserGetResponseDto : BaseResponseDto
    {
        public DateTime created_at { get; set; }

        public string email { get; set; } = string.Empty;

        public bool email_verified { get; set; }

        public Identity[] identities { get; set; }

        public string name { get; set; } = string.Empty;

        public string nickname { get; set; } = string.Empty;

        public Uri picture { get; set; }

        public DateTime updated_at { get; set; }

        public string user_id { get; set; } = string.Empty;

        public AppMetadata app_metadata { get; set; } = new AppMetadata();

        public string last_ip { get; set; } = string.Empty;
        public string language_code { get; set; } = CommonConstants.DefaultLanguageCode;

        public DateTime last_login { get; set; }

        public int logins_count { get; set; }
        public bool DeviceRegistered { get; set; }
        public string? EnrollmentStatus { get; set; }
        public TenantDto? TenantInfo { get; set; }
        public GetConsumerByEmailResponseDto? ConsumerInfo { get; set; }
    }
}
