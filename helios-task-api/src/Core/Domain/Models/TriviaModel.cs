using SunnyRewards.Helios.Common.Core.Domain.Models;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Task.Core.Domain.Models
{
    public class TriviaModel : BaseModel
    {
        public virtual long TriviaId { get; set; }
        public virtual string? TriviaCode { get; set; }
        public virtual long TaskRewardId { get; set; }
        public virtual string? CtaTaskExternalCode { get; set; }
        public virtual string? ConfigJson { get; set; }
      
    }
}
