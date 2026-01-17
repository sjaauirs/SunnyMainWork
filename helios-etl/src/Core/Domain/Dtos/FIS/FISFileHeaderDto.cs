using SunnyRewards.Helios.ETL.Common.Domain.Config;
using SunnyRewards.Helios.ETL.Common.Domain.Dtos;
using SunnyRewards.Helios.ETL.Common.Domain.Enum;
using SunnyRewards.Helios.ETL.Core.Domain.Constants.FIS;

namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos.FIS
{
    public class FISFileHeaderDto : FISFlatFileRecordBaseDto
    {
        public static Dictionary<string, FieldConfiguration> FieldConfigurationMap = new Dictionary<string, FieldConfiguration>
        {
            { "RecordType", new FieldConfiguration(2, Justification.Left) },
            { "FileDate", new FieldConfiguration(8, Justification.Left, ' ') },
            { "FileTime", new FieldConfiguration(6, Justification.Left, ' ') },
            { "CompanyID", new FieldConfiguration(6, Justification.Left, ' ') },
            { "Version", new FieldConfiguration(2, Justification.Left, ' ') },
            { "LogFileIndicator", new FieldConfiguration(1, Justification.Left, ' ') },
            { "TestProdIndicator", new FieldConfiguration(1, Justification.Left, ' ') },
            { "Reserved", new FieldConfiguration(8, Justification.Left, ' ') },
            { "ProcessDate", new FieldConfiguration(8, Justification.Left, ' ') },
            { "ProcessTime", new FieldConfiguration(6, Justification.Left, ' ') },
            { "ClientLookup", new FieldConfiguration(1, Justification.Left, ' ') },
            { "Filler", new FieldConfiguration(1, Justification.Left, ' ') },
            { "ProxyLookup", new FieldConfiguration(1, Justification.Left, ' ') },
            { "CompanyIDExtended", new FieldConfiguration(9, Justification.Right, '0') },
            { "Filler2", new FieldConfiguration(340, Justification.Left, ' ') }
        };


        public FISFileHeaderDto() : base(FISBatchConstants.RECORD_TYPE_FILE_HEADER) { }
        //public int RecordType { get; } = FISBatchConstants.RECORD_TYPE_FILE_HEADER;
        public string? FileDate { get; set; }
        public string? FileTime { get; set; }
        public string CompanyID { get; } = string.Empty; // deprecated
        public string Version { get; } = FISBatchConstants.FILE_HEADER_FORMAT_VERSION; // fixed
        public string? LogFileIndicator { get; } = FISBatchConstants.FILE_HEADER_LOG_FILE_INDICATOR; // fixed
        public string? TestProdIndicator { get; } = FISBatchConstants.FILE_HEADER_PROD_INDICATOR; // fixed
        public string? Reserved { get; } = string.Empty; // fixed
        public string? ProcessDate { get; } = string.Empty; // fixed
        public string? ProcessTime { get; } = string.Empty; // fixed
        public string ClientLookup { get; } = string.Empty; // fixed
        public string Filler { get; } = string.Empty; // fixed
        public string ProxyLookup { get; } = string.Empty; // fixed
        public long CompanyIDExtended { get; set; }
        public string Filler2 { get; } = string.Empty; // fixed
    }
}
