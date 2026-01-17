using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NHibernate.Cache;
using SunnyRewards.Helios.Admin.Core.Domain.Constants;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Core.Domain.Models;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Constants;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Tenant.Core.Domain.Models;
using System.Text;
using System.Threading.Tasks;
using Constant = SunnyRewards.Helios.Admin.Core.Domain.Constants.Constant;
using ImportTaskRewardDto = SunnyRewards.Helios.Task.Core.Domain.Dtos.ImportTaskRewardDto;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services
{
    public class AdminService : IAdminService
    {
        private readonly ILogger<AdminService> _logger;
        private readonly IEventHandlerScriptRepo _eventHandlerScriptRepo;
        private readonly ITenantTaskRewardScriptRepo _tenantTaskRewardScriptRepo;
        private readonly IScriptRepo _scriptRepo;
        private readonly IMapper _mapper;
        private readonly Dictionary<string, long> _scriptLookUp;
        private readonly IUserContextService _userContextService;
        private const string className = nameof(AdminService);

        public AdminService(ILogger<AdminService> logger, IEventHandlerScriptRepo eventHandlerScriptRepo,
            ITenantTaskRewardScriptRepo tenantTaskRewardScriptRepo, IMapper mapper, IScriptRepo scriptRepo, IUserContextService userContextService)
        {
            _logger = logger;
            _eventHandlerScriptRepo = eventHandlerScriptRepo;
            _tenantTaskRewardScriptRepo = tenantTaskRewardScriptRepo;
            _mapper = mapper;
            _scriptRepo = scriptRepo;
            _scriptLookUp = new Dictionary<string, long>();
            _userContextService = userContextService;
        }

        /// <summary>
        /// Exports Admin data for a given tenant.
        /// </summary>
        /// <param name="tenantCode">The tenant code.</param>
        /// <returns>Admin export response DTO containing event handler scripts, tenant task-reward scripts, and scripts.</returns>
        public ExportAdminResponseDto GetAdminScripts(string tenantCode)
        {
            const string methodName = nameof(GetAdminScripts);
            _logger.LogInformation("{ClassName}.{MethodName}: Started export for TenantCode: {TenantCode}", className, methodName, tenantCode);

            try
            {
                // Retrieve event handler and task-reward script responses
                var eventHandlerScripts = _eventHandlerScriptRepo.GetEventHandlerScripts(tenantCode);
                var taskRewardScripts = _tenantTaskRewardScriptRepo.GetTenantTaskRewardScripts(tenantCode);

                if (eventHandlerScripts.Count == 0 && taskRewardScripts.Count == 0)
                {
                    _logger.LogWarning("{ClassName}.{MethodName}: No scripts found for TenantCode: {TenantCode}", className, methodName, tenantCode);
                    return new ExportAdminResponseDto
                    {
                        ErrorCode = StatusCodes.Status404NotFound,
                        ErrorMessage = $"No event handler or task-reward scripts found for TenantCode: {tenantCode}"
                    };
                }

                // Extract and combine script entities
                var allScripts = eventHandlerScripts.Select(e => e.Script)
                                                      .Concat(taskRewardScripts.Select(t => t.Script))
                                                      .Distinct()
                                                      .ToList();

                _logger.LogInformation("{ClassName}.{MethodName}: Successfully retrieved and mapped data for TenantCode: {TenantCode}", className, methodName, tenantCode);

                return new ExportAdminResponseDto
                {
                    EventHandlerScripts = _mapper.Map<List<EventHandlerScriptDto>>(eventHandlerScripts.Select(e => e.EventHandlerScript)),
                    TenantTaskRewardScripts = _mapper.Map<List<TenantTaskRewardScriptDto>>(taskRewardScripts.Select(t => t.TenantTaskRewardScript)),
                    Scripts = _mapper.Map<List<ScriptDto>>(allScripts)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: An error occurred while exporting data for TenantCode: {TenantCode}", className, methodName, tenantCode);
                return new ExportAdminResponseDto
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = ex.Message
                };
            }
        }
        /// <summary>
        /// Creates or updates admin-related scripts including Script, TenantTaskRewardScript, and TenantEventHandlerScript entities.
        /// </summary>
        /// <param name="importAdminRequest">The request DTO containing admin script data to be imported.</param>
        /// <param name="taskrewards">
        /// An optional dictionary mapping task identifiers to reward identifiers, 
        /// used for associating scripts with specific task rewards.
        /// </param>
        /// <returns>
        /// A <see cref="BaseResponseDto"/> indicating the result of the operation:
        /// - Success if all scripts are processed correctly.
        /// - Partial content (206) if some scripts failed to process.
        /// - Internal server error (500) if an unhandled exception occurs.
        /// </returns>
        public async Task<BaseResponseDto> CreateAdminScripts(ImportAdminRequestDto importAdminRequest, Dictionary<string, string> taskrewards = null)
        {
            try
            {
                var errorMessage = new StringBuilder();
                await CreateOrUpdateScript(importAdminRequest, errorMessage);
                await CreateOrUpdateTenantTaskRewardScript(importAdminRequest, errorMessage,taskrewards);
                await CreateOrUpdateTenantEventHandlerScript(importAdminRequest,errorMessage);

                if (errorMessage.Length > 0)
                {
                    return new BaseResponseDto
                    {
                        ErrorCode = StatusCodes.Status206PartialContent,
                        ErrorMessage = errorMessage.ToString()
                    };
                }
                return new BaseResponseDto();
            }
            catch (Exception ex)
            {
                return new BaseResponseDto
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = ex.Message
                };
            }
        }
        /// <summary>
        /// Creates or updates TenantTaskRewardScript entries based on the provided import request.
        /// </summary>
        /// <param name="importAdminRequest"></param>
        /// <param name="scriptIds"></param>
        /// <param name="errorMessage"></param>
        /// <param name="taskRewardCodes"></param>
        /// <returns></returns>
        private async System.Threading.Tasks.Task CreateOrUpdateTenantTaskRewardScript(ImportAdminRequestDto importAdminRequest,StringBuilder errorMessages,
              Dictionary<string, string> taskRewardCodes)
        {
            const string methodName = nameof(CreateOrUpdateTenantTaskRewardScript);
            var tenantCode = importAdminRequest.TenantCode;
            string errorMessage = "";
            _logger.LogInformation("{ClassName}.{MethodName} : Started processing with TenantCode:{TenantCode}", className, methodName, tenantCode);

            foreach (var scriptDto in importAdminRequest.TenantTaskRewardScripts)
            {
                try
                {
                    if (!taskRewardCodes.TryGetValue(scriptDto.TaskRewardCode, out var newTaskRewardCode))
                    {
                        errorMessage = $"Invalid taskRewardCode: '{scriptDto.TaskRewardCode} with TenantCode:{tenantCode}',Skipping record.";
                        _logger.LogError("{ClassName}.{MethodName} : Error:{Error}", className, methodName, errorMessage);
                        errorMessages.AppendLine(errorMessage + " ");
                        continue;
                    }

                    var scriptId = await GetScriptId(importAdminRequest,errorMessage, scriptDto.ScriptId);

                    if (errorMessage != null && scriptId <= 0)
                    {
                        errorMessages.AppendLine(errorMessage + " ");
                        continue;
                    }

                    var taskRewardScriptModel = await _tenantTaskRewardScriptRepo.FindOneAsync(x =>
                        x.TenantCode == tenantCode &&
                        x.TenantTaskRewardScriptCode == scriptDto.TenantTaskRewardScriptCode &&
                        x.ScriptId == scriptDto.ScriptId &&
                        x.DeleteNbr == 0);

                    if (taskRewardScriptModel != null)
                    {
                        taskRewardScriptModel.TaskRewardCode = newTaskRewardCode;
                        taskRewardScriptModel.ScriptId = scriptId;
                        taskRewardScriptModel.ScriptType = scriptDto.ScriptType;
                        taskRewardScriptModel.UpdateTs = DateTime.UtcNow;
                        taskRewardScriptModel.UpdateUser = _userContextService.GetUpdateUser();
                        await _tenantTaskRewardScriptRepo.UpdateAsync(taskRewardScriptModel);

                        _logger.LogInformation("{ClassName}.{MethodName} : Taskrewardscript updated sucessfully with TenantCode:{TenantCode}", className, methodName, tenantCode);
                        continue;
                    }

                    taskRewardScriptModel = _mapper.Map<TenantTaskRewardScriptModel>(scriptDto);
                    taskRewardScriptModel.TenantTaskRewardScriptId = 0;
                    taskRewardScriptModel.ScriptId = scriptId;
                    taskRewardScriptModel.CreateTs = DateTime.UtcNow;
                    taskRewardScriptModel.CreateUser = Constant.ImportUser;
                    taskRewardScriptModel.TaskRewardCode = newTaskRewardCode;
                    taskRewardScriptModel.TenantCode = tenantCode;

                    await _tenantTaskRewardScriptRepo.CreateAsync(taskRewardScriptModel);

                    _logger.LogInformation("{ClassName}.{MethodName} : Taskrewardscript created sucessfully with TenantCode:{TenantCode}", className, methodName, tenantCode);
                }
                catch (Exception ex)
                {
                    errorMessage = ex.Message;
                    _logger.LogError("{ClassName}.{MethodName} : Error:{Error}", className, methodName, errorMessage);
                    errorMessages.AppendLine($"ScriptCode '{scriptDto.TenantTaskRewardScriptCode}': {ex.Message}");
                    continue;
                }
            }
        }

        private async Task<long> GetScriptId(ImportAdminRequestDto importAdminRequest, string errorMessage, long scriptId)
        {
            var script = importAdminRequest.Scripts.FirstOrDefault(x => x.ScriptId == scriptId && x.DeleteNbr == 0);
            if (script == null)
            {
                errorMessage = $"Invalid scriptId '{scriptId}'. Skipping record.";
                return 0;
            }
            if (!_scriptLookUp.TryGetValue(script!.ScriptCode ?? string.Empty, out var newScriptId))
            {
                var scriptModel = await _scriptRepo.FindOneAsync(x =>
                    x.ScriptCode == script.ScriptCode &&
                    x.DeleteNbr == 0);

                if (scriptModel == null)
                {
                    errorMessage = $"Invalid scriptcode {script.ScriptCode}'. Skipping record.";
                    return 0;
                }
            }
            return newScriptId;
    }

        /// <summary>
        /// Creates or updates Tenant event handler entries based on the provided import request.
        /// </summary>
        /// <param name="importAdminRequest"></param>
        /// <param name="scriptIds"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        private async System.Threading.Tasks.Task CreateOrUpdateTenantEventHandlerScript(ImportAdminRequestDto importAdminRequest,
           StringBuilder errorMessages)
        {
            const string methodName = nameof(CreateOrUpdateTenantEventHandlerScript);
            var tenantCode = importAdminRequest.TenantCode;
            var errorMessage = "";

            _logger.LogInformation("{ClassName}.{MethodName} : Started processing with TenantCode:{TenantCode}", className, methodName, tenantCode);

            foreach (var scriptDto in importAdminRequest.EventHandlerScripts)
            {
                try
                {
                    var scriptId = await GetScriptId(importAdminRequest, errorMessage, scriptDto.ScriptId);
                    if (errorMessage != null && scriptId <= 0)
                    {
                        errorMessages.AppendLine(errorMessage + " ");
                        continue;
                    }
                    var eventHandlerScript = await _eventHandlerScriptRepo.FindOneAsync(x =>
                        x.TenantCode == tenantCode &&
                        x.ScriptId == scriptDto.ScriptId &&
                        x.DeleteNbr == 0);

                    if (eventHandlerScript != null)
                    {
                        eventHandlerScript.EventType = scriptDto.EventType;
                        eventHandlerScript.EventSubType = scriptDto.EventSubType;
                        eventHandlerScript.UpdateTs = DateTime.UtcNow;
                        eventHandlerScript.UpdateUser = _userContextService.GetUpdateUser();
                        eventHandlerScript.ScriptId = scriptId;
                        await _eventHandlerScriptRepo.UpdateAsync(eventHandlerScript);

                        _logger.LogInformation("{ClassName}.{MethodName} : EventHandlerScript updated successfully for ScriptId:{ScriptId} with TenantCode:{TenantCode}",
                            className, methodName, scriptDto.ScriptId, tenantCode);
                        continue;
                    }

                    var newHandlerScript = _mapper.Map<EventHandlerScriptModel>(scriptDto);
                    newHandlerScript.EventHandlerId = 0;
                    newHandlerScript.EventHandlerCode = $"evh-{Guid.NewGuid():N}";
                    newHandlerScript.CreateTs = DateTime.UtcNow;
                    newHandlerScript.CreateUser = Constant.ImportUser;
                    newHandlerScript.TenantCode = tenantCode;
                    newHandlerScript.ScriptId = scriptId;

                    await _eventHandlerScriptRepo.CreateAsync(newHandlerScript);

                    _logger.LogInformation("{ClassName}.{MethodName} : EventHandlerScript created successfully for ScriptId:{ScriptId} with TenantCode:{TenantCode}", className, methodName, scriptDto.ScriptId, tenantCode);
                }
                catch (Exception ex)
                {
                    var error = $"ScriptId '{scriptDto.ScriptId}': {ex.Message}";
                    errorMessages.AppendLine(error);
                    _logger.LogError("{ClassName}.{MethodName} : Exception occurred - {Error}", className, methodName, error);
                    continue ;
                }
            }
        }

        /// <summary>
        /// Creates or updates script entries based on the provided import request.
        /// </summary>
        /// <param name="importAdminRequest"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        private async System.Threading.Tasks.Task CreateOrUpdateScript(ImportAdminRequestDto importAdminRequest,StringBuilder errorMessages)
        {
            const string methodName = nameof(CreateOrUpdateScript);
            var tenantCode = importAdminRequest.TenantCode;

            _logger.LogInformation("{ClassName}.{MethodName} : Started script creation/updation for TenantCode: {TenantCode}", className, methodName, tenantCode);

            foreach (var scriptDto in importAdminRequest.Scripts)
            {
                try
                {
                    ScriptModel scriptModel = null;

                    if (!string.IsNullOrWhiteSpace(scriptDto.ScriptName))
                    {
                        scriptModel = await _scriptRepo.FindOneAsync(x => x.ScriptName != null &&
                            x.ScriptName.ToLower() == scriptDto.ScriptName.ToLower() && x.DeleteNbr == 0);
                    }

                    if (scriptModel == null && !string.IsNullOrWhiteSpace(scriptDto.ScriptCode))
                    {
                        scriptModel = await _scriptRepo.FindOneAsync(x =>
                            x.ScriptCode == scriptDto.ScriptCode && x.DeleteNbr == 0);
                    }

                    if (scriptModel != null)
                    {
                        scriptModel.ScriptDescription = scriptDto.ScriptDescription;
                        scriptModel.ScriptName = scriptDto.ScriptName;
                        scriptModel.ScriptSource = scriptDto.ScriptSource;
                        scriptModel.ScriptJson = scriptDto.ScriptJson;
                        scriptModel.UpdateTs = DateTime.UtcNow;
                        scriptModel.UpdateUser = _userContextService.GetUpdateUser(); ;
                        await _scriptRepo.UpdateAsync(scriptModel);

                        _logger.LogInformation("{ClassName}.{MethodName} : Script updated successfully. ScriptName: {ScriptName}, ScriptId: {ScriptId}", className, methodName, scriptModel.ScriptName, scriptModel.ScriptId);
                        _scriptLookUp[scriptModel.ScriptCode?? string.Empty] = scriptModel.ScriptId;
                    }
                    else
                    {
                        scriptModel = _mapper.Map<ScriptModel>(scriptDto);
                        scriptModel.ScriptId = 0;
                        scriptModel.CreateTs = DateTime.UtcNow;
                        scriptModel.CreateUser = Constant.ImportUser;
                        await _scriptRepo.CreateAsync(scriptModel);

                        _logger.LogInformation("{ClassName}.{MethodName} : Script created successfully. ScriptName: {ScriptName}", className, methodName, scriptModel.ScriptName);
                        _scriptLookUp[scriptModel.ScriptCode ?? string.Empty] = scriptModel.ScriptId;
                    }
                }
                catch (Exception ex)
                {
                    var error = $"Error processing script '{scriptDto.ScriptName ?? "Unknown"}': {ex.Message}";
                    errorMessages.AppendLine(error);
                    _logger.LogError("{ClassName}.{MethodName} : Exception occurred - {Error}", className, methodName, error);
                    continue;
                }
            }
        }
    }
}

