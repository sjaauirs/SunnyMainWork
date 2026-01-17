namespace SunnyRewards.Helios.ETL.Infrastructure.Logs
{
    public class S3LogContext
    {
        public string? Message { get; set; }
        public string? TenantCode { get; set; }
        public string? ConsumerCode { get; set; }

        public string? MemberNbr { get; set; }

        public Exception? Ex { get; set; }


        public string ToLogContext()
        {
            if (!string.IsNullOrEmpty(ConsumerCode) && !string.IsNullOrEmpty(MemberNbr))
                return $"[{DateTime.UtcNow}] [{TenantCode}|{MemberNbr}|{ConsumerCode}] {Message} {Ex?.Message}";

            if (string.IsNullOrEmpty(ConsumerCode) && !string.IsNullOrEmpty(MemberNbr))
                return $"[{DateTime.UtcNow}] [{TenantCode}|{MemberNbr}] {Message} {Ex?.Message}";

            return $"[{DateTime.UtcNow}] [{TenantCode}] {Message} {Ex?.Message}";
        }
    }
}
