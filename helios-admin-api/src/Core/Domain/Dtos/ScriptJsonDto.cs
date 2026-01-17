using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Admin.Core.Domain.Dtos
{

    public class ScriptJsonDto
    {
        [JsonProperty("args")]

        public List<Argument> Args { get; set; }
        [JsonProperty("result")]

        public Result Result { get; set; }
    }

    public class Argument
    {
        [JsonProperty("argName")]
        public string ArgName { get; set; }
        [JsonProperty("argType")]

        public string ArgType { get; set; }
    }

    public class Result
    {
        [JsonProperty("ResultMap")]

        public string ResultMap { get; set; }

        [JsonProperty("ResultCode")]

        public string ResultCode { get; set; }
        [JsonProperty("ErrorMessage")]

        public string ErrorMessage { get; set; }

        

    }
}
