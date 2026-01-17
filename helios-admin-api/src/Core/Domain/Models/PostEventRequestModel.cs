using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using System.ComponentModel.DataAnnotations;

namespace SunnyRewards.Helios.Admin.Core.Domain.Models
{
    public class PostEventRequestModel
    {
        public string EventCode { get; set; } = "evt-" + Guid.NewGuid().ToString("N");
        [Required]
        public string EventType { get; set; } = null!;
        [Required]
        public string EventSubtype { get; set; } = null!;
        [Required]
        public string EventSource { get; set; } = null!;

        [Required]
        public string TenantCode { get; set; } = null!;

        [Required]
        public string ConsumerCode { get; set; } = null!;
        public DateTime SourceTs { get; set; } = DateTime.UtcNow;
        public string EventData { get; set; } = null!;

    }



    public class PostedEventData
    {
        public string EventType { get; set; } = null!;
        public string EventSubtype { get; set; } = null!;
    }


    public class ConsumerErrorEventBodyDto
    {
        public string Detail { get; set; }
        public string ReqDetail { get; set; }
    }

    public class ConsumerErrorEventDto
    {
        public string EventCode { get; set; } = "err-" + Guid.NewGuid().ToString("N");
        public PostedEventData Header { get; set; } 
        public ConsumerErrorEventBodyDto Message { get; set; }
    }
}
