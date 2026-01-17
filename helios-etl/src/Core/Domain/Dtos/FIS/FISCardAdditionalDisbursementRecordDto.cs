using SunnyRewards.Helios.ETL.Common.Domain.Config;
using SunnyRewards.Helios.ETL.Common.Domain.Dtos;
using SunnyRewards.Helios.ETL.Common.Domain.Enum;
using SunnyRewards.Helios.ETL.Core.Domain.Constants.FIS;

namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos.FIS
{
    public class FISCardAdditionalDisbursementRecordDto: FISFlatFileRecordBaseDto
    {
        public static Dictionary<string, FieldConfiguration> FieldConfigurationMap = new Dictionary<string, FieldConfiguration>
        {
            { "RecordType", new FieldConfiguration(2, Justification.Right,'0') },
            { "ActionType", new FieldConfiguration(2, Justification.Right, '0') },
            { "LastName", new FieldConfiguration(50, Justification.Left, ' ') },
            { "SSN", new FieldConfiguration(9, Justification.Right, '0') },
            { "PANProxyClientUniqueID", new FieldConfiguration(19, Justification.Right, ' ') },
            { "Purse", new FieldConfiguration(6, Justification.Right, '0') },
            { "ClientReferenceNumber", new FieldConfiguration(25, Justification.Left, ' ') },
            { "PaymentAmount", new FieldConfiguration(10, Justification.Right, '0') },
            { "Comment", new FieldConfiguration(150, Justification.Left, ' ') },
            { "CustomTransactionDescription", new FieldConfiguration(40, Justification.Left, ' ') },
            { "LoadUponActivation", new FieldConfiguration(1, Justification.Left, ' ') },
            { "Filler", new FieldConfiguration(43, Justification.Left, ' ') },
            { "CardRecordStatusCode", new FieldConfiguration(2, Justification.Right, '0') },
            { "ProcessingMessage", new FieldConfiguration(41, Justification.Left, ' ') },
        };
        public FISCardAdditionalDisbursementRecordDto() : base(FISBatchConstants.RECORD_TYPE_CARD_LOAD_ADDITIONAL_DATA) { }
        //public int RecordType { get; set; } = FISBatchConstants.RECORD_TYPE_CARD_LOAD_DATA;
        public int ActionType { get; set; } = FISBatchConstants.CARD_LOAD_ACTION_TYPE;
        public string? LastName { get; set; } = string.Empty;
        public string? SSN { get; set; } = string.Empty;
        public string? PANProxyClientUniqueID { get; set; }
        public int Purse { get; set; }
        public string? ClientReferenceNumber { get; set; }
        public string? PaymentAmount { get; set; }
        public string? Comment { get; set; } = string.Empty;
        public string? CustomTransactionDescription { get; set; } = string.Empty;
        public string? LoadUponActivation { get; set; } = string.Empty;
        public string? Filler { get; set; } = string.Empty;
        public int CardRecordStatusCode { get; set; } = FISBatchConstants.CARD_CREATE_CARD_RECORD_STATUS_CODE_SENT;
        public string? ProcessingMessage { get; set; } = string.Empty;
    }
}
