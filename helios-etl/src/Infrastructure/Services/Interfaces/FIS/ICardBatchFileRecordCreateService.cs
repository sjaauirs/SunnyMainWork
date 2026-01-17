using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos.Json;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces.FIS
{
    public interface ICardBatchFileRecordCreateService
    {
        /// <summary>
        /// Initializes the service
        /// </summary>
        /// <param name="etlExecutionContext"></param>
        /// <returns>SubprogramId</returns>
        Task<string> Init(EtlExecutionContext etlExecutionContext);

        /// <summary>
        /// Generates FIS Card file 10 record type
        /// </summary>
        /// <param name="etlExecutionContext"></param>
        /// <returns></returns>
        string GenerateFileHeader(EtlExecutionContext etlExecutionContext);

        /// <summary>
        /// Generates FIS Card file 20 record type
        /// </summary>
        /// <param name="etlExecutionContext"></param>
        /// <param name="batchSequence">1 if just one batch per file</param>
        /// <returns></returns>
        string GenerateBatchHeader(EtlExecutionContext etlExecutionContext, long batchSequence, string? proxyIndicatorProcessing = null, string? generateClientUniqueID = null);

        /// <summary>
        /// Generates FIS Card file 80 record type
        /// </summary>
        /// <param name="etlExecutionContext"></param>
        /// <param name="batchSequence">1 if just one batch per file</param>
        /// <param name="totalRecords">total num of records in batch including batch header, trailer</param>
        /// <param name="totalDebit">total cash debit sent in this file (as 60 type records)</param
        /// <returns></returns>
        string GenerateBatchTrailer(EtlExecutionContext etlExecutionContext, long batchSequence,
             long totalRecords, double totalDebit, double totalCredit);

        /// <summary>
        /// Generates FIS Card file 90 record type
        /// </summary>
        /// <param name="etlExecutionContext"></param>
        /// <param name="totalRecords"></param>
        /// <param name="batchCount"></param>
        /// <param name="detailCount"></param>
        /// <param name="totalDebit"></param>
        /// <returns></returns>
        string GenerateFileTrailer(EtlExecutionContext etlExecutionContext,
            long totalRecords, long batchCount, long detailCount, double totalDebit, double totalCredit);

        /// <summary>
        /// Generates FIS Card file 30 record type
        /// </summary>
        /// <param name="etlExecutionContext"></param>
        /// <param name="skip">chunk looping</param>
        /// <param name="take">chunk looping</param>
        /// <returns></returns>
        Task<List<string>> GenerateCardHolderData(EtlExecutionContext etlExecutionContext, int skip, int take, TenantOption tenant);
        Task<List<string>> UpdateCardHolderData(EtlExecutionContext etlExecutionContext,int take);
    }
}
