using Microsoft.AspNetCore.Http;
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

namespace SunnyRewards.Helios.Admin.Infrastructure.Services
{
    
    public class EventProcessorHelper : IEventProcessorHelper
    {
        private readonly ILogger<EventProcessorHelper> _logger;
        private readonly IEventHandlerScriptRepo _eventHandlerScriptRepo;
        private readonly IEventHandlerResultRepo _eventHandlerResultRepo;
        private readonly IHeliosScriptEngine _heliosScriptEngine;
        private readonly IScriptRepo _scriptRepo;
        const string className = nameof(EventProcessorHelper);

        public EventProcessorHelper(ILogger<EventProcessorHelper> logger,
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

        public async Task<bool> ProcessEventAsync(PostEventRequestModel eventRequest, Dictionary<string, object> argInstances , string eventHandlerName)
        {
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
                var msg = $"Failed to create script Argument for script {eventScript.Script.ScriptId}, while processing event {eventRequest.EventCode}, Error Msg : {ex.Message}";

                _logger.LogError(msg);
                scriptExecutionResult.ErrorMessage = msg;
                scriptExecutionResult.ResultCode = StatusCodes.Status500InternalServerError;
                await SaveEventResult(eventRequest, eventScript.EventHandlerScript, scriptExecutionResult , eventHandlerName);
                throw;
            }

            if (argumentContext != null)
            {
                try
                {
                    scriptExecutionResult = _heliosScriptEngine.ExecuteScript(context, argumentContext, eventScript.Script.ScriptSource!);
                    await SaveEventResult(eventRequest, eventScript.EventHandlerScript, scriptExecutionResult , eventHandlerName);

                    if (scriptExecutionResult != null && scriptExecutionResult.ResultCode != 0)
                    {
                        // API did not return 200, move event to dead letter queue
                        throw new InvalidDataException();
                    }
                    return true;
                }
                catch (InvalidDataException)
                {
                    throw;  // Error from API, move event to dead letter queue
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error in script execution: {msg}", ex.Message);
                    scriptExecutionResult.ErrorMessage = ex.Message;
                    scriptExecutionResult.ResultCode = StatusCodes.Status500InternalServerError;
                    await SaveEventResult(eventRequest, eventScript.EventHandlerScript, scriptExecutionResult, eventHandlerName);
                    throw;
                }
            }

            return false;
        }

        private async Task<EventHandlerScripts?> GetEventHandlerScript(PostEventRequestModel eventRequest)
        {

            var eventHandlerScript =  await _eventHandlerScriptRepo.FindOneAsync(x => x.EventType == eventRequest.EventType
             && x.EventSubType == eventRequest.EventSubtype && x.TenantCode == eventRequest.TenantCode && x.DeleteNbr == 0);

            if (eventHandlerScript != null)
            {
                var scriptModel =  await _scriptRepo.FindOneAsync(x => x.ScriptId == eventHandlerScript.ScriptId && x.DeleteNbr == 0);

                if (scriptModel != null)
                {

                    return new EventHandlerScripts(eventHandlerScript , scriptModel);
                }
                return null;

            }
            return null;
        }
        private async Task<bool> SaveEventResult(PostEventRequestModel eventRequest, EventHandlerScriptModel eventScript, ScriptExecutionResultDto scriptExecutionResult, string EventHandlerName)
        {
            var eventHandlerResultModel = new EventHandlerResultModel()
            {
                EventCode = eventRequest.EventCode,
                EventHandlerScriptId = eventScript.EventHandlerId,
                EventHandlerName = EventHandlerName,
                EventData = eventRequest.EventData,
                ResultStatus = scriptExecutionResult.ResultCode.ToString(),
                ResultDescriptionJson = scriptExecutionResult.ToJson(),
                CreateUser = Constant.CreateUser,
                CreateTs = DateTime.UtcNow,
                DeleteNbr = 0
            };

            try
            {
                await _eventHandlerResultRepo.CreateAsync(eventHandlerResultModel);
                _logger.LogInformation("Script Result saved successfully for EventCode {EventCode}", eventRequest.EventCode);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("Script Result not saved  for EventCode {EventCode}, Error : {Error}", eventRequest.EventCode, ex.Message);
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
                            _logger.LogError("{className}.{methodName}: Error setting Argument for Script,Argument instance not found  for Argument Name: {Argname} ", className, methodName, argument.ArgName);
                    }

                }

            }
            else
                _logger.LogError("{className}.{methodName}: scriptJson not found  for script  {script} ", className, methodName, script);

            return argumentContext;
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
    }
}
