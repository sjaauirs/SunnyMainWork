using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Sunny.Benefits.Bff.Core.Domain.Constants;
using Sunny.Benefits.Bff.Core.Domain.Dtos;
using Sunny.Benefits.Bff.Infrastructure.HttpClients.Interfaces;
using Sunny.Benefits.Bff.Infrastructure.Services.Interfaces;
using SunnyBenefits.Fis.Core.Domain.Dtos;
using SunnyBenefits.Fis.Core.Domain.Fis.Dtos;

namespace Sunny.Benefits.Bff.Infrastructure.Services
{
    public class FisNotificationEnrollmentService : IFisNotificationEnrollmentService
    {
        private readonly ILogger<CardOperationService> _logger;
        private readonly IFisClient _fisClient;
        private readonly IConsumerAccountService _consumerAccountService;

        private const string className = nameof(FisNotificationEnrollmentService);

        public FisNotificationEnrollmentService(ILogger<CardOperationService> logger, IFisClient fisClient, IConsumerAccountService consumerAccountService)
        {
            _logger = logger;
            _fisClient = fisClient;
            _consumerAccountService = consumerAccountService;
        }

        public async Task<FisGetNotificationsEnrollmentResponseDto> GetNotificationsEnrollmentAsync(FisGetNotificationsEnrollmentRequestDto requestDto)
        {
            const string methodName = nameof(GetNotificationsEnrollmentAsync);
            try
            {
                var response = new FisGetNotificationsEnrollmentResponseDto();
                var fisApiResponse = await _fisClient.Post<GetNotificationsEnrollmentResponseDto>(FisNotificationConstants.FisGetNotificationsEnrollmentApiUrl, requestDto);
                _logger.LogInformation("{ClassName}.{MethodName} - Executed card operation Successfully for TenantCode:{TenantCode}, ConsuemrCode :{ConsumerCode}",
                    className, methodName, requestDto.TenantCode, requestDto.ConsumerCode);

                if (fisApiResponse.ErrorCode != null)
                {
                    return new FisGetNotificationsEnrollmentResponseDto { ErrorCode = fisApiResponse.ErrorCode };
                }

                response = await MapToFisNotificationsEnrollmentResponseDto(fisApiResponse);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Error Occurred While fetching FIS notifications enrollment for TenantCode:{TenantCode}, ConsumerCode :{ConsumerCode}, ErrorCode:{ErrorCode}, ERROR: {Message}",
                    className, methodName, requestDto.TenantCode, requestDto.ConsumerCode, StatusCodes.Status500InternalServerError, ex.Message);

                throw new InvalidOperationException("GetCardStatus :Error ", ex);
            }
        }

        public async Task<FisEnrollNotificationsResponseDto> SetNotificationsEnrollmentAsync(FisSetEnrollNotificationsRequestDto requestDto)
        {
            const string methodName = nameof(SetNotificationsEnrollmentAsync);
            const string errorMessage = "{ClassName}.{MethodName} - Error occurred while setting FIS Notifications Enrollment for TenantCode:{TenantCode} and ConsumerCode : {ConsumerCode}, " +
                 "ErrorCode:{ErrorCode}, ERROR: {ErrorMessage}";

            _logger.LogInformation("{ClassName}.{MethodName} Started setting FIS Notifications Enrollment with TenantCode:{TenantCode} and ConsumerCode : {ConsumerCode}.", className, methodName, requestDto.TenantCode, requestDto.ConsumerCode);
            try
            {
                var consumerAccountRequest = new GetConsumerAccountRequestDto
                {
                    TenantCode = requestDto.TenantCode,
                    ConsumerCode = requestDto.ConsumerCode
                };

                EnrollNotificationsResponseDto response;
                var consumerAccountResponse = await _consumerAccountService.GetConsumerAccount(consumerAccountRequest);
                if (!string.IsNullOrEmpty(consumerAccountResponse?.ConsumerAccount?.NotificationsEnrollmentUid))
                {
                    var updateRequestDto = new UpdateNotificationsRequestDto
                    {
                        ConsumerCode = requestDto.ConsumerCode,
                        TenantCode = requestDto.TenantCode,
                        EnrolledNotifications = requestDto.EnrolledNotifications,
                        EnrollmentUID = consumerAccountResponse?.ConsumerAccount?.NotificationsEnrollmentUid
                    };
                    response = await _fisClient.Put<EnrollNotificationsResponseDto>(FisNotificationConstants.FisNotificationsEnrollmentApiUrl, updateRequestDto);
                }
                else
                {
                    response = await _fisClient.Post<EnrollNotificationsResponseDto>(FisNotificationConstants.FisNotificationsEnrollmentApiUrl, requestDto);
                }

                if (response.ErrorCode != null)
                {
                    var errorCode = response.ErrorCode;
                    _logger.LogError(errorMessage, className, methodName, requestDto.TenantCode, requestDto.ConsumerCode, response.ErrorCode, response.ErrorMessage);

                    if (response.ErrorCode != null)
                    {
                        return new FisEnrollNotificationsResponseDto { ErrorCode = response.ErrorCode };
                    }
                }
                return new FisEnrollNotificationsResponseDto { Success = true };
                ;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, errorMessage, className, methodName, requestDto.TenantCode, requestDto.ConsumerCode, StatusCodes.Status500InternalServerError, ex.Message);

                throw new InvalidOperationException("Enroll FIS Notifications: Error ", ex);
            }
        }

        public async Task<FisGetClientConfigResponseDto> GetClientConfigAsync(FisGetNotificationsEnrollmentRequestDto requestDto)
        {
            const string methodName = nameof(GetNotificationsEnrollmentAsync);
            try
            {
                var response = await _fisClient.Post<FisGetClientConfigResponseDto>(FisNotificationConstants.FisGetClientConfigApiUrl, requestDto);
                _logger.LogInformation("{ClassName}.{MethodName} - Executed card operation Successfully for TenantCode:{TenantCode}, ConsuemrCode :{ConsumerCode}",
                    className, methodName, requestDto.TenantCode, requestDto.ConsumerCode);

                if (response.ErrorCode != null)
                {
                    return new FisGetClientConfigResponseDto { ErrorCode = response.ErrorCode };
                }

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Error Occurred While fetching FIS notifications enrollment for TenantCode:{TenantCode}, ConsumerCode :{ConsumerCode}, ErrorCode:{ErrorCode}, ERROR: {Message}",
                    className, methodName, requestDto.TenantCode, requestDto.ConsumerCode, StatusCodes.Status500InternalServerError, ex.Message);

                throw new InvalidOperationException("GetCardStatus :Error ", ex);
            }
        }
        private async Task<FisGetNotificationsEnrollmentResponseDto> MapToFisNotificationsEnrollmentResponseDto(GetNotificationsEnrollmentResponseDto source)
        {
            var response = new FisGetNotificationsEnrollmentResponseDto();

            var messages = source?.FisResponse?.CardholderEnrollmentData?.MessageData?.Messages;
            if (messages != null)
            {
                response.EnrollmentUid = source.FisResponse.CardholderEnrollmentData.EnrollmentUid;
                response.EnrolledNotifications = new List<FisNotificationsEnrollment>();
                foreach (var msg in messages)
                {
                    response.EnrolledNotifications.Add(new FisNotificationsEnrollment
                    {
                        MessageId = msg.MsgId,
                        MessageType = msg.MsgType,
                        MessageDescription = msg.MsgDescription
                    });
                }
            }

            return response;
        }
    }
}
