
namespace SunnyRewards.Helios.ETL.Common.CustomException
{
    public class ETLException : Exception
    {
        // Custom property to hold the error code
        public int ErrorCode { get; }

        // Constructor that accepts a custom message and an error code
        public ETLException(ETLExceptionCodes errorCode, string message)
            : base($"{(int)errorCode}: {message}")
        {
            ErrorCode = (int)errorCode;
        }
    }

    public enum ETLExceptionCodes
    {
        NotFoundInDb = 1000,
        NullValue = 1001,
        NetworkFailure = 1002,
        UnauthorizedAccess = 1003,
        NullResponseFromAPI = 1004,
        AWSSecretNotFound = 1005,
        InValidValue = 1006,
        ErrorFromAPI = 1007,
        NotFoundInDynamoDb = 1008,
        DynamoDbInsertFailed = 1009,
        DynamoDbUpdateFailed = 1010
    }
}
