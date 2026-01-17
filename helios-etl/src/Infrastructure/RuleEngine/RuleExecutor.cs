using Newtonsoft.Json;
using RulesEngine.Models;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using SunnyRewards.Helios.ETL.Infrastructure.RuleEngine.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Services;
using System.Dynamic;
using RE = RulesEngine.RulesEngine;

namespace SunnyRewards.Helios.ETL.Infrastructure.RuleEngine
{
    /// <summary>
    /// 
    /// </summary>
    public class RuleExecutor : IRuleExecutor
    {
        private List<Workflow> _workflows;
        private const string workFlowName = "HeliosCohortRuleSet";
        /// <summary>
        /// 
        /// </summary>
        public RuleExecutor()
        {
            _workflows = new List<Workflow>();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="workflow"></param>
        private void AddRuleWorkflow(string ruleJson)
        {
            var cohortRule = DeserializeCohortRule(ruleJson) ?? throw new InvalidOperationException("Cohort rule is not specified or invalid");
            _workflows = new List<Workflow>
            {
                new Workflow()
                {
                    WorkflowName = workFlowName,
                    Rules = new List<Rule>()
                    {
                        new Rule
                        {
                            RuleName = "CohortRule",
                            SuccessEvent = "10",
                            ErrorMessage = "One or more adjust rules failed.",
                            RuleExpressionType = RuleExpressionType.LambdaExpression,
                            Expression = $"{cohortRule.RuleExpr}",
                            Actions = new RuleActions
                            {
                                OnSuccess = new ActionInfo
                                {
                                    Name = "OutputExpression",
                                    Context = new Dictionary<string, object>
                                    {
                                        { "Expression", cohortRule.SuccessExpr }
                                    }
                                }
                            }
                        }
                       }
                    }
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private ReSettings GetReSettings()
        {
            return new ReSettings
            {
                CustomTypes = new Type[] {
                    typeof(ETLConsumerModel),
                    typeof(Util),
                    typeof(ETLPersonModel),
                    typeof(ConsumerQuery),
                    typeof(TaskCompletionCheckerService)
                }
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ruleJson"></param>
        /// <returns></returns>
        private CohortRule? DeserializeCohortRule(string ruleJson)
        {
            if (ruleJson == null)
                return null;

            return JsonConvert.DeserializeObject<CohortRule>(ruleJson);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ruleJson"></param>
        /// <param name="context"></param>
        public async Task<CohortRuleExecutionDto> Execute(string ruleJson, Dictionary<string, object> contextList)
        {
            AddRuleWorkflow(ruleJson);
            var inputs = AddVariables(contextList);
            var ruleEngine = new RE(_workflows.ToArray(), GetReSettings());

            var ruleResultSet = await ruleEngine.ExecuteAllRulesAsync(workFlowName, inputs);

            return new CohortRuleExecutionDto() { IsSuccess = ruleResultSet.All(x => x.IsSuccess), 
                CohortRuleSuccessResult = string.Join(",", ruleResultSet.Select(x => x.ActionResult?.Output)) };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contextList"></param>
        /// <returns></returns>
        private ExpandoObject AddVariables(Dictionary<string, object> contextList)
        {
            var expandoObj = new ExpandoObject();
            foreach (var context in contextList)
            {
                expandoObj.TryAdd(context.Key, context.Value);
            }
            expandoObj.TryAdd("util", new Util());
            return expandoObj;
        }
    }
}
