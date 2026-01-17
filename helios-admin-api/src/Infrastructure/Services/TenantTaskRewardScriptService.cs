using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using NHibernate.Engine;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos.Constants;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos.Enums;
using SunnyRewards.Helios.Admin.Core.Domain.Models;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Repositories;
using SunnyRewards.Helios.Admin.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Common.Core.Helpers;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Models;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;
using SunnyRewards.Helios.Tenant.Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services
{
    public class TenantTaskRewardScriptService : ITenantTaskRewardScriptService
    {
        private readonly ILogger<TenantTaskRewardScriptService> _serviceLogger;
        private readonly ITenantTaskRewardScriptRepo _tenantTaskRewardScriptRepo;
        private readonly IScriptRepo _scriptRepo;
        private readonly IConsumerTaskService _consumerTaskService;
        private readonly IMapper _mapper;
        private readonly ITaskClient _taskClient;
        private readonly IUserContextService _userContextService;

        private readonly NHibernate.ISession _session;


        const string className = nameof(ScriptService);

        public TenantTaskRewardScriptService(ILogger<TenantTaskRewardScriptService> serviceLogger, ITenantTaskRewardScriptRepo tenantTaskRewardScriptRepo
            , IScriptRepo scriptRepo, NHibernate.ISession session, IMapper mapper, IConsumerTaskService consumerTaskService, ITaskClient taskClient
            , IUserContextService userContextService
)
        {
            _serviceLogger = serviceLogger;
            _tenantTaskRewardScriptRepo = tenantTaskRewardScriptRepo;
            _mapper = mapper;
            _session = session;
            _consumerTaskService = consumerTaskService;
            _scriptRepo = scriptRepo;
            _taskClient = taskClient;
            _userContextService = userContextService;



        }
        public async Task<BaseResponseDto> PostTenantTaskRewardScriptRequest(TenantTaskRewardScriptRequestDto requestDto)
        {
            const string methodName = nameof(PostTenantTaskRewardScriptRequest);
            bool tenantCodeExists = false;
            bool tasktCodeExists = false;
            try
            {
                if (requestDto == null)
                {
                    _serviceLogger.LogError("{className}.{methodName}: Failed to Saved data for Tenant Task Reward Script : {requestDto}", className, methodName, requestDto?.ToJson());
                    return new BaseResponseDto { ErrorCode = StatusCodes.Status404NotFound, ErrorMessage = "Tenant Task Reward Script request data Not Found" };
                }
                tenantCodeExists = await _consumerTaskService.GetTenantByTenantCode(requestDto.TenantCode) ?? false;
                tasktCodeExists = await TaskRewardCodeExists(requestDto.TaskRewardCode);
                if (!tenantCodeExists || !tasktCodeExists || !ScriptTypeExists(requestDto.ScriptType))
                {
                    _serviceLogger.LogError("{className}.{methodName}: tenant code or Task code or script Type does not exists for Tenant Task Reward Script : {requestDto}", className, methodName, requestDto?.ToJson());
                    return new BaseResponseDto { ErrorCode = StatusCodes.Status404NotFound, ErrorMessage = "Tenant  code or Task code or  script Type  Not Found" };
                }


                var taskrewardModel = await _tenantTaskRewardScriptRepo.FindOneAsync(x => x.TaskRewardCode == requestDto.TaskRewardCode && x.DeleteNbr == 0
                && x.ScriptType != null && x.ScriptType.ToLower() == requestDto.ScriptType.ToLower());
                var script = await _scriptRepo.FindOneAsync(x => x.ScriptCode == requestDto.ScriptCode && x.DeleteNbr == 0);
                if (taskrewardModel != null)
                {
                    _serviceLogger.LogInformation("{className}.{methodName}: Task Reward Code for given script type already exists: {requestDto}", className, methodName, requestDto.ToJson());
                    return new BaseResponseDto { ErrorCode = StatusCodes.Status409Conflict, ErrorMessage = "Task Reward Code for given script type already exists" };

                }
                if (script == null)
                {
                    _serviceLogger.LogInformation("{className}.{methodName}: Script not found for request: {requestDto}", className, methodName, requestDto.ToJson());
                    return new BaseResponseDto { ErrorCode = StatusCodes.Status404NotFound, ErrorMessage = "Script not found" };

                }

                TenantTaskRewardScriptModel tenantTaskRewardScriptModel = new TenantTaskRewardScriptModel();
                tenantTaskRewardScriptModel = _mapper.Map<TenantTaskRewardScriptModel>(requestDto);
                tenantTaskRewardScriptModel.CreateTs = DateTime.Now;
                tenantTaskRewardScriptModel.TenantTaskRewardScriptCode = "trs-" + Guid.NewGuid().ToString("N");
                tenantTaskRewardScriptModel.ScriptId = script.ScriptId;
                tenantTaskRewardScriptModel.CreateUser = Constant.CreateUser;
                tenantTaskRewardScriptModel = await _tenantTaskRewardScriptRepo.CreateAsync(tenantTaskRewardScriptModel);
                if (tenantTaskRewardScriptModel.TenantTaskRewardScriptId > 0)
                {
                    _serviceLogger.LogInformation("{className}.{methodName}: Successfully Saved data for tenantTaskRewardScript  request: {requestDto}", className, methodName, requestDto.ToJson());
                    return new BaseResponseDto();
                }
                _serviceLogger.LogError("{className}.{methodName}: Failed to Saved data for tenantTaskRewardScript request: {requestDto}", className, methodName, requestDto.ToJson());
                return new BaseResponseDto { ErrorCode = StatusCodes.Status404NotFound, ErrorMessage = "Tenant Task Reward Script Model record Not Created" };


            }
            catch (Exception ex)
            {
                _serviceLogger.LogError(ex, "{className}.{methodName}: ERROR - msg : {msg}, for requestDto: {RequestDto}", className, methodName, ex.Message, requestDto.ToJson());
                throw;
            }
        }

        public async Task<BaseResponseDto> UpdateTenantTaskRewardScriptRequest(UpdateTenantTaskRewardScriptRequestDto requestDto)
        {
            const string methodName = nameof(UpdateTenantTaskRewardScriptRequest);
            try
            {
                bool tenantCodeExists = false;
                bool tasktCodeExists = false;


                if (requestDto == null)
                {
                    _serviceLogger.LogError("{className}.{methodName}: Failed to Saved data for Tenant Task Reward Script : {requestDto}", className, methodName, requestDto?.ToJson());
                    return new BaseResponseDto { ErrorCode = StatusCodes.Status404NotFound, ErrorMessage = "Tenant Task Reward Script request data Not Found" };
                }
                var taskrewardModel = await _tenantTaskRewardScriptRepo.FindOneAsync(x => x.TenantTaskRewardScriptCode == requestDto.TenantTaskRewardScriptCode && x.DeleteNbr == 0);
                var script = await _scriptRepo.FindOneAsync(x => x.ScriptCode == requestDto.ScriptCode && x.DeleteNbr == 0);
                if (taskrewardModel == null || script == null)
                {
                    _serviceLogger.LogInformation("{className}.{methodName}: tenant Task Reward Code or Script not found for request: {requestDto}", className, methodName, requestDto.ToJson());
                    return new BaseResponseDto { ErrorCode = StatusCodes.Status404NotFound, ErrorMessage = "Tenant Task Reward Code or Script not found" };

                }
                requestDto.TenantCode = requestDto.TenantCode.IsNullOrEmpty() ? taskrewardModel.TenantCode : requestDto.TenantCode;
                requestDto.TaskRewardCode = requestDto.TaskRewardCode.IsNullOrEmpty() ? taskrewardModel.TaskRewardCode : requestDto.TaskRewardCode;
                requestDto.ScriptType = requestDto.TenantCode.IsNullOrEmpty() ? taskrewardModel.ScriptType : requestDto.ScriptType;

                tenantCodeExists = await _consumerTaskService.GetTenantByTenantCode(requestDto.TenantCode) ?? false;
                tasktCodeExists = await TaskRewardCodeExists(requestDto.TaskRewardCode);
                if (!tenantCodeExists || !tasktCodeExists || !ScriptTypeExists(requestDto?.ScriptType))
                {
                    _serviceLogger.LogError("{className}.{methodName}: tenant code or Task code or script Type does not exists for Tenant Task Reward Script : {requestDto}", className, methodName, requestDto?.ToJson());
                    return new BaseResponseDto { ErrorCode = StatusCodes.Status404NotFound, ErrorMessage = "Tenant  code or Task code or  script Type  Not Found" };
                }


                var taskrewardModelExists = await _tenantTaskRewardScriptRepo.FindOneAsync(x => x.TaskRewardCode == requestDto.TaskRewardCode && x.DeleteNbr == 0
              && x.ScriptType != null && x.ScriptType.ToLower() == requestDto.ScriptType.ToLower());
                if (taskrewardModelExists != null)
                {
                    _serviceLogger.LogInformation("{className}.{methodName}: Task Reward Code for given script type already exists: {requestDto}", className, methodName, requestDto.ToJson());
                    return new BaseResponseDto { ErrorCode = StatusCodes.Status409Conflict, ErrorMessage = "Task Reward Code for given script type already exists" };

                }

                taskrewardModel.UpdateTs = DateTime.Now;
                taskrewardModel.TenantCode = requestDto?.TenantCode;
                taskrewardModel.ScriptType = requestDto?.ScriptType;
                taskrewardModel.TaskRewardCode = requestDto?.TaskRewardCode;
                taskrewardModel.UpdateTs = DateTime.Now;
                taskrewardModel.ScriptId = script.ScriptId;
                taskrewardModel.UpdateUser = _userContextService.GetUpdateUser();

                taskrewardModel = await _tenantTaskRewardScriptRepo.UpdateAsync(taskrewardModel);
                if (taskrewardModel.TenantTaskRewardScriptId > 0)
                {
                    _serviceLogger.LogInformation("{className}.{methodName}: Successfully updated data for tenantTaskRewardScript  request: {requestDto}", className, methodName, requestDto.ToJson());
                    return new BaseResponseDto();
                }
                _serviceLogger.LogError("{className}.{methodName}: Failed to update data for tenantTaskRewardScript request: {requestDto}", className, methodName, requestDto.ToJson());
                return new BaseResponseDto { ErrorCode = StatusCodes.Status404NotFound, ErrorMessage = "Tenant Task Reward Script Model record Not Created" };


            }
            catch (Exception ex)
            {
                _serviceLogger.LogError(ex, "{className}.{methodName}: ERROR - msg : {msg}, for requestDto: {RequestDto}", className, methodName, ex.Message, requestDto.ToJson());
                throw;
            }
        }
        private async Task<bool> TaskRewardCodeExists(string taskRewardCode)
        {
            var taskRewardCodeExists = false;
            var getRequestDto = new GetTaskRewardByCodeRequestDto()
            {
                TaskRewardCode = taskRewardCode,
            };

            var response = await _taskClient.Post<GetTaskRewardByCodeResponseDto>("get-task-reward-by-code", getRequestDto);
            _serviceLogger.LogInformation("Retrieved TaskRewardCode Data Successfully for TaskRewardCode : {partnerCode}", getRequestDto);
            if (response != null || response.TaskRewardDetail != null)
            {
                taskRewardCodeExists = response.TaskRewardDetail?.TaskReward != null;
            }
            return taskRewardCodeExists;
        }
        private bool ScriptTypeExists(string scriptType)
        {
            var scriptTypeList = Enum.GetNames(typeof(ScriptTypes)).ToList();
            var value= scriptTypeList?.Where(x=>x.ToLower()==(scriptType.ToLower())).FirstOrDefault() ;
            return value!=null;
        }

    }
}
