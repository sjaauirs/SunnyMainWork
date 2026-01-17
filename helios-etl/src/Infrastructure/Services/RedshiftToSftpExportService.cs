using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces;
using System.Text.RegularExpressions;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services
{
    public class RedshiftToSftpExportService : IRedshiftToSftpExportService
    {
        private readonly ILogger<RedshiftToSftpExportService> _logger;
        private readonly IRedshiftDataService _redshiftService;
        private readonly IFileExportService _fileExportService;
        private readonly ISftpUploader _sftpUploader;
        private const string className = nameof(RedshiftToSftpExportService);

        public RedshiftToSftpExportService(
            ILogger<RedshiftToSftpExportService> logger,
            IRedshiftDataService redshiftService,
            IFileExportService fileExportService,
            ISftpUploader sftpUploader)
        {
            _logger = logger;
            _redshiftService = redshiftService;
            _fileExportService = fileExportService;
            _sftpUploader = sftpUploader;
        }

        public async Task ExecuteExportAsync(EtlExecutionContext context)
        {
            const string methodName = nameof(ExecuteExportAsync);
            string tempInputFilePath = string.Empty;

            ValidateContext(context);

            _logger.LogInformation("{Class}.{Method}: Starting export for Table: {TableName}, CustomerCode: {CustomerCode}", className, methodName, context.TableName, context.CustomerCode);

            try
            {
                string data = await RetryPolicyAsync(() => _redshiftService.FetchDataAsync(
                    context.TableName, context.ColumnName,
                    context.DateRangeStart, context.DateRangeEnd, context.Delimiter,context.RedshiftDatabaseName, context.DateFormat, context.ShouldAppendTotalRowCount));

                string fileName = GetFileName(context.OutboundFileNamePattern, context.TableName);
                tempInputFilePath = await _fileExportService.WriteDataToFile(data, fileName);

                await RetryPolicyAsync(() => _fileExportService.EncryptFileIfRequiredThenUploadToS3Async(
                    context, tempInputFilePath, fileName, context.ShouldEncrypt));
                if (context.EnableSftp)
                {
                    await RetryPolicyAsync(() => _sftpUploader.UploadFile(context, tempInputFilePath));
                }

                _logger.LogInformation("{Class}.{Method}: ETL export completed for Table: {TableName}", className, methodName, context.TableName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Class}.{Method}: Error during export for Table: {TableName}", className, methodName, context.TableName);
                throw;
            }
            finally
            {
                if (!string.IsNullOrWhiteSpace(tempInputFilePath))
                    TryDeleteFile(tempInputFilePath);
            }
        }

        private void ValidateContext(EtlExecutionContext context)
        {
            if (string.IsNullOrWhiteSpace(context.TableName))
                throw new ArgumentException("TableName is required.");
            if (string.IsNullOrWhiteSpace(context.OutboundFileNamePattern))
                throw new ArgumentException("OutboundFileNamePattern is required.");
            if (string.IsNullOrWhiteSpace(context.TenantCode))
                throw new ArgumentException("TenantCode is required.");
        }

        private void TryDeleteFile(string path)
        {
            try
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
            catch (IOException ex)
            {
                _logger.LogWarning(ex, "Failed to delete temp file '{Path}'", path);
            }
        }

        private static string GetFileName(string pattern, string tableName)
        {
            string uuid = Guid.NewGuid().ToString("N");
            string mmddyyyy = DateTime.UtcNow.ToString("MMddyyyy");
            string timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");

            return pattern
                .Replace("{uuid}", uuid)
                .Replace("{mmddyyyy}", mmddyyyy)
                .Replace("{table_name}", tableName)
                .Replace("{yyyyMMdd_HHmmss}", timestamp);
        }

        private async Task RetryPolicyAsync(Func<Task> action, int maxRetries = 3, int delayMilliseconds = 1000)
        {
            int attempt = 0;
            while (true)
            {
                try
                {
                    await action();
                    break;
                }
                catch when (attempt < maxRetries)
                {
                    attempt++;
                    await Task.Delay(delayMilliseconds * attempt);
                }
            }
        }

        private async Task<T> RetryPolicyAsync<T>(Func<Task<T>> action, int maxRetries = 3, int delayMilliseconds = 1000)
        {
            int attempt = 0;
            while (true)
            {
                try
                {
                    return await action();
                }
                catch when (attempt < maxRetries)
                {
                    attempt++;
                    await Task.Delay(delayMilliseconds * attempt);
                }
            }
        }
    }
}
