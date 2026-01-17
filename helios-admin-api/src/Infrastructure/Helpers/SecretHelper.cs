using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Admin.Infrastructure.Helpers.Interface;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using ISecretHelper = SunnyRewards.Helios.Admin.Infrastructure.Helpers.Interface.ISecretHelper;

namespace SunnyRewards.Helios.Admin.Infrastructure.Helpers
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
        /// Retrieves a tenant-specific secret from the vault.
        /// </summary>
        /// <param name="tenantCode">The tenant code.</param>
        /// <param name="secretKey">The secret key.</param>
        /// <returns>The secret value.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the secret is not configured.</exception>
        private async Task<string> GetTenantSecret(string tenantCode, string secretKey)
        {
            var secret = await _vault.GetTenantSecret(tenantCode ?? string.Empty, secretKey);
            if (string.IsNullOrEmpty(secret) || secret == _vault.InvalidSecret)
            {
                _logger.LogError("GetTenantSecret: {secretKey} is not configured for Tenant: {tenantCode}", secretKey, tenantCode);
                throw new InvalidOperationException($"{secretKey} is not configured.");
            }

            return secret;
        }

        /// <summary>
        /// Retrieves a secret from the vault.
        /// </summary>
        /// <param name="secretKey">The secret key.</param>
        /// <returns>The secret value.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the secret is not configured.</exception>
        private async Task<string> GetSecret(string secretKey)
        {
            var secret = await _vault.GetSecret(secretKey);
            if (string.IsNullOrEmpty(secret) || secret == _vault.InvalidSecret)
            {
                _logger.LogError("GetSecret: {secretKey} is not configured.", secretKey);
                throw new InvalidOperationException($"{secretKey} is not configured.");
            }

            return secret;
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
        /// Retrieves the AWS access key from the configuration.
        /// </summary>
        /// <returns>The AWS access key.</returns>
        public async Task<string> GetAwsAccessKey()
        {
            var awsAccessKey = _configuration.GetValue<string>("AWS:AWS_ACCESS_KEY_NAME") ?? "";
            return await GetSecret(awsAccessKey);
        } 
        public async Task<string> GetAwsFireBaseCredentialKey()
        {
            var awsAccessKey = _configuration.GetValue<string>("AWS:FIREBASE_CREDENTIALS_JSON_NAME") ?? "";
            return await GetSecret(awsAccessKey);
        }

        /// <summary>
        /// Retrieves the AWS secret key from the configuration.
        /// </summary>
        /// <returns>The AWS secret key.</returns>
        public async Task<string> GetAwsSecretKey()
        {
            var awsSecretKey = _configuration.GetValue<string>("AWS:AWS_SECRET_KEY_NAME") ?? "";
            return await GetSecret(awsSecretKey);
        }

        /// <summary>
        /// Retrieves the environment from the vault.
        /// </summary>
        /// <returns>The environment.</returns>
        public async Task<string> GetEnvironment()
        {
            return await GetSecret("env");
        }

        /// <summary>
        /// Retrieves the temporary S3 bucket name from the configuration.
        /// </summary>
        /// <returns>The temporary S3 bucket name.</returns>
        public string GetAwsTmpS3BucketName()
        {
            return GetConfigValue("AWS:AWS_TMP_BUCKET_NAME");
        }

        /// <summary>
        /// Retrieves the export tenant version from the configuration.
        /// </summary>
        /// <returns>The export tenant version.</returns>
        public string GetExportTenantVersion()
        {
            return GetConfigValue("ExportTenantVersion");
        }
    }
}
