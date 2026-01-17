namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos
{
    public class CohortConsumerImportCSVDto
    {
        public string CohortCode { get; set; } = string.Empty;
        public string ConsumerCode { get; set; } = string.Empty;
        public string? DetectDescription { get; set; }
    }
}
