namespace SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces
{
    public interface IAwsBatchService
    {
        /// <summary>
        /// Triggers the specified AWS Batch job with optional parameters.
        /// </summary>
        /// <param name="jobName">AWS batch job creates with this name</param>
        /// <param name="parameters">Parameters that are required to Trigger job</param>
        /// <returns></returns>
        Task<string> TriggerBatchJob(string jobName, Dictionary<string, string>? parameters = null);
        Task<string> TriggerProcessDepositInstrcutionsBatchJob(string jobName, Dictionary<string, string>? parameters = null);
    }
}
