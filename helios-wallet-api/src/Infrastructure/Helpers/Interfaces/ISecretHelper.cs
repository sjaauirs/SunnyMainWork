namespace SunnyRewards.Helios.Wallet.Infrastructure.Helpers.Interfaces
{
    public interface ISecretHelper
    {
        /// <summary>
        /// Retrieves the reward wallet type code from the configuration.
        /// </summary>
        /// <returns>The reward wallet type code.</returns>
        string GetRewardWalletTypeCode();

        /// <summary>
        /// Retrieves the sweepstakes entries wallet type code from the configuration.
        /// </summary>
        /// <returns>The sweepstakes entries wallet type code.</returns>
        string GetSweepstakesEntriesWalletTypeCode();

        /// <summary>
        /// Retrieves the redemption wallet type code from the configuration.
        /// </summary>
        /// <returns>The redemption wallet type code.</returns>
        string GetRedemptionWalletTypeCode();

        /// <summary>
        /// Retrieves the sweepstakes entries redemption wallet type code from the configuration.
        /// </summary>
        /// <returns>The sweepstakes entries redemption wallet type code.</returns>
        string GetSweepstakesEntriesRedemptionWalletTypeCode();
    }
}
