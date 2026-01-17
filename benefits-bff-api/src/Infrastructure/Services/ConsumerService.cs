using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Sunny.Benefits.Bff.Core.Constants;
using Sunny.Benefits.Bff.Core.Domain.Constants;
using Sunny.Benefits.Bff.Core.Domain.Dtos;
using Sunny.Benefits.Bff.Infrastructure.Repositories;
using Sunny.Benefits.Bff.Infrastructure.Repositories.Interfaces;
using Sunny.Benefits.Bff.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Tenant.Core.Domain.Models;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.enums;
using SunnyRewards.Helios.User.Core.Domain.Models;

namespace Sunny.Benefits.Bff.Infrastructure.Services
{
    public class ConsumerService : IConsumerService
    {
        private readonly ILogger<ConsumerService> _logger;
        private readonly IUserClient _userClient;
        private readonly IMapper _mapper;
        private const string className = nameof(ConsumerService);
        public ConsumerService(ILogger<ConsumerService> logger, IUserClient userClient, IMapper mapper)
        {
            _logger = logger;
            _userClient = userClient;
            _mapper = mapper;
        }
        /// <summary>
        /// Updates a consumer's information asynchronously.
        /// </summary>
        /// <param name="consumerId">The unique identifier of the consumer to be updated.</param>
        /// <param name="consumerRequestDto">The DTO containing the updated consumer information.</param>
        /// <returns>A <see cref="ConsumerResponseDto"/> containing the result of the update operation, including any error information.</returns>
        /// <remarks>
        public async Task<ConsumerResponseDto> UpdateConsumerAsync(long consumerId, ConsumerRequestDto consumerRequestDto)
        {
            const string methodName = nameof(UpdateConsumerAsync);
            _logger.LogInformation("{ClassName}.{MethodName} : Started updating consumer with ConsumerCode:{Code} and TenantCode:{Tenant}",
                        className, methodName, consumerRequestDto.ConsumerCode, consumerRequestDto.TenantCode);
            try
            {
                var response = await _userClient.Put<ConsumerResponseDto>($"{CommonConstants.ConsumerAPIUrl}/{consumerId}", consumerRequestDto);

                if (response.ErrorCode != null)
                {
                    _logger.LogError("{ClassName}.{MethodName}: Error occurred while updating consumer with ConsumerCode:{Code} and TenantCode:{Tenant}, ErrorCode: {ErrorCode} and Error Message: {ErrorMessage}",
                        className, methodName, consumerRequestDto.ConsumerCode, consumerRequestDto.TenantCode, response.ErrorCode, response.ErrorMessage);
                    return response;
                }
                _logger.LogInformation("{ClassName}.{MethodName} : Successfully updated consumer with ConsumerCode:{Code} and TenantCode:{Tenant}",
                   className, methodName, consumerRequestDto.ConsumerCode, consumerRequestDto.TenantCode);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} : Error occurred while updating consumer with ConsumerCode:{Code} and TenantCode:{Tenant},ERROR:{Msg}",
                        className, methodName, consumerRequestDto.ConsumerCode, consumerRequestDto.TenantCode, ex.Message);
                return new ConsumerResponseDto()
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = ex.Message,
                    Consumer = _mapper.Map<ConsumerDto>(consumerRequestDto)

                };
            }
        }

        public async Task<ConsumerResponseDto> DeactivateConsumer(DeactivateConsumerRequestDto consumerRequestDto)
        {
            const string methodName = nameof(DeactivateConsumer);
            _logger.LogInformation("{ClassName}.{MethodName} : Started deactivating consumer with ConsumerCode:{Code} and TenantCode:{Tenant}",
                className, methodName, consumerRequestDto.ConsumerCode, consumerRequestDto.TenantCode);

            try
            {
                var enrollmentUpdateRequest = new UpdateEnrollmentStatusRequestDto
                {
                    TenantCode = consumerRequestDto.TenantCode,
                    ConsumerCode = consumerRequestDto.ConsumerCode,
                    EnrollmentStatus = EnrollmentStatus.DEACTIVATED.ToString()
                };

                var response = await _userClient.Put<ConsumerResponseDto>(
                    $"{CommonConstants.UpdateEnrollmentStatusAPIUrl}",
                    enrollmentUpdateRequest
                );

                if (response.ErrorCode != null)
                {
                    _logger.LogError("{ClassName}.{MethodName}: Error occurred while deactivating consumer with ConsumerCode:{Code} and TenantCode:{Tenant}, ErrorCode: {ErrorCode}, Error Message: {ErrorMessage}",
                        className, methodName, consumerRequestDto.ConsumerCode, consumerRequestDto.TenantCode, response.ErrorCode, response.ErrorMessage);
                    return response;
                }

                _logger.LogInformation("{ClassName}.{MethodName} : Successfully deactivated consumer with ConsumerCode:{Code} and TenantCode:{Tenant}",
                    className, methodName, consumerRequestDto.ConsumerCode, consumerRequestDto.TenantCode);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} : Exception occurred while deactivating consumer with ConsumerCode:{Code} and TenantCode:{Tenant}, ERROR: {Message}",
                    className, methodName, consumerRequestDto.ConsumerCode, consumerRequestDto.TenantCode, ex.Message);

                throw;
            }
        }

        public async Task<ConsumerResponseDto> ReactivateConsumer(ReactivateConsumerRequestDto consumerRequestDto)
        {
            const string methodName = nameof(ReactivateConsumer);
            _logger.LogInformation("{ClassName}.{MethodName} : Started reactivating consumer with ConsumerCode:{Code} and TenantCode:{Tenant}",
                className, methodName, consumerRequestDto.ConsumerCode, consumerRequestDto.TenantCode);

            try
            {
                var enrollmentUpdateRequest = new UpdateEnrollmentStatusRequestDto
                {
                    TenantCode = consumerRequestDto.TenantCode,
                    ConsumerCode = consumerRequestDto.ConsumerCode,
                    EnrollmentStatus = EnrollmentStatus.ENROLLED.ToString()
                };

                var response = await _userClient.Put<ConsumerResponseDto>(
                    $"{CommonConstants.UpdateEnrollmentStatusAPIUrl}",
                    enrollmentUpdateRequest
                );

                if (response.ErrorCode != null)
                {
                    _logger.LogError("{ClassName}.{MethodName}: Error occurred while reactivating consumer with ConsumerCode:{Code} and TenantCode:{Tenant}, ErrorCode: {ErrorCode}, Error Message: {ErrorMessage}",
                        className, methodName, consumerRequestDto.ConsumerCode, consumerRequestDto.TenantCode, response.ErrorCode, response.ErrorMessage);
                    return response;
                }

                _logger.LogInformation("{ClassName}.{MethodName} : Successfully reactivated consumer with ConsumerCode:{Code} and TenantCode:{Tenant}",
                    className, methodName, consumerRequestDto.ConsumerCode, consumerRequestDto.TenantCode);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} : Exception occurred while reactivating consumer with ConsumerCode:{Code} and TenantCode:{Tenant}, ERROR: {Message}",
                    className, methodName, consumerRequestDto.ConsumerCode, consumerRequestDto.TenantCode, ex.Message);

                throw;
            }
        }

        /// <summary>
        /// Retrieves consumer attributes based on the provided request data.
        /// </summary>
        /// <param name="consumerAttributesRequestDto"></param>
        /// <returns></returns>
        public async Task<ConsumerAttributesResponseDto> ConsumerAttributes(ConsumerAttributesRequestDto consumerAttributesRequestDto)
        {
            const string methodName = nameof(ConsumerAttributes);
            _logger.LogInformation("{ClassName}.{MethodName} : Started With ConsumerCode : {ConsumerCode}, TenantCode : {TenantCode}",
                className, methodName, consumerAttributesRequestDto.ConsumerAttributes[0].ConsumerCode, consumerAttributesRequestDto.TenantCode);
            try
            {
                var response = await _userClient.Post<ConsumerAttributesResponseDto>(CommonConstants.ConsumerAttributersUrl, consumerAttributesRequestDto);
                if (response.ErrorCode != null)
                {
                    _logger.LogError("{ClassName}.{MethodName}: Error occurred while processing consumer attributes with ConsumerCode:{Code} and TenantCode:{Tenant}, ErrorCode: {ErrorCode}, Error Message: {ErrorMessage}",
                        className, methodName, consumerAttributesRequestDto.ConsumerAttributes[0].ConsumerCode, consumerAttributesRequestDto.TenantCode, response.ErrorCode, response.ErrorMessage);
                    return response;
                }

                _logger.LogInformation("{ClassName}.{MethodName} : Successfully processed consumer attributes with ConsumerCode:{Code} and TenantCode:{Tenant}",
                    className, methodName, consumerAttributesRequestDto.ConsumerAttributes[0].ConsumerCode, consumerAttributesRequestDto.TenantCode);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} : Exception occurred while processing consumer attributes with ConsumerCode:{Code} and TenantCode:{Tenant}, ERROR: {Message}",
                    className, methodName, consumerAttributesRequestDto.ConsumerAttributes[0].ConsumerCode, consumerAttributesRequestDto.TenantCode, ex.Message);
                throw;
            }
        }

        public async Task<GetConsumerResponseDto> GetConsumer(GetConsumerRequestDto consumerSummaryRequestDto)
        {
            const string methodName = nameof(GetConsumer);
            _logger.LogInformation("{ClassName}.{MethodName} - Consumer get started with Consumer code: {ConsumerCode}", className, methodName, consumerSummaryRequestDto.ConsumerCode);
            var consumer = await _userClient.Post<GetConsumerResponseDto>(UserConstants.GetConsumerAPIUrl, consumerSummaryRequestDto);
            if (consumer?.Consumer == null)
            {
                _logger.LogError("{ClassName}.{MethodName} - Invalid Consumer code: {ConsumerCode},ErrorCode:{Code}", className, methodName, consumerSummaryRequestDto.ConsumerCode, StatusCodes.Status404NotFound);
                return new GetConsumerResponseDto();
            }
            _logger.LogInformation("{ClassName}.{MethodName} - Ending to GetConsumer, ConsumerCode : {ConsumerCode}", className, methodName, consumerSummaryRequestDto.ConsumerCode);
            return consumer;
        }

        /// <summary>
        /// Update the subscription status of a consumer.
        /// </summary>
        /// <param name="requestDto"></param>
        /// <returns></returns>
        public async Task<BaseResponseDto> UpdateConsumerSubscriptionStatus(ConsumerSubscriptionStatusRequestDto requestDto)
        {
            const string methodName = nameof(UpdateConsumerSubscriptionStatus);
            _logger.LogInformation("{ClassName}.{MethodName} - Started checking subscription status for ConsumerCode: {ConsumerCode}, TenantCode: {TenantCode}",
                className, methodName, requestDto.ConsumerCode, requestDto.TenantCode);
            try
            {
                var userResponse = await _userClient.Post<BaseResponseDto>(CommonConstants.ConsumerSubscriptionStatusUrl, requestDto);
                if (userResponse.ErrorCode != null)
                {
                    _logger.LogError("{ClassName}.{MethodName} - Error occurred while checking subscription status for ConsumerCode: {ConsumerCode}, TenantCode: {TenantCode}, ErrorCode: {ErrorCode}, Error Message: {ErrorMessage}",
                        className, methodName, requestDto.ConsumerCode, requestDto.TenantCode, userResponse.ErrorCode, userResponse.ErrorMessage);
                    return userResponse;
                }
                _logger.LogInformation("{ClassName}.{MethodName} - Successfully updated subscription status json for ConsumerCode: {ConsumerCode}, TenantCode: {TenantCode}",
                    className, methodName, requestDto.ConsumerCode, requestDto.TenantCode);
                return userResponse;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} : Exception occurred while updating consumer subscription status json with ConsumerCode:{Code} and TenantCode:{Tenant}, ERROR: {Message}",
                    className, methodName, requestDto.ConsumerCode, requestDto.TenantCode, ex.Message);
                throw;
            }
        }
    }
}
