namespace SunnyRewards.Helios.User.Core.Domain.Dtos
{
    public class ConsumerLoginRequestDto
    {
        public string? TenantCode { get; set; }
        public string? MemberId { get; set; }  // MemNbr is used if not null as primary lookup
        public string? Email { get; set; }  // either one of Email or MemNbr must not be null 
        public string? ConsumerCode { get; set; } // if this is set, it overrides all other parameters
        public string? ApiToken { get; set; }
        public string? EncKeyId { get; set; }
        public string? EncToken { get; set; }
        public string? UserAgent { get; set; }
    }
}