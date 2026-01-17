using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Core.Domain.Models;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces
{
    public interface IConsumerCohortRuleProcessor
    {
        Task<CohortRuleExecutionDto> EvaluateRule(string cohortName, CohortRuleArrayJson cohortRule, CohortRuleInput input);
    }
}
