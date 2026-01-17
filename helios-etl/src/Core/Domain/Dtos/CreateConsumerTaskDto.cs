extern alias SunnyRewards_Task;
namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos
{
    public class CreateConsumerTaskDto 
    {
        public string? TenantCode { get; set; }
        public long TaskId { get; set; }
        public string? TaskStatus { get; set; }
        public string? ConsumerCode { get; set; }
        public bool AutoEnrolled { get; set; }
    }
}
