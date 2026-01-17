using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos.Json;
using SunnyRewards.Helios.ETL.Core.Domain.Models;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces
{
    public interface IJobReportService
    {
       void CollectError(int recordNbr ,int errorCode , string? Message, Exception? ex);

        Task<bool> SaveEtlErrors(string? filePath = "");
        JobResultDetails JobResultDetails { get; }
        BatchJobRecordsDto BatchJobRecords { get; }
        Dictionary<string, RecordError> keyRecordErrorMap {  get; }
        EtlExecutionContext SetJobHistoryStatus(EtlExecutionContext etlExecutionContext);
    }
}
