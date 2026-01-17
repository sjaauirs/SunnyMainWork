using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Core.Domain.Models.DynamoDb;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces.DynamoDb
{
    public interface IJobHistoryService
    {
        Task<JobHistoryModel> GetJobHistoryById(string jobHistoryId);
        Task<JobHistoryModel> UpdateJobHistory(JobHistoryModel jobHistory);
        Task<JobHistoryModel> InsertJobHistory(JobHistoryModel jobHistory);
        Task<JobHistoryModel> UpdateJobDefinitionInJobHistory(JobHistoryModel jobHistory, string jobDefinitionId);
        Task<JobHistoryModel> GetJobHistoryCreateRequest(EtlExecutionContext etlExecutionContext);

    }
}
