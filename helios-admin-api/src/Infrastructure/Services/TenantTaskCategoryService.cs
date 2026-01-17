using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Core.Domain.Constants;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services
{
    public class TenantTaskCategoryService : ITenantTaskCategoryService
    {
        public readonly ILogger<TenantTaskCategoryService> _logger;
        public readonly ITaskClient _taskClient;
        public const string className = nameof(TenantTaskCategoryService);

        public TenantTaskCategoryService(ILogger<TenantTaskCategoryService> logger, ITaskClient taskClient)
        {
            _logger = logger;
            _taskClient = taskClient;
        }

        public async Task<BaseResponseDto> CreateTenantTaskCategory(TenantTaskCategoryRequestDto tenantTaskCategoryRequestDto)
        {
            const string methodName = nameof(CreateTenantTaskCategory);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName}: Create Tenant Task Category process started for TenantCode: {TenantCode}", className, methodName, tenantTaskCategoryRequestDto.TenantCode);

                var taskResponse = await _taskClient.Post<BaseResponseDto>(Constant.CreateTenantTaskCategoryUrl, tenantTaskCategoryRequestDto);
                if (taskResponse.ErrorCode != null)
                {
                    _logger.LogWarning("{ClassName}.{MethodName}: Error occurred while creating tenantTaskCategory, TenantCode: {TenantCode}, ErrorCode: {ErrorCode}", className, methodName, tenantTaskCategoryRequestDto.TenantCode, taskResponse.ErrorCode);
                    return taskResponse;
                }
                _logger.LogInformation("{ClassName}.{MethodName}: tenantTaskCategory created successfully, TenantCode: {TenantCode}", className, methodName, tenantTaskCategoryRequestDto.TenantCode);
                return new BaseResponseDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: Exception occurred while creating TenantTaskCategory. ErrorMessage: {ErrorMessage}, StackTrace: {StackTrace}", className, methodName, ex.Message, ex.StackTrace);
                throw;
            }
        }

    }
}
