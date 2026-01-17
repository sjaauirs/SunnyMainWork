using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Sunny.Benefits.Bff.Core.Domain.Dtos
{
    public class AppMetadataDto
    {
        [JsonPropertyName("app_metadata")]
        public AppMetadataDetails app_metadata { get; set; }
    }

    public class AppMetadataDetails
    {
        [JsonPropertyName("https://www.sunnyrewards.com/app_metadata")]
        public SunnyRewadAppMetaData SunnyRewadAppMetaData { get; set; }
    }

    //public class SunnyRewadAppMetaData
    //{
    //    [JsonPropertyName("consumer_code")]
    //    public string consumerCode { get; set; }

    //    [JsonPropertyName("env")]
    //    public string env { get; set; }

    //    [JsonPropertyName("role")]
    //    public string role { get; set; }

    //    [JsonPropertyName("tenant_code")]
    //    public string tenantCode { get; set; }
    //}


}
