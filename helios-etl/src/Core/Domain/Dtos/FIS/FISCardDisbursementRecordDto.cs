using SunnyRewards.Helios.ETL.Common.Domain.Config;
using SunnyRewards.Helios.ETL.Common.Domain.Dtos;
using SunnyRewards.Helios.ETL.Common.Domain.Enum;
using SunnyRewards.Helios.ETL.Core.Domain.Constants.FIS;

namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos.FIS
{
    public class FISCardDisbursementRecordDto : FISFlatFileRecordBaseDto
    {
        public static Dictionary<string, FieldConfiguration> FieldConfigurationMap = new Dictionary<string, FieldConfiguration>
        {
            { "RecordType", new FieldConfiguration(2, Justification.Left) },
            { "ActionType", new FieldConfiguration(2, Justification.Right, '0') },
            { "Purse", new FieldConfiguration(6, Justification.Right, '0') },
            { "ClientReferenceNumber", new FieldConfiguration(25, Justification.Left, ' ') },
            { "LastName", new FieldConfiguration(50, Justification.Left, ' ') },
            { "SSN", new FieldConfiguration(9, Justification.Right, '0') },
            { "ApplyDate", new FieldConfiguration(8, Justification.Right, '0') },
            { "PaymentAmount", new FieldConfiguration(10, Justification.Right, '0') },
            { "PromoCode", new FieldConfiguration(25, Justification.Left, ' ') },
            { "PANProxyClientUniqueID", new FieldConfiguration(19, Justification.Right, ' ') },
            { "AdditionalInfo1", new FieldConfiguration(15, Justification.Left, ' ') },
            { "AdditionalInfo2", new FieldConfiguration(15, Justification.Left, ' ') },
            { "Comment", new FieldConfiguration(40, Justification.Left, ' ') },
            { "SetCardAssigned", new FieldConfiguration(1, Justification.Left, ' ') },
            { "LoadUponActivation", new FieldConfiguration(1, Justification.Left, ' ') },
            { "TransactionLogID", new FieldConfiguration(10, Justification.Left, ' ') },
            { "Filler", new FieldConfiguration(119, Justification.Left, ' ') },
            { "CardRecordStatusCode", new FieldConfiguration(2, Justification.Right, '0') },
            { "ProcessingMessage", new FieldConfiguration(41, Justification.Left, ' ') }
        };
        public FISCardDisbursementRecordDto() : base(FISBatchConstants.RECORD_TYPE_CARD_LOAD_DATA) { }
        //public int RecordType { get; set; } = FISBatchConstants.RECORD_TYPE_CARD_LOAD_DATA;
        public int ActionType { get; set; } = FISBatchConstants.CARD_LOAD_ACTION_TYPE;
        public int Purse { get; set; }
        public string? ClientReferenceNumber { get; set; } 
        public string? LastName { get; set; } = string.Empty;
        public string? SSN { get; set; } = string.Empty;
        public string? ApplyDate { get; set; } = FISBatchConstants.APPLY_DATE;
        public string? PaymentAmount { get; set; }
        public string? PromoCode { get; set; } = string.Empty;
        public string? PANProxyClientUniqueID { get; set; }
        public string? AdditionalInfo1 { get; set; } = string.Empty;
        public string? AdditionalInfo2 { get; set; } = string.Empty;
        public string? Comment { get; set; } = string.Empty;
        public string? SetCardAssigned { get; set; } = string.Empty;
        public string? LoadUponActivation { get; set; } = string.Empty;
        public string? TransactionLogID { get; set; } = string.Empty;
        public string? Filler { get; set; } = string.Empty;
        public int CardRecordStatusCode { get; set; } = FISBatchConstants.CARD_CREATE_CARD_RECORD_STATUS_CODE_SENT;
        public string? ProcessingMessage { get; set; } = string.Empty;
    }
}
