using Microsoft.Extensions.Logging;
using SunnyBenefits.Fis.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Tenant.Core.Domain.Models;
using SunnyRewards.Helios.User.Core.Domain.Models;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services
{

    public class FisValueAdjustStep : ISagaStep
    {
        private readonly IFisClient _fisClient;
        private readonly AdjustValueRequestDto _request;
        private readonly ILogger<FisValueAdjustStep> _logger;
        private bool _executed;

        public FisValueAdjustStep(IFisClient fisClient, AdjustValueRequestDto request, ILogger<FisValueAdjustStep> logger)
        {
            _fisClient = fisClient;
            _request = request;
            _logger = logger;
        }

        public async Task<BaseResponseDto> ExecuteAsync()
        {
            _logger.LogInformation("Executing FIS adjust for Consumer: {Consumer}, Amount: {Amount}", _request.ConsumerCode, _request.Amount);

            var response = await RetryHelper.ExecuteWithRetryAsync(async () =>
            {
                _logger.LogInformation("request: {@request}", _request.ToJson());
                var resp = await _fisClient.Post<AdjustValueResponseDto>("fis/adjust-value", _request);

                if (resp == null || resp.ErrorCode != null)
                    throw new Exception($"FIS adjust failed with code: {resp?.ErrorCode} for Consumer: {_request.ConsumerCode}, Amount: {_request.Amount}");

                return resp;
            },_logger, maxRetries: 3 ,initialDelayMs: 1000);

            _executed = true;
            _logger.LogInformation("FIS adjust [amount deducted] successful for Consumer: {Consumer},  Amount: {Amount}", _request.ConsumerCode, _request.Amount);

            return response;
        }

        public async Task<BaseResponseDto> CompensateAsync()
        {
            if (!_executed)
                return new BaseResponseDto { ErrorCode = 200, ErrorMessage = $"Skipped compensation (step not executed) - No duduction for { _request.ConsumerCode} " };

            var compensateRequest = new LoadValueRequestDto
            {
                ConsumerCode = _request.ConsumerCode,
                TenantCode = _request.TenantCode,
                PurseWalletType = _request.PurseWalletType,
                Amount = _request.Amount, 
                Currency = _request.Currency,
                MerchantName = Core.Domain.Constants.Constant.MerchantNameForFundTransfer,
                Comment = "Reverting the Decution as load in source wallet failed!"
            };

            _logger.LogWarning("Compensating FIS adjust, By adding back amount for Consumer: {Consumer}, Amount: {Amount}", compensateRequest.ConsumerCode, compensateRequest.Amount);

            var response = await RetryHelper.ExecuteWithRetryAsync(async () =>
            {
                _logger.LogInformation("request: {@request}", compensateRequest.ToJson());
                var resp = await _fisClient.Post<LoadValueResponseDto>("fis/load-value", compensateRequest);

                if (resp == null || resp.ErrorCode != null)
                    throw new Exception($"FIS Value Load failed for consumer : {compensateRequest.ConsumerCode}, Amount: {compensateRequest.Amount} with code: {resp?.ErrorCode}");

                return resp;
            }, maxRetries: 3, initialDelayMs: 1000);

            return response;
        }
    }


    public class FisValueLoadStep : ISagaStep
    {
        private readonly IFisClient _fisClient;
        private readonly LoadValueRequestDto _request;
        private readonly ILogger<FisValueLoadStep> _logger;
        private bool _executed;

        public FisValueLoadStep(IFisClient fisClient, LoadValueRequestDto request, ILogger<FisValueLoadStep> logger)
        {
            _fisClient = fisClient;
            _request = request;
            _logger = logger;
        }

        public async Task<BaseResponseDto> ExecuteAsync()
        {
            _logger.LogInformation("Executing FIS Value Load (Adding Amount) for Consumer: {Consumer}, Amount: {Amount}", _request.ConsumerCode, _request.Amount);

            var response = await RetryHelper.ExecuteWithRetryAsync(async () =>
            {
                _logger.LogInformation("request: {@request}", _request.ToJson());
                var resp = await _fisClient.Post<LoadValueResponseDto>("fis/load-value", _request);

                if (resp == null || resp.ErrorCode != null)
                    throw new Exception($"FIS load failed with code: {resp?.ErrorCode} for Consumer: {_request.ConsumerCode}, Amount: {_request.Amount}");

                return resp;
            }, maxRetries: 3, initialDelayMs: 1000);

            _executed = true;
            _logger.LogInformation("FIS load value [Amount Added] successful for Consumer: {Consumer} ,  Amount: {Amount}", _request.ConsumerCode , _request.Amount);

            return response;
        }

        public async Task<BaseResponseDto> CompensateAsync()
        {
            if (!_executed)
                return new BaseResponseDto { ErrorCode = 200, ErrorMessage = "Skipped compensation (step not executed) - Amount not Added for { _request.ConsumerCode} " };

            var compensateRequest = new AdjustValueRequestDto()
            {
                ConsumerCode = _request.ConsumerCode,
                TenantCode = _request.TenantCode,
                PurseWalletType = _request.PurseWalletType,
                Amount = _request.Amount, // reverse
                Currency = _request.Currency,
                Comment = $"Failed to create tranactions in wallet, reverting Added Amount, ConsumerCode : {_request.ConsumerCode} , Amount : {_request.Amount} "
            };

            _logger.LogWarning("Compensating previous Load for Consumer: {Consumer}, Amount: {Amount}", compensateRequest.ConsumerCode, compensateRequest.Amount);

            var response = await RetryHelper.ExecuteWithRetryAsync(async () =>
            {
                _logger.LogInformation("request: {@request}", compensateRequest.ToJson());
                var resp = await _fisClient.Post<AdjustValueResponseDto>("fis/adjust-value", compensateRequest);

                if (resp == null || resp.ErrorCode != null)
                    throw new Exception($"FIS adjust failed with code: {resp?.ErrorCode}");

                return resp;
            }, maxRetries: 3, initialDelayMs: 1000);

            return response;
        }
    }


    public class CreateTransactionStep : ISagaStep
    {
        private readonly IWalletClient _walletClient;
        private readonly CreateTransactionsRequestDto _request;      
        private readonly ILogger<CreateTransactionStep> _logger;
        private bool _executed;
        private CreateTransactionsResponseDto _createTransactionsResponseDto;

        public CreateTransactionStep(IWalletClient walletClient, CreateTransactionsRequestDto request, ILogger<CreateTransactionStep> logger)
        {
            _walletClient = walletClient;
            _request = request;
            _logger = logger;
        }

        public async Task<BaseResponseDto> ExecuteAsync()
        {
            _logger.LogInformation("Creating wallet transactions for Consumer: {Consumer}, Amount: {Amount}", _request.ConsumerCode, _request.TransactionAmount);

            var response = await RetryHelper.ExecuteWithRetryAsync(async () =>
            {
                _logger.LogInformation("request: {@request}", _request.ToJson());
                var resp = await _walletClient.Post<CreateTransactionsResponseDto>("transaction/create-transactions", _request);
                if (resp == null || resp.ErrorCode != null)
                    throw new Exception($"Wallet transaction failed with code: {resp?.ErrorCode}");
                _createTransactionsResponseDto = resp;
                return resp;
            }, maxRetries: 3 , initialDelayMs: 500);

            _executed = true;
            _logger.LogInformation("Wallet transaction created for Consumer: {Consumer}", _request.ConsumerCode);
            return response;
        }

        public async Task<BaseResponseDto> CompensateAsync()
        {
            var req = new RemoveTransactionsRequestDto() { 
            TransactionDetailId = _createTransactionsResponseDto.TransactionDetailId
            };
            var response = await RetryHelper.ExecuteWithRetryAsync(async () =>
            {
                _logger.LogInformation("request: {@request}", req.ToJson());
                var resp = await _walletClient.Post<BaseResponseDto>("transaction/remove-transactions", req);
                if (resp == null || resp.ErrorCode != null)
                    throw new Exception($"Wallet transaction failed with code: {resp?.ErrorCode}");
                return resp;
            }, maxRetries: 3, initialDelayMs: 500);

            _executed = true;
            _logger.LogInformation("Wallet transaction created for Consumer: {Consumer}", _request.ConsumerCode);
            return response;
        }


    }


}
