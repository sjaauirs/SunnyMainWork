using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos
{
    public class ETLMemberAttributeDetailDto
    {
        [JsonProperty(Required = Required.Always)]
        public string MemberNbr { get; set; } = string.Empty;

        [JsonProperty(Required = Required.Always)]
        public string GroupName { get; set; } = string.Empty;

        [JsonProperty(Required = Required.Always)]
        public string AttributeName { get; set; } = string.Empty;

        [JsonProperty(Required = Required.Always)]
        public string AttributeValue { get; set; } = string.Empty;
    }
}
