using CsvHelper.Configuration.Attributes;

namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos
{
    public class WalletBalancesReportDto
    {
        [Name("Time Stamp")]
        public DateTime TimeStamp { get; set; }
        [Name("Consumer Code")]
        public string ConsumerCode { get; set; } = string.Empty;
        [Name("Wallet Code")]
        public string WalletCode { get; set; } = string.Empty;
        [Name("First Name")]
        public string FirstName { get; set; } = string.Empty;
        [Name("Last Name")]
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public virtual double Balance { get; set; }
    }
}
