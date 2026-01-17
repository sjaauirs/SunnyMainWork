using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SunnyBenefits.Fis.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Core.Domain.Constants;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services
{
    public class CardOperationService : ICardOperationService
    {
        private readonly ILogger<CardOperationService> _logger;
        private readonly IFisClient _fisClient;
        private const string className = nameof(CardOperationService);

        public CardOperationService(ILogger<CardOperationService> logger, IFisClient fisClient)
        {
            _logger = logger;
            _fisClient = fisClient;
        }

        public async Task<ConsumerCardsStatusResponseDto> GetConsumerCardsStatus(GetCardStatusRequestDto requestDto)
        {
            const string methodName = nameof(GetConsumerCardsStatus);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName}: Fetching ConsumerAccount Details for TenantCode:{Tenant}, Consumer:{Consumer}", className, methodName, requestDto.TenantCode, requestDto.ConsumerCode);
                var response = await _fisClient.Post<ConsumerCardsStatusResponseDto>(Constant.ConsumerCardsStatusAPIUrl, requestDto);
                if (response.ErrorCode != null)
                {
                    _logger.LogError("{ClassName}.{MethodName} - Consumer Cards status Details Not Found for ConsumerCode : {Consumer}", className, methodName, requestDto.ConsumerCode);
                    return new ConsumerCardsStatusResponseDto { ErrorCode = response.ErrorCode, ErrorMessage = response.ErrorMessage };
                }
                _logger.LogInformation("{ClassName}.{MethodName}: Successfully Fetched Consumer Cards statuses for ConsumerCode:{Consumer}", className, methodName, requestDto.ConsumerCode);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Error occured while fetching Consumer cards status Details, ConsumerCode : {Consumer} - ErrorCode:{ErrorCode}, ERROR:{ErrorMessage}", className, methodName, requestDto.ConsumerCode, StatusCodes.Status500InternalServerError, ex.Message);
                throw;
            }
        }
    }
}
