using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sunny.Benefits.Bff.Core.Domain.Dtos;
using Sunny.Benefits.Bff.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace Sunny.Benefits.Bff.Api.Controllers
{
    [Route("api/v1/fis")]
    [ApiController]
    [Authorize]
    public class FisNotificationsEnrollmentController : ControllerBase
    {
        private readonly ILogger<FisNotificationsEnrollmentController> _logger;
        private readonly IFisNotificationEnrollmentService _fisNotificationEnrollmentService;
        private const string className = nameof(FisNotificationsEnrollmentController);

        public FisNotificationsEnrollmentController(ILogger<FisNotificationsEnrollmentController> logger, IFisNotificationEnrollmentService fisNotificationEnrollmentService)
        {
            _logger = logger;
            _fisNotificationEnrollmentService = fisNotificationEnrollmentService;
        }

        [HttpPost("set-notifications-enrollment")]
        public async Task<ActionResult<FisGetNotificationsEnrollmentResponseDto>> SetNotificationsEnrollment([FromBody] FisSetEnrollNotificationsRequestDto requestDto)
        {
            return await HandleFisRequestAsync(
                methodName: nameof(GetNotificationsEnrollment),
                errorMessage: "{ClassName}.{MethodName} - Error occurred while setting FIS Notifications Enrollment for TenantCode:{TenantCode} and ConsumerCode : {ConsumerCode}, ErrorCode:{ErrorCode}, ERROR: {ErrorMessage}",
                logMessage: "{ClassName}.{MethodName} Started setting FIS Notifications Enrollment with TenantCode:{TenantCode} and ConsumerCode : {ConsumerCode}.",
                requestDto: requestDto,
                serviceCall: () => _fisNotificationEnrollmentService.SetNotificationsEnrollmentAsync(requestDto)
            );
        }

        [HttpPost("get-notifications-enrollment")]
        public async Task<ActionResult<FisGetNotificationsEnrollmentResponseDto>> GetNotificationsEnrollment([FromBody] FisGetNotificationsEnrollmentRequestDto requestDto)
        {
            return await HandleFisRequestAsync(
                methodName: nameof(GetNotificationsEnrollment),
                errorMessage: "{ClassName}.{MethodName} - Error occurred while getting FIS Notifications Enrollment for TenantCode:{TenantCode} and ConsumerCode : {ConsumerCode}, ErrorCode:{ErrorCode}, ERROR: {ErrorMessage}",
                logMessage: "{ClassName}.{MethodName} Started processing get card holder enrollment with TenantCode:{TenantCode} and ConsumerCode : {ConsumerCode}.",
                requestDto: requestDto,
                serviceCall: () => _fisNotificationEnrollmentService.GetNotificationsEnrollmentAsync(requestDto)
            );
        }

        [HttpPost("get-client-config")]
        public async Task<ActionResult> GetClientConfig([FromBody] FisGetNotificationsEnrollmentRequestDto requestDto)
        {
            return await HandleFisRequestAsync(
                methodName: nameof(GetClientConfig),
                errorMessage: "{ClassName}.{MethodName} - Error occurred while getting client config for TenantCode:{TenantCode} and ConsumerCode : {ConsumerCode}, ErrorCode:{ErrorCode}, ERROR: {ErrorMessage}",
                logMessage: "{ClassName}.{MethodName} Started processing get client config with TenantCode:{TenantCode} and ConsumerCode : {ConsumerCode}.",
                requestDto: requestDto,
                serviceCall: () => _fisNotificationEnrollmentService.GetClientConfigAsync(requestDto)
            );
        }

        private async Task<ActionResult> HandleFisRequestAsync<TRequest, TResponse>(
            string methodName,
            string errorMessage,
            string logMessage,
            TRequest requestDto,
            Func<Task<TResponse>> serviceCall
        ) where TResponse : BaseResponseDto, new()
        {
            _logger.LogInformation(logMessage, className, methodName,
                GetPropertyValue(requestDto, "TenantCode"),
                GetPropertyValue(requestDto, "ConsumerCode"));

            try
            {
                var response = await serviceCall();
                if (response.ErrorCode != null)
                {
                    var errorCode = response.ErrorCode;
                    _logger.LogError(errorMessage, className, methodName,
                        GetPropertyValue(requestDto, "TenantCode"),
                        GetPropertyValue(requestDto, "ConsumerCode"),
                        response.ErrorCode, response.ErrorMessage);
                    return StatusCode((int)errorCode, response);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, errorMessage, className, methodName,
                    GetPropertyValue(requestDto, "TenantCode"),
                    GetPropertyValue(requestDto, "ConsumerCode"),
                    StatusCodes.Status500InternalServerError, ex.Message);

                // Always return FisGetNotificationsEnrollmentResponseDto for error
                return StatusCode(StatusCodes.Status500InternalServerError, new FisGetNotificationsEnrollmentResponseDto()
                {
                    ErrorCode = StatusCodes.Status500InternalServerError
                });
            }
        }

        private static object? GetPropertyValue(object obj, string propertyName)
        {
            return obj?.GetType().GetProperty(propertyName)?.GetValue(obj, null);
        }
    }
}