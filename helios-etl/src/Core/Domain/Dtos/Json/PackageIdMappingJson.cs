using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos.Json
{
    public class PackageIdMappingJson
    {
        [JsonProperty("ruleId")]
        public int RuleId { get; set; }
        [JsonProperty("cohorts")]
        public List<string>? Cohorts { get; set; }
        [JsonProperty("matchType")]
        public string? MatchType { get; set; }
        [JsonProperty("packageId")]
        public string? PackageId { get; set; }
    }
}
