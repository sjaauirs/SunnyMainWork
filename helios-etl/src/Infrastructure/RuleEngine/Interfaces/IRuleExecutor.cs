using SunnyRewards.Helios.ETL.Core.Domain.Dtos;

namespace SunnyRewards.Helios.ETL.Infrastructure.RuleEngine.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IRuleExecutor
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ruleJson"></param>
        /// <param name="contextList"></param>
        /// <returns></returns>
        Task<CohortRuleExecutionDto> Execute(string ruleJson, Dictionary<string, object> contextList);
    }
}
