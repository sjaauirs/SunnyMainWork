using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using SunnyRewards.Helios.Common.Core.Mappings;
using SunnyRewards.Helios.Common.Core.Domain.Models;
using System.Diagnostics.CodeAnalysis;

namespace SunnyRewards.Helios.User.Core.Domain.Models
{
   public class ConsumerHistoryModel : BaseConsumerModel
    {
        public virtual long ConsumerHistoryId { get; set; }

    }
}