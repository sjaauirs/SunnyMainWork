using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SunnyBenefits.Fis.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Core.Domain.Constants;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services
{
    public class ConsumerAccountService : IConsumerAccountService
    {
        private readonly ILogger<ConsumerAccountService> _logger;
        private readonly IFisClient _fisClient;
        private const string className = nameof(ConsumerAccountService);

        public ConsumerAccountService(ILogger<ConsumerAccountService> logger, IFisClient fisClient)
        {
            _logger = logger;
            _fisClient = fisClient;
        }
        public async Task<ConsumerAccountDto> CreateConsumerAccount(CreateConsumerAccountRequestDto requestDto)
        {
            try
            {
                var consumerAccount = await _fisClient.Post<ConsumerAccountDto>("create-consumer-account", requestDto);
                if (consumerAccount.ConsumerAccountId <= 0)
                {
                    var errorMessage = $"Consumer account creation failed. TenantCode: {requestDto.TenantCode}, ConsumerCode: {requestDto.ConsumerCode}";
                    _logger.LogError(errorMessage);
                    throw new Exception(errorMessage);
                }
                _logger.LogInformation($"CreateConsumerAccount: Consumer account created successfully. ConsumerAccountId:{consumerAccount.ConsumerAccountId}");
                return consumerAccount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateConsumerAccount: Error:{Message}", ex.Message);
                throw;
            }
        }

        public async Task<GetConsumerAccountResponseDto> GetConsumerAccount(GetConsumerAccountRequestDto requestDto)
        {
            try
            {
                var consumerAccountResponose = await _fisClient.Post<GetConsumerAccountResponseDto>("get-consumer-account", requestDto);
                return consumerAccountResponose;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateConsumerAccount: Error:{Message}", ex.Message);
                throw;
            }
        }

        public async Task<ConsumerAccountUpdateResponseDto> UpdateConsumerAccountConfig(ConsumerAccountUpdateRequestDto consumerAccountUpdateRequest)
        {
            const string methodName = nameof(UpdateConsumerAccountConfig);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName}: Fetching ConsumerAccount Details for TenantCode:{Tenant}, Consumer:{Consumer}", className, methodName, consumerAccountUpdateRequest.TenantCode, consumerAccountUpdateRequest.ConsumerCode);
                var response = await _fisClient.Patch<ConsumerAccountUpdateResponseDto>(Constant.ConsumerAccountAPIUrl, consumerAccountUpdateRequest);
                if (response.ErrorCode != null)
                {
                    _logger.LogError("{ClassName}.{MethodName} - Consumer Account Details Not Found for ConsumerCode : {Consumer}", className, methodName, consumerAccountUpdateRequest.ConsumerCode);
                    return new ConsumerAccountUpdateResponseDto { ErrorCode = response.ErrorCode, ErrorMessage = response.ErrorMessage };
                }
                _logger.LogInformation("{ClassName}.{MethodName}: Successfully Updated Consumer Account Config for ConsumerCode:{Consumer}", className, methodName, consumerAccountUpdateRequest.ConsumerCode);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Error occured while Updating ConsumerAccount Details, ConsumerCode : {Consumer} - ErrorCode:{ErrorCode}, ERROR:{ErrorMessage}", className, methodName, consumerAccountUpdateRequest.ConsumerCode, StatusCodes.Status500InternalServerError, ex.Message);
                throw;
            }
        }
    }
}
