using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SunnyBenefits.Fis.Core.Domain.Dtos;
using SunnyBenefits.Fis.Core.Domain.Dtos.Json;
using SunnyRewards.Helios.Admin.Core.Domain.Constants;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services
{
    public class ConsumerPurseAssignmentService : IConsumerPurseAssignmentService
    {
        private readonly IFisClient _fisClient;
        private readonly IWalletClient _walletClient;
        private readonly IUserClient _userClient;
        private readonly ILogger<ConsumerPurseAssignmentService> _logger;
        private readonly ILoggerFactory _loggerFactory;
        const string className = nameof(ConsumerPurseAssignmentService);
        public ConsumerPurseAssignmentService(ILogger<ConsumerPurseAssignmentService> logger,
            IFisClient fisClient , IWalletClient walletClient,IUserClient userClient, ILoggerFactory loggerFactory)
        {
            _fisClient = fisClient;
            _logger = logger;
            _walletClient = walletClient;
            _userClient = userClient;
            _loggerFactory = loggerFactory;
        }
        /// <summary>
        /// Adds or removes a purse from a consumer account config based on the given action.
        /// </summary>
        /// <param name="tenantCode">Tenant identifier.</param>
        /// <param name="consumerCode">Unique consumer identifier.</param>
        /// <param name="purseWalletTypeCode">Wallet type code of the purse.</param>
        /// <param name="purseNumber">Purse number associated with the wallet type.</param>
        /// <param name="action">Action to perform: <c>Add</c> or <c>Remove</c>.</param>
        /// <returns>
        /// <see cref="BaseResponseDto"/> indicating success with the updated config, 
        /// or failure if tenant, consumer, or purse is not found.
        /// </returns

        public async Task<BaseResponseDto> ConsumerPurseAssignment(string tenantCode, string consumerCode, string purseWalletTypeCode,
            int purseNumber, string action , List<PlanCohortPurseMappingDto>? flexMapping = null)
        {
            const string methodName = nameof(ConsumerPurseAssignment);
            try
            {
                _logger.LogInformation(
                    "{Class}.{Method}:Starting ConsumerPurseAssignment for Tenant: {TenantCode}, Consumer: {ConsumerCode}, WalletType: {WalletType}, PurseNumber: {PurseNumber}, Action: {Action}",
                    className, methodName, tenantCode, consumerCode, purseWalletTypeCode, purseNumber, action);

                // Step 1: Get tenant purse
                var existingPurse = await GetTenantPurseAsync(tenantCode, purseWalletTypeCode, purseNumber);
                if (existingPurse == null)
                    return new BaseResponseDto { ErrorCode = StatusCodes.Status404NotFound };

                // Step 2: Get consumer account config
                var consumerAccountConfig = await GetConsumerAccountConfigAsync(tenantCode, consumerCode);
                if (consumerAccountConfig == null || consumerAccountConfig.PurseConfig?.Purses == null)
                {
                    consumerAccountConfig = new ConsumerAccountConfig
                    {
                        PurseConfig = new ConsumerAccountPurseConfig
                        {
                            Purses = new List<ConsumerAccountPurse>()
                        }
                    };
                }
                   

                // Step 3: Apply add/remove
                var response = await UpdateConsumerPurseList(consumerAccountConfig, existingPurse, action, consumerCode, tenantCode, flexMapping);

                return response ?? new BaseResponseDto { ErrorCode = StatusCodes.Status500InternalServerError };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "{Class}.{Method}: Unexpected error during ConsumerPurseAssignment for Tenant: {TenantCode}, Consumer: {ConsumerCode}",
                    className, methodName, tenantCode, consumerCode);
                throw;
            }
        }

        private async Task<Purse> GetTenantPurseAsync(string tenantCode, string purseWalletTypeCode, int purseNumber)
        {
            const string methodName = nameof(GetTenantPurseAsync);
            var tenantAccount = await _fisClient.Post<TenantAccountDto>(
                Constant.GetTenantAccount,
                new TenantAccountCreateRequestDto { TenantCode = tenantCode });

            if (tenantAccount == null)
            {
                _logger.LogError("{Class}.{Method}:Tenant account not found for TenantCode: {TenantCode}", className, methodName, tenantCode);
                return null;
            }

            TenantConfig tenantConfig;
            try
            {
                tenantConfig = JsonConvert.DeserializeObject<TenantConfig>(tenantAccount.TenantConfigJson);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Class}.{Method}:Failed to deserialize TenantConfig for TenantCode: {TenantCode}", className, methodName, tenantCode);
                return null;
            }

            var purse = tenantConfig?.PurseConfig?.Purses?
                .FirstOrDefault(p => p.PurseWalletType == purseWalletTypeCode && p.PurseNumber == purseNumber);

            if (purse == null)
                _logger.LogError("{Class}.{Method}:Purse not found in tenant config for WalletType: {WalletType}, PurseNumber: {PurseNumber}",
                    className, methodName, purseWalletTypeCode, purseNumber);

            return purse;
        }

        private async Task<ConsumerAccountConfig> GetConsumerAccountConfigAsync(string tenantCode, string consumerCode)
        {
            const string methodName = nameof(GetConsumerAccountConfigAsync);
            var consumerAccount = await _fisClient.Post<GetConsumerAccountResponseDto>(
                Constant.GetConsumerAccount,
                new GetConsumerAccountRequestDto { ConsumerCode = consumerCode, TenantCode = tenantCode });

            if (consumerAccount == null)
            {
                _logger.LogError("{Class}.{Method}:Consumer account not found for ConsumerCode: {ConsumerCode}, TenantCode: {TenantCode}",
                    className, methodName, consumerCode, tenantCode);
                return null;
            }

            try
            {
                return JsonConvert.DeserializeObject<ConsumerAccountConfig>(
                    consumerAccount.ConsumerAccount.ConsumerAccountConfigJson);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Class}.{Method}:Failed to deserialize ConsumerAccountConfig for ConsumerCode: {ConsumerCode}",
                    className, methodName, consumerCode);
                return null;
            }
        }

        private async Task<BaseResponseDto> UpdateConsumerPurseList(ConsumerAccountConfig consumerAccountConfig, Purse existingPurse,
            string action, string consumerCode, string tenantCode ,  List<PlanCohortPurseMappingDto>? flexMapping = null)
        {
            List<ConsumerAccountPurse> consumerPurses = consumerAccountConfig!.PurseConfig!.Purses!;
            ConsumerAccountPurse? addedPurse = null;
            ConsumerAccountPurse? removedPurse = null;


            const string methodName = nameof(UpdateConsumerPurseList);
            var matchingConsumerPurse = consumerPurses.FirstOrDefault(p => p.PurseLabel == existingPurse.PurseLabel);

            if (matchingConsumerPurse != null)
            {
                if (string.Equals(action, Constant.Remove, StringComparison.OrdinalIgnoreCase))
                {
                    if (matchingConsumerPurse.IsDeactivated)
                    {
                        return new BaseResponseDto() { };   // matching purse is already deactivated
                    }
                    matchingConsumerPurse.IsDeactivated = true;

                    //Get the purse Added - for every removal there should be a purse Added
                    PlanCohortPurseMappingDto? addedPurseMapping = null;
                    if (flexMapping?.Any() ?? false)
                    {
                        addedPurseMapping = flexMapping.FirstOrDefault();
                    }

                    ConsumerAccountPurse? addedConsumerPurse = null;
                    Purse? addedtenantPurse = null;

                    if (addedPurseMapping != null)
                    {
                        addedtenantPurse = await GetTenantPurseAsync(tenantCode, addedPurseMapping.PurseWalletTypeCode, int.Parse(addedPurseMapping.FisPurseNumber!));
                        addedConsumerPurse = consumerPurses.FirstOrDefault(p => p.PurseLabel == addedtenantPurse.PurseLabel);
                        if(addedConsumerPurse is not null)
                        {
                            addedConsumerPurse.Index = matchingConsumerPurse.Index;
                            matchingConsumerPurse.Index = consumerPurses.Count;
                        }
                        
                        _logger.LogInformation("{Class}.{Method}:purse Added was : {PurseLabel} for ConsumerCode: {ConsumerCode}",
                        className, methodName, addedtenantPurse.PurseLabel, consumerCode);
                    }


                    _logger.LogInformation("{Class}.{Method}:purse to de deactivated : {PurseLabel} for ConsumerCode: {ConsumerCode}",
                        className, methodName, existingPurse.PurseLabel, consumerCode);


                   await UpdateConsumerAccountAsync(tenantCode, consumerCode, consumerAccountConfig);
                    if (addedConsumerPurse != null)
                    {
                        //Show Banner- true
                        await updateConaumerAttributeForBanner(consumerCode, tenantCode);


                        _logger.LogInformation("{Class}.{Method}:Purse Deactivated -- Need to transfer fund from: {DeactivatedPurse} to {AddedPurse} for ConsumerCode: {ConsumerCode}",
                                                className, methodName, matchingConsumerPurse.PurseLabel, addedConsumerPurse.PurseLabel, consumerCode);
                        
                        await purseBalanceTransfer(consumerCode, tenantCode, addedConsumerPurse, addedtenantPurse,  matchingConsumerPurse , existingPurse);

                    }
                }
                else
                {
                    //we have a matching purse exist
                    matchingConsumerPurse.IsDeactivated = false; // activate again 
                    _logger.LogInformation("{Class}.{Method}:Purse already exists for ConsumerCode: {ConsumerCode}, skipping add.",
                        className, methodName, consumerCode);

                   await UpdateConsumerAccountAsync(tenantCode, consumerCode, consumerAccountConfig);
                }
            }
            else if (string.Equals(action, Constant.Add, StringComparison.OrdinalIgnoreCase))
            {

                consumerPurses.Add(new ConsumerAccountPurse
                {
                    Enabled = true,
                    IsFilteredSpend = existingPurse.IsFilteredSpend,
                    PurseLabel = existingPurse.PurseLabel,
                    RedemptionTarget = existingPurse.RedemptionTarget,
                    Index = consumerPurses.Count
                });
                _logger.LogInformation("Added purse {PurseLabel} for ConsumerCode: {ConsumerCode}",
                    existingPurse.PurseLabel, consumerCode);

                await UpdateConsumerAccountAsync(tenantCode, consumerCode, consumerAccountConfig);
            }

            return new BaseResponseDto();
        }

        
        public async Task<BaseResponseDto> purseBalanceTransfer(string consumerCode, string tenantCode, ConsumerAccountPurse addedPurse, Purse addedTenantPurse, ConsumerAccountPurse removedPurse, Purse removedTenantPurse)
        {
            var methodName = nameof(purseBalanceTransfer);
            _logger.LogInformation($"Starting Purse-To-Purse Fund Transfer for  CosnumerCode : {consumerCode} , tenantCode: {tenantCode} , AddedPurse : {addedPurse.PurseLabel} , RemovedPurse : {removedPurse.PurseLabel} ");

            // get external sync wallet details- Removed wallet
            var removedExtsyncConsumerWallets = await GetConsumerWalletByWalletType(consumerCode, removedTenantPurse!.PurseWalletType ?? "");
            var removedConsmerWallet = removedExtsyncConsumerWallets?.ConsumerWallets?.FirstOrDefault();
            WalletDto removedEsWallet = await _walletClient.GetById<WalletDto>("wallet/", removedConsmerWallet?.WalletId ?? 0);
            
            // get Consumer shadow wallet Details- For removed wallet
            var removedShadowConsumerWallets = await GetConsumerWalletByWalletType(consumerCode, removedTenantPurse!.MasterWalletType ?? "");
            var removedShadowConsmerWallet = removedShadowConsumerWallets?.ConsumerWallets?.FirstOrDefault();
            WalletDto removedshadowWallet = await _walletClient.GetById<WalletDto>("wallet/", removedShadowConsmerWallet?.WalletId ?? 0);


            // get external sync wallet details - get consumer wallet for the added purse
            var addedExtsyncConsumerWallets = await GetConsumerWalletByWalletType(consumerCode, addedTenantPurse!.PurseWalletType ?? "");
            var addedConsmerWallet = addedExtsyncConsumerWallets?.ConsumerWallets?.FirstOrDefault();
            WalletDto addedEswallet = await _walletClient.GetById<WalletDto>("wallet/", addedConsmerWallet?.WalletId ?? 0);
            WalletTypeDto addedWalletType = await _walletClient.GetById<WalletTypeDto>("wallet/walletTypeId?walletTypeId=", addedEswallet.WalletTypeId);

            // get Consumer shadow wallet Details-For Added wallet
            var addedShadowConsumerWallets = await GetConsumerWalletByWalletType(consumerCode, addedTenantPurse!.MasterWalletType ?? "");
            var addedShadowConsmerWallet = addedShadowConsumerWallets?.ConsumerWallets?.FirstOrDefault();
            WalletDto addedshadowWallet = await _walletClient.GetById<WalletDto>("wallet/", addedShadowConsmerWallet?.WalletId ?? 0);

            // get Balance of removed perse - Live balance
            ExternalSyncWalletResponseDto liveBalance = new ExternalSyncWalletResponseDto();
            try
            {
                ExternalSyncWalletRequestDto externalSyncWalletRequestDto = new ExternalSyncWalletRequestDto();
                externalSyncWalletRequestDto.ConsumerCode = consumerCode;
                externalSyncWalletRequestDto.TenantCode = tenantCode;

                liveBalance = await _fisClient.Post<ExternalSyncWalletResponseDto>("fis/get-purse-balances", externalSyncWalletRequestDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Live Balance details not found, ConsumerCode : {ConsumerCode}, ErrorCode:{ErrorCode}, ERROR: {Message} ",
                    className, methodName, consumerCode, StatusCodes.Status500InternalServerError, ex.Message);
                return new BaseResponseDto() { ErrorCode = StatusCodes.Status404NotFound, ErrorMessage = $"Live Balance details not found, ConsumerCode : {consumerCode}, ERROR: {ex.Message} " };

            }

            var liveRemovedPurse = liveBalance.Wallets.FirstOrDefault(p => p.PurseWalletType == removedTenantPurse.PurseWalletType);

            var TransferAmount = liveRemovedPurse?.Wallet.Balance ?? 0;

            if (TransferAmount <= 0)
            {
                _logger.LogInformation("{Class}.{Method}:No funds to transfer from Purse: {PurseLabel} for ConsumerCode: {ConsumerCode}",
                    className, methodName, removedTenantPurse.PurseLabel, consumerCode);
                return new BaseResponseDto() { }; // No funds to transfer
            }
            else
            {
                // make sure wallet balance is synced before proceeding
                if (removedEsWallet.Balance != TransferAmount)
                {
                    _logger.LogInformation("{Class}.{Method}:Wallet balance not synced for WalletId: {WalletId}. Expected: {ExpectedBalance}, Actual: {ActualBalance}",
                        className, methodName, removedEsWallet.WalletId, TransferAmount, removedEsWallet.Balance);

                    var walletLst = new List<WalletDto>();
                    removedEsWallet.Balance = TransferAmount;
                    walletLst.Add(removedEsWallet);
                    _logger.LogInformation("{ClassName}.{MethodName} - Updating Wallet Balance before Purse-To-Purse Transfer, WalletId : {WalletId}, Updated Balance : {Balance} ",
                        className, methodName, removedEsWallet.WalletId, removedEsWallet.Balance);
                    await _walletClient.Post<WalletDto>("wallet/update-wallet-balance", walletLst);
                }
            }

            // we have valid balance to transfer - proceed with saga

            var (sagaLogger, fisLogger, fisLoadLogger, walletLogger) = getLoggers();

           
            // remove fund request from removed purse
            var removeFundRequest = new AdjustValueRequestDto()
            {
                ConsumerCode = consumerCode,
                TenantCode = tenantCode,
                PurseWalletType = removedTenantPurse!.PurseWalletType,
                Amount = TransferAmount,
                PurseBalance = TransferAmount,
                Currency = "USD",
                Comment = $"Fund removed from {removedTenantPurse.PurseLabel} "
            };
            // add fund request to added purse
            var addFundRequest = new LoadValueRequestDto()
            {
                ConsumerCode = consumerCode,
                TenantCode = tenantCode,
                PurseWalletType = addedTenantPurse!.PurseWalletType,
                Amount = TransferAmount,
                Currency = "USD",
                Comment = $"Fund Added to {addedTenantPurse.PurseLabel} ",
                MerchantName = Constant.MerchantNameForFundTransfer
            };

            Func<long, long, string, string, CreateTransactionsRequestDto> build = (addedId, removedId, type, notes) =>
                new CreateTransactionsRequestDto
                {
                    ConsumerCode = consumerCode,
                    AddedWalletId = addedId,
                    RemovedWalletId = removedId,
                    TransactionAmount = Convert.ToDecimal(TransferAmount),
                    TransactionDetail = new TransactionDetailDto
                    {
                        ConsumerCode = consumerCode,
                        TransactionDetailType = type,
                        Notes = notes
                    }
                };

            var createTransactions1 = build(removedshadowWallet!.WalletId, removedEsWallet!.WalletId, "TRANSFER", $"Balance Transfer to {addedWalletType.WalletTypeLabel}");
            var createTransactions2 = build(addedshadowWallet!.WalletId, removedshadowWallet.WalletId, "TRANSFER", $"{removedTenantPurse.PurseLabel} Balance Transfer to {addedTenantPurse.PurseLabel}");
            var createTransactions3 = build(addedEswallet!.WalletId, addedshadowWallet.WalletId, "ADJUSTMENT", $"{removedTenantPurse.PurseLabel} Balance Transfer");

            var saga = new SagaExecutor(sagaLogger);
            // Step1: Substract the amount from removed purse---> add the amount to removed Consumer purse---> create transaction for removed Purse and cosbusmer wallet
            saga.AddStep(new FisValueAdjustStep(_fisClient, removeFundRequest, fisLogger));
            saga.AddStep(new CreateTransactionStep(_walletClient, createTransactions1, walletLogger)); 

            //Step2: move amount from consumer wallet(removed) ----> consumer wallet(added)
            saga.AddStep(new CreateTransactionStep(_walletClient, createTransactions2, walletLogger));

            // Step3: Move amount to Purse, remove from conusmer wallet
            saga.AddStep(new FisValueLoadStep(_fisClient, addFundRequest, fisLoadLogger));
            saga.AddStep(new CreateTransactionStep(_walletClient, createTransactions3, walletLogger));

            var result = await saga.ExecuteAsync();
            _logger.LogInformation("purse-2-Purse Transfer result: {Code} - {Msg}", result.ErrorCode, result.ErrorMessage);

            return result;
        }

        private async System.Threading.Tasks.Task updateConaumerAttributeForBanner(string consumerCode, string tenantCode)
        {
            var consumerAttributesRequestDto = new ConsumerAttributesRequestDto
            {
                TenantCode = tenantCode,
                ConsumerAttributes = new[]
                             {
                      new ConsumerAttributeDetailDto
                      {
                          ConsumerCode = consumerCode,
                          GroupName = "uiDisplayFlags",
                          AttributeName = "showNewPurseBanner",
                          AttributeValue = "true"
                      }
                  }
            };

            var response = await _userClient.Post<ConsumerAttributesResponseDto>("consumer/consumer-attributes", consumerAttributesRequestDto);
        }

        private (ILogger<SagaExecutor> SagaLogger,ILogger<FisValueAdjustStep> FisLogger,ILogger<FisValueLoadStep> FisLoadLogger, ILogger<CreateTransactionStep> transactionLogger ) getLoggers()
        {
            var sagaLogger = _loggerFactory.CreateLogger<SagaExecutor>();
            var fisLogger = _loggerFactory.CreateLogger<FisValueAdjustStep>();
            var fisLoadLogger = _loggerFactory.CreateLogger<FisValueLoadStep>();
            var walletLogger = _loggerFactory.CreateLogger<CreateTransactionStep>();

            return (sagaLogger, fisLogger, fisLoadLogger, walletLogger);
        }


        private async Task<FindConsumerWalletResponseDto> GetConsumerWalletByWalletType(string consumerCode, string walletTypeCode)
        {
            var findConsumerWalletByWalletTypeRequestDto = new FindConsumerWalletByWalletTypeRequestDto()
            {
                ConsumerCode = consumerCode,
                WalletTypeCode = walletTypeCode

            };
           var consumerWalletResponse = await _walletClient.Post<FindConsumerWalletResponseDto>("consumer-wallet/find-consumer-wallet-by-wallet-type", findConsumerWalletByWalletTypeRequestDto);
            return consumerWalletResponse;
        }

        private async Task<BaseResponseDto> UpdateConsumerAccountAsync(string tenantCode, string consumerCode,
            ConsumerAccountConfig consumerAccountConfig)
        {
            const string methodName = nameof(UpdateConsumerAccountAsync);
            var response = await _fisClient.Patch<ConsumerAccountUpdateResponseDto>(
                Constant.ConsumerAccountAPIUrl,
                new ConsumerAccountUpdateRequestDto
                {
                    ConsumerAccountConfig = consumerAccountConfig,
                    TenantCode = tenantCode,
                    ConsumerCode = consumerCode
                });
            
            if (response == null)
            {
                _logger.LogError("{Class}.{Method}:Failed to update consumer account for ConsumerCode: {ConsumerCode}", className, methodName, consumerCode);
                return null;
            }

            _logger.LogInformation("{Class}.{Method}:Successfully completed ConsumerPurseAssignment for ConsumerCode: {ConsumerCode}", className, methodName, consumerCode);
            return response;
        }
    }
}