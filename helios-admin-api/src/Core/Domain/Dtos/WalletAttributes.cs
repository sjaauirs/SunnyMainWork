namespace SunnyRewards.Helios.Admin.Core.Domain.Dtos
{
    public class WalletAttributes
    {
        public double OwnerMax { get; set; }
        public double ContributorMax { get; set; }
        public double WalletMax { get; set; }
        public bool IndividualWallet { get; set; }
        public double MembershipWalletEarnMax { get; set; }

        public WalletAttributes(double ownerMax, double contributorMax, double walletMax, bool individualWallet, double membershipWalletEarnMax)
        {
            OwnerMax = ownerMax;
            ContributorMax = contributorMax;
            WalletMax = walletMax;
            IndividualWallet = individualWallet;
            MembershipWalletEarnMax = membershipWalletEarnMax;
        }
    }

}
