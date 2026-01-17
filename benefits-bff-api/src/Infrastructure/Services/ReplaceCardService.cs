using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Sunny.Benefits.Bff.Core.Domain.Constants;
using Sunny.Benefits.Bff.Core.Domain.Dtos;
using Sunny.Benefits.Bff.Core.Domain.Enums;
using Sunny.Benefits.Bff.Infrastructure.Helpers;
using Sunny.Benefits.Bff.Infrastructure.Helpers.Interface;
using Sunny.Benefits.Bff.Infrastructure.HttpClients.Interfaces;
using Sunny.Benefits.Bff.Infrastructure.Services.Interfaces;
using SunnyBenefits.Fis.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.enums;

namespace Sunny.Benefits.Bff.Infrastructure.Services
{
    public class ReplaceCardService : IReplaceCardService
    {
        private readonly ILogger<ReplaceCardService> _logger;
        private readonly IFisClient _fisClient;
        private readonly ICardOperationsHelper _CardOperationHelper;
        private readonly IPersonHelper _personHelper;
        private const string className = nameof(ReplaceCardService);

        public ReplaceCardService(ILogger<ReplaceCardService> logger, IFisClient fisClient, ICardOperationsHelper cardOperationsHelper, IPersonHelper personHelper)
        {
            _logger = logger;
            _fisClient = fisClient;
            _CardOperationHelper = cardOperationsHelper;
            _personHelper = personHelper;
        }

        public async Task<ExecuteReplaceCardResponseDto> ExecuteCardReplacement(ReplaceCardRequestDto requestDto)
        {
            const string methodName = nameof(ExecuteCardReplacement);
            try
            {
                var fisGetStatus = await _fisClient.Post<CardOperationResponseDto>(CardOperationConstants.FisCardStatusApiUrl, requestDto);
                if (fisGetStatus == null)
                {
                    _logger.LogError("{ClassName}.{MethodName} - Null response received from fis/card-status for TenantCode:{TenantCode}, ConsumerCode :{ConsumerCode}", 
                        className,methodName, requestDto.TenantCode,requestDto.ConsumerCode);
                    throw new InvalidOperationException("Null response received from fis/card-status");
                }
         
                if (fisGetStatus.ErrorCode != null)
                {
                    _logger.LogInformation("{ClassName}.{MethodName} - Card Status response ErrorCode received from fis/card-status for TenantCode:{TenantCode}, ConsumerCode :{ConsumerCode}, ErrorCode:{ErrorCode}", 
                        className,methodName, requestDto.TenantCode, requestDto.ConsumerCode,fisGetStatus.ErrorCode);
                    return new ExecuteReplaceCardResponseDto { ErrorCode = fisGetStatus.ErrorCode };
                }

                var cardStatus = _CardOperationHelper.ExtractCardStatusFromFisResponse(fisGetStatus.FisResponse);

                if (cardStatus?.ToUpper().Trim() == nameof(CardStatus.SUSPENDED))
                {
                    await UpdateCardStatus(CardOperationConstants.FisSetCardStatus, requestDto);
                }

                await UpdateCardStatus(CardOperationConstants.FisSetCardStatusLost, requestDto);

                var response = await _fisClient.Post<ReplaceCardResponseDto>(CardOperationConstants.FisReplaceCardApiUrl, requestDto);
                _logger.LogInformation("{ClassName}.{MethodName} - card Replacement operation Successfull for TenantCode:{TenantCode}, ConsumerCode :{ConsumerCode}", 
                    className, methodName, requestDto.ConsumerCode,requestDto.TenantCode);

                if (response.ErrorCode != null)
                {
                    return new ExecuteReplaceCardResponseDto { ErrorCode = response.ErrorCode };
                }

                if (string.IsNullOrEmpty(response.ProxyNumber))
                {
                    _logger.LogError("{ClassName}.{MethodName} - Invalid response from FisAPI, Request Data: {RequestData}", className, methodName, requestDto.ToJson());
                    throw new InvalidOperationException("Invalid response from FisAPI");
                }
                
                return new ExecuteReplaceCardResponseDto()
                {
                    Success = true,
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Error Occurred While performing card operation,TenantCode:{TenantCode}, ConsumerCode :{ConsumerCode}, ErrorCode:{ErrorCode}, ERROR: {Message}", 
                    className, methodName, requestDto.TenantCode, requestDto.ConsumerCode, StatusCodes.Status500InternalServerError, ex.Message);
                throw;
            }
        }

        private async Task UpdateCardStatus(string cardStatus, ReplaceCardRequestDto requestDto)
        {
            var cardOperationRequest = new CardOperationRequestDto()
            {
                ConsumerCode = requestDto.ConsumerCode,
                TenantCode = requestDto.TenantCode,
                CardOperation = cardStatus
            };

            var setStatusResponse = await _fisClient.Post<CardOperationResponseDto>(CardOperationConstants.FisCardOperationApiUrl, cardOperationRequest);

            if (setStatusResponse.ErrorCode != null)
            {
                throw new InvalidOperationException($"Error occurred while updating card status: {setStatusResponse.ErrorCode}");
            }
        }
    }
}
