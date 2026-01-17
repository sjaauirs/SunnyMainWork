namespace SunnyRewards.Helios.ETL.Infrastructure.RuleEngine.Interfaces
{
    public interface IConsumerQuery
    {
        /// <summary>
        /// 
        /// </summary>
        string? ConsumerCode { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cohortName"></param>
        /// <returns></returns>
        bool IsInCohort(object cohortName);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskExternalCode"></param>
        /// <returns></returns>
        bool HasCompletedTask(object taskExternalCode);
    }
}
