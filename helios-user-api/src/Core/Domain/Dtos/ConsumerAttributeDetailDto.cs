using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.User.Core.Domain.Dtos
{
    public class ConsumerAttributeDetailDto
    {
        [JsonProperty(Required = Required.Always)]
        public string ConsumerCode { get; set; } = string.Empty;
        public string GroupName { get; set; } = string.Empty;

        [JsonProperty(Required = Required.Always)]
        public string AttributeName { get; set; } = string.Empty;

        [JsonProperty(Required = Required.Always)]
        public string AttributeValue { get; set; } = string.Empty;
    }
}
