using System.ComponentModel.DataAnnotations;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class GetConsumerTaskByTaskId
    {
        [Required]
        public string TenantCode { get; set; } = null!;
        [Required]
        public int TaskId { get; set; } 
        [Required]
        public DateTime StartDate { get; set; } 

        public DateTime EndDate { get; set; }

        public int Skip { get; set; }
        public int  PageSize { get; set; }
    }
}
