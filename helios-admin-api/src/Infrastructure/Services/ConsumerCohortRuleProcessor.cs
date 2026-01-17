using Microsoft.Extensions.Logging;
using RulesEngine.Models;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Core.Domain.Models;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using System.Data;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Rule = RulesEngine.Models.Rule;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services
{

    public class ConsumerCohortRuleProcessor : IConsumerCohortRuleProcessor
    {
        private readonly ILogger<IConsumerCohortRuleProcessor> _logger;
        private readonly IConsumerPurseCohortAssignmentService _consumerPurseCohortAssignmentService;
        const string className = nameof(ConsumerCohortRuleProcessor);
        public ConsumerCohortRuleProcessor(
            ILogger<IConsumerCohortRuleProcessor> logger, IConsumerPurseCohortAssignmentService consumerPurseCohortAssignmentService)
        {
            _logger = logger;
            _consumerPurseCohortAssignmentService = consumerPurseCohortAssignmentService;
        }

        public async Task<CohortRuleExecutionDto> EvaluateRule(string cohortName, CohortRuleArrayJson cohortRule, CohortRuleInput input)
        {

            var validationMessages = new StringBuilder();
            if (cohortRule?.RuleExpr == null || !cohortRule.RuleExpr.Any())
            {
                validationMessages.AppendLine($"Cohort '{cohortName}' has no rules defined.");
                _logger.LogWarning(validationMessages.ToString());
                return new CohortRuleExecutionDto { CohortName = cohortName, RulesExecutionResult = false };
            }

            
            var workflowName = $"{cohortName}_CohortRuleWorkflow";

            var rules = cohortRule.RuleExpr
                .Select((expr, index) => new Rule
                {
                    RuleName = $"{cohortName}_Rule_{index + 1}",
                    Expression = NormalizeIdentifiers(expr),
                    SuccessEvent = cohortRule.SuccessExpr,
                }).ToList();

            var workflow = new Workflow
            {
                WorkflowName = workflowName,
                Rules = rules
            };

            var settings = new ReSettings
            {
                EnableExceptionAsErrorMessageForRuleExpressionParsing = true,
                IgnoreException = false,                
                EnableExceptionAsErrorMessage = true,  
                EnableFormattedErrorMessage = true,
                CustomTypes = new[] { typeof(string) },  // Allow to use String fn like ToUpper/ToLower or !string.IsNullOrEmpty(person.LanguageCode)
            };

            ParsingConfig.Default.IsCaseSensitive = false;

            var rulesEngine = new RulesEngine.RulesEngine(new[] { workflow }, settings);

            var person = new RuleParameter("person", input.Person);
            var consumer = new RuleParameter("consumer", input.Consumer);
            var consumerPurseCohortAssignmentService = new RuleParameter("_consumerPurseCohortAssignmentService", _consumerPurseCohortAssignmentService);

            var results = await rulesEngine.ExecuteAllRulesAsync(workflowName, person, consumer, consumerPurseCohortAssignmentService);

            var passedRules = results.Where(r => r.IsSuccess).ToList();
            var failedRules = results.Where(r => !r.IsSuccess).ToList();

            if (failedRules.Any())
            {
                validationMessages.AppendLine($"Rules that failed for cohort '{cohortName}':");
                foreach (var result in failedRules)
                {
                    if (!string.IsNullOrWhiteSpace(result.ExceptionMessage))
                    {
                        validationMessages.AppendLine($"{result.Rule.RuleName} | {result.Rule.Expression} | Error: {result.ExceptionMessage} , ");
                    }
                    else
                    {
                        validationMessages.AppendLine($"{result.Rule.RuleName} | {result.Rule.Expression} , ");
                    }
                }
            }

            if (passedRules.Any())
            {
                validationMessages.AppendLine($"Rules that passed for cohort '{cohortName}':");
                foreach (var passed in passedRules)
                {
                    validationMessages.AppendLine($"{passed.Rule.RuleName} | {passed.Rule.Expression} , ");
                }
            }
            _logger.LogInformation(validationMessages.ToString());
            return new CohortRuleExecutionDto() { CohortName = cohortName, RulesExecutionResult = passedRules.Any() };
        }
        public static string NormalizeIdentifiers(string expression)
        {
            var identifierMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    { "Person", "person" },
                    { "Consumer", "consumer" }
                };

            foreach (var kvp in identifierMap)
            {
                expression = Regex.Replace(expression, $@"\b{kvp.Key}\b", kvp.Value, RegexOptions.IgnoreCase);
            }

            return expression;
        }

    }
}
