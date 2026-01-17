namespace SunnyRewards.Helios.ETL.Common.Domain.Dtos
{
    public class BaseDto
    {
        public DateTime CreateTs { get; set; }
        public DateTime UpdateTs { get; set; }
        public string? CreateUser { get; set; } = string.Empty;
        public string? UpdateUser { get; set; } = string.Empty;
        public long DeleteNbr { get; set; }
    }
}
