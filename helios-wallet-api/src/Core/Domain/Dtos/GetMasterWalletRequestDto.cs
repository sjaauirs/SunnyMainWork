namespace SunnyRewards.Helios.Wallet.Core.Domain.Dtos
{
    public class GetMasterWalletRequestDto
    {

        public string? WalletTypeName { get; set; } //(REWARD, REDEMPTION, FUND etc)
        public string? TenantCode { get; set; }
        public string? SponsorCode { get; set; }// (not used in MVP)
        public string? CustomerCode { get; set; } // (not used in MVP)
    }
}