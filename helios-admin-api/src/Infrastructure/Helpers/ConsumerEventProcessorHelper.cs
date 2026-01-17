using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos.Constants;
using SunnyRewards.Helios.Admin.Core.Domain.Models;
using SunnyRewards.Helios.Admin.Infrastructure.Helpers.Interface;
using SunnyRewards.Helios.Admin.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace SunnyRewards.Helios.Admin.Infrastructure.Helpers
{
    public class ConsumerEventProcessorHelper : IConsumerEventProcessorHelper
    {
        private readonly ILogger<ConsumerEventProcessorHelper> _logger;
        private readonly IEventHandlerScriptRepo _eventHandlerScriptRepo;
        private readonly IEventHandlerResultRepo _eventHandlerResultRepo;
        private readonly IHeliosScriptEngine _heliosScriptEngine;
        private readonly IScriptRepo _scriptRepo;
        const string className = nameof(Services.EventProcessorHelper);

        public ConsumerEventProcessorHelper(ILogger<ConsumerEventProcessorHelper> logger,
            IEventHandlerScriptRepo eventHandlerScriptRepo,
            IEventHandlerResultRepo eventHandlerResultRepo,
            IScriptRepo scriptRepo,
            IHeliosScriptEngine heliosScriptEngine)
        {
            _logger = logger;
            _eventHandlerScriptRepo = eventHandlerScriptRepo;
            _eventHandlerResultRepo = eventHandlerResultRepo;
            _scriptRepo = scriptRepo;
            _heliosScriptEngine = heliosScriptEngine;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventRequest"></param>
        /// <param name="argInstances"></param>
        /// <param name="eventHandlerName"></param>
        /// <returns></returns>
        /// <exception cref="InvalidDataException"></exception>
        public async Task<bool> ProcessEventAsync<T>(EventDto<T> eventRequest, Dictionary<string, object> argInstances, string eventHandlerName)
        {
            const string methodName = nameof(ProcessEventAsync);
            _logger.LogInformation("{ClassName}.{MethodName} : Started processing ConsumerCode:{ConsumeCode},TenantCode:{TenantCode}",
                       className, methodName, eventRequest.Header.ConsumerCode, eventRequest.Header.TenantCode);

            var eventScript = await GetEventHandlerScript(eventRequest);

            if (eventScript == null)
            {
                throw new InvalidDataException("Event Script not Found");
            }

            var context = new ScriptContext { ScriptLanguage = Constant.ScriptLanguage };
            var scriptExecutionResult = new ScriptExecutionResultDto();
            ScriptArgumentContext? argumentContext;

            try
            {
                argumentContext = CreatsScriptArgumentContext(eventScript.Script, argInstances);
            }
            catch (Exception ex)
            {
                argumentContext = null;
                var msg = $"Failed to create script Argument for script {eventScript.Script.ScriptId}, while processing event {eventRequest.Header.EventId}, Error Msg : {ex.Message}";

                _logger.LogError(ex,msg);
                scriptExecutionResult.ErrorMessage = msg;
                scriptExecutionResult.ResultCode = StatusCodes.Status500InternalServerError;
                await SaveEventResult(eventRequest, eventScript.EventHandlerScript, scriptExecutionResult, eventHandlerName);
                throw;
            }

            if (argumentContext != null)
            {
                try
                {
                    _logger.LogInformation("{ClassName}.{MethodName} : Started Executing script ConsumerCode:{ConsumeCode},TenantCode:{TenantCode}",
                        className, methodName, eventRequest.Header.ConsumerCode, eventRequest.Header.TenantCode);
                    scriptExecutionResult = _heliosScriptEngine.ExecuteScript(context, argumentContext, eventScript.Script.ScriptSource!);
                    await SaveEventResult(eventRequest, eventScript.EventHandlerScript, scriptExecutionResult, eventHandlerName);

                    if (scriptExecutionResult != null && scriptExecutionResult.ResultCode != 0)
                    {
                        // API did not return 200, move event to dead letter queue
                        throw new InvalidDataException();
                    }
                    _logger.LogInformation("{ClassName}.{MethodName} : Successfully Executed script ConsumerCode:{ConsumeCode},TenantCode:{TenantCode}",
                        className, methodName, eventRequest.Header.ConsumerCode, eventRequest.Header.TenantCode);
                    return true;
                }
                catch (InvalidDataException)
                {
                    throw;  // Error from API, move event to dead letter queue
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,"{ClassName}.{MethodName}:Error in script execution: {Msg}", className,methodName,ex.Message);
                    scriptExecutionResult.ErrorMessage = ex.Message;
                    scriptExecutionResult.ResultCode = StatusCodes.Status500InternalServerError;
                    await SaveEventResult(eventRequest, eventScript.EventHandlerScript, scriptExecutionResult, eventHandlerName);
                    throw;
                }
            }

            return false;
        }

        private async Task<EventHandlerScripts?> GetEventHandlerScript<T>(EventDto<T> eventRequest)
        {
            var eventHandlerScript = await _eventHandlerScriptRepo.FindOneAsync(x => x.EventType == eventRequest.Header.EventType
             && x.EventSubType == eventRequest.Header.EventSubtype && x.TenantCode == eventRequest.Header.TenantCode && x.DeleteNbr == 0);

            if (eventHandlerScript != null)
            {
                var scriptModel = await _scriptRepo.FindOneAsync(x => x.ScriptId == eventHandlerScript.ScriptId && x.DeleteNbr == 0);

                if (scriptModel != null)
                {

                    return new EventHandlerScripts(eventHandlerScript, scriptModel);
                }
                return null;

            }
            return null;
        }
        private async Task<bool> SaveEventResult<T>(EventDto<T> eventRequest, EventHandlerScriptModel eventScript, ScriptExecutionResultDto scriptExecutionResult, string EventHandlerName)
        {
            const string methodName = nameof(SaveEventResult);
            var eventHandlerResultModel = new EventHandlerResultModel()
            {
                EventCode = eventRequest.Header.EventId,
                EventHandlerScriptId = eventScript.EventHandlerId,
                EventHandlerName = EventHandlerName,
                EventData = JsonSerializer.Serialize(eventRequest.Data),
                ResultStatus = scriptExecutionResult.ResultCode.ToString(),
                ResultDescriptionJson = scriptExecutionResult.ToJson(),
                CreateUser = Constant.CreateUser,
                CreateTs = DateTime.UtcNow,
                DeleteNbr = 0
            };

            try
            {
                await _eventHandlerResultRepo.CreateAsync(eventHandlerResultModel);
                _logger.LogInformation("{ClassName}.{MethodName}: Script Result saved successfully for EventCode {EventCode}", 
                    className,methodName,eventRequest.Header.EventId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"{ClassName}.{MethodName}:Script Result not saved  for EventCode {EventCode}, Error : {Error}",
                    className,methodName, eventRequest.Header.EventId, ex.Message);
                return false;
            }
        }
        private ScriptArgumentContext CreatsScriptArgumentContext(ScriptModel script, Dictionary<string, object> argInstances)
        {
            var methodName = nameof(CreatsScriptArgumentContext);
            var argumentContext = new ScriptArgumentContext();
            var scriptJson = JsonConvert.DeserializeObject<ScriptJsonDto>(script.ScriptJson ?? "");
            if (scriptJson != null)
            {
                foreach (var argument in scriptJson.Args)
                {
                    if (argInstances.Count > 0)
                    {
                        if (argInstances[CapitalizeFirstCharacter(argument.ArgName)] != null)
                            argumentContext.ArgumentMap.Add(argument.ArgName, argInstances[CapitalizeFirstCharacter(argument.ArgName)]);
                        else
                            _logger.LogError("{ClassName}.{MethodName}: Error setting Argument for Script,Argument instance not found  for Argument Name: {Argname} ", 
                                className, methodName, argument.ArgName);
                    }

                }

            }
            else
                _logger.LogError("{ClassName}.{MethodName}: scriptJson not found  for script  {Script} ", className, methodName, script);

            return argumentContext;
        }
        private static string CapitalizeFirstCharacter(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            var value = char.ToUpper(input[0]) + input.Substring(1);
            return value;
        }
        
    }
}
