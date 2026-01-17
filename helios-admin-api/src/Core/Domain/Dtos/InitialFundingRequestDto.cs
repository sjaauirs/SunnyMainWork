namespace SunnyRewards.Helios.Admin.Core.Domain.Dtos
{
    public class InitialFundingRequestDto
    {
        public string? TenantCode { get; set; }
        public string? ConsumerCode { get; set; }
        public List<string>? SelectedPurses { get; set; }
    }
}
