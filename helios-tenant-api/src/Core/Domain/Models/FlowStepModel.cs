using SunnyRewards.Helios.Common.Core.Domain.Models;

namespace SunnyRewards.Helios.Tenant.Core.Domain.Models
{
    public class FlowStepModel : BaseModel
    {
        public virtual long Pk { get; set; }
        public virtual long FlowFk { get; set; }
        public virtual int StepIdx { get; set; }
        public virtual long CurrentComponentCatalogueFk { get; set; }
        public virtual long? OnSuccessComponentCatalogueFk { get; set; }
        public virtual long? OnFailureComponentCatalogueFk { get; set; }
        public virtual string? StepConfig { get; set; }
    }
}
