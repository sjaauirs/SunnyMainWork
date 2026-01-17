using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Sunny.Benefits.Bff.Core.Domain.Constants;
using Sunny.Benefits.Bff.Core.Domain.Dtos;
using Sunny.Benefits.Bff.Core.Domain.Enums;
using Sunny.Benefits.Bff.Infrastructure.Helpers.Interface;
using Sunny.Benefits.Bff.Infrastructure.HttpClients.Interfaces;
using Sunny.Benefits.Bff.Infrastructure.Services.Interfaces;
using SunnyBenefits.Fis.Core.Domain.Dtos;

namespace Sunny.Benefits.Bff.Infrastructure.Services
{
    public class CardReissueService : ICardReissueService
    {
        private readonly ILogger<CardReissueService> _logger;
        private readonly IFisClient _fisClient;
        private readonly ICardOperationsHelper _cardOperationHelper;
        private const string className = nameof(CardReissueService);

        public CardReissueService(ILogger<CardReissueService> logger, IFisClient fisClient, ICardOperationsHelper cardOperationsHelper)
        {
            _logger = logger;
            _fisClient = fisClient;
            _cardOperationHelper = cardOperationsHelper;
        }
        public async Task<ExecuteCardReissueResponseDto> ExecuteCardReissue(CardReissueRequestDto cardReissueRequestDto)
        {
            const string methodName = nameof(ExecuteCardReissue);
            try
            {
                var fisGetStatus = await _fisClient.Post<CardOperationResponseDto>(CardOperationConstants.FisCardStatusApiUrl, cardReissueRequestDto);
                if (fisGetStatus == null)
                {
                    _logger.LogError("{ClassName}.{MethodName} - Null response received from fis/card-status for TenantCode:{TenantCode}, ConsumerCode :{ConsumerCode}",
                        className, methodName, cardReissueRequestDto.TenantCode, cardReissueRequestDto.ConsumerCode);
                    throw new InvalidOperationException("Null response received from fis/card-status");
                }

                if (fisGetStatus.ErrorCode != null)
                {
                    _logger.LogInformation("{ClassName}.{MethodName} - Card Status response ErrorCode received from fis/card-status for TenantCode:{TenantCode}, ConsumerCode :{ConsumerCode}, ErrorCode:{ErrorCode}",
                        className, methodName, cardReissueRequestDto.TenantCode, cardReissueRequestDto.ConsumerCode, fisGetStatus.ErrorCode);
                    return new ExecuteCardReissueResponseDto { ErrorCode = fisGetStatus.ErrorCode };
                }

                var cardStatus = _cardOperationHelper.ExtractCardStatusFromFisResponse(fisGetStatus.FisResponse);

                if (cardStatus?.ToUpper().Trim() == nameof(CardStatus.SUSPENDED))
                {
                    await UpdateCardStatus(CardOperationConstants.FisSetCardStatus, cardReissueRequestDto);
                    fisGetStatus = await _fisClient.Post<CardOperationResponseDto>(CardOperationConstants.FisCardStatusApiUrl, cardReissueRequestDto);
                    cardStatus = _cardOperationHelper.ExtractCardStatusFromFisResponse(fisGetStatus.FisResponse);
                }
                if (cardStatus?.ToUpper().Trim() == nameof(CardStatus.READY) || cardStatus?.ToUpper().Trim() == nameof(CardStatus.ACTIVE))
                {
                    var response = await _fisClient.Post<CardReissueResponseDto>(CardOperationConstants.FisCardReissueApiUrl, cardReissueRequestDto);
                    _logger.LogInformation("{ClassName}.{MethodName} - Executed card operation Successfully for TenantCode:{TenantCode}, ConsumerCode :{ConsumerCode}",
                        className, methodName, cardReissueRequestDto.TenantCode, cardReissueRequestDto.ConsumerCode);
                    if (response.ErrorCode != null)
                    {
                        return new ExecuteCardReissueResponseDto { ErrorCode = response.ErrorCode };
                    }
                    return new ExecuteCardReissueResponseDto
                    {
                        Success = true
                    };
                }
                else
                {
                    _logger.LogError("{ClassName}.{MethodName} - Statust of card must be ready or active for TenantCode:{TenantCode}, ConsumerCode :{ConsumerCode}, ErrorCode:{ErrorCode}",
                        className, methodName, cardReissueRequestDto.TenantCode, cardReissueRequestDto.ConsumerCode, StatusCodes.Status412PreconditionFailed);
                    return new ExecuteCardReissueResponseDto { ErrorCode = StatusCodes.Status412PreconditionFailed, ErrorDescription = "Statust of card must be ready or active" };
                }

            }
            catch (Exception ex){

                _logger.LogError(ex, "{ClassName}.{MethodName} - Error Occurred While performing card operation,TenantCode:{TenantCode}, ConsuemrCode :{ConsumerCode}, ErrorCode:{ErrorCode}, ERROR: {Message}", 
                    className, methodName, cardReissueRequestDto.TenantCode ,cardReissueRequestDto.ConsumerCode, StatusCodes.Status500InternalServerError,ex.Message);
                throw new InvalidOperationException("ExecuteCardReissue :Error ", ex);
            }
        }

        private async Task UpdateCardStatus(string cardStatus, CardReissueRequestDto requestDto)
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
