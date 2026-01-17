using System.ComponentModel.DataAnnotations;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class GetSelfReportTaskReward
    {
        [Required]
        public string? TenantCode { get; set; }
        public bool selfReport { get; set; }
    }
}
