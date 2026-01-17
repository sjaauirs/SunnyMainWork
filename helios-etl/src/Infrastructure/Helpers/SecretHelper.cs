using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using SunnyRewards.Helios.ETL.Common.Constants;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos.Json;
using System.Text.Json;
using ISecretHelper = SunnyRewards.Helios.ETL.Infrastructure.Helpers.Interfaces.ISecretHelper;

namespace SunnyRewards.Helios.ETL.Infrastructure.Helpers
{
    /// <summary>
    /// SecretHelper
    /// </summary>
    public class SecretHelper : ISecretHelper
    {
        private readonly IVault _vault;
        private readonly IConfiguration _configuration;
        private readonly ILogger<SecretHelper> _logger;
        const string className = nameof(SecretHelper);
        public SecretHelper(IVault vault, IConfiguration configuration, ILogger<SecretHelper> logger)
        {
            _vault = vault;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// GetTenantXApiKey
        /// </summary>
        /// <param name="tenantCode"></param>
        /// <returns></returns>
        public async Task<string> GetTenantXApiKey(string tenantCode)
        {
            return await GetTenantSecret(tenantCode, Constants.XApiKeySecret);
        }

        /// <summary>
        /// GetTenantSecret
        /// </summary>
        /// <param name="tenantCode"></param>
        /// <param name="secretKey"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<string> GetTenantSecret(string tenantCode, string secretKey)
        {
            const string methodName = nameof(GetTenantSecret);
            _logger.LogInformation($"{className}.{methodName}: Stared processing for tenantCode: {tenantCode}, secretKey: {secretKey}");

            var secret = await _vault.GetTenantSecret(tenantCode ?? string.Empty, secretKey);

            if (string.IsNullOrEmpty(secret) || secret == _vault.InvalidSecret)
            {
                _logger.LogError($"{className}.{methodName}: Failed processing., Error Code:{StatusCodes.Status500InternalServerError}");
                throw new InvalidOperationException($"{secretKey} is not configured.");
            }

            _logger.LogInformation($"{className}.{methodName}: Completed processing.");
            return secret;
        }
        public async Task<string> GetTenantSftpPassphraseSecret(string tenantCode)
        {
            const string methodName = nameof(GetTenantSecret);
            _logger.LogInformation($"{className}.{methodName}: Stared processing for tenantCode: {tenantCode}, secretKey: {Constants.SftpPrivateKeyPassphrase}");

            var secret = await _vault.GetTenantSecret(tenantCode ?? string.Empty, Constants.SftpPrivateKeyPassphrase);

            if (string.IsNullOrEmpty(secret) || secret == _vault.InvalidSecret)
            {
                _logger.LogWarning($"{className}.{methodName}: {Constants.SftpPrivateKeyPassphrase} not found., Error Code:{StatusCodes.Status404NotFound}");
                return string.Empty;
                
            }

            _logger.LogInformation($"{className}.{methodName}: Completed processing.");
            return secret;
        }

        public async Task<string> GetSecret(string secretKey)
        {
            const string methodName = nameof(GetSecret);
            var secret = await _vault.GetSecret(secretKey);
            if (string.IsNullOrEmpty(secret) || secret == _vault.InvalidSecret)
            {
                _logger.LogError("{ClassName}.{MethodName}: Failed processing., Error Code:{ErrorCode}", className, methodName, StatusCodes.Status500InternalServerError);
                throw new InvalidOperationException($"{secretKey} is not configured.");
            }
            _logger.LogInformation($"{className}.{methodName}: Completed processing.");
            return secret;
        }

        public async Task<string> GetRedshiftConnectionString()
        {
            return await GetSecret(Constants.RedShiftConnectionStringKey);
        }

        public async Task<string> GetPostgresConnectionString()
        {
            return await GetSecret(Constants.PostgresConnectionStringKey);
        }

        public async Task<string> GetTenantSpecificPublicKey(string tenantCode)
        {
            return await GetTenantSecret(tenantCode, Constants.TenantSpecificPublicKey);
        }
        public async Task<string> GetSunnyPrivateKeyByTenantCode(string tenantCode)
        {
            return await GetTenantSecret(tenantCode, Constants.SunnyPrivateKey);
        }
        public async Task<string> GetSunnyPrivateKeyPassphraseByTenantCode(string tenantCode)
        {
            return await GetTenantSecret(tenantCode, Constants.SunnyPrivateKeyPassphrase);
        }
        public async Task<SftpConfig> GetSftpDetailsByTenantCode(string tenantCode)
        {
            try
            {
                var secretKeys = new Dictionary<string, string?>
                {
                    [nameof(SftpConfig.Host)] = await GetTenantSecret(tenantCode, Constants.SftpHost),
                    [nameof(SftpConfig.Port)] = await GetTenantSecret(tenantCode, Constants.SftpPort),
                    [nameof(SftpConfig.UserName)] = await GetTenantSecret(tenantCode, Constants.SftpUserName),
                    [nameof(SftpConfig.PrivateKey)] = await GetTenantSecret(tenantCode, Constants.SftpPrivateKey),
                    [nameof(SftpConfig.PrivateKeyPassphrase)] = await GetTenantSftpPassphraseSecret(tenantCode),
                    [nameof(SftpConfig.RemoteDirectory)] = await GetTenantSecret(tenantCode, Constants.SftpRemoteDirectory)
                };

                return new SftpConfig
                {
                    Host = secretKeys[nameof(SftpConfig.Host)],
                    Port = int.TryParse(secretKeys[nameof(SftpConfig.Port)], out var port) ? port : 22,
                    UserName = secretKeys[nameof(SftpConfig.UserName)],
                    PrivateKey = secretKeys[nameof(SftpConfig.PrivateKey)],
                    PrivateKeyPassphrase = secretKeys[nameof(SftpConfig.PrivateKeyPassphrase)],
                    RemoteDirectory = secretKeys[nameof(SftpConfig.RemoteDirectory)]
                };
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error occurred while fetching SFTP config for tenant '{TenantCode}'.", tenantCode);
                throw;
            }
        }

    }
}
