using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos.Constants;
using SunnyRewards.Helios.Admin.Core.Domain.Models;
using SunnyRewards.Helios.Admin.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services
{
    public class CohortConsumerTaskService : ICohortConsumerTaskService
    {


        private readonly ILogger<CohortConsumerTaskService> _consumerTaskServiceLogger;

        private readonly ITenantTaskRewardScriptRepo _tenantTaskRewardScriptRepo;
        private readonly IHeliosScriptEngine _heliosScriptEngine;
        private readonly IScriptRepo _scriptRepo;
        private readonly ITaskRewardScriptResultRepo _taskRewardSriptResultRepo;
        private readonly ICohortConsumerService _cohortConsumerService;
        private readonly ITaskService _taskService;
        private readonly Random _random = new Random();
        private readonly IConsumerLoginService _consumerLoginService;
        private readonly NHibernate.ISession _session;
        private static Dictionary<string, object> argInstances = new Dictionary<string, object>();

        const string className = nameof(CohortConsumerTaskService);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="consumerTaskServiceLogger"></param>
        /// <param name="walletClient"></param>
        /// <param name="taskClient"></param>
        /// <param name="tenantClient"></param>
        /// <param name="userClient"></param>
        /// <param name="config"></param>
        public CohortConsumerTaskService(
            ILogger<CohortConsumerTaskService> consumerTaskServiceLogger,
           ITenantTaskRewardScriptRepo tenantTaskRewardScriptRepo, IHeliosScriptEngine heliosScriptEngine
            , IScriptRepo scriptRepo, ITaskRewardScriptResultRepo taskRewardSriptResultRepo, 
           ITaskService taskService, ICohortConsumerService cohortConsumerService, NHibernate.ISession session, IConsumerLoginService consumerLoginService)
        {
            _consumerTaskServiceLogger = consumerTaskServiceLogger;
            _tenantTaskRewardScriptRepo = tenantTaskRewardScriptRepo;
            _heliosScriptEngine = heliosScriptEngine;
            _scriptRepo = scriptRepo;
            _taskRewardSriptResultRepo = taskRewardSriptResultRepo;
            _cohortConsumerService = cohortConsumerService;
            _taskService = taskService;
            _session = session;
            _consumerLoginService = consumerLoginService;
            argInstances[nameof(CohortConsumerService)] = _cohortConsumerService;
            argInstances[nameof(TaskService)] = _taskService;
            argInstances[nameof(ConsumerLoginService)] = _consumerLoginService;
        }

        public async Task<bool> TaskCompletionPrePostScriptCheck(FindConsumerTasksByIdResponseDto consumerTaskUpdateRequestDto,
            ConsumerDto consumer, string scriptType, PersonDto? person = null)
        {
            var methodName = nameof(TaskCompletionPrePostScriptCheck);
            // the flag is only used for TASK_COMPLETE_PRE script and  if
            // script execution return code != 0 then do not continue with the rest of the steps of task completion logic
            var taskUpdateRequestDto = consumerTaskUpdateRequestDto?.TaskRewardDetail?.TaskReward;
            argInstances[nameof(ConsumerDto)] = consumer; // Use the string as a key
            argInstances[nameof(TaskRewardDetailDto)] = consumerTaskUpdateRequestDto?.TaskRewardDetail;
            if (person != null)
            {
                argInstances[nameof(PersonDto)] = person; // Use the string as a key
            }
            string consumerCode = consumer.ConsumerCode;
            bool isPreScriptCheckComplete = false;
            try
            {
                if (taskUpdateRequestDto != null || taskUpdateRequestDto?.TenantCode != null || argInstances.Count > 0)
                {

                    var tenantTaskReward = await _tenantTaskRewardScriptRepo.FindOneAsync(x =>
                        x.TenantCode == taskUpdateRequestDto.TenantCode &&
                        x.TaskRewardCode == taskUpdateRequestDto.TaskRewardCode && x.ScriptType != null &&
                        x.ScriptType.Trim().ToLower() == scriptType.Trim().ToLower() && x.DeleteNbr == 0);
                    if (tenantTaskReward != null)
                    {
                        var script = await _scriptRepo.FindOneAsync(x =>
                          x.ScriptId == tenantTaskReward.ScriptId && x.DeleteNbr == 0);
                        if (script == null || String.IsNullOrEmpty(script.ScriptJson) || String.IsNullOrEmpty(script.ScriptSource))
                        {
                            _consumerTaskServiceLogger.LogError("{className}.{methodName}:Script not found for Tenant Task Reward  data {rewarddata} ", className, methodName, tenantTaskReward.ToJson());
                            return isPreScriptCheckComplete;
                        }
                        var context = new ScriptContext { ScriptLanguage = Constant.ScriptLanguage };
                        var argumentContext = CreatsScriptArgumentContext(script);
                        if (argumentContext != null)
                        {
                            var result = _heliosScriptEngine.ExecuteScript(context, argumentContext, script.ScriptSource);
                            if (result != null)
                            {

                                var taskRewardScriptResultModel = new TaskRewardScriptResultModel();
                                taskRewardScriptResultModel.TenantTaskRewardScriptId = tenantTaskReward.ScriptId;
                                taskRewardScriptResultModel.ConsumerCode = consumerCode;
                                taskRewardScriptResultModel.ExecutionContextJson = context.ToJson();
                                taskRewardScriptResultModel.ExecutionResultJson = result.ToJson();
                                taskRewardScriptResultModel.CreateUser = Constant.CreateUser;
                                taskRewardScriptResultModel.CreateTs = DateTime.Now;
                                await _session.SaveAsync(taskRewardScriptResultModel);
                                if (result.ResultCode != 0)
                                {
                                    _consumerTaskServiceLogger.LogError("{className}.{methodName}:Script Execution Result  was not not successful for scriptType {scriptType}", className, methodName, scriptType);
                                    return isPreScriptCheckComplete;
                                }
                            }
                            else
                            {
                                _consumerTaskServiceLogger.LogError("{className}.{methodName}: Script Execution Result  not found", className, methodName);
                                return isPreScriptCheckComplete;

                            }

                        }
                        else
                        {
                            _consumerTaskServiceLogger.LogError("{className}.{methodName}: argumentContext not found for Script {script} ", className, methodName, script.ToJson());
                            return isPreScriptCheckComplete;

                        }
                        return !isPreScriptCheckComplete;
                    }
                    //returning true means script not exists so can proceed with regular flow.
                    return !isPreScriptCheckComplete;
                }
                _consumerTaskServiceLogger.LogError("{className}.{methodName}task Update Request Dto not found {scriptType}", className, methodName, scriptType);
                return isPreScriptCheckComplete;

            }
            catch (Exception ex)
            {

                _consumerTaskServiceLogger.LogError("{className}.{methodName}:An Error occured {error} ", className, methodName, ex.Message);
                return false;
            }
        }

        private string CapitalizeFirstCharacter(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            var value = char.ToUpper(input[0]) + input.Substring(1);
            return value;
        }
        private ScriptArgumentContext CreatsScriptArgumentContext(ScriptModel script)
        {
            var methodName = nameof(CreatsScriptArgumentContext);
            var argumentContext = new ScriptArgumentContext();
            var scriptJson = JsonConvert.DeserializeObject<ScriptJsonDto>(script.ScriptJson);
            if (scriptJson != null)
            {
                foreach (var argument in scriptJson.Args)
                {
                    if (argInstances.Count > 0)
                    {
                        if (argInstances[CapitalizeFirstCharacter(argument.ArgName)] != null)
                            argumentContext.ArgumentMap.Add(argument.ArgName, argInstances[CapitalizeFirstCharacter(argument.ArgName)]);
                        else
                            _consumerTaskServiceLogger.LogError("{className}.{methodName}: Class instance not found  for class Name  {className} ", className, methodName, argument.ArgName);
                    }

                }

            }
            else
                _consumerTaskServiceLogger.LogError("{className}.{methodName}: scriptJson not found  for script  {script} ", className, methodName, script);

            return argumentContext;
        }
    }
}
