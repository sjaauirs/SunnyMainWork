using SunnyRewards.Helios.ETL.Core.Domain.Dtos;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces.FIS
{
    public interface ICardDisbursementFileCreateService
    {
        /// <summary>
        /// Generates card load file for given tenant
        /// </summary>
        /// <param name="etlExecutionContext"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        //Task GenerateCardLoadFileAsync(EtlExecutionContext etlExecutionContext);
        Task GenerateCardLoadFile(EtlExecutionContext etlExecutionContext);
        Task EncryptFile(EtlExecutionContext etlExecutionContext);
        Task CopyFileToDestination(EtlExecutionContext etlExecutionContext);
        Task ArchiveFile(EtlExecutionContext etlExecutionContext);
        Task DeleteFile(EtlExecutionContext etlExecutionContext);
    }
}
