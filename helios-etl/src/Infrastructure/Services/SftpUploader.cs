using Microsoft.Extensions.Logging;
using Renci.SshNet;
using Renci.SshNet.Common;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos.Json;
using SunnyRewards.Helios.ETL.Infrastructure.Helpers.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services
{
    public class SftpUploader : ISftpUploader
    {
        private readonly ISecretHelper _secretHelper;
        private readonly ILogger<SftpUploader> _logger;
        private const string ClassName = nameof(SftpUploader);

        public SftpUploader(ILogger<SftpUploader> logger, ISecretHelper secretHelper)
        {
            _secretHelper = secretHelper ?? throw new ArgumentNullException(nameof(secretHelper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task UploadFile(EtlExecutionContext context, string localFilePath)
        {
            const string methodName = nameof(UploadFile);

            if (!File.Exists(localFilePath)) throw new FileNotFoundException("Local file not found.", localFilePath);

            var config = await _secretHelper.GetSftpDetailsByTenantCode(context.TenantCode);

            ValidateSftpConfig(config, context.TenantCode, methodName);

            using var keyStream = new MemoryStream(Convert.FromBase64String(config.PrivateKey!));
            var keyFile = string.IsNullOrEmpty(config.PrivateKeyPassphrase)
                ? new PrivateKeyFile(keyStream)
                : new PrivateKeyFile(keyStream, config.PrivateKeyPassphrase);

            var connectionInfo = new ConnectionInfo(
                config.Host,
                config.Port,
                config.UserName,
                new PrivateKeyAuthenticationMethod(config.UserName, keyFile)
            );

            using var sftpClient = new SftpClient(connectionInfo);

            try
            {
                sftpClient.Connect();
                EnsureRemoteDirectoryExists(sftpClient, config.RemoteDirectory);

                using var fileStream = new FileStream(localFilePath, FileMode.Open, FileAccess.Read);
                string remotePath = $"{config.RemoteDirectory.TrimEnd('/')}/{Path.GetFileName(localFilePath)}";

                sftpClient.UploadFile(fileStream, remotePath, true);
                _logger.LogInformation("Uploaded file to {RemotePath} for tenant {TenantCode}", remotePath, context.TenantCode);
            }
            catch (SshException ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: Failed to upload file for tenant {TenantCode}", ClassName, methodName, context.TenantCode);
                throw;
            }
            finally
            {
                if (sftpClient.IsConnected)
                    sftpClient.Disconnect();
            }
        }

        private void ValidateSftpConfig(SftpConfig config, string tenantCode, string methodName)
        {
            if (string.IsNullOrEmpty(config.Host))
                LogAndThrowConfigError("Host", tenantCode, methodName);

            if (string.IsNullOrEmpty(config.UserName))
                LogAndThrowConfigError("UserName", tenantCode, methodName);

            if (string.IsNullOrEmpty(config.PrivateKey))
                LogAndThrowConfigError("PrivateKey", tenantCode, methodName);
        }

        private void LogAndThrowConfigError(string field, string tenantCode, string methodName)
        {
            _logger.LogError("{ClassName}.{MethodName}: SFTP {Field} is not configured for Tenant: {TenantCode}", ClassName, methodName, field, tenantCode);
            throw new InvalidOperationException($"SFTP {field} is not configured for tenant {tenantCode}.");
        }

        private void EnsureRemoteDirectoryExists(SftpClient client, string path)
        {
            var folders = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            var currentPath = "/";

            foreach (var folder in folders)
            {
                currentPath = $"{currentPath.TrimEnd('/')}/{folder}";
                if (!client.Exists(currentPath))
                {
                    client.CreateDirectory(currentPath);
                    _logger.LogInformation("Created remote directory: {Path}", currentPath);
                }
            }
        }


    }

}
