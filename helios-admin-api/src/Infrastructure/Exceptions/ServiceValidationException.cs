namespace SunnyRewards.Helios.Admin.Infrastructure.Exceptions
{
    public class ServiceValidationException : Exception
    {
        public int StatusCode { get; }

        public ServiceValidationException()
        {
        }
        public ServiceValidationException(string message, int? statusCode = 400)
            : base(message)
        {
            StatusCode = statusCode ?? 400;
        }

        public ServiceValidationException(string message, Exception innerException, int statusCode = 400)
            : base(message, innerException)
        {
            StatusCode = statusCode;
        }
    }
}

