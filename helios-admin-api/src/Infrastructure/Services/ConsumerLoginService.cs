using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos.Constants;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Cohort.Core.Domain.Dtos;
using SunnyRewards.Helios.Tenant.Core.Domain.Models;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services
{
    public class ConsumerLoginService : IConsumerLoginService
    {

        public readonly ILogger<ConsumerLoginService> _logger;
        public readonly IUserClient _userClient;
        public const string className = nameof(ConsumerLoginService);

        public ConsumerLoginService(ILogger<ConsumerLoginService> logger, IUserClient userClient)
        {
            _logger = logger;
            _userClient = userClient;
        }

        public ConsumerLoginDateResponseDto GetConsumerFirstLoginDate(dynamic request)
        {
            const string methodName = nameof(GetConsumerFirstLoginDate);
            try
            {
                if (String.IsNullOrEmpty(request))
                {
                    _logger.LogWarning("{ClassName}.{MethodName}: Invalid data format for getting consumer login details", className, methodName);
                    return new ConsumerLoginDateResponseDto
                    {
                        ErrorCode = StatusCodes.Status400BadRequest,
                        ErrorMessage = "Invalid data format"
                    };
                }
                var consumerCode = request.ToString();
                _logger.LogInformation("{ClassName}.{MethodName}: API Call to get consumer login date", className, methodName);
                IDictionary<string, long> parameters = new Dictionary<string, long>();

                ConsumerLoginDateResponseDto loginResponse = _userClient.Get<ConsumerLoginDateResponseDto>($"{Constant.ConsumerLoginDetailUrl}/{consumerCode}", parameters).GetAwaiter().GetResult();
                if (loginResponse.ErrorCode != null)
                {
                    _logger.LogWarning("{ClassName}.{MethodName}: Error occurred while getting consumer login date, ErrorCode: {ErrorCode}", className, methodName, loginResponse.ErrorCode);
                    return loginResponse;
                }

                _logger.LogInformation("{ClassName}.{MethodName}: consumer login date fetched successfully", className, methodName);
                return loginResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: Exception occurred while getting consumer login date. ErrorMessage: {ErrorMessage}, StackTrace: {StackTrace}", className, methodName, ex.Message, ex.StackTrace);
                return new ConsumerLoginDateResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError, ErrorMessage = ex.Message };
            }
        }
        public GetConsumerEngagementDetailResponseDto GetConsumerEngagementDetail(dynamic request)
        {
            const string methodName = nameof(GetConsumerEngagementDetail);
            try
            {
                var engagementDetailRequestDto = CreateConsumerRequestDto(request);

                if (engagementDetailRequestDto == null)
                {
                    _logger.LogWarning("{ClassName}.{MethodName}: Invalid data format for getting consumer login details", className, methodName);
                    return new GetConsumerEngagementDetailResponseDto
                    {
                        ErrorCode = StatusCodes.Status400BadRequest,
                        ErrorMessage = "Invalid data format"
                    };
                }
                _logger.LogInformation("{ClassName}.{MethodName}: API Call to get consumer login date", className, methodName);

                GetConsumerEngagementDetailResponseDto engagementResponse = _userClient.Post<GetConsumerEngagementDetailResponseDto>($"{Constant.ConsumerEngagementDetailUrl}", engagementDetailRequestDto).GetAwaiter().GetResult();
                if (engagementResponse.ErrorCode != null)
                {
                    _logger.LogWarning("{ClassName}.{MethodName}: Error occurred while getting consumer login date, ErrorCode: {ErrorCode}", className, methodName, engagementResponse.ErrorCode);
                    return engagementResponse;
                }

                _logger.LogInformation("{ClassName}.{MethodName}: consumer login date fetched successfully", className, methodName);
                return engagementResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: Exception occurred while getting consumer login date. ErrorMessage: {ErrorMessage}, StackTrace: {StackTrace}", className, methodName, ex.Message, ex.StackTrace);
                return new GetConsumerEngagementDetailResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError, ErrorMessage = ex.Message };
            }
        }
        private GetConsumerEngagementDetailRequestDto? CreateConsumerRequestDto(dynamic request)
        {
            const string methodName = nameof(CreateConsumerRequestDto);
            try
            {
                // Extract ConsumerCode
                string consumerCode = request.ConsumerCode?.ToString();
                DateTime engagementFrom;
                DateTime engagementUntil;
                if (string.IsNullOrWhiteSpace(consumerCode))
                {
                    throw new ArgumentException("consumerCode is required and cannot be null or empty.");
                }

                // Parse EngagementFrom
                if (!DateTime.TryParseExact(
        request.EngagementFrom?.ToString(),
        "yyyy-MM-ddTHH:mm:ss.fffZ",
        CultureInfo.InvariantCulture,
        DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
        out engagementFrom))
                {
                    throw new ArgumentException("EngagementFrom is required and must be a valid date.");
                }

                // Parse EngagementUntil
                if (!DateTime.TryParseExact(request.EngagementUntil?.ToString(),"yyyy-MM-ddTHH:mm:ss.fffZ",CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                     out engagementUntil))
                {
                    throw new ArgumentException("EngagementUntil is required and must be a valid date.");
                }

                // Create and return DTO
                return new GetConsumerEngagementDetailRequestDto
                {
                    ConsumerCode = consumerCode,
                    EngagementFrom = engagementFrom,
                    EngagementUntil = engagementUntil
                };
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex,
                    "{ClassName}.{MethodName}: Argument Exception occurred. ErrorMessage: {ErrorMessage}",
                    className, methodName, ex.Message);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "{ClassName}.{MethodName}: Exception occurred while creating GetConsumerEngagementDetailRequestDto. ErrorMessage: {ErrorMessage}, StackTrace: {StackTrace}",
                    className, methodName, ex.Message, ex.StackTrace);
                return null;
            }
        }



    }
}
