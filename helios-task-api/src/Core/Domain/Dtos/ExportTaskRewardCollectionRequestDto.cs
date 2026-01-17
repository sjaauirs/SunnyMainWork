using System.ComponentModel.DataAnnotations;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class ExportTaskRewardCollectionRequestDto
    {
        [Required]
        public string TenantCode { get; set; } = string.Empty;
    }
}
