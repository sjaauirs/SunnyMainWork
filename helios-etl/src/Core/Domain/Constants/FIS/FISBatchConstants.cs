namespace SunnyRewards.Helios.ETL.Core.Domain.Constants.FIS
{
    public class FISBatchConstants
    {
        public const int RECORD_TYPE_FILE_HEADER = 10;
        public const int RECORD_TYPE_BATCH_HEADER = 20;
        public const int RECORD_TYPE_CARD_HOLDER_DATA = 30;
        public const int RECORD_TYPE_CARD_HOLDER_ADDITIONAL_DATA = 31;
        public const int RECORD_TYPE_CARD_HOLDER_ADDITIONAL_CARRIER_DATA = 33;
        public const int RECORD_TYPE_CARD_LOAD_DATA = 60;
        public const int RECORD_TYPE_BATCH_TRAILER = 80;
        public const int RECORD_TYPE_FILE_TRAILER = 90;
        public const int RECORD_TYPE_CARD_LOAD_ADDITIONAL_DATA = 61;

        // file header constants
        public const string FILE_HEADER_FORMAT_VERSION = "01";
        public const string FILE_HEADER_LOG_FILE_INDICATOR = "0";
        public const string FILE_HEADER_PROD_INDICATOR = "P";
        public const string FILE_HEADER_PROXY_INDICATOR = "P";
        public const string FILE_HEADER_CLIENT_UNIQUE_ID_INDICATOR = "U";
        public const string GENERATE_CLIENT_UNIQUE_ID_INDICATOR = "1";

        // card create constants
        public const int CARD_CREATE_NEW_CARD = 1;
        public const int CARD_ADDITIONAL_ACTION_TYPE = 1;
        public const int CARD_CREATE_DELIVERY_FIRST_CLASS = 1;
        public const int CARD_ADDITIONAL_CARRIER_DATA_ACTION_TYPE = 1;
        public const int CARD_CREATE_DELIVERY_NEXT_DAY_AIR = 2;
        public const int CARD_CREATE_CARD_RECORD_STATUS_CODE_SENT = 1;
        public const int CARD_CREATE_CARD_ADDITIONAL_RECORD_STATUS_CODE_SENT = 1;
        public const int UPDATE_FIS_INFO_ACTION_TYPE = 2;
        public const int CARD_CREATE_CARD_ADDITIONAL_CARRIER_DATA_RECORD_STATUS_CODE_SENT = 1;
        public const int UPDATE_FIS_INFO_ADDITIONAL_ACTION_TYPE = 2;

        // card load constants
        public const int CARD_LOAD_ACTION_TYPE = 1;
        public const string APPLY_DATE = "00000000";

        public const string INVALID_RECORD = "INVALID_RECORD";
        public const string MONETORY_RECORD_TYPE = "D";
        public const char MONETORY_RECORD_DELIMITER = '|';
        public const char CONSUMER_NON_MONETORY_RECORD_DELIMITER = '|';
        public const int CARD_RECORD_MIN_ERROR_STATUS_CODE = 10;
        public const int CARD_RECORD_MAX_ERROR_STATUS_CODE = 99;
        public const string ARCHIVE_FOLDER_NAME = "archive";
        public const string DECRYPTED_FOLDER_NAME = "decrypted";
        public const string ENCRYPTED_FILE_EXTENSION = ".pgp";
        public const string FIS_INBOUND_ARCHIVE_FOLDER = "fis/batch/inbound";
        public const string FIS_OUTBOUND_ARCHIVE_FOLDER = "fis/batch/outbound";
        public const string FIS_APL_FILE_OUTBOUND_ARCHIVE_FOLDER = "fis/costco/outbound";
        public const string MerchantNameForFundTransfer = "Value Load";

        public static Dictionary<string, int> DELIVERY_METHODS = new Dictionary<string, int>
        {
            {"FIRST_CLASS", 1},
            {"NEXT_DAY_AIR", 2},
            {"UPS_INTERNATIONAL", 10},
            {"UPS_GROUND", 11},
            {"UPS_SECOND_DAY", 12},
            {"UPS_NEXT_DAY", 13},
            {"UPS_FIRST_CLASS", 20},
            {"UPS_FIRST_CLASS_W_TRK", 21},
            {"FEDEX_INTERNATIONAL", 30},
            {"FEDEX_GROUND", 31},
            {"FEDEX_SECOND_DAY", 32},
            {"FEDEX_NEXT_DAY", 33},
            {"DHL_INTERNATIONAL", 40},
        };
        public const string FIS_INBOUND_FOLDER = "FIS/Batch/inbound";
        public const string FIS_DATA_EXTRACT_INBOUND_FOLDER = "FIS/DataExtract/inbound";
        public const string FIS_DATA_EXTRACT_INBOUND_ARCHIVE_FOLDER = "fis/DataExtract/inbound/archive";

        public const int MON_TXN_FILE_DATE_START_INDEX = 6;
        public const int NON_MON_TXN_FILE_DATE_START_INDEX = 9;

    }
}
