using SunnyRewards.Helios.ETL.Core.Domain.Dtos.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.ETL.Infrastructure.Helpers.Interfaces
{
    /// <summary>
    /// Secrets Helper
    /// </summary>
    public interface ISecretHelper
    {
        /// <summary>
        /// GetTenantXApiKey
        /// </summary>
        /// <param name="tenantCode"></param>
        /// <returns></returns>
        Task<string> GetTenantXApiKey(string tenantCode);

        /// <summary>
        /// GetTenantSecret
        /// </summary>
        /// <param name="tenantCode"></param>
        /// <param name="secretKey"></param>
        /// <returns></returns>
        Task<string> GetTenantSecret(string tenantCode, string secretKey);

        Task<string> GetSecret(string secretKey);

        Task<string> GetRedshiftConnectionString();

        Task<string> GetPostgresConnectionString();

        Task<string> GetTenantSpecificPublicKey(string tenantCode);
        Task<string> GetSunnyPrivateKeyByTenantCode(string tenantCode);
        Task<string> GetSunnyPrivateKeyPassphraseByTenantCode(string tenantCode);
        /// <summary>
        /// Get client sftp details by tenant code
        /// </summary>
        /// <param name="tenantCode"></param>
        /// <returns></returns>
        Task<SftpConfig> GetSftpDetailsByTenantCode(string tenantCode);
    }
}
