using Npgsql;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces
{
    public interface IRedshiftDataReader
    {
        Task<List<RedShiftMemberImportFileDataDto>> FetchBatchAsync(string redshiftConnectionString, long? lastMemberImportFileId, int batchSize);
        Task<List<RedShiftMemberImportFileDataDto>> FetchAndClaimBatchAsync(string redshiftConnectionString, string partnerCode, string jobId, int batchSize);
        Task MarkPublishStatusAsync(string redshiftConnectionString, long rowId, string publishStatus);
        Task MarkPublishStatusBatchAsync(string redshiftConnectionString, IEnumerable<(long RowId, string PublishStatus)> updates);
        Task<List<RedShiftCohortDataDto>> FetchAndClaimCohortBatchAsync(string redshiftConnectionString, string partnerCode, string jobId, int batchSize);
        Task MarkCohortPublishStatusAsync(string redshiftConnectionString, long rowId, string publishStatus);
        Task MarkCohortPublishStatusBatchAsync(string redshiftConnectionString, IEnumerable<(long RowId, string PublishStatus)> updates);
    }
}
