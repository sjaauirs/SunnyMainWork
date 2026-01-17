using SunnyRewards.Helios.ETL.Common.Domain.Config;
using SunnyRewards.Helios.ETL.Common.Domain.Dtos;
using SunnyRewards.Helios.ETL.Common.Domain.Enum;
using SunnyRewards.Helios.ETL.Core.Domain.Constants.FIS;

namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos.FIS
{
    public class FISCardHolderDataDto : FISFlatFileRecordBaseDto
    {
        public static Dictionary<string, FieldConfiguration> FieldConfigurationMap = new Dictionary<string, FieldConfiguration>
        {
            { "RecordType", new FieldConfiguration(2, Justification.Left) },
            { "ActionType", new FieldConfiguration(2, Justification.Right, '0') },
            { "FirstName", new FieldConfiguration(50, Justification.Left, ' ') },
            { "MiddleInitial", new FieldConfiguration(1, Justification.Left, ' ') },
            { "LastName", new FieldConfiguration(50, Justification.Left, ' ') },
            { "Suffix", new FieldConfiguration(3, Justification.Left, ' ') },
            { "SSN", new FieldConfiguration(9, Justification.Right, '0') },
            { "MailingAddr1", new FieldConfiguration(50, Justification.Left, ' ') },
            { "MailingAddr2", new FieldConfiguration(50, Justification.Left, ' ') },
            { "MailingCity", new FieldConfiguration(35, Justification.Left, ' ') },
            { "MailingState", new FieldConfiguration(25, Justification.Left, ' ') },
            { "MailingPostalCode", new FieldConfiguration(30, Justification.Left, ' ') },
            { "MailingCountryCode", new FieldConfiguration(3, Justification.Left, ' ') },
            { "HomeNumber", new FieldConfiguration(23, Justification.Right, '0') },
            { "DeliveryMethod", new FieldConfiguration(2, Justification.Right, '0') },
            { "PANProxyClientUniqueID", new FieldConfiguration(19, Justification.Right, ' ') },
            { "Filler", new FieldConfiguration(3, Justification.Left, ' ') },
            { "CardRecordStatusCode", new FieldConfiguration(2, Justification.Right, '0') },
            { "ProcessingMessage", new FieldConfiguration(41, Justification.Left, ' ') }
        };

        public FISCardHolderDataDto() : base(FISBatchConstants.RECORD_TYPE_CARD_HOLDER_DATA) { }

        //public int RecordType { get; set; } = FISBatchConstants.RECORD_TYPE_CARD_HOLDER_DATA;
        public int ActionType { get; set; } = FISBatchConstants.CARD_CREATE_NEW_CARD;
        public string? FirstName { get; set; }
        public string? MiddleInitial { get; set; } = string.Empty;
        public string? LastName { get; set; }
        public string? Suffix { get; set; } = string.Empty;
        public string? SSN { get; set; }
        public string? MailingAddr1 { get; set; }
        public string? MailingAddr2 { get; set; }
        public string? MailingCity { get; set; }
        public string? MailingState { get; set; }
        public string? MailingPostalCode { get; set; }
        public string? MailingCountryCode { get; set; }
        public string? HomeNumber { get; set; }
        public int DeliveryMethod { get; set; } = FISBatchConstants.CARD_CREATE_DELIVERY_FIRST_CLASS;
        public string? PANProxyClientUniqueID { get; set; } = string.Empty;
        public string Filler { get; set; } = string.Empty;
        public int CardRecordStatusCode { get; set; } = FISBatchConstants.CARD_CREATE_CARD_RECORD_STATUS_CODE_SENT;
        public string? ProcessingMessage { get; set; } = string.Empty;
    }
}
