using CsvHelper.Configuration.Attributes;

namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos
{
    public class EtlTaskUpdateCustomFormatRecordDto
    {
        [Name("completion_date")]
        public DateTime? Completed { get; set; }
        [Name("third_party_task_external_code")]
        public string? TaskThirdPartyCode { get; set; }
        [Name("person_unique_identifier")]
        public string? PersonUniqueIdentifier { get; set; }
        [Name("completion_status")]
        public string? CompletionStatus { get; set; }

        [Name("partner_code")]
        public string? PartnerCode { get; set; }
    }
}
