using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Sunny.Benefits.Bff.Core.Constants;
using Sunny.Benefits.Bff.Infrastructure.Repositories.Interfaces;
using Sunny.Benefits.Bff.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.User.Core.Domain.Dtos;

namespace Sunny.Benefits.Bff.Infrastructure.Services
{
    public class ConsumerActivityService : IConsumerActivityService
    {
        private readonly IUserClient _userClient;
        private readonly ILogger<ConsumerActivityService> _logger;
        private const string className = nameof(ConsumerActivityService);

        public ConsumerActivityService(ILogger<ConsumerActivityService> logger, IUserClient userClient)
        {
            _userClient = userClient;
            _logger = logger;
        }
        /// <summary>
        /// Handles the creation of a consumer activity.
        /// </summary>
        /// <param name="consumerActivityRequestDto">
        /// The DTO containing details of the consumer activity, such as TenantCode, ConsumerCode, ActivitySource, ActivityType, and ActivityJson.
        /// </param>
        /// <returns>
        /// A <see cref="ConsumerActivityResponseDto"/> object indicating the success of the operation.
        /// </returns>
        /// <exception cref="Exception">
        /// Thrown when an error occurs during the creation of the consumer activity.
        /// </exception>
        public async Task<ConsumerActivityResponseDto> CreateConsumerActivityAsync(ConsumerActivityRequestDto consumerActivityRequestDto)
        {
            const string methodName = nameof(ConsumerActivityService);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName}: Started processing create consumer activity for TenantCode:{Code},ConsumerCode:{Consumer}",
                       className, methodName, consumerActivityRequestDto.TenantCode, consumerActivityRequestDto.ConsumerCode);

                var response = await _userClient.Post<ConsumerActivityResponseDto>(CommonConstants.ConsumerActivityApiUrl, consumerActivityRequestDto);

                if (response.ErrorCode != null)
                {
                    _logger.LogError("{ClassName}.{MethodName}: Error processing for TenantCode:{TenantCode},ConsumerCode:{ConsumerCode},ErrorCode:{ErrorCode},ERROR:{Error}",
                           className, methodName, consumerActivityRequestDto.TenantCode, consumerActivityRequestDto.ConsumerCode, response.ErrorCode, response.ErrorMessage);
                    return response;
                }

                _logger.LogInformation("{ClassName}.{MethodName}: Successfully created consumer activity for TenantCode:{Code},ConsumerCode:{Consumer}",
                            className, methodName, consumerActivityRequestDto.TenantCode, consumerActivityRequestDto.ConsumerCode);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: Error processing for TenantCode:{TenantCode},ConsumerCode:{ConsumerCode},ErrorCode:{ErrorCode},ERROR:{Error}",
                        className, methodName, consumerActivityRequestDto.TenantCode, consumerActivityRequestDto.ConsumerCode, StatusCodes.Status500InternalServerError, ex.Message);
                return new ConsumerActivityResponseDto()
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = ex.Message
                };

            }
        }
    }
}
