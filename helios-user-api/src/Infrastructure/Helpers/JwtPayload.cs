namespace SunnyRewards.Helios.User.Infrastructure.Helpers
{
    public class JwtPayload
    {
        public string? ConsumerCode { get; set; }
        public string? Email { get; set; }
        public string? TenantCode { get; set; }
        public string? Role { get; set; }
        public DateTime? Expiry { get; set; }
        public string? Environment { get; set; }
        public string? PersonUniqueIdentifier { get; set; }
        public bool? IsSSOUser { get; set; }
        
    }
}
