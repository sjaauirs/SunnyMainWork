using SunnyRewards.Helios.ETL.Common.Domain.Config;
using SunnyRewards.Helios.ETL.Common.Domain.Dtos;
using SunnyRewards.Helios.ETL.Common.Domain.Enum;
using SunnyRewards.Helios.ETL.Core.Domain.Constants.FIS;
using System.Text.Json.Serialization;

namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos.FIS
{
    public class FISCardHolderAdditionalCardCarrierDataDto : FISFlatFileRecordBaseDto
    {
        public static Dictionary<string, FieldConfiguration> FieldConfigurationMap = new Dictionary<string, FieldConfiguration>
        {
            { "RecordType", new FieldConfiguration(2, Justification.Right) },
            { "ActionType", new FieldConfiguration(2, Justification.Right, '0') },
            { "PANProxyClientUniqueID", new FieldConfiguration(19, Justification.Right, ' ') },
            { "LastName", new FieldConfiguration(50, Justification.Left, ' ') },
            { "SSN", new FieldConfiguration(9, Justification.Right, '0') },
            { "DiscretionaryData1", new FieldConfiguration(50, Justification.Left, ' ') },
            { "DiscretionaryData2", new FieldConfiguration(50, Justification.Left, ' ') },
            { "DiscretionaryData3", new FieldConfiguration(50, Justification.Left, ' ') },
            { "OrderId", new FieldConfiguration(18, Justification.Right, '0') },
            { "Filler", new FieldConfiguration(107, Justification.Left, ' ') },
            { "CardRecordStatusCode", new FieldConfiguration(2, Justification.Right, '0') },
            { "ProcessingMessage", new FieldConfiguration(41, Justification.Left, ' ') }
        };

        public FISCardHolderAdditionalCardCarrierDataDto() : base(FISBatchConstants.RECORD_TYPE_CARD_HOLDER_ADDITIONAL_CARRIER_DATA)
        {
        }
        public int ActionType { get; set; } = FISBatchConstants.CARD_ADDITIONAL_CARRIER_DATA_ACTION_TYPE;
        public string? PANProxyClientUniqueID { get; set; } = string.Empty;
        public string? LastName { get; set; }
        public string? SSN { get; set; }
        public string? DiscretionaryData1 { get; set; } = string.Empty;
        public string? DiscretionaryData2 { get; set; } = string.Empty;
        public string? DiscretionaryData3 { get; set; } = string.Empty;
        public string? OrderId { get; set; }
        public string Filler { get; set; } = string.Empty;
        public int CardRecordStatusCode { get; set; } = FISBatchConstants.CARD_CREATE_CARD_ADDITIONAL_CARRIER_DATA_RECORD_STATUS_CODE_SENT;
        public string? ProcessingMessage { get; set; } = string.Empty;
    }


    public class DicretnaryDataItem
    {
        [JsonPropertyName("consAttr")]
        public List<Dictionary<string, object>> ConsAttr { get; set; } = new();

        [JsonPropertyName("DiscretaryData1")]
        public string DiscretaryData1 { get; set; } = string.Empty;
    }

    public class DicretnaryDataRoot
    {
        [JsonPropertyName("dicretnaryDataMap")]
        public List<DicretnaryDataItem> DicretnaryDataMap { get; set; } = new();
    }

}