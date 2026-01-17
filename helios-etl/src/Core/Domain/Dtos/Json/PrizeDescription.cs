using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos.Json
{
    public class PrizeDescription
    {
        [JsonProperty("prizeDetails")]
        public List<PrizeDetail>? PrizeDetails { get; set; }
    }

    public class PrizeDetail
    {
        [JsonProperty("winnerPosition")]
        public int WinnerPosition { get; set; }
        [JsonProperty("maxWinners")]
        public int MaxWinners { get; set; }
        [JsonProperty("prizeType")]
        public string? PrizeType { get; set; }
        [JsonProperty("amount")]
        public int Amount { get; set; }
        [JsonProperty("description")]
        public string? Description { get; set; }
        [JsonProperty("winnerCohort")]
        public string? WinnerCohort { get; set; }
    }
}
