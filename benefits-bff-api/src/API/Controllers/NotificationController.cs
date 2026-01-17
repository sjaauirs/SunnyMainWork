using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sunny.Benefits.Bff.Core.Domain.Dtos;
using Sunny.Benefits.Bff.Infrastructure.Services;
using Sunny.Benefits.Bff.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.NotificationService.Core.Domain.Dtos;

namespace Sunny.Benefits.Bff.Api.Controllers
{
    [Route("api/v1/notification")]
    [ApiController]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly ILogger<NotificationController> _notificationLogger;
        private readonly INotificationService _notificationService;
        private const string className = nameof(NotificationController);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="notificationLogger"></param>
        /// <param name="notificationService"></param>
        public NotificationController(ILogger<NotificationController> notificationLogger,
            INotificationService notificationService)
        {
            _notificationLogger = notificationLogger;
            _notificationService = notificationService;
        }

        [HttpGet("get-tenant-notification-category")]
        public async Task<ActionResult<GetTenantNotificationCategoryResponseDto>> GetNotificationCategoryByTenant(string tenantCode)
        {
            const string methodName = nameof(GetNotificationCategoryByTenant);
            try
            {
                _notificationLogger.LogInformation("{ClassName}.{MethodName} - Started processing GetNotificationCategoryByTenant With ConsumerCode : {ConsumerCode}", className, methodName, tenantCode);
                var tenantResponse = await _notificationService.GetNotificationCategoryByTenant(tenantCode);

                return tenantResponse != null ? Ok(tenantResponse) : NotFound();
            }
            catch (Exception ex)
            {
                _notificationLogger.LogError(ex, "{ClassName}.{MethodName} - Error occured while processing GetNotificationCategoryByTenant With ConsumerCode : {ConsumerCode} - ErrorCode:{ErrorCode}, ERROR:{ErrorMessage}",
                    className, methodName, tenantCode, StatusCodes.Status500InternalServerError, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new GetTenantNotificationCategoryResponseDto()
                {
                    ErrorCode = StatusCodes.Status500InternalServerError
                });
            }
            finally
            {
                _notificationLogger.LogInformation("{ClassName}.{MethodName} - GetNotificationCategoryByTenant API - Ended With ConsumerCode : {ConsumerCode}", className, methodName, tenantCode);
            }
        }

        [HttpGet("get-consumer-notification-pref")]
        public async Task<ActionResult<ConsumerNotificationPrefResponseDto>> GetConsumerNotificationPref(string tenantCode, string consumerCode)
        {
            const string methodName = nameof(GetConsumerNotificationPref);
            try
            {
                _notificationLogger.LogInformation("{ClassName}.{MethodName} - Started processing GetConsumerNotificationPref With ConsumerCode : {ConsumerCode} and TenantCode : {TenantCode}", className, methodName, consumerCode, tenantCode);
                var consumerResponse = await _notificationService.GetConsumerNotificationPref(tenantCode, consumerCode);
                return consumerResponse != null ? Ok(consumerResponse) : NotFound();
            }
            catch (Exception ex)
            {
                _notificationLogger.LogError(ex, "{ClassName}.{MethodName} - Error occured while processing GetConsumerNotificationPref With ConsumerCode : {ConsumerCode} and TenantCode : {TenantCode} - ErrorCode:{ErrorCode}, ERROR:{ErrorMessage}",
                    className, methodName, consumerCode, tenantCode, StatusCodes.Status500InternalServerError, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new ConsumerNotificationPrefResponseDto()
                {
                    ErrorCode = StatusCodes.Status500InternalServerError
                });
            }
            finally
            {
                _notificationLogger.LogInformation("{ClassName}.{MethodName} - GetConsumerNotificationPref API - Ended With ConsumerCode : {ConsumerCode} and TenantCode : {TenantCode}", className, methodName, consumerCode, tenantCode);
            }
        }

        [HttpPost("create-consumer-notification-pref")]
        public async Task<ActionResult<ConsumerNotificationPrefResponseDto>> CreateConsumerNotificationPref(CreateConsumerNotificationPrefRequestDto createConsumerNotificationPrefRequestDto)
        {
            const string methodName = nameof(CreateConsumerNotificationPref);
            try
            {
                _notificationLogger.LogInformation("{ClassName}.{MethodName} - Started processing CreateConsumerNotificationPref With ConsumerCode : {ConsumerCode} and TenantCode : {TenantCode}", className, methodName, createConsumerNotificationPrefRequestDto.ConsumerCode, createConsumerNotificationPrefRequestDto.TenantCode);
                var consumerResponse = await _notificationService.CreateConsumerNotificationPref(createConsumerNotificationPrefRequestDto);
                if (consumerResponse.ErrorCode != null)
                {
                    return StatusCode((int)consumerResponse.ErrorCode, consumerResponse);
                }
                return Ok(consumerResponse);
            }
            catch (Exception ex)
            {
                _notificationLogger.LogError(ex, "{ClassName}.{MethodName} - Error occured while processing CreateConsumerNotificationPref With ConsumerCode : {ConsumerCode} and TenantCode : {TenantCode} - ErrorCode:{ErrorCode}, ERROR:{ErrorMessage}",
                    className, methodName, createConsumerNotificationPrefRequestDto.ConsumerCode, createConsumerNotificationPrefRequestDto.TenantCode, StatusCodes.Status500InternalServerError, ex.Message);
                return new ConsumerNotificationPrefResponseDto()
                {
                    ErrorCode = StatusCodes.Status500InternalServerError
                };
            }
            finally
            {
                _notificationLogger.LogInformation("{ClassName}.{MethodName} - CreateConsumerNotificationPref API - Ended With ConsumerCode : {ConsumerCode} and TenantCode : {TenantCode}", className, methodName, createConsumerNotificationPrefRequestDto.ConsumerCode, createConsumerNotificationPrefRequestDto.TenantCode);
            }
        }

        [HttpPut("update-consumer-notification-pref")]
        public async Task<ActionResult<ConsumerNotificationPrefResponseDto>> UpdateCustomerNotificationPref(UpdateConsumerNotificationPrefRequestDto updateConsumerNotificationPrefRequestDto)
        {
            const string methodName = nameof(UpdateCustomerNotificationPref);
            try
            {
                _notificationLogger.LogInformation("{ClassName}.{MethodName} - Started processing UpdateCustomerNotificationPref With ConsumerNotificationPreferenceId : {ConsumerNotificationPreferenceId}", className, methodName, updateConsumerNotificationPrefRequestDto.ConsumerNotificationPreferenceId);
                var consumerResponse = await _notificationService.UpdateCustomerNotificationPref(updateConsumerNotificationPrefRequestDto);
                if (consumerResponse.ErrorCode != null)
                {
                    return StatusCode((int)consumerResponse.ErrorCode, consumerResponse);
                }
                return Ok(consumerResponse);
            }
            catch (Exception ex)
            {
                _notificationLogger.LogError(ex, "{ClassName}.{MethodName} - Error occured while processing UpdateCustomerNotificationPref With ConsumerNotificationPreferenceId : {ConsumerNotificationPreferenceId}- ErrorCode:{ErrorCode}, ERROR:{ErrorMessage}",
                    className, methodName, updateConsumerNotificationPrefRequestDto.ConsumerNotificationPreferenceId, StatusCodes.Status500InternalServerError, ex.Message);
                return new ConsumerNotificationPrefResponseDto()
                {
                    ErrorCode = StatusCodes.Status500InternalServerError
                };
            }
            finally
            {
                _notificationLogger.LogInformation("{ClassName}.{MethodName} - UpdateCustomerNotificationPref API - Ended With ConsumerNotificationPreferenceId : {ConsumerNotificationPreferenceId}", className, methodName, updateConsumerNotificationPrefRequestDto.ConsumerNotificationPreferenceId);
            }
        }
    }
}
