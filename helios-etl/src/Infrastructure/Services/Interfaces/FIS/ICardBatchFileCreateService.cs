using SunnyRewards.Helios.ETL.Core.Domain.Dtos;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces.FIS
{
    public interface ICardBatchFileCreateService
    {
        /// <summary>
        /// Generates batch file with card create (30) records for all consumers in execution context tenant,
        /// and upload to S3
        /// </summary>
        /// <param name="etlExecutionContext"></param>
        /// <returns></returns>
        Task GenerateCardCreateFile(EtlExecutionContext etlExecutionContext);
        Task EncryptFile(EtlExecutionContext etlExecutionContext);
        Task CopyFileToDestination(EtlExecutionContext etlExecutionContext);
        Task ArchiveFile(EtlExecutionContext etlExecutionContext);
        Task DeleteFile(EtlExecutionContext etlExecutionContext);
    }
}
