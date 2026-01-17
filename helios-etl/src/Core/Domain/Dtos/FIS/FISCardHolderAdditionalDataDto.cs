using SunnyRewards.Helios.ETL.Common.Domain.Config;
using SunnyRewards.Helios.ETL.Common.Domain.Dtos;
using SunnyRewards.Helios.ETL.Common.Domain.Enum;
using SunnyRewards.Helios.ETL.Core.Domain.Constants.FIS;

namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos.FIS
{
    public class FISCardHolderAdditionalDataDto : FISFlatFileRecordBaseDto
    {
        public static Dictionary<string, FieldConfiguration> FieldConfigurationMap = new Dictionary<string, FieldConfiguration>
        {
            { "RecordType", new FieldConfiguration(2, Justification.Right) },
            { "ActionType", new FieldConfiguration(2, Justification.Right, '0') },
            { "LastName", new FieldConfiguration(50, Justification.Left, ' ') },
            { "SSN", new FieldConfiguration(9, Justification.Right, '0') },
            { "DOB", new FieldConfiguration(8, Justification.Right, '0') },
            { "MothersMaidenName", new FieldConfiguration(21, Justification.Left, ' ') },
            { "Filler", new FieldConfiguration(2, Justification.Left, ' ') },
            { "EmailAddress", new FieldConfiguration(80, Justification.Left, ' ') },
            { "OtherInformation", new FieldConfiguration(60, Justification.Left, ' ') },
            { "Filler1", new FieldConfiguration(15, Justification.Left, ' ') },
            { "ForthLine", new FieldConfiguration(26, Justification.Left ,' ') },
            { "NameOnCard", new FieldConfiguration(26, Justification.Left , ' ') },
            { "PANProxyClientUniqueID", new FieldConfiguration(19, Justification.Right, ' ') },
            { "ClientUniquePersonIdentifier", new FieldConfiguration(30, Justification.Left, ' ') },
            { "DeliveryMethod", new FieldConfiguration(2, Justification.Right, '0') },
            { "Filler2", new FieldConfiguration(5, Justification.Left, ' ') },
            { "CardRecordStatusCode", new FieldConfiguration(2, Justification.Right, '0') },
            { "ProcessingMessage", new FieldConfiguration(41, Justification.Left, ' ') }
        };

        public FISCardHolderAdditionalDataDto() : base(FISBatchConstants.RECORD_TYPE_CARD_HOLDER_ADDITIONAL_DATA) { }

        //public int RecordType { get; set; } = FISBatchConstants.RECORD_TYPE_CARD_HOLDER_DATA;
        public int ActionType { get; set; } = FISBatchConstants.CARD_ADDITIONAL_ACTION_TYPE;
        public string? LastName { get; set; }
        public string? SSN { get; set; }
        public string? DOB { get; set; }
        public string? MothersMaidenName { get; set; } = string.Empty;
        public string Filler { get; set; } = string.Empty;
        public string? EmailAddress { get; set; }
        public string? OtherInformation { get; set; } =  string.Empty;
        public string Filler1 { get; set; } = string.Empty;
        public string? ForthLine { get; set; } = string.Empty;
        public string? NameOnCard { get; set; } = string.Empty; // Get data from card 30
        public string? PANProxyClientUniqueID { get; set; } = string.Empty;
        public string? ClientUniquePersonIdentifier  { get; set; } = string.Empty;
        public int DeliveryMethod { get; set; } = FISBatchConstants.CARD_CREATE_DELIVERY_FIRST_CLASS;
        public string Filler2 { get; set; } = string.Empty;
        public int CardRecordStatusCode { get; set; } = FISBatchConstants.CARD_CREATE_CARD_ADDITIONAL_RECORD_STATUS_CODE_SENT;
        public string? ProcessingMessage { get; set; } = string.Empty;
    }
}
