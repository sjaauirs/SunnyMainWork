namespace SunnyRewards.Helios.ETL.Common.Domain.Dtos
{
    public class BaseResponseDto
    {
        public string? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }
        public string? ErrorDescription { get; set; }
        public string? ErrorDescriptionType { get; set; }
    }
}
