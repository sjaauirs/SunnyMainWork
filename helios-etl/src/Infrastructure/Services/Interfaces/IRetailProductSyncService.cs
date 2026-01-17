using SunnyRewards.Helios.ETL.Core.Domain.Dtos;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces
{
    public interface IRetailProductSyncService
    {
        /// <summary>
        /// Processes messages from the Retail product AWS SQS queue and generate FIS APL file
        /// </summary>
        /// <param name="etlExecutionContext"></param>
        /// <returns></returns>
        Task ProcessQueueMessages(EtlExecutionContext etlExecutionContext);

        /// <summary>
        /// Decrypt the encrypted files from S3 and saves them to the specified local folder location
        /// </summary>
        /// <param name="localFolderPath"></param>
        /// <returns></returns>
        Task DecryptAndSaveToLocalPath(EtlExecutionContext etlExecutionContext, string localFolderPath);

        Task EncryptFile(EtlExecutionContext etlExecutionContext);
        Task CopyFileToDestination(EtlExecutionContext etlExecutionContext);
        Task ArchiveFile(EtlExecutionContext etlExecutionContext);
        Task DeleteFile(EtlExecutionContext etlExecutionContext);



        /// <summary>
        /// Restore Costco backup from DynamoDB
        /// </summary>
        /// <param name="etlExecutionContext"></param>
        /// <returns></returns>
        Task RestoreCostcoBackupFromDynamoDB(EtlExecutionContext etlExecutionContext);
    }
}
