using CsvHelper.Configuration.Attributes;

namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos
{
    public class HsaResponseDto
    {
        public string? Name { get; set; }
        [Name("Consumer Code")]
        public string? ConsumerCode { get; set; }
        public double Amount { get; set; }
        public string? Comments { get; set; }
    }
}
