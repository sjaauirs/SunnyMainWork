using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NHibernate;
using SunnyRewards.Helios.Common.Core.Domain;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Domain.Enums;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Common.Core.Services;
using SunnyRewards.Helios.Common.Core.Services.Interfaces;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.Wallet.Core.Domain.Constants;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos.Json;
using SunnyRewards.Helios.Wallet.Core.Domain.Models;
using SunnyRewards.Helios.Wallet.Infrastructure.Helpers.Interfaces;
using SunnyRewards.Helios.Wallet.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Wallet.Infrastructure.Services.Interfaces;
using System.Data;
using System.Text;

namespace SunnyRewards.Helios.Wallet.Infrastructure.Services
{
    public class WalletService : BaseService, IWalletService
    {
        private static int RETRY_MIN_WAIT_MS = 10; // min amount of milliseconds to wait before retrying
        private static int RETRY_MAX_WAIT_MS = 101; // max amount of milliseconds to wait before retrying

        private readonly ILogger<WalletService> _walletLogger;
        private readonly IMapper _mapper;
        private readonly IWalletRepo _walletRepo;
        private readonly IWalletTypeRepo _walletTypeRepo;
        private readonly ITransactionRepo _transactionRepo;
        private readonly ITransactionDetailRepo _transactionDetailRepo;
        private readonly IConsumerWalletRepo _consumerWalletRepo;
        private readonly NHibernate.ISession _session;
        private readonly IRedemptionRepo _redemptionRepo;
        private readonly IConfiguration _configuration;
        private readonly IAuditTrailService _auditTrailService;
        private readonly ITransactionService _transactionService;
        private readonly ISecretHelper _secretHelper;
        private readonly Random _random = new Random();
        private readonly int _maxTries;
        private readonly IWalletTypeTransferRuleRepo _walletTypeTransferRuleRepo;
        private readonly IConsumerWalletService _consumerWalletService;
        private readonly IConsumerService _consumerService;
        const string className = nameof(WalletService);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="walletLogger"></param>
        /// <param name="mapper"></param>
        /// <param name="walletRepo"></param>
        /// <param name="walletTypeRepo"></param>
        /// <param name="transactionRepo"></param>
        /// <param name="transactionDetailRepo"></param>
        /// <param name="session"></param>
        /// <param name="redemptionRepo"></param>
        public WalletService
            (
            ILogger<WalletService> walletLogger,
            IMapper mapper,
            IWalletRepo walletRepo,
            IWalletTypeRepo walletTypeRepo,
            ITransactionRepo transactionRepo,
            ITransactionDetailRepo transactionDetailRepo,
            IConsumerWalletRepo consumerWalletRepo,
            NHibernate.ISession session,
            IRedemptionRepo redemptionRepo,
            IConfiguration configuration,
            IAuditTrailService auditTrailService,
            ITransactionService transactionService,
            ISecretHelper secretHelper,
            IWalletTypeTransferRuleRepo walletTypeTransferRuleRepo,
            IConsumerWalletService consumerWalletService,
            IConsumerService consumerService
            )
        {
            _walletLogger = walletLogger;
            _mapper = mapper;
            _walletRepo = walletRepo;
            _walletTypeRepo = walletTypeRepo;
            _transactionRepo = transactionRepo;
            _transactionDetailRepo = transactionDetailRepo;
            _consumerWalletRepo = consumerWalletRepo;
            _session = session;
            _redemptionRepo = redemptionRepo;
            _configuration = configuration;
            _auditTrailService = auditTrailService;
            _transactionService = transactionService;
            _secretHelper = secretHelper;

            _maxTries = 5;
            string? opMaxTries = _configuration.GetSection("OperationMaxTries").Value;
            if (!string.IsNullOrEmpty(opMaxTries))
            {
                _maxTries = Convert.ToInt32(opMaxTries);
            }

            _walletTypeTransferRuleRepo = walletTypeTransferRuleRepo;
            _consumerWalletService = consumerWalletService;
            _consumerService = consumerService;
        }

        /// <summary>
        ///  Get Wallet Table Data based on given walletId
        /// </summary>
        /// <param name="walletId"></param>
        /// <returns></returns>
        public async Task<WalletDto> GetWalletData(long walletId)
        {
            const string methodName = nameof(GetWalletData);
            try
            {

                var wallet = await _walletRepo.FindOneAsync(x => x.WalletId == walletId);
                if (wallet == null)
                    return new WalletDto();
                var response = _mapper.Map<WalletDto>(wallet);
                _walletLogger.LogInformation("{className}.{methodName}: Retrieved Wallet Details Successfully for WalletId : {walletId}", className, methodName, walletId);

                return response;
            }
            catch (Exception ex)
            {

                _walletLogger.LogError(ex, "{className}.{methodName}: - Error :{msg}", className, methodName, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="walletTypeId"></param>
        /// <returns></returns>
        public async Task<WalletTypeDto> GetWalletType(long walletTypeId)
        {
            const string methodName = nameof(GetWalletType);
            try
            {
                var wallet = await _walletTypeRepo.FindOneAsync(x => x.WalletTypeId == walletTypeId);
                if (wallet == null)
                    return new WalletTypeDto();
                var response = _mapper.Map<WalletTypeDto>(wallet);
                _walletLogger.LogInformation("{className}.{methodName}: Retrieved Wallet Type Details Successfully for wallletTypeId : {wallletTypeId}", className, methodName, walletTypeId);

                return response;
            }
            catch (Exception ex)
            {

                _walletLogger.LogError(ex, "{className}.{methodName}: - Error :{msg}", className, methodName, ex.Message);
                throw;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="walletTypeDto"></param>
        /// <returns></returns>
        public async Task<WalletTypeDto> GetWalletTypeCode(WalletTypeDto walletTypeDto)
        {
            const string methodName = nameof(GetWalletTypeCode);
            try
            {

                var walletTypeData = await _walletTypeRepo.FindOneAsync(x => x.WalletTypeCode == walletTypeDto.WalletTypeCode);
                if (walletTypeData == null)
                    return new WalletTypeDto();
                var response = _mapper.Map<WalletTypeDto>(walletTypeData);
                _walletLogger.LogInformation("{className}.{methodName}: Retrieved Wallet Type Details Successfully for WalletTypeCode : {walletTypeCode}", className, methodName, walletTypeDto.WalletTypeCode);

                return response;
            }
            catch (Exception ex)
            {

                _walletLogger.LogError(ex, "{className}.{methodName}: - Error :{msg}", className, methodName, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="postRewardRequestDto"></param>
        /// <returns></returns>
        public async Task<PostRewardResponseDto> RewardDetailsOuter(PostRewardRequestDto postRewardRequestDto)
        {
            const string methodName = nameof(RewardDetailsOuter);
            int maxTries = _maxTries;
            PostResponseMultiTransactionDto? response = null;
            _walletLogger.LogInformation("{className}.{methodName}:  Has been Invoked for ConsumerCode: {ConsumerCode}", className, methodName, postRewardRequestDto.ConsumerCode);

            while (maxTries > 0)
            {
                try
                {
                    response = await RewardDetails(postRewardRequestDto);
                    if (response.ErrorCode == null)
                    {
                        break; // Op success no need to retry.
                    }

                    _session.Clear();
                    _walletLogger.LogError("{className}.{methodName}: Response errorCode {errCode} in RewardDetailsOuter retrying count left={maxTries}, consumer: {consCode}", className, methodName, response.ErrorCode, maxTries,
                        postRewardRequestDto.ConsumerCode);
                    maxTries--;
                }
                catch (Exception ex)
                {
                    _session.Clear();
                    _walletLogger.LogError(ex, "{className}.{methodName}: Error in RewardDetailsOuter retrying count left={maxTries} and Error Msg:{errorMsg}", className, methodName, maxTries, ex.Message);
                    maxTries--;
                    Thread.Sleep(_random.Next(RETRY_MIN_WAIT_MS, RETRY_MAX_WAIT_MS));
                }
            }
            if (response == null)
            {
                _walletLogger.LogError("{className}.{methodName}: Error. Error Code:{errorCode}", className, methodName, StatusCodes.Status500InternalServerError);
                _walletLogger.LogWarning("{className}.{methodName}: Using a fallback response due to multiple retries: {request}", className, methodName, postRewardRequestDto.ToJson());
                return new PostRewardResponseDto()
                {
                    ErrorCode = StatusCodes.Status500InternalServerError
                };
            }

            if (response.ErrorCode == null || response.ErrorCode == StatusCodes.Status200OK)
            {
                // audit trail
                await _auditTrailService.PostAuditTrail(new AuditTrailDto()
                {
                    SourceModule = "WALLET_API",
                    SourceContext = "walletService.RewardDetailsOuter",
                    AuditName = "REWARD",
                    AuditMessage = $"Rewarded to: consumer: {postRewardRequestDto.ConsumerCode}, amount: {postRewardRequestDto.RewardAmount}",
                    CreateUser = "SYSTEM",
                    AuditJsonData = postRewardRequestDto.ToJson()
                });
                _walletLogger.LogInformation("{className}.{methodName}: Succeeded for ConsumerCode: {ConsumerCode}", className, methodName, postRewardRequestDto.ConsumerCode);
            }
            else
            {
                _walletLogger.LogError("{className}.{methodName}: Response errorCode {errCode} in RewardDetailsOuter  consumer: {consCode}", className, methodName, response.ErrorCode,
                        postRewardRequestDto.ConsumerCode);
            }

            return response?.PostRewardResponses?[0] ?? new PostRewardResponseDto() { ErrorCode = 400, ErrorMessage = "Not Found" };
        }


        /// <summary>
        /// Get Reward Details based on the give values in the PostRewardRequestDto
        /// </summary>
        /// <param name="postRewardRequestDto"></param>
        /// <returns></returns>
        public async Task<PostResponseMultiTransactionDto> RewardDetails(PostRewardRequestDto postRewardRequestDto)
        {
            const string methodName = nameof(RewardDetails);

            var result = new PostResponseMultiTransactionDto
            {
                PostRewardResponses = new List<PostRewardResponseDto>()
            };
            var consumerTaskRewardInfo = new ConsumerTaskRewardInfoDto();

            _walletLogger.LogInformation("{className}.{methodName}: Started with ConsumerCode : {ConsumerCode}", className, methodName, postRewardRequestDto.ConsumerCode);

            var masterWalletType = await GetWalletTypeByIdOrCode(null, postRewardRequestDto.MasterWalletTypeCode);
            var consumerWalletType = await GetWalletTypeByIdOrCode(null, postRewardRequestDto.ConsumerWalletTypeCode);

            if (masterWalletType == null || consumerWalletType == null)
            {
                _walletLogger.LogError("Wallet type not found: MasterWalletCode = {MasterWalletType}, ConsumerWalletCode = {ConsumerWalletType}",
                    postRewardRequestDto.MasterWalletTypeCode, postRewardRequestDto.ConsumerWalletTypeCode);
                return new PostResponseMultiTransactionDto { ErrorCode = StatusCodes.Status400BadRequest, ErrorMessage = "Invalid wallet type" };
            }


            var (masterWallet, consumerWallet) = await GetWallets(consumerWalletType.WalletTypeId, postRewardRequestDto.TenantCode!, postRewardRequestDto.ConsumerCode!, masterWalletType.WalletTypeId);

            if (masterWallet == null || consumerWallet == null)
            {
                _walletLogger.LogError("Wallets not found for MasterWallet: {MasterWallet} or ConsumerWallet: {ConsumerWallet}", masterWallet, consumerWallet);
                return new PostResponseMultiTransactionDto { ErrorCode = StatusCodes.Status400BadRequest, ErrorMessage = "Wallet/ConsumerWallet not found" };
            }

            _walletLogger.LogInformation("{className}.{methodName}: consumer wallet, xmin: {xmin}", className, methodName, consumerWallet.Xmin);

            var walletTypeConfig = consumerWalletType?.ConfigJson != null
               ? JsonConvert.DeserializeObject<WalletTypeConfig>(consumerWalletType.ConfigJson)
               : null;
            if (walletTypeConfig == null || walletTypeConfig?.Currency == null)
            {
                _walletLogger.LogError("Wallet type config not found for WalletTypeId: {WalletTypeId}", consumerWalletType?.WalletTypeId);
                return new PostResponseMultiTransactionDto { ErrorCode = StatusCodes.Status400BadRequest, ErrorMessage = "Wallet type config not found" };
            }

            consumerTaskRewardInfo.Currency = walletTypeConfig.Currency;

            var processRewardRequest = new ProcessRewardsRequest
            {
                RewardAmount = postRewardRequestDto.RewardAmount,
                ConsumerCode = postRewardRequestDto.ConsumerCode,
                RewardDescription = postRewardRequestDto.RewardDescription,
                SplitRewardOverflow = postRewardRequestDto.SplitRewardOverflow,
                TaskRewardCode = postRewardRequestDto.TaskRewardCode,
                TenantCode = postRewardRequestDto.TenantCode,
                ConsumerWalletModel = consumerWallet!,
                MasterWalletModel = masterWallet
            };

            if (IsRewardLimitReached(consumerWallet!))
            {
                consumerTaskRewardInfo.OriginalCurrency = consumerTaskRewardInfo.Currency;
                consumerTaskRewardInfo.OriginalRewardAmount = processRewardRequest.RewardAmount;
                return await HandleRewardLimitExceeded(processRewardRequest, result, consumerTaskRewardInfo);
            }
            return await ProcessRewards(processRewardRequest, result, consumerTaskRewardInfo, null);
        }

        private async Task<PostResponseMultiTransactionDto> HandleRewardLimitExceeded(ProcessRewardsRequest request, PostResponseMultiTransactionDto result, ConsumerTaskRewardInfoDto consumerTaskRewardInfo)
        {
            if (!request.SplitRewardOverflow) // no split currency , return Error
            {
                _walletLogger.LogError("{className}.{methodName}: Consumer Wallet Total Earn Greater than Maximum Earn. For Consumer Code: {consumerCode}, Error Code: {errorCode}",
                    className, nameof(RewardDetails), request.ConsumerCode, HeliosErrorCode.ConsumerLimitReached);

                return new PostResponseMultiTransactionDto
                {
                    ErrorMessage = "Consumer has reached earn maximum allowed",
                    ErrorCode = (int?)HeliosErrorCode.ConsumerLimitReached,
                };
            }

            _walletLogger.LogInformation("SplitRewardOverflow is enabled, But wallet is already full for consumerCode :{consumerCode} ConsumerWalletCode : {ConsumerWalletCode}",
                                        request.ConsumerCode, request.ConsumerWalletModel.WalletCode);
            var setupwalletsforRemainingAmount = await SetupRemainingAmountWallets(request, request.RewardAmount);
            consumerTaskRewardInfo.OverflowAmount = request.RewardAmount;
            consumerTaskRewardInfo.ConversionRatio = setupwalletsforRemainingAmount?.TransferRule;
            consumerTaskRewardInfo.OverflowCurrency = setupwalletsforRemainingAmount?.WalletTypeCurrency;
            consumerTaskRewardInfo.Currency = setupwalletsforRemainingAmount?.WalletTypeCurrency ?? string.Empty;

            if (setupwalletsforRemainingAmount != null && setupwalletsforRemainingAmount.ErrorCode != null)
            {
                return new PostResponseMultiTransactionDto() { ErrorCode = setupwalletsforRemainingAmount.ErrorCode, ErrorMessage = setupwalletsforRemainingAmount.ErrorMessage };
            }

            return await ProcessRewards(request, result, consumerTaskRewardInfo, null);
        }
        private async Task<PostResponseMultiTransactionDto> ProcessRewards(ProcessRewardsRequest processRewardsRequest, PostResponseMultiTransactionDto result, ConsumerTaskRewardInfoDto consumerTaskRewardInfo, ITransaction? existingTransaction = null)
        {
            bool isRootTransaction = existingTransaction == null;
            const string methodName = nameof(ProcessRewards);
            var transactionTs = DateTime.UtcNow;
            var consumerWallet = processRewardsRequest.ConsumerWalletModel;
            var masterWallet = processRewardsRequest.MasterWalletModel;

            ITransaction transaction = isRootTransaction ? _session.BeginTransaction() : existingTransaction!;
            try
            {
                var maxMasterWalletTransactionId = await _transactionRepo.GetMaxTransactionIdByWallet(masterWallet?.WalletId ?? 0);
                var lastMasterWalletTransaction = await _transactionRepo.FindOneAsync(t => t.TransactionId == maxMasterWalletTransactionId);

                var maxConsumerWalletTransactionId = await _transactionRepo.GetMaxTransactionIdByWallet(consumerWallet?.WalletId ?? 0);

                var lastConsumerWalletTransaction = await _transactionRepo.FindOneAsync(t => t.TransactionId == maxConsumerWalletTransactionId);

                var transactionDetail = new TransactionDetailModel()
                {
                    TransactionDetailType = "REWARD",
                    ConsumerCode = processRewardsRequest.ConsumerCode,
                    TaskRewardCode = processRewardsRequest.TaskRewardCode,
                    RewardDescription = processRewardsRequest.RewardDescription,
                    Notes = null,
                    RedemptionRef = null,
                    RedemptionItemDescription = null,
                    CreateTs = transactionTs,
                    CreateUser = Constants.CreateUser,
                    UpdateTs = DateTime.UtcNow,
                    UpdateUser = Constants.CreateUser,
                    DeleteNbr = 0
                };

                var transactionDetailResponse = await _session.SaveAsync(transactionDetail);
                transactionDetail.TransactionDetailId = Convert.ToInt64(transactionDetailResponse);

                string newTxnCode = $"txn-{Guid.NewGuid().ToString().Replace("-", "")}";

                var actualRewardAmount = Math.Min(processRewardsRequest.RewardAmount, (consumerWallet?.EarnMaximum ?? 0) - (consumerWallet?.TotalEarned ?? 0));
                var remainingRewardAmount = processRewardsRequest.RewardAmount - actualRewardAmount;

                if (isRootTransaction)
                {
                    consumerTaskRewardInfo.RewardAmount = actualRewardAmount;
                }

                var masterWalletBalance = masterWallet?.Balance - actualRewardAmount;
                var subTransaction = new TransactionModel()
                {
                    WalletId = masterWallet?.WalletId ?? 0,
                    TransactionCode = newTxnCode,
                    TransactionType = Constants.Subtract,
                    PreviousBalance = masterWallet?.Balance,
                    TransactionAmount = actualRewardAmount,
                    Balance = masterWalletBalance,
                    PrevWalletTxnCode = masterWallet?.WalletId + ":" + (lastMasterWalletTransaction != null ?
                    lastMasterWalletTransaction.TransactionCode : "init"),
                    CreateTs = transactionTs,
                    CreateUser = Constants.CreateUser,
                    UpdateTs = DateTime.UtcNow,
                    UpdateUser = Constants.CreateUser,
                    DeleteNbr = 0,
                    TransactionDetailId = transactionDetail.TransactionDetailId
                };

                await _session.SaveAsync(subTransaction);

                var consumerWalletBalance = consumerWallet?.Balance + actualRewardAmount;
                var totalEarned = consumerWallet?.TotalEarned + actualRewardAmount;

                var addTransaction = new TransactionModel()
                {
                    WalletId = consumerWallet?.WalletId ?? 0,
                    TransactionCode = newTxnCode,
                    TransactionType = Constants.Addition,
                    PreviousBalance = consumerWallet?.Balance,
                    TransactionAmount = actualRewardAmount,
                    Balance = consumerWalletBalance,
                    PrevWalletTxnCode = consumerWallet?.WalletId + ":" + (lastConsumerWalletTransaction != null ?
                    lastConsumerWalletTransaction.TransactionCode : "init"),
                    CreateTs = transactionTs,
                    CreateUser = Constants.CreateUser,
                    UpdateTs = DateTime.UtcNow,
                    UpdateUser = Constants.CreateUser,
                    DeleteNbr = 0,
                    TransactionDetailId = transactionDetail.TransactionDetailId
                };
                if (actualRewardAmount > 0)
                {
                    await _session.SaveAsync(addTransaction);
                }

                int rec = _walletRepo.UpdateMasterWalletBalance(transactionTs, masterWalletBalance, masterWallet?.WalletId ?? 0, masterWallet?.Xmin ?? 0);
                if (rec == 0)
                {
                    _walletLogger.LogError("{className}.{methodName}: Error: Concurrency error could not update master wallet: WalletId: {walletId} xmin: {xmin}, Error Code:{errorCode}", className, methodName,
                        masterWallet?.WalletId, masterWallet?.Xmin, StatusCodes.Status409Conflict);

                    await transaction.RollbackAsync();
                    return new PostResponseMultiTransactionDto()
                    {
                        ErrorMessage = $"Concurrency error in updating Master Wallet: {masterWallet?.WalletId}",
                        ErrorCode = StatusCodes.Status409Conflict
                    };
                }
                else
                {
                    _walletLogger.LogInformation("{className}.{methodName}: updated master wallet: {walletId}, balance: {balance}, count: {rec}, xmin: {xmin}", className, methodName,
                        masterWallet?.WalletId, masterWalletBalance, rec, masterWallet?.Xmin);
                }

                int recc = _walletRepo.UpdateConsumerWalletBalance(transactionTs, consumerWalletBalance, totalEarned, consumerWallet?.WalletId ?? 0, consumerWallet?.Xmin ?? 0);
                if (recc == 0)

                {
                    _walletLogger.LogError("{className}.{methodName}: Error: Concurrency error could not update consumer wallet: WalletId: {walletId} xmin: {xmin}, Error Code:{errorCode}", className, methodName,
                        consumerWallet?.WalletId, consumerWallet?.Xmin, StatusCodes.Status409Conflict);

                    await transaction.RollbackAsync();
                    return new PostResponseMultiTransactionDto()
                    {
                        ErrorMessage = $"Concurrency error in updating Consumer Wallet: {consumerWallet?.WalletId}",
                        ErrorCode = StatusCodes.Status409Conflict
                    };
                }
                else
                {
                    _walletLogger.LogInformation("{className}.{methodName}: updated consumer wallet: {walletId}, balance: {balance}, count: {rec}, xmin: {xmin}", className, methodName,
                        consumerWallet?.WalletId, consumerWalletBalance, rec, consumerWallet?.Xmin);

                    var rewardResponse = new PostRewardResponseDto()
                    {
                        AddEntry = _mapper.Map<TransactionDto>(addTransaction),
                        SubEntry = _mapper.Map<TransactionDto>(subTransaction),
                        TransactionDetail = _mapper.Map<TransactionDetailDto>(transactionDetail),
                        ConsumerTaskRewardInfo = consumerTaskRewardInfo
                    };

                    result.PostRewardResponses!.Add(rewardResponse);
                }

                consumerTaskRewardInfo.SplitCurrency = processRewardsRequest.SplitRewardOverflow;

                if (remainingRewardAmount > 0.0 && processRewardsRequest.SplitRewardOverflow) // we have remaining Amount and valid Flag
                {

                    consumerTaskRewardInfo.OriginalCurrency = consumerTaskRewardInfo.Currency;
                    consumerTaskRewardInfo.OriginalRewardAmount = processRewardsRequest.RewardAmount;
                    var walletSetup = await SetupRemainingAmountWallets(processRewardsRequest, remainingRewardAmount);
                    consumerTaskRewardInfo.OverflowAmount = processRewardsRequest.RewardAmount;
                    if (walletSetup == null || walletSetup.ErrorCode != null)
                    {
                        _walletLogger.LogError("Error in processing Remaining Amount : {errorMsg}", walletSetup?.ErrorMessage ?? "wallet not found");
                    }
                    consumerTaskRewardInfo.ConversionRatio = walletSetup?.TransferRule;
                    consumerTaskRewardInfo.OverflowCurrency = walletSetup?.WalletTypeCurrency;
                    consumerTaskRewardInfo.OverflowAmount = processRewardsRequest.RewardAmount;

                    await ProcessRewards(processRewardsRequest, result, consumerTaskRewardInfo, transaction);

                }




                if (isRootTransaction) /// Commit for root transaction
                {
                    await transaction.CommitAsync();
                    _walletLogger.LogInformation("{className}.{methodName}: Commit Processing Rewards with Request : {ConsumerCode}", className, methodName, processRewardsRequest.ConsumerCode);
                }



                return result;
            }
            catch (Exception ex)
            {

                _walletLogger.LogError(ex, "{className}.{methodName}: ERROR - msg : {msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status409Conflict);

                await transaction.RollbackAsync();
                return new PostResponseMultiTransactionDto()
                {
                    ErrorCode = StatusCodes.Status409Conflict,
                    ErrorMessage = ex.Message,
                    ErrorDescription = ex.InnerException?.Message
                };
            }

        }

        private static bool IsRewardLimitReached(WalletModel consumerWallet)
        {
            return consumerWallet.TotalEarned >= consumerWallet.EarnMaximum;
        }

        private async Task<WalletTypeRuleResult?> SetupRemainingAmountWallets(ProcessRewardsRequest processRewardsRequest, double remainingRewardAmount)
        {
            var walletTRansferRequestRule = new WalletTransferRequest()
            {
                SourceWalletTypeId = processRewardsRequest.ConsumerWalletModel!.WalletTypeId,
                TenantCode = processRewardsRequest.TenantCode!,
                ConsumerCode = processRewardsRequest.ConsumerCode!
            };
            var walletTransferRulesData = await GetWalletTransferRules(walletTRansferRequestRule);

            if (walletTransferRulesData != null && walletTransferRulesData.ErrorCode == null)
            {

                processRewardsRequest.ConsumerWalletModel = walletTransferRulesData.ConsumerWallet;
                processRewardsRequest.MasterWalletModel = walletTransferRulesData.MasterWallet;
                processRewardsRequest.RewardAmount = remainingRewardAmount * walletTransferRulesData.TransferRule;

                if (walletTransferRulesData.ConsumerWalletTypeCode == WalletConstants.SweepsTakes_walletTypeCode)
                {
                    processRewardsRequest.RewardAmount = Math.Round(processRewardsRequest.RewardAmount);
                }


                return walletTransferRulesData;
            }
            return walletTransferRulesData;
        }

        private async Task<WalletTypeRuleResult> GetWalletTransferRules(WalletTransferRequest walletTransferRequest)
        {
            var sweepstakesWalletType = await GetWalletTypeByIdOrCode(null, WalletConstants.SweepsTakes_walletTypeCode);
            try
            {
                var transferRule = await _walletTypeTransferRuleRepo.FindOneAsync(x => x.TenantCode == walletTransferRequest.TenantCode
                                                                              && x.SourceWalletTypeId == walletTransferRequest.SourceWalletTypeId
                                                                              && x.DeleteNbr == 0);

                if (transferRule == null)
                {
                    _walletLogger.LogError("Transfer rule not found for SourceWalletTypeId: {SourceWalletTypeId}", walletTransferRequest.SourceWalletTypeId);
                    if (sweepstakesWalletType == null)
                        return new WalletTypeRuleResult { ErrorCode = StatusCodes.Status404NotFound, ErrorMessage = "Wallet type transfer rule is not configured and SweepstakesWalletType not found" };

                    transferRule = new WalletTypeTransferRuleModel
                    {
                        SourceWalletTypeId = walletTransferRequest.SourceWalletTypeId,
                        TargetWalletTypeId = sweepstakesWalletType.WalletTypeId,
                        TransferRule = WalletConstants.DefaultWalletTypeTransferRuleConfig,
                    };
                }

                var validTransferRule = ValidateAndParseTransferRule(transferRule.TransferRule);
                if (validTransferRule.ErrorCode != null)
                {
                    if (sweepstakesWalletType == null)
                        return new WalletTypeRuleResult { ErrorCode = StatusCodes.Status404NotFound, ErrorMessage = "Invalid wallet type transfer rule and SweepstakesWalletType not found" };
                    transferRule = new WalletTypeTransferRuleModel
                    {
                        SourceWalletTypeId = walletTransferRequest.SourceWalletTypeId,
                        TargetWalletTypeId = sweepstakesWalletType.WalletTypeId,
                        TransferRule = WalletConstants.DefaultWalletTypeTransferRuleConfig,
                    };
                }

                var targetConsumerWalletType = await GetWalletTypeByIdOrCode(transferRule.TargetWalletTypeId, null);

                if (targetConsumerWalletType == null)
                {
                    _walletLogger.LogError("Wallet type not found for TargetWalletTypeId: {TargetWalletTypeId}", transferRule.TargetWalletTypeId);
                    return new WalletTypeRuleResult { ErrorCode = StatusCodes.Status400BadRequest, ErrorMessage = $"Wallet type not found for TargetWalletTypeId: {transferRule.TargetWalletTypeId}" };
                }

                var walletTypeConfig = targetConsumerWalletType?.ConfigJson != null
               ? JsonConvert.DeserializeObject<WalletTypeConfig>(targetConsumerWalletType.ConfigJson)
               : null;
                if (walletTypeConfig == null || walletTypeConfig?.Currency == null)
                {
                    _walletLogger.LogError("Wallet type config not found for TargetWalletTypeId: {TargetWalletTypeId}", transferRule?.TargetWalletTypeId);
                    return new WalletTypeRuleResult { ErrorCode = StatusCodes.Status400BadRequest, ErrorMessage = $"Wallet type config not found for TargetWalletTypeId: {transferRule?.TargetWalletTypeId}" };
                }

                // Assuming consumer Wallet and Master wallet have same wallet type Code

                var (masterWallet, consumerWallet) = await GetWallets(targetConsumerWalletType.WalletTypeId, walletTransferRequest.TenantCode!, walletTransferRequest.ConsumerCode!, null);

                if (masterWallet == null || consumerWallet == null)
                {
                    _walletLogger.LogError("Wallets not found for MasterWallet: {MasterWallet} or ConsumerWallet: {ConsumerWallet}", masterWallet?.WalletTypeId, consumerWallet?.WalletTypeId);
                    return new WalletTypeRuleResult { ErrorCode = StatusCodes.Status400BadRequest, ErrorMessage = "Wallet/ConsumerWallet not found" };
                }

                _walletLogger.LogInformation("{className}.GetWalletTransferRules: consumer wallet, xmin: {xmin}", className, consumerWallet.Xmin);

                return new WalletTypeRuleResult()
                {
                    ConsumerWallet = consumerWallet!,
                    MasterWallet = masterWallet!,
                    TransferRule = validTransferRule!.TransferRule,
                    ConsumerWalletTypeCode = targetConsumerWalletType.WalletTypeCode,
                    WalletTypeCurrency = walletTypeConfig.Currency
                };
            }
            catch (Exception ex)
            {
                _walletLogger.LogError(ex, "failed to fetch Wallet Transfer Rule for TenantCode : {TenantCode} and SourceConsumerWallet :{consumerWallet}",
                   walletTransferRequest.TenantCode, walletTransferRequest.SourceWalletTypeId);
                return new WalletTypeRuleResult() { ErrorCode = StatusCodes.Status500InternalServerError, ErrorMessage = ex.Message };
            }
        }


        private WalletTypeRuleResult ValidateAndParseTransferRule(string? transferRuleJson)
        {
            if (string.IsNullOrWhiteSpace(transferRuleJson) || transferRuleJson == "{}")
            {
                _walletLogger.LogInformation($"Error in paesing transferRuleJson {transferRuleJson}, hence defaulting to 1");
                return new WalletTypeRuleResult { TransferRule = 1 };
            }

            var jObject = JObject.Parse(transferRuleJson);
            var knownProperties = typeof(TransferRule).GetProperties().Select(p => p.Name.ToUpper());

            if (!jObject.Properties().Any(p => knownProperties.Contains(p.Name.ToUpper())))
            {
                _walletLogger.LogInformation($"Error in paesing transferRuleJson {transferRuleJson}, hence defaulting to 1");
                return new WalletTypeRuleResult { TransferRule = 1 };
            }

            try
            {
                var rule = JsonConvert.DeserializeObject<TransferRule>(transferRuleJson);
                if (rule != null)
                {
                    rule.TransferRatio = rule.TransferRatio <= 0 ? 1 : rule.TransferRatio;
                    return new WalletTypeRuleResult { TransferRule = rule.TransferRatio };
                }
            }
            catch (Exception)
            {
                _walletLogger.LogInformation($"Error in paesing transferRuleJson {transferRuleJson}, hence defaulting to 1");
                return new WalletTypeRuleResult { TransferRule = 1 };
            }

            return new WalletTypeRuleResult { TransferRule = 1 };
        }


        private async Task<WalletTypeModel?> GetWalletTypeByIdOrCode(long? walletTypeId, string? walletTypeCode)
        {
            if (walletTypeId.HasValue)
            {
                return await _walletTypeRepo.FindOneAsync(x => x.WalletTypeId == walletTypeId && x.DeleteNbr == 0);
            }
            else if (!string.IsNullOrEmpty(walletTypeCode))
            {
                return await _walletTypeRepo.FindOneAsync(x => x.WalletTypeCode == walletTypeCode);
            }
            return null;
        }



        private async Task<(WalletModel? MasterWallet, WalletModel? ConsumerWallet)> GetWallets(long consumerWalletTypeId, string tenantCode, string consumerCode, long? masterWalletTypeId = null)
        {
            var masterWallet = await _walletRepo.GetMasterWallet(masterWalletTypeId ?? consumerWalletTypeId, tenantCode);
            var consumerWallet = await _walletRepo.GetConsumerWallet(consumerWalletTypeId, consumerCode);

            return (masterWallet, consumerWallet);
        }




















        /// <summary>
        /// 
        /// </summary>
        /// <param name="redeemStartRequestDto"></param>
        /// <returns></returns>
        public async Task<PostRedeemStartResponseDto> RedeemStartOuter(PostRedeemStartRequestDto redeemStartRequestDto)
        {
            const string methodName = nameof(RedeemStartOuter);
            int maxTries = _maxTries;
            PostRedeemStartResponseDto? response = null;
            _walletLogger.LogInformation("{className}.{methodName}: Has been Invoked for ConsumerCode: {ConsumerCode}", className, methodName, redeemStartRequestDto.ConsumerCode);

            while (maxTries > 0)
            {
                try
                {
                    response = await RedeemStart(redeemStartRequestDto);
                    if (response.ErrorCode == null)
                    {
                        break; // Op success no need to retry.
                    }

                    _session.Clear();
                    _walletLogger.LogError("{className}.{methodName}: Response errorCode {errCode} in RedeemStartOuter retrying count left={maxTries}, redemptionRef: {redRef}", className, methodName, response.ErrorCode, maxTries,
                        redeemStartRequestDto.RedemptionRef);
                    maxTries--;
                }
                catch (Exception ex)
                {
                    _session.Clear();
                    _walletLogger.LogError(ex, "{className}.{methodName}: Error in RedeemStartOuter retrying count left={maxTries}", className, methodName, maxTries);
                    maxTries--;
                    Thread.Sleep(_random.Next(RETRY_MIN_WAIT_MS, RETRY_MAX_WAIT_MS));
                }
            }
            if (response == null)
            {
                response = new PostRedeemStartResponseDto()
                {
                    ErrorCode = StatusCodes.Status500InternalServerError
                };
                _walletLogger.LogWarning("{className}.{methodName}: Using a fallback response due to multiple retries: {response}", className, methodName, response.ToJson());
                return response;
            }

            // audit trail
            if (response.ErrorCode == null || response.ErrorCode == StatusCodes.Status200OK)
            {
                await _auditTrailService.PostAuditTrail(new AuditTrailDto()
                {
                    SourceModule = "WALLET_API",
                    SourceContext = "walletService.RedeemStartOuter",
                    AuditName = "REDEEM_START",
                    AuditMessage = $"Redeem started for: consumer: {redeemStartRequestDto.ConsumerCode}",
                    CreateUser = "SYSTEM",
                    AuditJsonData = redeemStartRequestDto.ToJson()
                });
                _walletLogger.LogInformation("{className}.{methodName}: Succeeded for ConsumerCode: {ConsumerCode}", className, methodName, redeemStartRequestDto.ConsumerCode);
            }
            else
            {
                _walletLogger.LogError("{className}.{methodName}: Response errorCode {errCode} in RedeemStartOuter  consumer: {consCode}", className, methodName, response.ErrorCode,
                        redeemStartRequestDto.ConsumerCode);
            }

            return response;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="redeemStartRequestDto"></param>
        /// <returns></returns>
        public async Task<PostRedeemStartResponseDto> RedeemStart(PostRedeemStartRequestDto redeemStartRequestDto)
        {
            const string methodName = nameof(RedeemStart);
            _walletLogger.LogInformation("{className}.{methodName}: Started with ConsumerCode : {ConsumerCode}", className, methodName, redeemStartRequestDto.ConsumerCode);
            var consumerWallet = new WalletModel();
            var redemptionWalletType = new WalletTypeModel();
            var consumerWalletType = new WalletTypeModel(); 
            if (!string.IsNullOrEmpty(redeemStartRequestDto.ConsumerWalletTypeCode))
            {
                consumerWalletType = await _walletTypeRepo.FindOneAsync(w => w.WalletTypeCode == redeemStartRequestDto.ConsumerWalletTypeCode);

                redemptionWalletType = await _walletTypeRepo.FindOneAsync(w => w.WalletTypeCode == redeemStartRequestDto.RedemptionWalletTypeCode);

                consumerWallet = await _walletRepo.GetConsumerWallet(consumerWalletType.WalletTypeId, redeemStartRequestDto.ConsumerCode ?? string.Empty);
            }
            else
            {
                redemptionWalletType = await _walletTypeRepo.FindOneAsync(w => w.WalletTypeCode == redeemStartRequestDto.RedemptionWalletTypeCode);
                consumerWallet = await _walletRepo.GetConsumerWalletById(redeemStartRequestDto.WalletId?? 0, redeemStartRequestDto.ConsumerCode ?? string.Empty);
            }

            _walletLogger.LogInformation("{className}.{methodName}: consumer wallet, xmin: {xmin}", className, methodName, consumerWallet?.Xmin);
            if (consumerWallet?.Balance < redeemStartRequestDto.RedemptionAmount)
            {
                _walletLogger.LogError("{className}.{methodName}: Consumer Wallet Balance Less than Redemption Amount. For Consumer Code:{consumer}, Error Code:{errorCode}", className, methodName, redeemStartRequestDto.ConsumerCode, HeliosErrorCode.InSufficientFunds);
                return new PostRedeemStartResponseDto()
                {
                    ErrorMessage = "Consumer wallet does not have sufficient funds",
                    ErrorCode = (int?)HeliosErrorCode.InSufficientFunds
                };
            }

            var redemptionWallets = await _walletRepo.FindAsync(x => x.WalletTypeId == redemptionWalletType.WalletTypeId &&
                                               x.TenantCode == redeemStartRequestDto.TenantCode && x.DeleteNbr == 0 && x.MasterWallet);

            var redemptionWallet = redemptionWallets?.Any() == true
                ? (redemptionWallets.Count == 1
                    ? redemptionWallets.First()
                    : redemptionWallets.FirstOrDefault(x => x.WalletName == "TENANT_MASTER_REDEMPTION:" + redeemStartRequestDto.RedemptionVendorCode)) // need to fix in the future RedemptionVendorCode
                : null;

            if (redemptionWallet == null)
            {
                _walletLogger.LogError("{className}.{methodName}: Master Redemption Wallet not found for TenantCode:{TenantCode}, Error Code:{errorCode}", className, methodName, redeemStartRequestDto.TenantCode, StatusCodes.Status404NotFound);
                return new PostRedeemStartResponseDto()
                {
                    ErrorMessage = "Master Redemption Wallet not found",
                    ErrorCode = (int?)StatusCodes.Status404NotFound
                };
            }


            var maxRedemptionWalletTransactionId = await _transactionRepo.GetMaxTransactionIdByWallet(redemptionWallet.WalletId);
            var lastRedemptionWalletTransaction = await _transactionRepo.FindOneAsync(t => t.TransactionId == maxRedemptionWalletTransactionId);

            var maxConsumerWalletTransactionId = await _transactionRepo.GetMaxTransactionIdByWallet(consumerWallet?.WalletId ?? 0);

            var lastConsumerWalletTransaction = await _transactionRepo.FindOneAsync(t => t.TransactionId == maxConsumerWalletTransactionId);

            var uniqueRedemptionRef = redeemStartRequestDto.RedemptionVendorCode + ":" + redeemStartRequestDto.RedemptionRef;

            using (var transaction = _session.BeginTransaction())
            {
                var transactionTs = DateTime.UtcNow;

                try
                {
                    var transactionDetail = new TransactionDetailModel
                    {
                        TransactionDetailType = "REDEMPTION",
                        ConsumerCode = redeemStartRequestDto.ConsumerCode,
                        TaskRewardCode = null,
                        Notes = redeemStartRequestDto.Notes,
                        RedemptionRef = uniqueRedemptionRef,
                        RedemptionItemDescription = redeemStartRequestDto.RedemptionItemDescription,
                        CreateTs = transactionTs,
                        CreateUser = Constants.CreateUser,
                        DeleteNbr = 0
                    };
                    var transactionDetailResponse = await _session.SaveAsync(transactionDetail);
                    transactionDetail.TransactionDetailId = Convert.ToInt64(transactionDetailResponse);

                    string newTxnCode = $"txn-{Guid.NewGuid().ToString().Replace("-", "")}";

                    var consumerWalletBalance = consumerWallet?.Balance - redeemStartRequestDto.RedemptionAmount;

                    var subTransaction = new TransactionModel
                    {
                        WalletId = consumerWallet?.WalletId ?? 0,
                        TransactionCode = newTxnCode,
                        TransactionType = Constants.Subtract,
                        PreviousBalance = consumerWallet?.Balance ?? 0,
                        TransactionAmount = redeemStartRequestDto.RedemptionAmount,
                        Balance = consumerWalletBalance,
                        PrevWalletTxnCode = consumerWallet?.WalletId + ":" + (lastConsumerWalletTransaction != null ?
                            lastConsumerWalletTransaction.TransactionCode : "init"),
                        CreateTs = transactionTs,
                        CreateUser = Constants.CreateUser,
                        UpdateTs = DateTime.UtcNow,
                        UpdateUser = Constants.CreateUser,
                        DeleteNbr = 0,
                        TransactionDetailId = transactionDetail.TransactionDetailId
                    };
                    var subTransactionResponse = await _session.SaveAsync(subTransaction);
                    subTransaction.TransactionId = Convert.ToInt64(subTransactionResponse);

                    var redemptionWalletBalance = redemptionWallet.Balance + redeemStartRequestDto.RedemptionAmount;
                    var addTransaction = new TransactionModel
                    {
                        WalletId = redemptionWallet.WalletId,
                        TransactionCode = newTxnCode,
                        TransactionType = Constants.Addition,
                        PreviousBalance = redemptionWallet.Balance,
                        TransactionAmount = redeemStartRequestDto.RedemptionAmount,
                        Balance = redemptionWalletBalance,
                        PrevWalletTxnCode = redemptionWallet.WalletId + ":" + (lastRedemptionWalletTransaction != null ?
                            lastRedemptionWalletTransaction.TransactionCode : "init"),
                        CreateTs = transactionTs,
                        CreateUser = Constants.CreateUser,
                        UpdateTs = DateTime.UtcNow,
                        UpdateUser = Constants.CreateUser,
                        DeleteNbr = 0,
                        TransactionDetailId = transactionDetail.TransactionDetailId
                    };
                    var addTransactionResponse = await _session.SaveAsync(addTransaction);
                    addTransaction.TransactionId = Convert.ToInt64(addTransactionResponse);

                    var redemption = new RedemptionModel
                    {
                        SubTransactionId = subTransaction.TransactionId,
                        AddTransactionId = addTransaction.TransactionId,
                        RedemptionStatus = Constants.InProgress,
                        RedemptionRef = uniqueRedemptionRef,
                        RedemptionStartTs = transactionTs,
                        CreateTs = transactionTs,
                        CreateUser = Constants.CreateUser,
                        UpdateTs = DateTime.UtcNow,
                        UpdateUser = Constants.CreateUser,
                        DeleteNbr = 0,
                        RedemptionItemDescription = redeemStartRequestDto.RedemptionItemDescription,
                        RedemptionItemData = redeemStartRequestDto.RedemptionItemData
                    };
                    await _session.SaveAsync(redemption);

                    int rec = _walletRepo.UpdateRedemptionWalletBalance(transactionTs, redemptionWalletBalance, redemptionWallet.WalletId, redemptionWallet.Xmin);
                    if (rec == 0)
                    {
                        _walletLogger.LogError("{className}.{methodName}: Error: Concurrency error could not update master wallet: WalletId: {walletId} xmin: {xmin}, Error Code:{errorCode}", className, methodName,
                            redemptionWallet.WalletId, redemptionWallet.Xmin, StatusCodes.Status409Conflict);

                        await transaction.RollbackAsync();
                        return new PostRedeemStartResponseDto()
                        {
                            ErrorMessage = $"Concurrency error in updating Master Wallet: {redemptionWallet.WalletId}",
                            ErrorCode = StatusCodes.Status409Conflict
                        };
                    }
                    else
                    {
                        _walletLogger.LogInformation("{className}.{methodName}: updated master wallet: {walletId}, balance: {balance}, count: {rec}, xmin: {xmin}", className, methodName,
                            consumerWallet?.WalletId, consumerWalletBalance, rec, consumerWallet?.Xmin);
                    }

                    int recc = _walletRepo.UpdateConsumerWalletBalance(transactionTs, consumerWalletBalance, consumerWallet?.TotalEarned, consumerWallet?.WalletId ?? 0, consumerWallet?.Xmin ?? 0);
                    if (recc == 0)
                    {
                        _walletLogger.LogError("{className}.{methodName}: Error: Concurrency error could not update master wallet: WalletId: {walletId} xmin: {xmin}, Error Code:{errorCode}", className, methodName, consumerWallet?.WalletId, consumerWallet?.Xmin, StatusCodes.Status409Conflict);

                        await transaction.RollbackAsync();
                        return new PostRedeemStartResponseDto()
                        {
                            ErrorMessage = $"Concurrency error in updating Consumer Wallet: {consumerWallet?.WalletId}",
                            ErrorCode = StatusCodes.Status409Conflict
                        };
                    }
                    else
                    {
                        _walletLogger.LogInformation("{className}.{methodName}: updated consumer wallet: {walletId}, balance: {balance}, count: {rec}, xmin: {xmin}", className, methodName,
                            consumerWallet?.WalletId, consumerWalletBalance, rec, consumerWallet?.Xmin);
                    }
                    await transaction.CommitAsync();

                    _walletLogger.LogInformation("{className}.{methodName}: commit with Request : {ConsumerCode}", className, methodName, redeemStartRequestDto.ConsumerCode);


                    return new PostRedeemStartResponseDto()
                    {
                        AddEntry = _mapper.Map<TransactionDto>(addTransaction),
                        SubEntry = _mapper.Map<TransactionDto>(subTransaction),
                        TransactionDetail = _mapper.Map<TransactionDetailDto>(transactionDetail),
                        Redemption = _mapper.Map<RedemptionDto>(redemption),
                    };
                }
                catch (Exception ex)
                {
                    _walletLogger.LogError(ex, "{className}.{methodName}: ERROR - msg : {msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status409Conflict);

                    transaction.Rollback();
                    return new PostRedeemStartResponseDto()
                    {
                        ErrorCode = StatusCodes.Status409Conflict,
                        ErrorMessage = ex.Message,
                        ErrorDescription = ex.InnerException?.Message
                    };
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="redeemCompleteRequestDto"></param>
        /// <returns></returns>
        public async Task<PostRedeemCompleteResponseDto> RedeemCompleteOuter(PostRedeemCompleteRequestDto redeemCompleteRequestDto)
        {
            const string methodName = nameof(RedeemCompleteOuter);
            int maxTries = _maxTries;
            PostRedeemCompleteResponseDto? response = null;
            _walletLogger.LogInformation("{className}.{methodName}: Has been Invoked for ConsumerCode: {ConsumerCode}", className, methodName, redeemCompleteRequestDto.ConsumerCode);

            while (maxTries > 0)
            {
                try
                {
                    response = await RedeemComplete(redeemCompleteRequestDto);
                    if (response.ErrorCode == null)
                    {
                        break; // Op success no need to retry.
                    }

                    _session.Clear();
                    _walletLogger.LogError("{className}.{methodName}: Response errorCode {errCode} in RedeemCompleteOuter retrying count left={maxTries}, redemptionRef: {redRef}", className, methodName, response.ErrorCode, maxTries,
                        redeemCompleteRequestDto.RedemptionRef);
                    maxTries--;
                }
                catch (Exception ex)
                {
                    _session.Clear();
                    _walletLogger.LogError(ex, "{className}.{methodName}: Error in RedeemCompleteOuter retrying count left={maxTries}", className, methodName, maxTries);
                    maxTries--;
                    Thread.Sleep(_random.Next(RETRY_MIN_WAIT_MS, RETRY_MAX_WAIT_MS));
                }
            }
            if (response == null)
            {
                response = new PostRedeemCompleteResponseDto()
                {
                    ErrorCode = StatusCodes.Status500InternalServerError
                };
                _walletLogger.LogError("{className}.{methodName}: ERROR Using a fallback response due to multiple retries: {response},Error Code:{errorCode}", className, methodName, response.ToJson(), StatusCodes.Status500InternalServerError);
                return response;
            }

            if (response.ErrorCode == null || response.ErrorCode == StatusCodes.Status200OK)
            {
                // audit trail
                await _auditTrailService.PostAuditTrail(new AuditTrailDto()
                {
                    SourceModule = "WALLET_API",
                    SourceContext = "walletService.RedeemCompleteOuter",
                    AuditName = "REDEEM_COMPLETE",
                    AuditMessage = $"Redeem completed for: consumer: {redeemCompleteRequestDto.ConsumerCode}",
                    CreateUser = "SYSTEM",
                    AuditJsonData = redeemCompleteRequestDto.ToJson()
                });
                _walletLogger.LogInformation("{className}.{methodName}: Succeeded for ConsumerCode: {ConsumerCode}", className, methodName, redeemCompleteRequestDto.ConsumerCode);
            }
            else
            {
                _walletLogger.LogError("{className}.{methodName}: Response errorCode {errCode} in RedeemCompleteOuter  consumer: {consCode}", className, methodName, response.ErrorCode,
                       redeemCompleteRequestDto.ConsumerCode);
            }

            return response;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="redeemCompleteRequestDto"></param>
        /// <returns></returns>
        public async Task<PostRedeemCompleteResponseDto> RedeemComplete(PostRedeemCompleteRequestDto redeemCompleteRequestDto)
        {
            const string methodName = nameof(RedeemComplete);
            _walletLogger.LogInformation("{className}.{methodName}: Started with Request : {ConsumerCode}", className, methodName, redeemCompleteRequestDto.ConsumerCode);


            var uniqueRedemptionRef = redeemCompleteRequestDto.RedemptionVendorCode + ":" + redeemCompleteRequestDto.RedemptionRef;

            using (var transaction = _session.BeginTransaction())
            {
                try
                {
                    var timeStamp = DateTime.UtcNow;

                    var walletRedemption = await _redemptionRepo.FindOneAsync(x => x.RedemptionRef == uniqueRedemptionRef && x.DeleteNbr == 0);

                    if (walletRedemption == null)
                    {
                        _walletLogger.LogError("{className}.{methodName}: Wallet redemption record Not Found. For Consumer Code:{consumer}, Error Code:{errorCode}", className, methodName, redeemCompleteRequestDto.ConsumerCode, StatusCodes.Status404NotFound);
                        return new PostRedeemCompleteResponseDto()
                        {
                            ErrorMessage = "Redemption record not found.",
                            ErrorCode = StatusCodes.Status404NotFound
                        };
                    }

                    if (walletRedemption.RedemptionStatus == Constants.Reverted)
                    {
                        _walletLogger.LogInformation("{className}.{methodName}: Wallet redemption status Already reverted successfully", className, methodName);
                        return new PostRedeemCompleteResponseDto()
                        {
                            ErrorMessage = "Already Reverted.",
                            ErrorCode = StatusCodes.Status200OK
                        };
                    }
                    var subTransaction = await _transactionRepo.FindOneAsync(x => x.TransactionId == walletRedemption.SubTransactionId);

                    var associatedTransactionDetail = await _transactionDetailRepo.FindOneAsync(x => x.TransactionDetailId == subTransaction.TransactionDetailId);

                    if (associatedTransactionDetail.ConsumerCode != redeemCompleteRequestDto.ConsumerCode)
                    {
                        _walletLogger.LogError("{className}.{methodName}: Invalid Consumer Code, record Not Found Consumer Code:{consumer}, Error Code:{errorCode}", className, methodName, redeemCompleteRequestDto.ConsumerCode, StatusCodes.Status400BadRequest);
                        return new PostRedeemCompleteResponseDto()
                        {
                            ErrorMessage = "Invalid Consumer Code.",
                            ErrorCode = StatusCodes.Status400BadRequest
                        };
                    }
                    int rec = _redemptionRepo.UpdateRedemption(timeStamp, walletRedemption.RedemptionId, walletRedemption.Xmin);
                    if (rec == 0)
                    {
                        _walletLogger.LogError("{className}.{methodName}: Error: Concurrency error could not update redemption wallet: RedemptionId: {redemptionId} xmin: {xmin}, Error Code:{errorCode}", className, methodName,
                            walletRedemption.RedemptionId, walletRedemption.Xmin, StatusCodes.Status409Conflict);

                        await transaction.RollbackAsync();
                        return new PostRedeemCompleteResponseDto()
                        {
                            ErrorMessage = $"Concurrency error in updating Master Wallet: {walletRedemption.RedemptionId}",
                            ErrorCode = StatusCodes.Status409Conflict
                        };
                    }
                    else
                    {
                        _walletLogger.LogInformation("{className}.{methodName}: updated master wallet: Redemption: {redemptionId}, count: {rec}, xmin: {xmin}", className, methodName,
                            walletRedemption.RedemptionId, rec, walletRedemption.Xmin);
                    }
                    walletRedemption.RedemptionStatus = Constants.Completed;
                    walletRedemption.RedemptionCompleteTs = timeStamp;
                    walletRedemption.UpdateTs = timeStamp;
                    walletRedemption.UpdateUser = Constants.CreateUser;
                    await _session.UpdateAsync(walletRedemption);

                    await transaction.CommitAsync();
                    _walletLogger.LogInformation("{className}.{methodName}: commit with Request : {ConsumerCode}", className, methodName, redeemCompleteRequestDto.ConsumerCode);
                    return new PostRedeemCompleteResponseDto()
                    {
                        Redemption = _mapper.Map<RedemptionDto>(walletRedemption)
                    };
                }
                catch (Exception ex)
                {
                    _walletLogger.LogError(ex, "{className}.{methodName}: ERROR - msg : {msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status409Conflict);
                    transaction.Rollback();
                    return new PostRedeemCompleteResponseDto()
                    {
                        ErrorCode = StatusCodes.Status409Conflict,
                        ErrorMessage = ex.Message,
                        ErrorDescription = ex.InnerException?.Message
                    };
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="postRedeemFailRequestDto"></param>
        /// <returns></returns>
        public async Task<PostRedeemFailResponseDto> RedeemFailOuter(PostRedeemFailRequestDto postRedeemFailRequestDto)
        {
            const string methodName = nameof(RedeemFailOuter);
            int maxTries = _maxTries;
            PostRedeemFailResponseDto? response = null;
            _walletLogger.LogInformation("{className}.{methodName}: Has been Invoked for ConsumerCode: {ConsumerCode}", className, methodName, postRedeemFailRequestDto.ConsumerCode);
            while (maxTries > 0)
            {
                try
                {
                    response = await RedeemFail(postRedeemFailRequestDto);
                    if (response.ErrorCode == null)
                    {
                        break; // Op success no need to retry.
                    }

                    _session.Clear();
                    _walletLogger.LogError("{className}.{methodName}: Response errorCode {errCode} in RedeemFailOuter retrying count left={maxTries}, redemptionRef: {redRef}", className, methodName, response.ErrorCode, maxTries,
                        postRedeemFailRequestDto.RedemptionRef);
                    maxTries--;
                }
                catch (Exception ex)
                {
                    _session.Clear();
                    _walletLogger.LogError(ex, "{className}.{methodName}: Error in RedeemFailOuter retrying count left={maxTries}", className, methodName, maxTries);
                    maxTries--;
                    Thread.Sleep(_random.Next(RETRY_MIN_WAIT_MS, RETRY_MAX_WAIT_MS));
                }
            }
            if (response == null)
            {
                response = new PostRedeemFailResponseDto()
                {
                    ErrorCode = StatusCodes.Status500InternalServerError
                };
                _walletLogger.LogWarning("{className}.{methodName}: Using a fallback response due to multiple retries: {response}, Error Code:{errorCode}", className, methodName, response.ToJson(), StatusCodes.Status500InternalServerError);
                return response;
            }

            if (response.ErrorCode == null || response.ErrorCode == StatusCodes.Status200OK)
            {
                // audit trail
                await _auditTrailService.PostAuditTrail(new AuditTrailDto()
                {
                    SourceModule = "WALLET_API",
                    SourceContext = "walletService.RedeemFailOuter",
                    AuditName = "REDEEM_FAIL",
                    AuditMessage = $"Redeem failed for: consumer: {postRedeemFailRequestDto.ConsumerCode}, amount: {postRedeemFailRequestDto.RedemptionAmount}",
                    CreateUser = "SYSTEM",
                    AuditJsonData = postRedeemFailRequestDto.ToJson()
                });
                _walletLogger.LogInformation("{className}.{methodName}: Succeeded for ConsumerCode: {ConsumerCode}", className, methodName, postRedeemFailRequestDto.ConsumerCode);
            }
            else
            {
                _walletLogger.LogError("{className}.{methodName}: Response errorCode {errCode} in RedeemFailOuter  consumer: {consCode}", className, methodName, response.ErrorCode,
                       postRedeemFailRequestDto.ConsumerCode);
            }
            return response;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="postRedeemFailRequestDto"></param>
        /// <returns></returns>
        public async Task<PostRedeemFailResponseDto> RedeemFail(PostRedeemFailRequestDto postRedeemFailRequestDto)
        {
            const string methodName = nameof(RedeemFail);
            _walletLogger.LogInformation("{className}.{methodName}: Started with Request : {ConsumerCode}", className, methodName, postRedeemFailRequestDto.ConsumerCode);

            var uniqueRedemptionRef = postRedeemFailRequestDto.RedemptionVendorCode + ":" + postRedeemFailRequestDto.RedemptionRef;
            var walletRedemption = await _redemptionRepo.FindOneAsync(x => x.RedemptionRef == uniqueRedemptionRef && x.DeleteNbr == 0);
            if (walletRedemption == null)
            {
                _walletLogger.LogInformation("{className}.{methodName}: Wallet redemption record Not Found. For Consumer Code:{consumer}, Error Code:{errorCode}", className, methodName, postRedeemFailRequestDto.ConsumerCode, StatusCodes.Status200OK);
                return new PostRedeemFailResponseDto()
                {
                    ErrorMessage = "Redemption record not found.",
                    ErrorCode = StatusCodes.Status200OK // need to return 200 OK as per Prizeout failure sequence
                };
            }
            if (walletRedemption.RedemptionStatus == Constants.Reverted)
            {
                _walletLogger.LogInformation("{className}.{methodName}: Wallet redemption status Already reverted successfully", className, methodName);
                return new PostRedeemFailResponseDto()
                {
                    ErrorMessage = "Already Reverted.",
                    ErrorCode = StatusCodes.Status200OK
                };
            }
            var originalSubTxn = await _transactionRepo.FindOneAsync(x => x.TransactionId == walletRedemption.SubTransactionId);
            var originalAddTxn = await _transactionRepo.FindOneAsync(x => x.TransactionId == walletRedemption.AddTransactionId);
            if (originalSubTxn == null || originalAddTxn == null)
            {
                _walletLogger.LogError("{className}.{methodName}: Original Sub txn or Original Add txn Null. Error Code:{errorCode}", className, methodName, StatusCodes.Status400BadRequest);
                return new PostRedeemFailResponseDto()
                {
                    ErrorCode = StatusCodes.Status400BadRequest
                };
            }
            if (postRedeemFailRequestDto.RedemptionAmount != originalSubTxn.TransactionAmount)
            {
                _walletLogger.LogError("{className}.{methodName}: Original Sub txn amount Not equal Redemption Amount. Error Code:{errorCode}", className, methodName, StatusCodes.Status400BadRequest);
                return new PostRedeemFailResponseDto()
                {
                    ErrorCode = StatusCodes.Status400BadRequest
                };
            }
            var consumerWalletId = originalSubTxn.WalletId;
            var consumerWallet = await _walletRepo.FindOneAsync(x => x.WalletId == consumerWalletId);
            var redemptionWalletId = originalAddTxn.WalletId;
            var redemptionWallet = await _walletRepo.FindOneAsync(x => x.WalletId == redemptionWalletId);
            var maxRedemptionWalletTransactionId = await _transactionRepo.GetMaxTransactionIdByWallet(redemptionWallet.WalletId);
            var lastRedemptionWalletTransaction = await _transactionRepo.FindOneAsync(t => t.TransactionId == maxRedemptionWalletTransactionId);
            var maxConsumerWalletTransactionId = await _transactionRepo.GetMaxTransactionIdByWallet(consumerWallet.WalletId);
            var lastConsumerWalletTransaction = await _transactionRepo.FindOneAsync(t => t.TransactionId == maxConsumerWalletTransactionId);
            using (var transaction = _session.BeginTransaction())
            {
                var transactionTs = DateTime.UtcNow;

                try
                {
                    var transactionDetail = new TransactionDetailModel
                    {
                        TransactionDetailType = "RETURN",
                        ConsumerCode = postRedeemFailRequestDto.ConsumerCode,
                        TaskRewardCode = null,
                        Notes = postRedeemFailRequestDto.Notes,
                        RedemptionRef = uniqueRedemptionRef,
                        RedemptionItemDescription = Constants.Refund,
                        CreateTs = transactionTs,
                        CreateUser = Constants.CreateUser,  // needs to be fixed
                        DeleteNbr = 0
                    };
                    var transactionDetailResponse = await _session.SaveAsync(transactionDetail);
                    transactionDetail.TransactionDetailId = Convert.ToInt64(transactionDetailResponse);

                    string newTxnCode = $"txn-{Guid.NewGuid().ToString().Replace("-", "")}";

                    var redemptionWalletBalance = redemptionWallet.Balance - originalSubTxn.TransactionAmount;
                    var subTransaction = new TransactionModel
                    {
                        WalletId = redemptionWalletId,
                        TransactionCode = newTxnCode,
                        TransactionType = Constants.Subtract,
                        PreviousBalance = redemptionWallet.Balance,
                        TransactionAmount = originalSubTxn.TransactionAmount ?? 0,
                        Balance = redemptionWalletBalance ?? 0,
                        PrevWalletTxnCode = redemptionWalletId + ":" + (lastRedemptionWalletTransaction != null ?
                            lastRedemptionWalletTransaction.TransactionCode : "init"),
                        CreateTs = transactionTs,
                        CreateUser = Constants.CreateUser,
                        DeleteNbr = 0,
                        TransactionDetailId = transactionDetail.TransactionDetailId
                    };
                    var subTransactionResponse = await _session.SaveAsync(subTransaction);
                    subTransaction.TransactionId = Convert.ToInt64(subTransactionResponse);

                    var consumerWalletBalance = consumerWallet.Balance + originalSubTxn.TransactionAmount;
                    var addTransaction = new TransactionModel
                    {
                        WalletId = consumerWalletId,
                        TransactionCode = newTxnCode,
                        TransactionType = Constants.Addition,
                        PreviousBalance = consumerWallet.Balance,
                        TransactionAmount = originalSubTxn.TransactionAmount,
                        Balance = consumerWalletBalance,
                        PrevWalletTxnCode = consumerWalletId + ":" + (lastConsumerWalletTransaction != null ?
                            lastConsumerWalletTransaction.TransactionCode : "init"),
                        CreateTs = transactionTs,
                        CreateUser = Constants.CreateUser,
                        DeleteNbr = 0,
                        TransactionDetailId = transactionDetail.TransactionDetailId
                    };
                    var addTransactionResponse = await _session.SaveAsync(addTransaction);
                    addTransaction.TransactionId = Convert.ToInt64(addTransactionResponse);
                    walletRedemption.RevertSubTransactionId = subTransaction.TransactionId;
                    walletRedemption.RevertAddTransactionId = addTransaction.TransactionId;
                    walletRedemption.RedemptionStatus = Constants.Reverted;
                    walletRedemption.RedemptionCompleteTs = null;
                    walletRedemption.RedemptionRevertTs = transactionTs;
                    walletRedemption.UpdateTs = transactionTs;
                    walletRedemption.UpdateUser = Constants.CreateUser;
                    var oldRedmXmin = redemptionWallet.Xmin;
                    await _session.UpdateAsync(walletRedemption);

                    int rec = _walletRepo.UpdateRedemptionWalletBalance(transactionTs, redemptionWalletBalance, redemptionWallet.WalletId, oldRedmXmin);
                    if (rec == 0)
                    {
                        _walletLogger.LogError("{className}.{methodName}: Error: Concurrency error could not update master wallet: WalletId: {walletId} xmin: {xmin}, Error Code:{errorCode}", className, methodName,
                            redemptionWallet.WalletId, redemptionWallet.Xmin, StatusCodes.Status409Conflict);

                        await transaction.RollbackAsync();
                        return new PostRedeemFailResponseDto()
                        {
                            ErrorMessage = $"Concurrency error in updating Master Wallet: {redemptionWallet.WalletId}",
                            ErrorCode = StatusCodes.Status409Conflict
                        };
                    }
                    else
                    {
                        _walletLogger.LogInformation("{className}.{methodName}: updated master wallet: {walletId}, balance: {balance}, count: {rec}, xmin: {xmin}", className, methodName,
                            consumerWallet.WalletId, consumerWalletBalance, rec, consumerWallet.Xmin);
                    }

                    int recc = _walletRepo.UpdateConsumerWalletBalance(transactionTs, consumerWalletBalance, consumerWallet?.TotalEarned, consumerWallet?.WalletId ?? 0, consumerWallet?.Xmin ?? 0);
                    if (recc == 0)
                    {
                        _walletLogger.LogError("{className}.{methodName}: Error Concurrency error could not update consumer wallet: WalletId: {walletId} xmin: {xmin}, Error Code:{errorCode}", className, methodName,
                            consumerWallet?.WalletId, consumerWallet?.Xmin, StatusCodes.Status409Conflict);

                        await transaction.RollbackAsync();
                        return new PostRedeemFailResponseDto()
                        {
                            ErrorMessage = $"Concurrency error in updating Consumer Wallet: {consumerWallet?.WalletId}",
                            ErrorCode = StatusCodes.Status409Conflict
                        };
                    }
                    else
                    {
                        _walletLogger.LogInformation("{className}.{methodName}: updated consumer wallet: {walletId}, balance: {balance}, count: {rec}, xmin: {xmin}", className, methodName,
                            consumerWallet?.WalletId, consumerWalletBalance, rec, consumerWallet?.Xmin);
                    }

                    await transaction.CommitAsync();
                    _walletLogger.LogInformation("{className}.{methodName}: commit with Request : {ConsumerCode}", className, methodName, postRedeemFailRequestDto.ConsumerCode);
                    return new PostRedeemFailResponseDto()
                    {
                        RevertAddEntry = _mapper.Map<TransactionDto>(addTransaction),
                        RevertSubEntry = _mapper.Map<TransactionDto>(subTransaction),
                        TransactionDetail = _mapper.Map<TransactionDetailDto>(transactionDetail),
                        Redemption = _mapper.Map<RedemptionDto>(walletRedemption),
                    };
                }
                catch (Exception ex)
                {

                    _walletLogger.LogError(ex, "{className}.{methodName}: ERROR - msg : {msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status409Conflict);

                    await transaction.RollbackAsync();
                    return new PostRedeemFailResponseDto()
                    {
                        ErrorCode = StatusCodes.Status409Conflict,
                        ErrorMessage = ex.Message,
                        ErrorDescription = ex.InnerException?.Message
                    };
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="findConsumerWalletRequestDto"></param>
        /// <returns></returns>
        public async Task<WalletResponseDto> GetWallets(FindConsumerWalletRequestDto findConsumerWalletRequestDto)
        {
            const string methodName = nameof(GetWallets);
            try
            {
                var walletResponse = new WalletResponseDto();
                var consumerWallet = await GetConsumerWalletsWithWalletdetails(findConsumerWalletRequestDto);

                if (consumerWallet == null || consumerWallet.Count <= 0)
                {
                    _walletLogger.LogError("{className}.{methodName}: ConsumerWallets not found for Consumer Code: {ConsumerCode}, Error Code:{errorCode}", className, methodName, findConsumerWalletRequestDto.ConsumerCode, StatusCodes.Status404NotFound);
                    return new WalletResponseDto
                    {
                        ErrorCode = StatusCodes.Status404NotFound,
                        ErrorMessage = "ConsumerWallets not found"
                    };
                }

                var walletDetailList = new List<WalletDetailDto>();

                decimal grandTotal = 0;
                var getConsumerRequestDto = new GetConsumerRequestDto
                {
                    ConsumerCode = findConsumerWalletRequestDto.ConsumerCode
                };
                var consumer = await _consumerService.GetConsumer(getConsumerRequestDto);
                foreach (var wallet in consumerWallet)
                {
                    if (wallet.Wallet == null)
                    {
                        _walletLogger.LogError("{className}.{methodName}: Wallet Not found for Consumer Code: {ConsumerCode}, Error Code:{errorCode}", className, methodName, findConsumerWalletRequestDto.ConsumerCode, StatusCodes.Status404NotFound);
                        continue;
                    }
                    if (wallet.WalletType == null)
                    {
                        _walletLogger.LogError("{className}.{methodName}: WalletType Not found for Consumer Code: {ConsumerCode}, Error Code:{errorCode}", className, methodName, findConsumerWalletRequestDto.ConsumerCode, StatusCodes.Status404NotFound);
                        continue;
                    }

                    grandTotal += Convert.ToDecimal(wallet.Wallet.Balance);

                    var recentTransactionsRequestDto = new GetRecentTransactionRequestDto
                    {
                        ConsumerCode = findConsumerWalletRequestDto.ConsumerCode,
                        WalletId = wallet.Wallet.WalletId,
                        Count = 4
                    };
                    var recentTransactionResponseDto = await _transactionService.GetTransactionDetails(recentTransactionsRequestDto, consumer);

                    walletDetailList.Add(new WalletDetailDto
                    {
                        Wallet = _mapper.Map<WalletDto>(wallet.Wallet),
                        WalletType = _mapper.Map<WalletTypeDto>(wallet.WalletType),
                        RecentTransaction = recentTransactionResponseDto.Transactions
                    });
                }

                walletResponse.walletDetailDto = walletDetailList.ToArray();

                walletResponse.GrandTotal = Convert.ToDouble(decimal.Round(grandTotal, 2, MidpointRounding.AwayFromZero));
                _walletLogger.LogInformation("{className}.{methodName}: Retrieved Wallet Successfully for ConsumerCode : {ConsumerCode}", className, methodName, findConsumerWalletRequestDto.ConsumerCode);
                return walletResponse;
            }
            catch (Exception ex)
            {
                _walletLogger.LogError(ex, "{className}.{methodName}: ERROR - msg : {msg}", className, methodName, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Set entries (secondary) wallet balance=0.0 for all consumers of a given tenant
        /// </summary>
        /// <param name="clearEntriesWalletRequestDto"></param>
        /// <returns></returns>
        public async Task<BaseResponseDto> ClearEntriesWallet(ClearEntriesWalletRequestDto clearEntriesWalletRequestDto)
        {
            const string methodName = nameof(ClearEntriesWallet);
            try
            {
                var walletTypeData = await GetSweepstakesEntriesWalletType();

                if (walletTypeData == null)
                {
                    _walletLogger.LogError("{className}.{methodName}: Wallet Type Data is Null For Tenant Code:{tenantCode}. Error Code:{errorCode}", className, methodName, clearEntriesWalletRequestDto.TenantCode, StatusCodes.Status404NotFound);
                    return new BaseResponseDto
                    {
                        ErrorCode = StatusCodes.Status404NotFound,
                        ErrorMessage = "Wallet type not found"
                    };

                }
                await _walletRepo.ClearEntriesWalletBalance(clearEntriesWalletRequestDto?.TenantCode, walletTypeData.WalletTypeId);

                return new BaseResponseDto();
            }
            catch (Exception ex)
            {
                _walletLogger.LogError(ex, "{className}.{methodName}: ERROR - msg : {msg}", className, methodName, ex.Message);
                throw;
            }
        }

        private async Task<IList<ConsumerWalletDetailsModel>> GetConsumerWalletsWithWalletdetails(FindConsumerWalletRequestDto findConsumerWalletRequestDto)
        {
            var consumerCode = findConsumerWalletRequestDto.ConsumerCode;
            var walletTypeData = await GetSweepstakesEntriesWalletType();
            // If SweepstakesEntriesWalletType not null we are excluding that particular walletType
            var walletDetails = await _consumerWalletRepo.GetConsumerWalletsWithDetails(consumerCode ?? "", findConsumerWalletRequestDto.IncludeRedeemOnlyWallets, walletTypeData?.WalletTypeId);
            return walletDetails;
        }

        public async Task<BaseResponseDto> UpdateWalletBalance(IList<WalletModel> walletModel)
        {
            try
            {
                foreach (var wallet in walletModel)
                {


                    var walletdata = await _walletRepo.FindOneAsync(w => w.WalletId == wallet.WalletId);

                    if (walletdata == null)
                        continue;

                    _walletRepo.UpdateWalletBalance(wallet);
                }
                return new BaseResponseDto();
            }
            catch (Exception ex)
            {
                _walletLogger.LogError(ex, "{className}.UpdateWalletBalance: ERROR - msg : {msg}, Error Code:{errorCode}", className, ex.Message, StatusCodes.Status500InternalServerError);
                return new BaseResponseDto
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = "Error Occured while Updating wallet Balance"
                };
            }
        }

        /// <summary>
        /// Creates the tenant master wallets.
        /// </summary>
        /// <param name="createTenantMasterWalletsRequest">The request DTO containing tenant and app information.</param>
        /// <returns>A <see cref="BaseResponseDto"/> indicating the result of the operation.</returns>
        public async Task<BaseResponseDto> CreateTenantMasterWallets(CreateTenantMasterWalletsRequestDto createTenantMasterWalletsRequest)
        {
            const string methodName = nameof(CreateTenantMasterWallets);
            _walletLogger.LogInformation("{ClassName}.{MethodName}: Creating wallets for TenantCode: {TenantCode}", className, methodName, createTenantMasterWalletsRequest.TenantCode);
            string masterwalletName;
            string masterRedemptionName;
            if (createTenantMasterWalletsRequest.Apps == null || createTenantMasterWalletsRequest.Apps.Length == 0)
            {
                return new BaseResponseDto { ErrorCode = StatusCodes.Status400BadRequest, ErrorMessage = "Error: Apps array cannot be empty." };
            }
            if (createTenantMasterWalletsRequest.Apps.Contains(WalletConstants.Apps.Rewards))
            {

                using var transaction = _session.BeginTransaction();
                try
                {
                    await CreateTenantMasterWallet(createTenantMasterWalletsRequest, GetRewardWalletType, WalletConstants.RewardWalletName);
                    await CreateTenantMasterWallet(createTenantMasterWalletsRequest, GetSweepstakesEntriesWalletType, WalletConstants.SweepstakesEntriesWalletName);
                    await CreateTenantMasterWallet(createTenantMasterWalletsRequest, GetRedemptionWalletType, WalletConstants.RedemptionHSAWalletName);
                    await CreateTenantMasterWallet(createTenantMasterWalletsRequest, GetRedemptionWalletType, WalletConstants.RedemptionPrizeOutWalletName);
                    await CreateTenantMasterWallet(createTenantMasterWalletsRequest, GetRedemptionWalletType, WalletConstants.RedemptionSuspenseWalletName);
                    await CreateTenantMasterWallet(createTenantMasterWalletsRequest, GetSweepstakesEntriesRedemptionWalletType, WalletConstants.SweepstakesEntriesRedemptionWalletName);
                    await transaction.CommitAsync();
                    _walletLogger.LogInformation("{ClassName}.{MethodName}: Successfully created wallets for Rewards App, TenantCode: {TenantCode},", className, methodName, createTenantMasterWalletsRequest.TenantCode);

                }
                catch (Exception ex)
                {
                    _walletLogger.LogError(ex, "{ClassName}.{MethodName}: Exception occurred while creating tenant master wallets for Rewards App. ErrorMessage: {ErrorMessage}, StackTrace: {StackTrace}", className, methodName, ex.Message, ex.StackTrace);
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            if (createTenantMasterWalletsRequest.Apps.Contains(WalletConstants.Apps.Benefits))
            {
                var purseConfig = createTenantMasterWalletsRequest.PurseConfig;
                if (purseConfig == null || purseConfig.Purses?.Count == 0 || purseConfig.Purses == null)
                {
                    return new BaseResponseDto()
                    {
                        ErrorCode = StatusCodes.Status400BadRequest,
                        ErrorMessage = "Purse config is null or empty"
                    };
                }
                using var transaction = _session.BeginTransaction();
                try
                {
                    foreach (var purse in purseConfig.Purses)
                    {
                        if (purse == null)
                        {
                            continue;
                        }
                        // getting master wallet type
                        var masterWalletType = await GetWalletType(purse.MasterWalletType);
                        // getting master redemption wallet type
                        var masterRedemptionWalletType = await GetWalletType(purse.MasterRedemptionWalletType);

                        if (masterWalletType == null || masterRedemptionWalletType == null)
                        {
                            var errorMessage = masterWalletType == null ? $"Master Wallet type not found with wallet type code:{purse.MasterWalletType}" :
                                                   $"Master redemption Wallet type not found with wallet type code:{purse.MasterRedemptionWalletType}";
                            _walletLogger.LogError("{ClassName}.{MethodName} - ErrorCode:{Code},ErrorMessage:{Msg}", className, methodName, StatusCodes.Status404NotFound, errorMessage);

                            return new BaseResponseDto()
                            {
                                ErrorCode = StatusCodes.Status404NotFound,
                                ErrorMessage = errorMessage
                            };
                        }
                        // creating master wallets
                        masterwalletName = await GetMasterWalletName(masterWalletType);
                        await SaveMasterWallet(createTenantMasterWalletsRequest, masterWalletType.WalletTypeId, masterwalletName, methodName);

                        // creating master redemeption wallets
                        masterRedemptionName = await GetMasterRedemptionWalletName(masterRedemptionWalletType);
                        await SaveMasterWallet(createTenantMasterWalletsRequest, masterRedemptionWalletType.WalletTypeId, masterRedemptionName, methodName);
                    }
                    await transaction.CommitAsync();
                    _walletLogger.LogInformation("{ClassName}.{MethodName}: Successfully created wallets for Benefits App, TenantCode: {TenantCode},", className, methodName, createTenantMasterWalletsRequest.TenantCode);
                }
                catch (Exception ex)
                {
                    _walletLogger.LogError(ex, "{ClassName}.{MethodName}: Exception occurred while creating Benefits tenant master wallets for Benefits App. " +
                        "ErrorMessage: {ErrorMessage}, StackTrace: {StackTrace}", className, methodName, ex.Message, ex.StackTrace);
                    await transaction.RollbackAsync();
                    throw;
                }

            }
            return new BaseResponseDto();
        }

        private static async Task<string> GetWalletName(WalletTypeModel masterWalletType, Func<Task<WalletTypeModel>> getWalletType, string defaultWalletName, string masterPrefix)
        {
            var walletType = await getWalletType();
            return masterWalletType.WalletTypeCode == walletType.WalletTypeCode
                ? defaultWalletName
                : masterPrefix + masterWalletType.WalletTypeLabel;
        }
        private Task<string> GetMasterRedemptionWalletName(WalletTypeModel masterRedemptionWalletType)
        {
            return GetWalletName(masterRedemptionWalletType, GetRedemptionWalletType, WalletConstants.RedemptionSuspenseWalletName, WalletConstants.TenantMasterRedemeption);
        }
        private Task<string> GetMasterWalletName(WalletTypeModel masterWalletType)
        {
            return GetWalletName(masterWalletType, GetRewardWalletType, WalletConstants.RewardWalletName, WalletConstants.TenantMaster);
        }
        private async Task SaveMasterWallet(CreateTenantMasterWalletsRequestDto createTenantMasterWalletsRequest, long walletTypeId, string walletName, string methodName)
        {
            try
            {
                _walletLogger.LogInformation("{ClassName}.{MethodName}: Starting wallet creation for TenantCode: {TenantCode}, WalletName: {WalletName}", className, methodName, createTenantMasterWalletsRequest.TenantCode, walletName);

                var masterWalletModel = await _walletRepo.FindOneAsync(x => x.WalletTypeId == walletTypeId &&
                                                    x.TenantCode == createTenantMasterWalletsRequest.TenantCode && x.MasterWallet && x.WalletName == walletName && x.DeleteNbr == 0);
                if (masterWalletModel != null && masterWalletModel.WalletId > 0)
                {
                    _walletLogger.LogWarning("{ClassName}.{MethodName} : Master wallet is already exist with TenantCode: {TenantCode}, WalletName: {WalletName}",
                        className, methodName, createTenantMasterWalletsRequest.TenantCode, walletName);
                    return;
                }
                var walletModel = new WalletModel()
                {
                    WalletTypeId = walletTypeId,
                    CustomerCode = createTenantMasterWalletsRequest.CustomerCode,
                    SponsorCode = createTenantMasterWalletsRequest.SponsorCode,
                    TenantCode = createTenantMasterWalletsRequest.TenantCode,
                    WalletCode = $"wal-{Guid.NewGuid().ToString("N")}",
                    WalletName = walletName,
                    ActiveStartTs = DateTime.UtcNow,
                    ActiveEndTs = DateTime.UtcNow.AddYears(1),
                    Balance = 0.00,
                    EarnMaximum = 0.00,
                    CreateTs = DateTime.UtcNow,
                    UpdateTs = null,
                    CreateUser = createTenantMasterWalletsRequest.CreateUser,
                    UpdateUser = null,
                    DeleteNbr = 0,
                    MasterWallet = true,
                    Active = true
                };

                await _session.SaveAsync(walletModel);

                _walletLogger.LogInformation("{ClassName}.{MethodName}: Successfully created wallet for TenantCode: {TenantCode}, WalletName: {WalletName}", className, methodName, createTenantMasterWalletsRequest.TenantCode, walletName);
            }
            catch (Exception ex)
            {
                _walletLogger.LogError(ex, "{ClassName}.{MethodName}: Exception occurred while creating wallet. TenantCode: {TenantCode}, WalletName: {WalletName}, ErrorMessage: {ErrorMessage}, StackTrace: {StackTrace}", className, methodName, createTenantMasterWalletsRequest.TenantCode, walletName, ex.Message, ex.StackTrace);
                throw;
            }
        }

        private async Task CreateTenantMasterWallet(CreateTenantMasterWalletsRequestDto createTenantMasterWalletsRequest, Func<Task<WalletTypeModel>> getWalletTypeFunc, string walletName)
        {
            const string methodName = nameof(CreateTenantMasterWallet);
            var walletType = await getWalletTypeFunc();
            await SaveMasterWallet(createTenantMasterWalletsRequest, walletType.WalletTypeId, walletName, methodName);

        }


        private async Task<WalletTypeModel> GetRewardWalletType()
        {
            var rewardWalletTypeCode = _secretHelper.GetRewardWalletTypeCode();
            return await GetWalletType(rewardWalletTypeCode);
        }

        private async Task<WalletTypeModel> GetSweepstakesEntriesWalletType()
        {
            var sweepstakesEntriesWalletTypeCode = _secretHelper.GetSweepstakesEntriesWalletTypeCode();
            return await GetWalletType(sweepstakesEntriesWalletTypeCode);
        }

        private async Task<WalletTypeModel> GetRedemptionWalletType()
        {
            var redemptionWalletTypeCode = _secretHelper.GetRedemptionWalletTypeCode();
            return await GetWalletType(redemptionWalletTypeCode);
        }
        private async Task<WalletTypeModel> GetSweepstakesEntriesRedemptionWalletType()
        {
            var sweepstakesEntriesRedemptionWalletTypeCode = _secretHelper.GetSweepstakesEntriesRedemptionWalletTypeCode();
            return await GetWalletType(sweepstakesEntriesRedemptionWalletTypeCode);
        }
        private async Task<WalletTypeModel> GetWalletType(string? walletTypeCode)
        {
            return await _walletTypeRepo.FindOneAsync(x => x.WalletTypeCode == walletTypeCode && x.DeleteNbr == 0);
        }
        /// <summary>
        /// Get all the wallet types available in database
        /// </summary>
        /// <returns>List of wallet types</returns>
        public async Task<GetWalletTypeResponseDto> GetAllWalletTypes()
        {
            var walletTypes = await _walletTypeRepo.FindAsync(x => x.DeleteNbr == 0);
            return new GetWalletTypeResponseDto
            {
                WalletTypes = _mapper.Map<List<WalletTypeDto>>(walletTypes),
            };
        }

        /// <summary>
        /// Creates new walletType with the given data
        /// </summary>
        /// <param name="walletTypeDto">request contains data to create wallet type</param>
        /// <returns>baseResponse(200), 400: if walletCode is null or empty, 409: If walletType already exist</returns>
        public async Task<BaseResponseDto> CreateWalletType(WalletTypeDto walletTypeDto)
        {
            const string methodName = nameof(CreateWalletType);
            try
            {
                if (string.IsNullOrEmpty(walletTypeDto.WalletTypeCode))
                {
                    _walletLogger.LogError("{ClassName}.{MethodName} - Invalid request WalletType data is null or Empty", className, methodName);

                    return new BaseResponseDto
                    {
                        ErrorCode = StatusCodes.Status400BadRequest,
                        ErrorMessage = $"Invalid request Wallet Type Data is null or empty",
                    };
                }
                var walletType = await _walletTypeRepo.FindOneAsync(x => x.WalletTypeCode == walletTypeDto.WalletTypeCode && x.DeleteNbr == 0);
                if (walletType != null)
                {
                    _walletLogger.LogError("{ClassName}.{MethodName} - WalletType is already exist with WalletTypeCode:{WalletTypeCode},ErrorCode:{Code}",
                       className, methodName, walletTypeDto.WalletTypeCode, StatusCodes.Status409Conflict);
                    return new BaseResponseDto
                    {
                        ErrorCode = StatusCodes.Status409Conflict,
                        ErrorMessage = $"Wallet Type is already exist with WalletTypeCode:{walletTypeDto.WalletTypeCode}"
                    };
                }
                var walletTypeModel = _mapper.Map<WalletTypeModel>(walletTypeDto);
                walletTypeModel.DeleteNbr = 0;
                walletTypeModel.WalletTypeId = 0;
                walletTypeModel.CreateTs = DateTime.UtcNow;
                walletTypeModel.CreateUser = WalletConstants.CreateUser;

                await _walletTypeRepo.CreateAsync(walletTypeModel);
                _walletLogger.LogInformation("{ClassName}.{MethodName} - WalletType Created successfully with WalletTypeCode:{WalletTypeCode}", className, methodName, walletTypeDto.WalletTypeCode);
                return new BaseResponseDto();

            }
            catch (Exception ex)
            {
                _walletLogger.LogError(ex, "{ClassName}.{MethodName} - Error occurred while creating Wallet Type with WalletTypeCode: {WalletTypeCode},ErrorCode:{ErrorCode},ERROR:{Msg}",
                 className, methodName, walletTypeDto.WalletTypeCode, StatusCodes.Status500InternalServerError, ex.Message);
                throw;
            }
        }

        public async Task<GetAllMasterWalletsResponseDto> GetMasterWallets(string tenantCode)
        {
            const string methodName = nameof(GetMasterWallets);
            try
            {
                var response = new GetAllMasterWalletsResponseDto
                {
                    MasterWallets = new List<TenantWalletDetailDto>()
                };

                _walletLogger.LogInformation("{ClassName}.{MethodName} - Fetching Master wallets with TenantCode:{Tenant}", className, methodName, tenantCode);

                var masterWallets = await _walletRepo.FindAsync(x => x.TenantCode == tenantCode && x.MasterWallet && x.DeleteNbr == 0);
                if (!masterWallets.Any())
                {
                    _walletLogger.LogError("{ClassName}.{MethodName} - No Master wallets Found with TenantCode:{Tenant}", className, methodName, tenantCode);
                    return new GetAllMasterWalletsResponseDto
                    {
                        ErrorCode = StatusCodes.Status404NotFound,
                        ErrorMessage = $"No Master Wallets Found with Request: {tenantCode}"
                    };
                }

                var walletTypeIds = masterWallets.Select(x => x.WalletTypeId).ToList();
                var walletTypes = await _walletTypeRepo.FindAsync(x => walletTypeIds.Contains(x.WalletTypeId));

                response.MasterWallets = masterWallets.Select(wallet =>
                {
                    var walletType = walletTypes?.FirstOrDefault(x => x.WalletTypeId == wallet.WalletTypeId);
                    return new TenantWalletDetailDto
                    {
                        WalletType = _mapper.Map<WalletTypeDto>(walletType),
                        Wallet = _mapper.Map<WalletDto>(wallet)
                    };
                }).ToList();

                return response;
            }
            catch (Exception ex)
            {
                _walletLogger.LogError(ex, "{ClassName}.{MethodName}: An error occurred while retrieving All WalletTypes for TenantCode: {TenantCode}. ErrorCode: {Code}, ERROR: {ErrorMessage}",
                    className, methodName, tenantCode, StatusCodes.Status500InternalServerError, ex.Message);
                throw;
            }
        }

        public async Task<BaseResponseDto> CreateWallet(WalletRequestDto walletRequestDto)
        {
            const string methodName = nameof(CreateWallet);
            try
            {
                _walletLogger.LogInformation("{ClassName}.{MethodName}: Started Wallet creation, For WalletCode:{WalletCode}, TenantCode:{TenantCode}", className, methodName, walletRequestDto.WalletCode, walletRequestDto.TenantCode);
                var walletType = await _walletTypeRepo.FindOneAsync(x => x.WalletTypeId == walletRequestDto.WalletTypeId && x.DeleteNbr == 0);
                if (walletType == null)
                {
                    return new BaseResponseDto { ErrorCode = StatusCodes.Status404NotFound, ErrorMessage = $"No WalletType found with the Given WalletTypeId: {walletRequestDto.WalletTypeId}" };
                }
                var wallet = await _walletRepo.FindOneAsync(x => x.WalletCode == walletRequestDto.WalletCode && x.DeleteNbr == 0);
                if (wallet != null)
                {
                    return new BaseResponseDto { ErrorCode = StatusCodes.Status409Conflict, ErrorMessage = $"Wallet already Exist with the Given WalletCode: {walletRequestDto.WalletCode}" };
                }
                var walletModel = _mapper.Map<WalletModel>(walletRequestDto);
                walletModel.WalletId = 0;
                walletModel.DeleteNbr = 0;
                walletModel.CreateTs = DateTime.UtcNow;
                walletModel.CreateUser = WalletConstants.CreateUser;
                await _walletRepo.CreateAsync(walletModel);
                _walletLogger.LogInformation("{ClassName}.{MethodName}: Wallet Created successfully, For WalletCode:{WalletCode}, TenantCode:{TenantCode}", className, methodName, walletRequestDto.WalletCode, walletRequestDto.TenantCode);
                return new BaseResponseDto();

            }
            catch (Exception ex)
            {
                _walletLogger.LogError(ex, "{ClassName}.{MethodName} - Error occurred while creating Wallet with WalletCode:{WalletCode} TenantCode: {TenantCode},ErrorCode:{ErrorCode},ERROR:{Msg}",
                 className, methodName, walletRequestDto.WalletCode, walletRequestDto.TenantCode, StatusCodes.Status500InternalServerError, ex.Message);
                throw;
            }
        }


        public async Task<MaxWalletTransferRuleResponseDto> GetWalletTypeTransferRule(GetWalletTypeTransferRule getWalletTypeTransferRule)
        {
            try
            {
                // Fetch all wallets for the given consumer and tenant
                var consumerWalletsDetails = await _consumerWalletService.GetAllConsumerWalletsAsync(new GetConsumerWalletRequestDto
                {
                    ConsumerCode = getWalletTypeTransferRule.ConsumerCode,
                    TenantCode = getWalletTypeTransferRule.TenantCode
                });

                var walletTypeCodes = new[]
                {
            _configuration["Health_Actions_Membership_Reward_Wallet_Type_Code"],
            _configuration["Reward_Wallet_Type_Code"],
            _configuration["Sweepstakes_Entries_Wallet_Type_Code"]
        }.Where(code => !string.IsNullOrWhiteSpace(code)).ToHashSet();

                if (consumerWalletsDetails == null || consumerWalletsDetails.ErrorCode != null)
                {
                    return new MaxWalletTransferRuleResponseDto()
                    {
                        ErrorCode = consumerWalletsDetails?.ErrorCode ?? StatusCodes.Status500InternalServerError,
                        ErrorMessage = consumerWalletsDetails?.ErrorMessage ?? $"Consumer wallet not found for ConsumerCode {getWalletTypeTransferRule.ConsumerCode}"
                    };
                }

                var relevantWallets = consumerWalletsDetails.ConsumerWalletDetails
                    .Where(w => walletTypeCodes.Contains(w.WalletType!.WalletTypeCode))
                    .ToList();

                if (relevantWallets.Count == 0)
                {
                    _walletLogger.LogInformation("Consumer wallet not found for ConsumerCode {CosumerCode}", getWalletTypeTransferRule.ConsumerCode);
                    return new MaxWalletTransferRuleResponseDto() { ErrorCode = StatusCodes.Status400BadRequest, ErrorMessage = $"Consumer wallet not found for ConsumerCode {getWalletTypeTransferRule.ConsumerCode}" };
                }

                // Find the wallets that has overflowed
                var overflowedWalletDetail = relevantWallets
                    .Where(w => w.Wallet!.TotalEarned >= w.Wallet.EarnMaximum).ToList();

                //if no overflowed wallet
                if (overflowedWalletDetail.Count == 0)
                {
                    return new MaxWalletTransferRuleResponseDto() { WalletOverFlowed = false }; // No overflowed wallets
                }

                var result = new MaxWalletTransferRuleResponseDto();
                result.WalletOverFlowed = true;
                foreach (var wallet in overflowedWalletDetail)
                {

                    var transferRule = new WalletTypeTransferRule
                    {
                        OverflowedConsumerWallet = wallet.Wallet,
                        OverflowedConsumerWalletType = wallet.WalletType,

                    };

                    var transferRequest = new WalletTransferRequest
                    {
                        ConsumerCode = getWalletTypeTransferRule.ConsumerCode,
                        TenantCode = getWalletTypeTransferRule.TenantCode,
                        SourceWalletTypeId = wallet.WalletType!.WalletTypeId
                    };

                    var transferRuleResult = await GetWalletTransferRules(transferRequest);

                    if (transferRuleResult.ErrorCode == null)
                    {
                        transferRule.TransferRatio = transferRuleResult.TransferRule;
                        transferRule.TargetConsumerWallet = _mapper.Map<WalletDto>(transferRuleResult.ConsumerWallet);
                        transferRule.TargetConsumerWalletType = await GetWalletType(transferRuleResult.ConsumerWallet!.WalletTypeId);
                    }
                    result.walletTypeTransferRules.Add(transferRule);

                }

                return result;

            }
            catch (Exception ex)
            {
                _walletLogger.LogError(ex, "Failed to fetch Wallet Transfer Rule. TenantCode: {TenantCode}, ConsumerCode: {ConsumerCode}",
                    getWalletTypeTransferRule.TenantCode, getWalletTypeTransferRule.ConsumerCode);

                return new MaxWalletTransferRuleResponseDto
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = ex.Message
                };
            }
        }
        /// <summary>
        /// Imports a list of wallet types from the request DTO. 
        /// Skips existing wallet types and logs errors for individual failures.
        /// </summary>
        /// <param name="walletTypeRequestDto">The import request containing wallet types.</param>
        /// <returns>Import result with error messages if any failures occur.</returns>
        public async Task<ImportWalletTypeResponseDto> ImportWalletTypesAsync(ImportWalletTypeRequestDto walletTypeRequestDto)
        {
            const string methodName = nameof(ImportWalletTypesAsync);
            var responseDto = new ImportWalletTypeResponseDto();
            var errorMessages = new StringBuilder();

            _walletLogger.LogInformation("{ClassName}.{MethodName} - Import started with {Count} wallet types.",
                className, methodName, walletTypeRequestDto.WalletTypes?.Count ?? 0);

            foreach (var walletType in walletTypeRequestDto.WalletTypes!)
            {
                try
                {
                    var existingWalletType = await _walletTypeRepo.FindOneAsync(x =>
                        x.WalletTypeCode == walletType.WalletTypeCode && x.DeleteNbr == 0);

                    if (existingWalletType != null)
                    {
                        _walletLogger.LogInformation("{ClassName}.{MethodName} - WalletType '{WalletTypeCode}' already exists. Skipping.",
                            className, methodName, walletType.WalletTypeCode);
                        continue;
                    }

                    await CreateWalletTypeAsync(walletType);
                    _walletLogger.LogInformation("{ClassName}.{MethodName} - WalletType '{WalletTypeCode}' imported successfully.",
                        className, methodName, walletType.WalletTypeCode);
                }
                catch (Exception ex)
                {
                    var errorMsg = $"Failed to import WalletType '{walletType.WalletTypeCode}': {ex.Message}";
                    _walletLogger.LogError(ex, "{ClassName}.{MethodName} - {ErrorMessage}", className, methodName, errorMsg);
                    errorMessages.AppendLine(errorMsg);
                }
            }

            if (errorMessages.Length > 0)
            {
                responseDto.ErrorCode = StatusCodes.Status206PartialContent;
                responseDto.ErrorMessage = errorMessages.ToString();
                _walletLogger.LogWarning("{ClassName}.{MethodName} - Partial import completed with errors.", className, methodName);
            }

            _walletLogger.LogInformation("{ClassName}.{MethodName} - Import completed successfully.", className, methodName);
            return responseDto;
        }

        /// <summary>
        /// Creates a new wallet type entry in the database.
        /// </summary>
        /// <param name="typeDto">The DTO containing wallet type details.</param>
        /// <returns>The created WalletTypeModel instance.</returns>
        private async Task<WalletTypeModel> CreateWalletTypeAsync(WalletTypeDto typeDto)
        {
            var methodName = nameof(CreateWalletTypeAsync);

            var walletTypeModel = _mapper.Map<WalletTypeModel>(typeDto);
            walletTypeModel.CreateTs = DateTime.UtcNow;
            walletTypeModel.CreateUser = WalletConstants.ImportUser;
            walletTypeModel.WalletTypeId = 0;

            await _walletTypeRepo.CreateAsync(walletTypeModel);

            _walletLogger.LogInformation("{ClassName}.{MethodName} - WalletType '{WalletTypeCode}' created.",
                className, methodName, walletTypeModel.WalletTypeCode);
            return walletTypeModel;
        }
    }
}
