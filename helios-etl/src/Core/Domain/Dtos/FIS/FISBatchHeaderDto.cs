using SunnyRewards.Helios.ETL.Common.Domain.Config;
using SunnyRewards.Helios.ETL.Common.Domain.Dtos;
using SunnyRewards.Helios.ETL.Common.Domain.Enum;
using SunnyRewards.Helios.ETL.Core.Domain.Constants.FIS;

namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos.FIS
{
    public class FISBatchHeaderDto : FISFlatFileRecordBaseDto
    {
        public static Dictionary<string, FieldConfiguration> FieldConfigurationMap = new Dictionary<string, FieldConfiguration>
        {
            { "RecordType", new FieldConfiguration(2, Justification.Left) },
            { "BatchSequence", new FieldConfiguration(6, Justification.Right, '0') },
            { "ClientID", new FieldConfiguration(6, Justification.Left, '0') },
            { "SubprogramID", new FieldConfiguration(6, Justification.Left, '0') },
            { "PackageID", new FieldConfiguration(6, Justification.Left, '0') },
            { "CompanyCode", new FieldConfiguration(16, Justification.Left, ' ') },
            { "SortCode", new FieldConfiguration(26, Justification.Left, ' ') },
            { "ShippingAddressee", new FieldConfiguration(30, Justification.Left, ' ') },
            { "ShippingAddress1", new FieldConfiguration(30, Justification.Left, ' ') },
            { "ShippingAddress2", new FieldConfiguration(30, Justification.Left, ' ') },
            { "ShippingCity", new FieldConfiguration(18, Justification.Left, ' ') },
            { "Filler", new FieldConfiguration(1, Justification.Left, ' ') },
            { "ShippingState", new FieldConfiguration(2, Justification.Left, ' ') },
            { "ShippingZip", new FieldConfiguration(9, Justification.Left, ' ') },
            { "ShippingCountryCode", new FieldConfiguration(3, Justification.Left, ' ') },
            { "ShippingAttention", new FieldConfiguration(26, Justification.Left, ' ') },
            { "Useronly", new FieldConfiguration(1, Justification.Left, ' ') },
            { "SpecialDuplicateProcessing", new FieldConfiguration(1, Justification.Left, ' ') },
            { "ProxyIndicatorProcessing", new FieldConfiguration(1, Justification.Left, ' ') },
            { "GroupDemograpicUpdateIndicator", new FieldConfiguration(1, Justification.Left, ' ') },
            { "SpecialProcessingIndicators", new FieldConfiguration(8, Justification.Left, ' ') },
            { "BulkShippingPhoneNumber", new FieldConfiguration(23, Justification.Left, ' ') },
            { "PhoneFormatOverride", new FieldConfiguration(1, Justification.Left, ' ') },
            { "GenerateOrderID", new FieldConfiguration(1, Justification.Left, ' ') },
            { "GenerateClientUniqueID", new FieldConfiguration(1, Justification.Left, ' ') },
            { "ClientIDExtended", new FieldConfiguration(9, Justification.Right, '0') },
            { "SubprogramIDExtended", new FieldConfiguration(9, Justification.Right, '0') },
            { "PackageIDExtended", new FieldConfiguration(9, Justification.Right, '0') },
            { "ShippingAddress1extended", new FieldConfiguration(50, Justification.Left, ' ') },
            { "ShippingAddress2extended", new FieldConfiguration(50, Justification.Left, ' ') },
            { "PhysicalExpirationinMonths", new FieldConfiguration(3, Justification.Left, ' ') },
            { "Filler2", new FieldConfiguration(15, Justification.Left, ' ') }
        };

        public FISBatchHeaderDto() : base(FISBatchConstants.RECORD_TYPE_BATCH_HEADER) { }
        //public int RecordType { get; } = FISBatchConstants.RECORD_TYPE_BATCH_HEADER;
        public long BatchSequence { get; set; }
        public long ClientID { get; } = 0;
        public long SubprogramID { get; } = 0;
        public long PackageID { get; } = 0;
        public string? CompanyCode { get; } = string.Empty;
        public string? SortCode { get; } = string.Empty;
        public string? ShippingAddressee { get; } = string.Empty;
        public string? ShippingAddress1 { get; } = string.Empty;
        public string? ShippingAddress2 { get; } = string.Empty;
        public string? ShippingCity { get; } = string.Empty;
        public string Filler { get; } = string.Empty;
        public string? ShippingState { get; } = string.Empty;
        public string? ShippingZip { get; set; } = string.Empty;
        public string? ShippingCountryCode { get; } = string.Empty;
        public string? ShippingAttention { get; } = string.Empty;
        public string? Useronly { get; } = string.Empty ;
        public string? SpecialDuplicateProcessing { get; } = string.Empty   ;
        public string? ProxyIndicatorProcessing { get; set; } = string.Empty;
        public string? GroupDemograpicUpdateIndicator { get; } = string.Empty;
        public string? SpecialProcessingIndicators { get; } = string.Empty;
        public string? BulkShippingPhoneNumber { get; set; } = string.Empty;
        public string? PhoneFormatOverride { get; set; } = string.Empty;
        public string? GenerateOrderID { get; } = string.Empty;
        public string? GenerateClientUniqueID { get; set; } = string.Empty;
        public long ClientIDExtended { get; set; }
        public long SubprogramIDExtended { get; set; }
        public long PackageIDExtended { get; set; }
        public string? ShippingAddress1extended { get; } = string.Empty;
        public string? ShippingAddress2extended { get; } = string.Empty;
        public string? PhysicalExpirationinMonths { get; } = string.Empty;
        public string Filler2 { get; } = string.Empty;
    }
}
