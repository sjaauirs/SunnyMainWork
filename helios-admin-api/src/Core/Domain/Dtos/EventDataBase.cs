using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace SunnyRewards.Helios.Admin.Core.Domain.Dtos
{
    public abstract class EventDataBase
    {
    }
    public class HealthTaskProgressEventDataDto : EventDataBase
    {
        public string HealthEvent { get; set; } = null!;
    }

    public class PickedPurseEventDataDto : EventDataBase
    {
        public List<string> pickedPurseLabels { get; set; } = null!;
    }
}
