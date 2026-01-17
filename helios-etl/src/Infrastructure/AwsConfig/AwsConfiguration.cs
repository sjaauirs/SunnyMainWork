using Microsoft.Extensions.Configuration;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;

namespace SunnyRewards.Helios.ETL.Infrastructure.AwsConfig
{
    /// <summary>
    /// 
    /// </summary>
    public class AwsConfiguration
    {
        private readonly IVault _vault;
        private readonly IConfiguration _configuration;

        public AwsConfiguration(IVault vault, IConfiguration configuration)
        {
            _vault = vault;
            _configuration = configuration;
        }

        public async Task<string> GetAwsAccessKey() => await _vault.GetSecret(_configuration.GetSection("AWS:AWS_ACCESS_KEY_NAME").Value?.ToString() ?? "");
        public async Task<string> GetAwsSecretKey() => await _vault.GetSecret(_configuration.GetSection("AWS:AWS_SECRET_KEY_NAME").Value?.ToString() ?? "");

        public string GetAwsSnsTopic(string topicName) => _configuration.GetSection($"AWS:{topicName}").Value?.ToString() ?? "";

        public async Task<string> GetAwsTaskUpdateQueueUrl() => await _vault.GetSecret(_configuration.GetSection("AWS:AWS_TASK_UPDATE_QUEUE_URL_KEY_NAME").Value?.ToString() ?? "");
        public string GetAwsMemberImportDlqQueueUrl() => _configuration.GetSection($"AWS:AWS_MEMBER_IMPORT_SQS_DLQ_TOPIC_NAME").Value?.ToString() ?? "";
        public async Task<string> GetAwsRetailProductSyncQueueUrl() => await _vault.GetSecret(_configuration.GetSection("AWS:AWS_RETAIL_PRODUCTS_SYNC_QUEUE_URL_KEY_NAME").Value?.ToString() ?? "");
        public async Task<string> GetAwsBatchJobReportQueueUrl() => await _vault.GetSecret(_configuration.GetSection("AWS:AWS_BATCH_JOB_REPORT_QUEUE_URL_KEY_NAME").Value?.ToString() ?? "");
        public string GetAwsTmpS3BucketName() => _configuration.GetSection("AWS:AWS_TMP_BUCKET_NAME").Value?.ToString() ?? "";

        public string GetAwsConsumerBucketName() => _configuration.GetSection("AWS:AWS_BUCKET_NAME").Value?.ToString() ?? "";


        public string GetFisAplPublicKeyName() => _configuration.GetSection("AWS:FIS_APL_PUBLIC_KEY_NAME").Value?.ToString() ?? "";
        public string GetSunnyAplPublicKeyName() => _configuration.GetSection("AWS:SUNNY_APL_PUBLIC_KEY_NAME").Value?.ToString() ?? "";
        public string GetFisEtgAplPublicKeyName() => _configuration.GetSection("AWS:FIS_ETG_APL_PUBLIC_KEY_NAME").Value?.ToString() ?? "";
        public string GetSunnyAplPrivateKeyName() => _configuration.GetSection("AWS:SUNNY_APL_PRIVATE_KEY_NAME").Value?.ToString() ?? "";
        public string GetAwsFisSftpS3BucketName() => _configuration.GetSection("AWS:AWS_FIS_SFTP_BUCKET_NAME").Value?.ToString() ?? "";
        public string GetAwsSweepstakesSftpS3BucketName() => _configuration.GetSection("AWS:AWS_SWEEPSTAKES_SFTP_BUCKET_NAME").Value?.ToString() ?? "";
        public string GetSunnyPrivateKeyPassPhraseKeyName() => _configuration.GetSection("AWS:SUNNY_APL_PRIVATE_KEY_PASS_PHRASE_NAME").Value?.ToString() ?? "";
        public string GetAwsSunnyArchiveFileBucketName() => _configuration.GetSection("AWS:AWS_SUNNY_FILE_ARCHIVE_BUCKET_NAME").Value?.ToString() ?? "";
        public string GetAwsSunnyPublicFileBucketName() => _configuration.GetSection("AWS:AWS_SUNNY_FILE_PUBLIC_BUCKET_NAME").Value?.ToString() ?? "";
        public string GetAwsSweepstakesSmtpS3BucketName() => _configuration.GetSection("AWS:AWS_SWEEPSTAKES_SFTP_BUCKET_NAME").Value?.ToString() ?? "";
        public string GetAwsReoprtS3BucketName() => _configuration.GetSection("AWS:AWS_REPORT_BUCKET_NAME").Value?.ToString() ?? "";
        public string GetAwsSunnyHeliosReportsS3BucketName() => _configuration.GetSection("AWS:AWS_SUNNY_HELIOS_REPORTS_BUCKET_NAME").Value?.ToString() ?? "";
        public async Task<string> GetHealthMetricQueueUrl() => await _vault.GetSecret(_configuration.GetSection("AWS:AWS_HEALTH_METRIC_QUEUE_NAME").Value?.ToString() ?? "");
        public async Task<string> GetTenantSecret(string tenantCode, string secretKeyName) => await _vault.GetTenantSecret(tenantCode, secretKeyName);
        public async Task<string> GetSecret(string secretKeyName) => await _vault.GetSecret(secretKeyName);
        public async Task<string> GetDynamoDbCostcoMessagesSQSTableName() => await _vault.GetSecret(_configuration.GetSection("AWS:DYNAMO_DB_COSTCO_MESSAGES_SQS_TABLE_NAME").Value?.ToString() ?? "");
        public async Task<string> GetDynamoDbJobHistoryTableName() => await _vault.GetSecret(_configuration.GetSection("AWS:DYNAMO_DB_JOB_HISTORY_TABLE_NAME").Value?.ToString() ?? "");
        public async Task<string> GetDynamoDbJobDefinitionTableName() => await _vault.GetSecret(_configuration.GetSection("AWS:DYNAMO_DB_JOB_DEFINITION_TABLE_NAME").Value?.ToString() ?? "");
        public async Task<string> GetImageBaseUrl() => await _vault.GetSecret(_configuration.GetSection("AWS:ENV_SPECIFIC_IMAGE_UPLOAD_BASE_URL").Value?.ToString() ?? "");

    }
}
