namespace SunnyRewards.Helios.Admin.Core.Domain.Dtos
{
    public class AgreementsVerifiedEventRequestDto
    {
        public string TenantCode { get; set; } = null!;
        public string ConsumerCode { get; set; } = null!;
    }
}
