using SunnyRewards.Helios.ETL.Common.Domain.Config;
using SunnyRewards.Helios.ETL.Common.Domain.Dtos;
using SunnyRewards.Helios.ETL.Common.Domain.Enum;
using SunnyRewards.Helios.ETL.Core.Domain.Constants.FIS;

namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos.FIS
{
    public class FISFileTrailerDto : FISFlatFileRecordBaseDto
    {
        public static Dictionary<string, FieldConfiguration> FieldConfigurationMap = new Dictionary<string, FieldConfiguration>
        {
            { "RecordType", new FieldConfiguration(2, Justification.Left) },
            { "TotalRecords", new FieldConfiguration(10, Justification.Right, '0') },
            { "BatchCount", new FieldConfiguration(6, Justification.Right, '0') },
            { "DetailCount", new FieldConfiguration(10, Justification.Right, '0') },
            { "TotalCredit", new FieldConfiguration(12, Justification.Right, '0') },
            { "TotalDebit", new FieldConfiguration(12, Justification.Right, '0') },
            { "TotalProcessed", new FieldConfiguration(10, Justification.Left, ' ') },
            { "TotalRejected", new FieldConfiguration(10, Justification.Left, ' ') },
            { "ValueProcessed", new FieldConfiguration(12, Justification.Left, ' ') },
            { "ValueRejected", new FieldConfiguration(12, Justification.Left, ' ') },
            { "TotalCashout", new FieldConfiguration(12, Justification.Left, ' ') },
            { "TotalEscheated", new FieldConfiguration(12, Justification.Left, ' ') },
            { "Filler", new FieldConfiguration(280, Justification.Left, ' ') }
        };

        public FISFileTrailerDto() : base(FISBatchConstants.RECORD_TYPE_FILE_TRAILER) { }
        //public int RecordType { get; } = FISBatchConstants.RECORD_TYPE_FILE_TRAILER;
        public long TotalRecords { get; set; }
        public long BatchCount { get; set; }
        public long DetailCount { get; set; }
        public string? TotalCredit { get; set; }
        public string? TotalDebit { get; set; }
        public string? TotalProcessed { get; set; }
        public string? TotalRejected { get; set; }
        public string? ValueProcessed { get; set; }
        public string? ValueRejected { get; set; }
        public string? TotalCashout { get; set; }
        public string? TotalEscheated { get; set; }
        public string Filler { get; } = string.Empty;
    }
}
