using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Models;
using SunnyRewards.Helios.Task.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Task.Infrastructure.Services.Interface;
using Constant = SunnyRewards.Helios.Task.Core.Domain.Constants.Constant;

namespace SunnyRewards.Helios.Task.Infrastructure.Services
{
    public class TenantTaskCategoryService : ITenantTaskCategoryService
    {
        private readonly ITenantTaskCategoryRepo _tenantTaskCategoryRepo;
        private readonly ILogger<TenantTaskCategoryService> _logger;
        private readonly IMapper _mapper;
        public const string className = nameof(TenantTaskCategoryService);

        public TenantTaskCategoryService(IMapper mapper, ITenantTaskCategoryRepo tenantTaskCategoryRepo, ILogger<TenantTaskCategoryService> logger)
        {
            _tenantTaskCategoryRepo = tenantTaskCategoryRepo;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<BaseResponseDto> CreateTenantTaskCategory(TenantTaskCategoryRequestDto tenantTaskCategoryRequestDto)
        {
            const string methodName = nameof(CreateTenantTaskCategory);
            try
            {
                _logger.LogInformation("{ClassName}:{MethodName}: Fetching tenantTaskCategory started for Tenant Code: {TenantCode}",
                   className, methodName, tenantTaskCategoryRequestDto.TenantCode);
                var tenantTaskCategory = await _tenantTaskCategoryRepo.FindOneAsync(x => x.TenantCode == tenantTaskCategoryRequestDto.TenantCode && x.DeleteNbr == 0 && x.TaskCategoryId == tenantTaskCategoryRequestDto.TaskCategoryId);
                if (tenantTaskCategory != null && tenantTaskCategory.ResourceJson == tenantTaskCategoryRequestDto.ResourceJson)
                {
                    return new BaseResponseDto() { ErrorCode = StatusCodes.Status409Conflict, ErrorMessage = $"TenantTaskCategory is already Existed with Tenant Code: {tenantTaskCategoryRequestDto.TenantCode}" };
                }
                var tenantTaskCategoryModel = _mapper.Map<TenantTaskCategoryModel>(tenantTaskCategoryRequestDto);
                tenantTaskCategoryModel.CreateTs = DateTime.UtcNow;
                tenantTaskCategoryModel.DeleteNbr = 0;
                tenantTaskCategoryModel.TenantTaskCategoryId = 0;
                await _tenantTaskCategoryRepo.CreateAsync(tenantTaskCategoryModel);
                _logger.LogInformation("{ClassName}.{MethodName}: TenantTaskCategory created Successfully. Tenant Code:{TenantCode}", className, methodName, tenantTaskCategoryRequestDto.TenantCode);
                return new BaseResponseDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}:{MethodName}: Error while Fetching tenantTaskCategory, for Tenant Code: {TenantCode}",
                  className, methodName, tenantTaskCategoryRequestDto.TenantCode);
                throw;
            }

        }

        /// <summary>
        /// Updates an existing TenantTaskCategory based on the provided request data.
        /// </summary>
        /// <param name="requestDto">The request data containing the details to update.</param>
        /// <returns>A response DTO indicating success or failure.</returns>
        public async Task<TenantTaskCategoryResponseDto> UpdateTenantTaskCategory(TenantTaskCategoryDto requestDto)
        {
            const string methodName = nameof(UpdateTenantTaskCategory);
            try
            {
                var existingTenantTaskCategory = await _tenantTaskCategoryRepo.FindOneAsync(x => x.TenantTaskCategoryId == requestDto.TenantTaskCategoryId && x.DeleteNbr == 0);
                if (existingTenantTaskCategory == null)
                {
                    _logger.LogError("{ClassName}.{MethodName}: No TenantTaskCategory found for TenantTaskCategoryId: {TenantTaskCategoryId}", className, methodName, requestDto.TenantTaskCategoryId);
                    return new TenantTaskCategoryResponseDto
                    {
                        ErrorCode = StatusCodes.Status404NotFound,
                        ErrorMessage = $"No TenantTaskCategory found for TenantTaskCategoryId: {requestDto.TenantTaskCategoryId}"
                    };
                }
                _mapper.Map(requestDto, existingTenantTaskCategory);

                existingTenantTaskCategory.UpdateUser = Constant.ImportUser;
                existingTenantTaskCategory.UpdateTs = DateTime.UtcNow;
                
                await _tenantTaskCategoryRepo.UpdateAsync(existingTenantTaskCategory);
                
                return new TenantTaskCategoryResponseDto { TenantTaskCategory = requestDto };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: Error processing. Request: {RequestData}, ErrorMessage: {ErrorMessage}, ErrorCode: {ErrorCode}", className, methodName, requestDto.ToJson(), ex.Message, StatusCodes.Status500InternalServerError);
                return new TenantTaskCategoryResponseDto { ErrorCode = StatusCodes.Status500InternalServerError, ErrorMessage = ex.Message };
            }
        }
    }
}
