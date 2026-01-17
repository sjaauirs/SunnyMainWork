using System.ComponentModel.DataAnnotations;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class GetTasksAndTaskRewardsRequestDto
    {
        [Required]
        public string? TenantCode { get; set; }
    }
}
