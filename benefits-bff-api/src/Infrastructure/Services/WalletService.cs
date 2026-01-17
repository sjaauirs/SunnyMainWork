using FluentNHibernate.Conventions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Sunny.Benefits.Bff.Core.Constants;
using Sunny.Benefits.Bff.Core.Domain.Constants;
using Sunny.Benefits.Bff.Core.Domain.Dtos;
using Sunny.Benefits.Bff.Infrastructure.HttpClients.Interfaces;
using Sunny.Benefits.Bff.Infrastructure.Repositories.Interfaces;
using Sunny.Benefits.Bff.Infrastructure.Services.Interfaces;
using SunnyBenefits.Fis.Core.Domain.Dtos;
using SunnyBenefits.Fis.Core.Domain.Dtos.Json;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos.Json;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;
using SunnyRewards.Helios.Wallet.Core.Domain.Models;

namespace Sunny.Benefits.Bff.Infrastructure.Services
{
    public class WalletService : IWalletService
    {
        private readonly ILogger<WalletService> _walletServiceLogger;
        private readonly IWalletClient _walletClient;
        private readonly ITaskClient _taskClient;
        private readonly ITenantService _tenantService;
        private readonly IFisClient _fisClient;
        private readonly IUserClient _userClient;
        private readonly IVault _vault;
        private const string className = nameof(WalletService);

        /// <summary>
        ///
        /// </summary>
        /// <param name="walletServiceLogger"></param>
        /// <param name="walletClient"></param>
        /// <param name="taskClient"></param>
        public WalletService(ILogger<WalletService> walletServiceLogger, IWalletClient walletClient, ITaskClient taskClient, ITenantService tenantService
            , IFisClient fisClient, IUserClient userClient, IVault vault)
        {
            _walletServiceLogger = walletServiceLogger;
            _walletClient = walletClient;
            _taskClient = taskClient;
            _tenantService = tenantService;
            _fisClient = fisClient;
            _userClient = userClient;
            _vault = vault;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="findConsumerWalletRequestDto"></param>
        /// <returns></returns>
        public async Task<WalletResponseDto> GetWallets(FindConsumerWalletRequestDto findConsumerWalletRequestDto, TenantDto? tenant = null)
        {
            const string methodName = nameof(GetWallets);
            try
            {
                var wallets = await _walletClient.Post<WalletResponseDto>("wallet/get-wallets", findConsumerWalletRequestDto);

                if (wallets.walletDetailDto == null || wallets.walletDetailDto?.Length == 0)
                {
                    _walletServiceLogger.LogError("{ClassName}.{MethodName} - Wallet details not found for consumercode : {ConsumerCode}", className, methodName, findConsumerWalletRequestDto.ConsumerCode);
                    return new WalletResponseDto()
                    {
                        ErrorCode = wallets.ErrorCode
                    };
                }

                var filterWallets = wallets.walletDetailDto?.Where(x => x.WalletType.IsExternalSync).ToArray();


                if (filterWallets?.Length == 0)
                {
                    _walletServiceLogger.LogError("{ClassName}.{MethodName} - No wallets found with external sync for consumercode : {ConsumerCode}", className, methodName, findConsumerWalletRequestDto.ConsumerCode);
                    return new WalletResponseDto()
                    {
                        ErrorCode = wallets.ErrorCode
                    };
                }
                else
                {
                    filterWallets = await PickAPurseOnboardEnabledProcess(findConsumerWalletRequestDto, filterWallets, tenant);
                    if (filterWallets?.Length == 0)
                    {
                        _walletServiceLogger.LogError("{ClassName}.{MethodName} - No wallets found in ConsumerAccountConfig Json for consumercode : {ConsumerCode}", className, methodName, findConsumerWalletRequestDto.ConsumerCode);
                        return new WalletResponseDto()
                        {
                            ErrorCode = StatusCodes.Status404NotFound
                        };
                    }

                    filterWallets = await GetUpdatedWalletBalance(findConsumerWalletRequestDto.ConsumerCode,
                        filterWallets ?? Array.Empty<WalletDetailDto>(), tenant);
                    filterWallets = await GetFundingDescription(findConsumerWalletRequestDto.ConsumerCode, 
                        filterWallets ?? Array.Empty<WalletDetailDto>(), tenant?.TenantCode ?? string.Empty);
                }

                double filterGrandTotal = Math.Round((double)filterWallets.Sum(x => Convert.ToDecimal(x.Wallet.Balance)), 2, MidpointRounding.AwayFromZero);

                _walletServiceLogger.LogInformation("{ClassName}.{MethodName} - Retrieved GetWallets Successfully for ConsumerCode : {ConsumerCode}", className, methodName, findConsumerWalletRequestDto.ConsumerCode);
                return new WalletResponseDto { walletDetailDto = filterWallets, GrandTotal = filterGrandTotal, ErrorCode = wallets.ErrorCode };
            }
            catch (Exception ex)
            {
                _walletServiceLogger.LogError(ex, "{ClassName}.{MethodName} - Error occured while retrieving GetWallets for ConsumerCode : {ConsumerCode}, ErrorCode:{ErrorCode}, ERROR: {Msg}",
                    className, methodName, findConsumerWalletRequestDto?.ConsumerCode, StatusCodes.Status500InternalServerError, ex.Message);
                return new WalletResponseDto();
            }
        }

        /// <summary>
        /// This method processes the onboarding of a consumer for the "Pick-A-PurseEnabled" feature based on the tenant Attribute Json's configuration.
        /// It checks if the feature is enabled for the tenant and filters the available wallets accordingly.
        /// If the feature is disabled or no wallets are eligible, the original list of wallets is returned unmodified.
        /// </summary>
        /// <param name="findConsumerWalletRequestDto">Contains the consumer's details, including the ConsumerCode, for which the wallet details are being processed.</param>
        /// <param name="filterWallets">A list of wallet details that may be filtered based on the enabled purses for the consumer's tenant.</param>
        /// <returns>A filtered array of wallet details based on enabled purse types, or the original list of wallets if no filtering is required.</returns>
        private async Task<WalletDetailDto[]?> PickAPurseOnboardEnabledProcess(FindConsumerWalletRequestDto findConsumerWalletRequestDto,
            WalletDetailDto[]? filterWallets, TenantDto? tenantDetails)
        {
            const string methodName = nameof(PickAPurseOnboardEnabledProcess);
            try
            {
                if (findConsumerWalletRequestDto == null || string.IsNullOrWhiteSpace(findConsumerWalletRequestDto.ConsumerCode))
                {
                    _walletServiceLogger.LogError("{ClassName}.{methodname}: Invalid ConsumerCode:{Consumer}.", className, methodName, findConsumerWalletRequestDto?.ConsumerCode);
                    return filterWallets;
                }

                var consumerCode = findConsumerWalletRequestDto.ConsumerCode;

                var tenant = tenantDetails != null ? tenantDetails : (await _tenantService.GetTenantByConsumerCode(consumerCode)).Tenant;

                if (tenant == null)
                {
                    _walletServiceLogger.LogError("{ClassName}.{MethodName}: Tenant not found for ConsumerCode: {ConsumerCode}", className, methodName, consumerCode);
                    return filterWallets;
                }

                var tenantCode = tenant.TenantCode;
                var tenantAttributes = JsonConvert.DeserializeObject<TenantAttributeDto>(tenant.TenantAttribute ?? string.Empty);

                if (tenantAttributes?.PickAPurseOnboardingEnabled != true)
                {
                    return filterWallets;
                }

                // Fetch consumer account
                GetConsumerAccountResponseDto consumerAccount = await GetConsumerAccount(consumerCode, tenantCode);

                var consumerAccountConfig = JsonConvert.DeserializeObject<ConsumerAccountConfig>(consumerAccount?.ConsumerAccount?.ConsumerAccountConfigJson ?? string.Empty);
                if (consumerAccountConfig == null)
                {
                    return filterWallets;
                }


                var enabledPurses = consumerAccountConfig.PurseConfig?.Purses?.Where(p => p.Enabled)
                                                                               .Select(p => p.PurseLabel)
                                                                               .ToList() ?? new List<string?>();
                if(enabledPurses == null || enabledPurses.Count == 0)
                {
                    return filterWallets;
                }
                // Fetch tenant account
                var tenantAccount = await GetTenantAccount(tenantCode ?? string.Empty);
                var tenantConfig = JsonConvert.DeserializeObject<TenantConfigDto>(tenantAccount?.TenantConfigJson ?? string.Empty);
                if (tenantConfig == null)
                {
                    return filterWallets;
                }
                var enabledPurseWalletTypes = tenantConfig.PurseConfig?.Purses?.Where(p => enabledPurses.Contains(p.PurseLabel))
                                                                                   .Select(p => p.PurseWalletType)
                                                                                   .ToList() ?? new List<string?>();

                // Filter wallets based on enabled purse wallet types
                filterWallets = filterWallets?.Where(wallet => enabledPurseWalletTypes.Contains(wallet.WalletType.WalletTypeCode)).ToArray();
                if (filterWallets?.Length > 0)
                {
                    filterWallets = UpdateFilteredSpendFlag(filterWallets, consumerAccountConfig, tenantConfig);
                }

                _walletServiceLogger.LogInformation("{ClassName}.{MethodName}: Filtered Wallets based on the ConsumerAccountConfigJson PurseLabel. ConsumerCode:{Consumer}", className, methodName, consumerCode);
                return filterWallets;
            }
            catch (Exception ex)
            {
                _walletServiceLogger.LogError(ex, "Error occurred while processing PickAPurseOnboardEnabledProcess for ConsumerCode: {ConsumerCode}", findConsumerWalletRequestDto?.ConsumerCode);
                return filterWallets;
            }

        }

        private async Task<GetConsumerAccountResponseDto> GetConsumerAccount(string consumerCode, string? tenantCode)
        {
            var consumerAccountRequest = new GetConsumerAccountRequestDto { TenantCode = tenantCode, ConsumerCode = consumerCode };
            var consumerAccount = await _fisClient.Post<GetConsumerAccountResponseDto>(WalletConstants.GetConsumerAccount, consumerAccountRequest);
            return consumerAccount;
        }

        private async Task<TenantAccountDto> GetTenantAccount(string tenantCode)
        {
            var tenantAccount = await _fisClient.Post<TenantAccountDto>(WalletConstants.GetTenantAccountByTenantCode,
                        new TenantAccountCreateRequestDto { TenantCode = tenantCode });
            return tenantAccount;
        }
        private static WalletDetailDto[] UpdateFilteredSpendFlag(WalletDetailDto[] filterWallets, ConsumerAccountConfig consumerAccountConfig, TenantConfigDto tenantConfig)
        {
            if (consumerAccountConfig?.PurseConfig?.Purses == null)
            {
                return filterWallets;
            }

            var tenantPurses = tenantConfig?.PurseConfig?.Purses ?? Enumerable.Empty<PurseDto>();

            foreach (var purse in consumerAccountConfig.PurseConfig.Purses)
            {
                var matchingPurse = tenantPurses
                    .FirstOrDefault(tp => string.Equals(tp.PurseLabel, purse.PurseLabel, StringComparison.OrdinalIgnoreCase));

                if (matchingPurse == null) continue; // Skip if no matching purse is found

                var wallet = filterWallets.FirstOrDefault(w => w.WalletType.WalletTypeCode == matchingPurse.PurseWalletType);

                if (wallet == null) continue;

                wallet.IsFilteredSpend = purse.IsFilteredSpend;
               wallet.Wallet.Index = purse.Index;
               wallet.Wallet.IsDeactivated = purse.IsDeactivated;
            }
            return filterWallets;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="postGetTransactionsRequestDto"></param>
        /// <returns></returns>
        public async Task<TransactionBySectionResponseDto> GetTransactions(PostGetTransactionsRequestDto postGetTransactionsRequestDto)
        {
            const string methodName = nameof(GetTransactions);
            try
            {
                List<TransactionEntryDto> cardTransactions = new List<TransactionEntryDto>();
                var wallets = await GetWallets(new FindConsumerWalletRequestDto
                {
                    ConsumerCode = postGetTransactionsRequestDto.ConsumerCode
                });
                if (wallets.walletDetailDto == null || wallets.walletDetailDto.Length == 0)
                    return new TransactionBySectionResponseDto { ErrorCode = wallets.ErrorCode };
                List<long>? filterWalletsWalletIds;
                if (postGetTransactionsRequestDto.WalletId > 0)
                {
                    // Ensure the provided WalletId exists in the consumer's wallets
                    var walletExists = wallets.walletDetailDto.Any(w => w.Wallet.WalletId == postGetTransactionsRequestDto.WalletId);
                    if (!walletExists)
                    {
                        return new TransactionBySectionResponseDto
                        {
                            ErrorCode = StatusCodes.Status404NotFound,
                            ErrorMessage = $"No Wallet exists with the given walletId:{postGetTransactionsRequestDto.WalletId}"
                        };
                    }

                    filterWalletsWalletIds = new List<long> { postGetTransactionsRequestDto.WalletId.Value };
                }
                else
                {
                    filterWalletsWalletIds = wallets.walletDetailDto.Select(wallet => wallet.Wallet.WalletId).ToList();
                }

                Func<TransactionEntryDto, bool> today = trs => trs.Transaction?.CreateTs >= DateTime.Today;
                Func<TransactionEntryDto, bool> thisMonth = trs => trs.Transaction?.CreateTs >= new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month,
                    1, 0, 0, 0, DateTimeKind.Utc)
                && trs.Transaction.CreateTs < DateTime.Today;

                Func<TransactionEntryDto, bool> allPrev = trs => trs.Transaction?.CreateTs < new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month,
                    1, 0, 0, 0, DateTimeKind.Utc);

                Dictionary<string, Func<TransactionEntryDto, bool>> transactionBuckets = new()
                {
                    { "Today", today },
                    { "This Month", thisMonth },
                    { "Previous Transactions", allPrev }
                };


                var walletTransactions = await _walletClient.Post<PostGetTransactionsResponseDto>("transaction/get-transactions",
                    postGetTransactionsRequestDto);

                if (walletTransactions.Transactions.Count == 0 && walletTransactions.ErrorCode != null)
                {
                    _walletServiceLogger.LogInformation("{ClassName}.{MethodName} - walletId is not associated this ConsumerCode : {ConsumerCode}, WalletId : {WalletId}",
                     className, methodName, postGetTransactionsRequestDto.ConsumerCode, postGetTransactionsRequestDto.WalletId);
                    return new TransactionBySectionResponseDto
                    {
                        ErrorCode = walletTransactions.ErrorCode
                    };
                }
                var tenant = await _tenantService.GetTenantByConsumerCode(postGetTransactionsRequestDto.ConsumerCode);

                cardTransactions = await GetCardTransactions(tenant?.Tenant, postGetTransactionsRequestDto.ConsumerCode, wallets.walletDetailDto);

                var bucketedTransaction = new TransactionBySectionResponseDto();
                foreach (var bucket in transactionBuckets)
                {
                    List<TransactionEntryDto> filteredTransactions = new List<TransactionEntryDto>();

                    filteredTransactions.AddRange(walletTransactions.Transactions.Where(bucket.Value).OrderByDescending(x => x.Transaction?.CreateTs).ToList());

                    if (cardTransactions != null && cardTransactions.Count > 0)
                    {
                        filteredTransactions.AddRange(cardTransactions.Where(bucket.Value).OrderByDescending(x => x.Transaction?.CreateTs).ToList());
                    }
                    var externalSyncWallet = filteredTransactions.Where(p => filterWalletsWalletIds.Any(p2 => p2 == p.Transaction?.WalletId)).ToList();

                    bucketedTransaction.Transaction?.Add(bucket.Key, externalSyncWallet);
                }
                var taskRewardCodeList = bucketedTransaction.Transaction?
                    .SelectMany(bucket => bucket.Value)
                    .Where(entry => !string.IsNullOrEmpty(entry.TransactionDetail?.TaskRewardCode))
                    .Select(entry => entry.TransactionDetail!.TaskRewardCode)
                    .ToList();

                bucketedTransaction.TaskReward = await GetTaskRewardDetail(taskRewardCodeList ?? new List<string?>());

                if (walletTransactions.Transactions?.Count <= 0)
                {
                    _walletServiceLogger.LogError("{ClassName}.{MethodName} - GetTransactions details not found  for consumercode : {ConsumerCode} , WalletId : {WalletId}",
                        className, methodName, postGetTransactionsRequestDto.ConsumerCode, postGetTransactionsRequestDto.WalletId);
                    return bucketedTransaction;
                }
                _walletServiceLogger.LogInformation("{ClassName}.{MethodName} - Retrieved GetTransaction Successfully for ConsumerCode : {ConsumerCode}, WalletId : {WalletId}",
                 className, methodName, postGetTransactionsRequestDto.ConsumerCode, postGetTransactionsRequestDto.WalletId);
                return bucketedTransaction;
            }
            catch (Exception ex)
            {
                _walletServiceLogger.LogError(ex, "{ClassName}.{MethodName} - Error occured while Retrieving GetTransaction for ConsumerCode : {ConsumerCode}, WalletId : {WalletId}, ErrorCode:{ErrorCode}, ERROR: {Msg}",
                    className, methodName, postGetTransactionsRequestDto?.ConsumerCode, postGetTransactionsRequestDto?.WalletId, StatusCodes.Status500InternalServerError, ex.Message);
                throw new InvalidOperationException("ERROR: GetTransactions ", ex);
            }
        }

        /// <summary>
        /// Gets the consumer benefits wallet types asynchronous.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        public async Task<ConsumerBenefitsWalletTypesResponseDto> GetConsumerBenefitsWalletTypesAsync(ConsumerBenefitsWalletTypesRequestDto request)
        {
            try
            {
                var consumerConfigWalletTypes = await _fisClient.Post<ConsumerBenefitsWalletTypesResponseDto>(WalletConstants.GetConsumerBenefitWalletTypesAPIUrl, request);

                if (consumerConfigWalletTypes == null || consumerConfigWalletTypes.ErrorCode.HasValue || consumerConfigWalletTypes.BenefitsWalletTypes.Count == 0)
                {
                    _walletServiceLogger.LogWarning("{ClassName}.{MethodName} - Error occurred while retrieving Consumer Benefits Wallet Types for TenantCode: {TenantCode}, ConsumerCode: {ConsumerCode}, ErrorCode: {ErrorCode}",
                        className, nameof(GetConsumerBenefitsWalletTypesAsync), request.TenantCode, request.ConsumerCode, consumerConfigWalletTypes?.ErrorCode);

                    var tenantAccount = await _fisClient.Post<TenantAccountDto>(WalletConstants.GetTenantAccountByTenantCode,
                        new TenantAccountCreateRequestDto { TenantCode = request.TenantCode });
                    if (tenantAccount == null || tenantAccount.ErrorCode.HasValue)
                    {
                        _walletServiceLogger.LogError("{ClassName}.{MethodName} - Error occurred while retrieving tenant account for TenantCode: {TenantCode}, ErrorCode: {ErrorCode}",
                        className, nameof(GetConsumerBenefitsWalletTypesAsync), request.TenantCode, tenantAccount?.ErrorCode);

                        return new ConsumerBenefitsWalletTypesResponseDto
                        {
                            ErrorCode = tenantAccount?.ErrorCode ?? StatusCodes.Status500InternalServerError
                        };
                    }
                    var tenantConfig = JsonConvert.DeserializeObject<TenantConfigDto>(tenantAccount?.TenantConfigJson ?? string.Empty);
                    if (tenantConfig == null || tenantConfig.PurseConfig?.Purses == null)
                    {
                        _walletServiceLogger.LogError("{ClassName}.{MethodName} - Tenant account's tenantConfigJson value is null for TenantCode: {TenantCode}",
                        className, nameof(GetConsumerBenefitsWalletTypesAsync), request.TenantCode);

                        return new ConsumerBenefitsWalletTypesResponseDto
                        {
                            ErrorCode = StatusCodes.Status500InternalServerError
                        };
                    }
                    var ConsumerBenefitWalletTypes = (from walletType in tenantConfig.PurseConfig?.Purses
                                                      where walletType.PickAPurseStatus != WalletConstants.DisabledPickAPurseStatus
                                                      select new ConsumerBenefitWalletTypeDto
                                                      {
                                                          PurseLabel = walletType.PurseLabel,
                                                          WalletType = walletType.PurseWalletType,
                                                          RedemptionTarget = walletType.RedemptionTarget,
                                                          IsFilteredSpend = walletType.IsFilteredSpend
                                                      }).ToList();
                    consumerConfigWalletTypes = new ConsumerBenefitsWalletTypesResponseDto
                    {
                        BenefitsWalletTypes = ConsumerBenefitWalletTypes
                    };
                }

                return consumerConfigWalletTypes;
            }
            catch (Exception ex)
            {
                _walletServiceLogger.LogError(ex, "An exception occurred in {ClassName}.{MethodName}: {ErrorMessage}", nameof(WalletService), nameof(GetConsumerBenefitsWalletTypesAsync), ex.Message);
                throw;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskRewardCodeList"></param>
        /// <returns></returns>
        private async Task<Dictionary<string, IEnumerable<TaskRewardDetailDto>>> GetTaskRewardDetail(List<string?> taskRewardCodeList)
        {
            if (taskRewardCodeList.Count > 0)
            {
                var taskRewardDetail = new Dictionary<string, IEnumerable<TaskRewardDetailDto>>();
                foreach (var taskRewardCode in taskRewardCodeList)
                    if (taskRewardCode != null)

                    {
                        var taskReward = await GetTaskRewardByCode(taskRewardCode);
                        taskRewardDetail[taskRewardCode] = new List<TaskRewardDetailDto>() { taskReward };
                    }

                return taskRewardDetail;
            }
            return new Dictionary<string, IEnumerable<TaskRewardDetailDto>>();
        }

        private async Task<TaskRewardDetailDto> GetTaskRewardByCode(string taskRewardCode)
        {
            const string methodName = nameof(GetTaskRewardByCode);
            var taskRewardRequestDto = new GetTaskRewardByCodeRequestDto()
            {
                TaskRewardCode = taskRewardCode
            };
            var taskReward = await _taskClient.Post<GetTaskRewardByCodeResponseDto>("get-task-reward-by-code", taskRewardRequestDto);
            if (taskReward.TaskRewardDetail == null)
            {
                _walletServiceLogger.LogError("{ClassName}.{MethodName} - Task Reward Details Not Found for TaskRewardCode : {TaskRewardCode}", className, methodName, taskRewardCode);
                return new TaskRewardDetailDto();
            }
            _walletServiceLogger.LogInformation("{ClassName}.{MethodName} - Retrieved Task Reward By Code Successfully for TaskRewardCode : {TaskRewardCode}", className, methodName, taskRewardCode);
            return taskReward.TaskRewardDetail;
        }
        private async Task<string?> GetTenantCode(string? consumerCode)
        {
            const string methodName = nameof(GetTenantCode);
            var consumerSummaryRequestDto = new BaseRequestDto()
            {
                consumerCode = consumerCode,
            };
            var consumer = await _userClient.Post<GetConsumerResponseDto>(WalletConstants.getConsumerApi, consumerSummaryRequestDto);
            if (consumer.Consumer == null)
            {
                _walletServiceLogger.LogError("{ClassName}.{MethodName} - Consumer Details Not Found For ConsumerCode : {ConsumerCode}", className, methodName, consumerCode);
                return string.Empty;
            }
            _walletServiceLogger.LogInformation("{ClassName}.{MethodName} - Retrieved Consumer Details Successfully for ConsumerCode : {ConsumerCode}", className, methodName, consumerCode);

            return consumer.Consumer.TenantCode;
        }
        private async Task<WalletDetailDto[]> GetUpdatedWalletBalance(string? consumerCode, WalletDetailDto[] filterWallets, TenantDto? tenant)
        {
            const string methodName = nameof(GetUpdatedWalletBalance);
            ExternalSyncWalletResponseDto liveBalance = new ExternalSyncWalletResponseDto();

            try
            {
                ExternalSyncWalletRequestDto externalSyncWalletRequestDto = new ExternalSyncWalletRequestDto();
                externalSyncWalletRequestDto.ConsumerCode = consumerCode;
                externalSyncWalletRequestDto.TenantCode = tenant != null
                    ? tenant.TenantCode
                    : (await GetTenantCode(consumerCode));

                liveBalance = await _fisClient.Post<ExternalSyncWalletResponseDto>(WalletConstants.liveBalanceApi, externalSyncWalletRequestDto);
            }
            catch (Exception ex)
            {
                _walletServiceLogger.LogError(ex, "{ClassName}.{MethodName} - Live Balance details not found, ConsumerCode : {ConsumerCode}, ErrorCode:{ErrorCode}, ERROR: {Message} ",
                    className, methodName, consumerCode, StatusCodes.Status500InternalServerError, ex.Message);
                return filterWallets;

            }
            if (liveBalance.ErrorCode.HasValue || liveBalance.Wallets == null)
            {

                _walletServiceLogger.LogError("{ClassName}.{MethodName} - Live Wallet Balance details not found, ConsumerCode : {ConsumerCode}", className, methodName, consumerCode);
                return filterWallets;
            }
            else
            {
                var bal = filterWallets.Join(liveBalance.Wallets,
                    filterwallet => filterwallet.WalletType.WalletTypeCode,
                      bal => bal.PurseWalletType,
                      (filterwallet, bal) => new
                      {
                          WalletCode = filterwallet.Wallet.WalletCode,
                          WalletId = filterwallet.Wallet.WalletId,
                          walletLiveBalance = bal.Wallet.Balance
                      });

                if (bal.Any())
                {

                    foreach (var wallet in filterWallets.Select(x => x.Wallet))
                    {
                        wallet.Balance = bal.Where(x => x.WalletCode == wallet.WalletCode).Select(x => x.walletLiveBalance).FirstOrDefault();
                    }
                    List<WalletModel> walletModel = bal.Select(m1 => new WalletModel
                    {
                        WalletId = m1.WalletId,
                        Balance = m1.walletLiveBalance
                    }).ToList();


                    await _walletClient.Post<BaseResponseDto>(WalletConstants.updateWalletBalanceApi, walletModel);
                }

                var tenantDetails = tenant != null 
                    ? tenant
                    : (await _tenantService.GetTenantByConsumerCode(consumerCode!)).Tenant;
                var cardTransactions = await GetCardTransactions(tenantDetails, consumerCode!, filterWallets);

                foreach (var wallet in filterWallets)
                {
                    if (wallet?.Wallet == null || wallet.RecentTransaction == null) continue;
                    var transtionsFromCard = cardTransactions.Where(t => t.Transaction?.WalletId == wallet.Wallet.WalletId).ToList();
                    var lstTransactions = wallet.RecentTransaction;
                    lstTransactions.AddRange(transtionsFromCard);
                    wallet.RecentTransaction = lstTransactions.OrderByDescending(x => x.Transaction?.CreateTs).Take(CommonConstants.WalletTransactionCount).ToList();
                }

                return filterWallets;

            }
        }
        private async Task<WalletDetailDto[]> GetFundingDescription(string? consumerCode, WalletDetailDto[] filterWallets, string tenantCode)
        {
            const string methodName = nameof(GetFundingDescription);
            FisGetFundingDescriptionResponseDto responseDto = new FisGetFundingDescriptionResponseDto();

            try
            {
                var wallets = filterWallets.Select(x => x.WalletType.WalletTypeCode ?? string.Empty).ToList();

                FisGetFundingDescriptionRequestDto requestDto = new FisGetFundingDescriptionRequestDto
                {
                    WalletTypes = wallets,
                    ConsumerCode = consumerCode,
                    TenantCode = string.IsNullOrEmpty(tenantCode) ? await GetTenantCode(consumerCode) : tenantCode
                };

                responseDto = await _fisClient.Post<FisGetFundingDescriptionResponseDto>(WalletConstants.GetFundingDescription, requestDto);
            }
            catch (Exception ex)
            {
                _walletServiceLogger.LogError(ex, "{ClassName}.{MethodName} - Live Balance details not found, ConsumerCode : {ConsumerCode}, ErrorCode:{ErrorCode}, ERROR: {Message} ",
                    className, methodName, consumerCode, StatusCodes.Status500InternalServerError, ex.Message);
                return filterWallets;

            }
            if (responseDto.ErrorCode.HasValue || !responseDto.FundingDescriptions.Any())
            {

                _walletServiceLogger.LogError("{ClassName}.{MethodName} - Funding Description not found, ConsumerCode : {ConsumerCode}", className, methodName, consumerCode);
                return filterWallets;
            }
            else
            {
                foreach (var wallets in filterWallets)
                {
                    var key = wallets.WalletType.WalletTypeCode;
                    if (key != null && responseDto.FundingDescriptions.ContainsKey(key))
                        wallets.Wallet.FundingDescription = responseDto.FundingDescriptions[key];
                }
                return filterWallets;
            }
        }
        private async Task<List<TransactionEntryDto>> GetCardTransactions(TenantDto? tenant, string consumerCode, WalletDetailDto[] wallet)
        {
            const string methodName = nameof(GetCardTransactions);
            string env = await _vault.GetSecret(CommonConstants.Env);
            if (string.IsNullOrEmpty(env) || env == _vault.InvalidSecret)
            {
                _walletServiceLogger.LogError("{ClassName}.{MethodName} - Invalid or missing environment secret,ErrorCode:{Code},ERROR:{Msg}",
                    className, methodName, StatusCodes.Status500InternalServerError, CommonConstants.InternalError);
                return new List<TransactionEntryDto>();
            }
            var cardTransactionRequestDto = new CardTransactionsRequestDto()
            {
                TenantCode = tenant?.TenantCode,
                ConsumerCode = consumerCode,
                TxnType = string.Equals(env, CommonConstants.DEVELOPMENT_ENV, StringComparison.OrdinalIgnoreCase)
                    ? CardOperationConstants.PendingAndAdjustTxnType
                    : CardOperationConstants.txnType,
                Days = CardOperationConstants.Days,
                StartDate = null
            };
            var cardTransaction = await _fisClient.Post<CardTransactionsResponseDto>(CardOperationConstants.FiscardTxnApi, cardTransactionRequestDto);
            if (cardTransaction.CardTransactions == null || cardTransaction.ErrorCode.HasValue)
            {
                _walletServiceLogger.LogError("{ClassName}.{MethodName} - Card Transactions Details Not Found for TenantCode : {TenantCode}, ConsumerCode : {ConsumerCode}", className, methodName, tenant?.TenantCode, consumerCode);
                return new List<TransactionEntryDto>();
            }
            // Filter out records where TxnType is in SkipTxnType and FIS adjustment transaction over isExternalSync wallet transactions
            var filteredTransactionDetails = (from ct in cardTransaction.CardTransactions
                                              join w in wallet on ct.PurseWalletType equals w.WalletType.WalletTypeCode
                                              where !TransactionConstants.SkipTxnType.Contains(ct.CardTransaction.TxnType?.Trim())
                                              && !w.RecentTransaction.Any(x => x?.TransactionDetail?.RedemptionRef == ct.CardTransaction.Reference)
                                              select ct
                       ).ToList();

            var cardTransactionEntry = filteredTransactionDetails.Join(wallet, t => t.PurseWalletType, w => w.WalletType.WalletTypeCode, (ct, w) => new TransactionEntryDto
            {
                Transaction = new TransactionDto
                {
                    TransactionAmount = ct.CardTransaction.Amt.HasValue ? Convert.ToDouble(ct.CardTransaction.Amt.Value) : 0,
                    TransactionType = ct.CardTransaction.Amt > 0 ? CardOperationConstants.TransactionTypeAdd : CardOperationConstants.TransactionTypeSubtract,
                    WalletId = w.Wallet.WalletId,
                    CreateTs = ct.CardTransaction.TranDate.HasValue ? ct.CardTransaction.TranDate.Value : DateTime.MinValue,


                },
                TransactionDetail = new TransactionDetailDto
                {
                    ConsumerCode = consumerCode,
                    Notes = ct.CardTransaction.Comment,
                    RedemptionItemDescription = ct.CardTransaction.MerchantName,
                    CreateTs = ct.CardTransaction.TranDate.HasValue ? ct.CardTransaction.TranDate.Value : DateTime.MinValue,
                    TransactionDetailType = GetMappingValue(ct.CardTransaction.TxnType?.Trim() ?? string.Empty),
                    IsPending = true,
                }
            }).ToList();

            _walletServiceLogger.LogInformation("{ClassName}.{MethodName} - Retrieved Card Transactions Details Successfully for ConsumerCode : {ConsumerCode}", className, methodName, consumerCode);
            return cardTransactionEntry;
        }
        private static string GetMappingValue(string key)
        {
            key = key.Contains(TransactionConstants.Purchase, StringComparison.OrdinalIgnoreCase) ? TransactionConstants.Purchase : key.Trim();

            if (!string.IsNullOrEmpty(key) && TransactionConstants.TxnTypeMapping.ContainsKey(key.Trim()))
            {
                return TransactionConstants.TxnTypeMapping[key];
            }
            else
            {
                return TransactionConstants.Unknown;
            }
        }

    }
}
