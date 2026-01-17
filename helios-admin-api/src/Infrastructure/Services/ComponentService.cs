using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Sunny.Benefits.Cms.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Core.Domain.Constants;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services
{
    public class ComponentService : IComponentService
    {
        public readonly ILogger<ComponentService> _logger;
        public readonly ICmsClient _cmsClient;
        public const string className = nameof(ComponentService);

        public ComponentService(ILogger<ComponentService> logger, ICmsClient cmsClient)
        {
            _logger = logger;
            _cmsClient = cmsClient;
        }

        /// <summary>
        /// Creates new component with the data given
        /// </summary>
        /// <param name="createComponentRequestDto">request contains data to create new component</param>
        /// <returns>base response</returns>
        public async Task<BaseResponseDto> CreateComponent(ComponentRequestDto createComponentRequestDto)
        {
            const string methodName = nameof(CreateComponent);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName}: Create Component process started for TenantCode: {TenantCode}", className, methodName, createComponentRequestDto.TenantCode);

                var response = await _cmsClient.Post<BaseResponseDto>(Constant.CreateComponentUrl, createComponentRequestDto);
                if (response.ErrorCode != null)
                {
                    _logger.LogError("{ClassName}.{MethodName}: Error occurred while creating Component, TenantCode: {TenantCode}, ErrorCode: {ErrorCode}", className, methodName, createComponentRequestDto.TenantCode, response.ErrorCode);
                    return response;
                }

                _logger.LogInformation("{ClassName}.{MethodName}: Component created successfully, TenantCode: {TenantCode}", className, methodName, createComponentRequestDto.TenantCode);
                return new BaseResponseDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: Exception occurred while creating Component. ErrorMessage: {ErrorMessage}, StackTrace: {StackTrace}", className, methodName, ex.Message, ex.StackTrace);
                throw;
            }
        }

        /// <summary>
        /// Get all component types
        /// </summary>
        /// <returns>List of component types</returns>
        public async Task<GetAllComponentTypesResponseDto> GetAllComponentTypes()
        {
            const string methodName = nameof(GetAllComponentTypes);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName}: FetchComponentTypes process started.", className, methodName);

                var response = await _cmsClient.Get<GetAllComponentTypesResponseDto>(Constant.GetAllComponentTypes, null);
                if (response.ErrorCode != null)
                {
                    _logger.LogWarning("{ClassName}.{MethodName}: Error occurred while Fetching ComponentTypes. ErrorCode: {ErrorCode}", className, methodName, response.ErrorCode);
                    return response;
                }

                _logger.LogInformation("{ClassName}.{MethodName}: Component fetched successfully.", className, methodName);
                return new GetAllComponentTypesResponseDto { ComponentTypes = response.ComponentTypes };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: Exception occurred while Fetching ComponentTypes. ErrorMessage: {ErrorMessage}, StackTrace: {StackTrace}", className, methodName, ex.Message, ex.StackTrace);
                throw;
            }
        }

        /// <summary>
        /// Get all components
        /// </summary>
        /// <param name="getAllComponentsRequestDto">request contains data to retrieve All components</param>
        /// <returns>Returns All the Tenant Specific Components</returns>
        public async Task<GetAllComponentsResponseDto> GetAllComponents(GetAllComponentsRequestDto getAllComponentsRequestDto)
        {
            const string methodName = nameof(GetAllComponents);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName}: Fetch Components process started for TenantCode: {TenantCode}", className, methodName, getAllComponentsRequestDto.TenantCode);

                var response = await _cmsClient.Post<GetAllComponentsResponseDto>(Constant.GetAllComponents, getAllComponentsRequestDto);
                if (response.ErrorCode != null)
                {
                    _logger.LogError("{ClassName}.{MethodName}: Error occurred while fetching Components, TenantCode: {TenantCode}, ErrorCode: {ErrorCode}", className, methodName, getAllComponentsRequestDto.TenantCode, response.ErrorCode);
                    return response;
                }

                _logger.LogInformation("{ClassName}.{MethodName}: Component Retrieved successfully, TenantCode: {TenantCode}", className, methodName, getAllComponentsRequestDto.TenantCode);
                return new GetAllComponentsResponseDto { Components = response.Components };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: Exception occurred while fetching Components. ErrorMessage: {ErrorMessage}, StackTrace: {StackTrace}", className, methodName, ex.Message, ex.StackTrace);
                throw;
            }
        }

        /// <summary>
        /// update existing component
        /// </summary>
        /// <param name="componentRequestDto">request contains data to update the component</param>
        /// <returns>returns the updated component</returns>
        public async Task<UpdateComponentResponseDto> UpdateComponent(ComponentRequestDto componentRequestDto)
        {
            const string methodName = nameof(UpdateComponent);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName}: Update Component process started for TenantCode: {TenantCode}", className, methodName, componentRequestDto.TenantCode);

                var response = await _cmsClient.Put<UpdateComponentResponseDto>(Constant.CreateComponentUrl, componentRequestDto);
                if (response.ErrorCode != null)
                {
                    _logger.LogError("{ClassName}.{MethodName}: Error occurred while Updating Component, TenantCode: {TenantCode}, ErrorCode: {ErrorCode}", className, methodName, componentRequestDto.TenantCode, response.ErrorCode);
                    return response;
                }

                _logger.LogInformation("{ClassName}.{MethodName}: Component Updated successfully, TenantCode: {TenantCode}", className, methodName, componentRequestDto.TenantCode);
                return new UpdateComponentResponseDto { Component = response.Component };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: Exception occurred while Updating Component. ErrorMessage: {ErrorMessage}, StackTrace: {StackTrace}", className, methodName, ex.Message, ex.StackTrace);
                throw;
            }
        }
        /// <summary>
        /// Imports a list of component types asynchronously by sending them to the CMS client.
        /// </summary>
        /// <param name="componentTypes">The list of component types to import.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains an 
        /// <see cref="ImportComponentTypeResponseDto"/> indicating the result of the import operation,
        /// including any error information if the operation fails.
        /// </returns>
        public async Task<ImportComponentTypeResponseDto> ImportComponentTypesAsync(List<ComponentTypeDto> componentTypes)
        {
            const string methodName = nameof(ImportComponentTypesAsync);
            try
            {
                return await _cmsClient.Post<ImportComponentTypeResponseDto>(Constant.ImportComponentTypes, new ImportComponentTypeRequestDto { ComponentTypes = componentTypes });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: Error Message: {Message}, Error Code: {ErrorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);

                return new ImportComponentTypeResponseDto
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = ex.Message
                };
            }
        }
    }
}
