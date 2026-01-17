using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using SunnyRewards.Helios.Wallet.Infrastructure.Helpers.Interfaces;

namespace SunnyRewards.Helios.Wallet.Infrastructure.Helpers
{
    public class SecretHelper : ISecretHelper
    {
        private readonly IConfiguration _configuration;
        private readonly IVault _vault;
        private readonly ILogger<SecretHelper> _logger;

        public SecretHelper(IConfiguration configuration, IVault vault, ILogger<SecretHelper> logger)
        {
            _configuration = configuration;
            _vault = vault;
            _logger = logger;
        }
        /// <summary>
        /// Retrieves a configuration value.
        /// </summary>
        /// <param name="keyName">The configuration key name.</param>
        /// <returns>The configuration value.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the configuration key is missing or empty.</exception>
        private string GetConfigValue(string keyName)
        {
            var keyValue = _configuration.GetValue<string>(keyName);
            if (string.IsNullOrWhiteSpace(keyValue))
            {
                _logger.LogError("GetConfigValue: Configuration key '{KeyName}' is missing or empty.", keyName);
                throw new InvalidOperationException($"{keyName} is not configured.");
            }

            return keyValue;
        }
        /// <summary>
        /// Retrieves the reward wallet type code from the configuration.
        /// </summary>
        /// <returns>The reward wallet type code.</returns>
        public string GetRewardWalletTypeCode()
        {
            return GetConfigValue("Reward_Wallet_Type_Code");
        }

        /// <summary>
        /// Retrieves the sweepstakes entries wallet type code from the configuration.
        /// </summary>
        /// <returns>The sweepstakes entries wallet type code.</returns>
        public string GetSweepstakesEntriesWalletTypeCode()
        {
            return GetConfigValue("Sweepstakes_Entries_Wallet_Type_Code");
        }

        /// <summary>
        /// Retrieves the redemption wallet type code from the configuration.
        /// </summary>
        /// <returns>The redemption wallet type code.</returns>
        public string GetRedemptionWalletTypeCode()
        {
            return GetConfigValue("Redemption_Wallet_Type_Code");
        }

        /// <summary>
        /// Retrieves the sweepstakes entries redemption wallet type code from the configuration.
        /// </summary>
        /// <returns>The sweepstakes entries redemption wallet type code.</returns>
        public string GetSweepstakesEntriesRedemptionWalletTypeCode()
        {
            return GetConfigValue("Sweepstakes_Entries_Redemption_Wallet_Type_Code");
        }
    }
}
