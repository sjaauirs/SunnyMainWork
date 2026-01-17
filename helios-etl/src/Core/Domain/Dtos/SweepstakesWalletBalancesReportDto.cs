using CsvHelper.Configuration.Attributes;

namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos
{
    public class SweepstakesWalletBalancesReportDto
    {
        public virtual string? userId { get; set; }
        public virtual string? firstName { get; set; }
        public virtual string? lastName { get; set; }
        public virtual string? phoneNumber { get; set; }
        public virtual string? secondaryPhoneNumber { get; set; }
        public virtual string? languageCode { get; set; }
        public virtual bool cardOrdered { get; set; }=false;
        public virtual double entries { get; set; }
    }
}
