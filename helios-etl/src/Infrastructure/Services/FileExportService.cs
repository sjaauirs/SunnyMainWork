using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Infrastructure.Helpers.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces;
using System.Text;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services
{
    public class FileExportService : IFileExportService
    {
        private readonly ILogger<FileExportService> _logger;
        private readonly IAwsS3Service _awsS3Service;
        private readonly IPgpS3FileEncryptionHelper _s3FileEncryptionHelper;
        private readonly ISecretHelper _secretHelper;
        private const string className = nameof(FileExportService);

        public FileExportService(
            ILogger<FileExportService> logger,
            IAwsS3Service awsS3Service, IPgpS3FileEncryptionHelper pgpS3FileEncryptionHelper, ISecretHelper secretHelper)
        {
            _logger = logger;
            _awsS3Service = awsS3Service;
            _s3FileEncryptionHelper = pgpS3FileEncryptionHelper;
            _secretHelper = secretHelper;
        }

        public async Task<string> WriteDataToFile(string data, string fileName)
        {
            try
            {
                string filePath = Path.Combine(Path.GetTempPath(), fileName);
                _logger.LogInformation("Writing data to file at {FilePath}", filePath);

                await File.WriteAllTextAsync(filePath, data, Encoding.UTF8);

                return filePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error writing data to file.");
                throw;
            }
        }

        public async Task EncryptFileIfRequiredThenUploadToS3Async(EtlExecutionContext context, string inputFilePath, string fileName, bool encrypt)
        {
            const string methodName = nameof(EncryptFileIfRequiredThenUploadToS3Async);

            string tempEncryptedPath = Path.Combine(Path.GetTempPath(), $"encrypted_{Guid.NewGuid()}.pgp");

            try
            {
                if (encrypt)
                {
                    var publicKeyBase64 = await _secretHelper.GetTenantSpecificPublicKey(context.TenantCode);
                    var publicKey = Convert.FromBase64String(publicKeyBase64);

                    await using var inputStream = File.OpenRead(inputFilePath);
                    await using var publicKeyStream = new MemoryStream(publicKey);
                    await using var encryptedFileStream = File.Create(tempEncryptedPath);

                    _s3FileEncryptionHelper.EncryptFile(inputStream, encryptedFileStream, publicKeyStream);
                }
                else
                {
                    // Simply copy to temp if encryption is off
                    File.Copy(inputFilePath, tempEncryptedPath, overwrite: true);
                }

                // Upload encrypted file from disk to S3
                await using var finalUploadStream = File.OpenRead(tempEncryptedPath);
                var outputKey = $"{context.ArchiveFilePath}/{fileName}";
                await _awsS3Service.UploadStreamToS3(finalUploadStream, context.ArchiveBucketName, outputKey);

                // Replace original file with encrypted one (overwrite)
                File.Copy(tempEncryptedPath, inputFilePath, overwrite: true);

                _logger.LogInformation("{Class}.{Method}: Successfully processed and uploaded file to '{OutputKey}'", className, methodName, outputKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Class}.{Method}: Error processing file", className, methodName);
                throw;
            }
            finally
            {
                if (File.Exists(tempEncryptedPath))
                    File.Delete(tempEncryptedPath);
            }
        }
    }
}
