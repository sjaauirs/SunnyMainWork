using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Common.Core.Domain;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Services;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Models;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;
using SunnyRewards.Helios.Wallet.Core.Domain.Enum;
using SunnyRewards.Helios.Wallet.Core.Domain.Models;
using SunnyRewards.Helios.Wallet.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Wallet.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Wallet.Infrastructure.Services.Interfaces;

namespace SunnyRewards.Helios.Wallet.Infrastructure.Services
{
    public class TransactionService : BaseService, ITransactionService
    {
        private readonly ILogger<TransactionService> _transactionLogger;
        private readonly IMapper _mapper;
        private readonly IConsumerWalletRepo _consumerWalletRepo;
        private readonly ITransactionRepo _transactionRepo;
        private readonly ITransactionDetailRepo _transactionDetailRepo;
        private readonly IWalletRepo _walletRepo;
        private readonly IWalletTypeRepo _walletTypeRepo;
        private readonly NHibernate.ISession _session;
        private readonly IConfiguration _configuration;
        private readonly IUserClient _userClient;
        private readonly IConsumerService _consumerService;
        const string className = nameof(TransactionService);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="transactionLogger"></param>
        /// <param name="mapper"></param>
        /// <param name="consumerWalletRepo"></param>
        /// <param name="transactionRepo"></param>
        /// <param name="transactionDetailRepo"></param>
        /// <param name="walletRepo"></param>
        /// <param name="walletTypeRepo"></param>
        /// <param name="session"></param>
        /// <param name="configuration"></param>
        /// <param name="userClient"></param>
        public TransactionService(
            ILogger<TransactionService> transactionLogger,
            IMapper mapper,
            IConsumerWalletRepo consumerWalletRepo,
            ITransactionRepo transactionRepo,
            ITransactionDetailRepo transactionDetailRepo,
            IWalletRepo walletRepo,
            IWalletTypeRepo walletTypeRepo,
            NHibernate.ISession session,
            IConfiguration configuration,
            IUserClient userClient,
            IConsumerService consumerService)
        {
            _transactionLogger = transactionLogger;
            _mapper = mapper;
            _consumerWalletRepo = consumerWalletRepo;
            _transactionRepo = transactionRepo;
            _transactionDetailRepo = transactionDetailRepo;
            _walletRepo = walletRepo;
            _walletTypeRepo = walletTypeRepo;
            _session = session;
            _configuration = configuration;
            _userClient = userClient;
            _consumerService = consumerService;
        }

        /// <summary>
        /// Retrieves recent transactions for a consumer, optionally filtered by wallet.
        /// </summary>
        /// <param name="recentTransactionRequestDto">Request data including consumer code, wallet ID, and count.</param>
        /// <param name="consumerDto">Optional consumer info; fetched if not provided.</param>
        /// <returns>Recent transactions with details and wallet type.</returns>
        public async Task<GetRecentTransactionResponseDto> GetTransactionDetails(GetRecentTransactionRequestDto recentTransactionRequestDto, GetConsumerResponseDto? consumerDto = null)
        {
            const string methodName = nameof(GetTransactionDetails);
            try
            {
                var response = new GetRecentTransactionResponseDto();
                var transactionDetails = new List<TransactionEntryDto>();

                // Fetch the consumer details if not already provided
                var consumer = consumerDto != null 
                    ? consumerDto : await _consumerService.GetConsumer(new GetConsumerRequestDto { ConsumerCode = recentTransactionRequestDto.ConsumerCode });
                bool isPrimaryConsumer = true;
                if (!recentTransactionRequestDto.IsIndividualWallet)
                {
                    // Determine if the consumer is the primary consumer (Subscriber)
                     isPrimaryConsumer = consumer.Consumer.MemberNbr == consumer.Consumer.SubscriberMemberNbr;
                }

                // Create a list of wallet IDs to be used in fetching transactions
                List<long> walletIds = recentTransactionRequestDto.WalletId > 0
                                        ? new List<long> { recentTransactionRequestDto.WalletId }
                                        : new List<long>();
                // Only limit the number of transactions if the user is a primary consumer
                int? count = isPrimaryConsumer ? recentTransactionRequestDto.Count : null;

                // Fetch the transaction list based on wallet or all transactions
                IList<TransactionModel> transactions = recentTransactionRequestDto.WalletId > 0
                    ? await _transactionRepo.GetConsumerWalletTopTransactions(walletIds, count, recentTransactionRequestDto.skipTransactionType)
                    : await GetConsumerAllTransactions(recentTransactionRequestDto.ConsumerCode, 
                    recentTransactionRequestDto.IsRewardAppTransactions, count, recentTransactionRequestDto.skipTransactionType);

                // Return early if no transactions found
                if (!transactions.Any())
                    return response;

                // Extract all TransactionDetailIds to fetch their corresponding details
                var transactionDetailIds = transactions.Select(x => x.TransactionDetailId).ToList();

                // Retrieve transaction details using the transactiondetailIDs
                var transactionDetailModels = await _transactionDetailRepo.FindAsync(x => transactionDetailIds.Contains(x.TransactionDetailId));

                // Map transactions and their details to DTOs
                var transactionsMap = _mapper.Map<List<TransactionDto>>(transactions);
                var transactionDetailsMap = _mapper.Map<List<TransactionDetailDto>>(transactionDetailModels);

                // Convert the transaction details list into a dictionary for quick lookup
                var transactionDetailDict = transactionDetailsMap.ToDictionary(x => x.TransactionDetailId);

                // Construct the final list of TransactionEntryDto to return
                foreach (var txn in transactionsMap)
                {
                    // Skip if transaction detail is not found
                    if (!transactionDetailDict.TryGetValue(txn.TransactionDetailId, out var detailDto))
                        continue;

                    // If not primary consumer, make sure detail belongs to them
                    if (!isPrimaryConsumer && detailDto.ConsumerCode != recentTransactionRequestDto.ConsumerCode)
                        continue;

                    // Add the transaction and its detail to the response list
                    transactionDetails.Add(new TransactionEntryDto
                    {
                        Transaction = txn,
                        TransactionDetail = detailDto,
                        TransactionWalletType = await GetWalletTypeByWalletId(txn.WalletId)
                    });
                    
                    // Limit the number of transactions added to match the requested count
                    if (recentTransactionRequestDto.Count > 0 && transactionDetails.Count >= recentTransactionRequestDto.Count)
                        break;
                }

                response.Transactions = transactionDetails;
                _transactionLogger.LogInformation("{className}.{methodName}: Retrieved Transaction Details Successfully for ConsumerCode : {ConsumerCode}",
                    className, methodName, recentTransactionRequestDto.ConsumerCode);

                return response;
            }
            catch (Exception ex)
            {
                _transactionLogger.LogError(ex, "{className}.{methodName}: ERROR - msg : {msg}", className, methodName, ex.Message);
                throw;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="postGetTransactionsRequestDto"></param>
        /// <returns></returns>
        public async Task<PostGetTransactionsResponseDto> GetTransaction(PostGetTransactionsRequestDto postGetTransactionsRequestDto)
        {
            const string methodName = nameof(GetTransaction);
            PostGetTransactionsResponseDto walletResponse = new();
            try
            {
                IList<TransactionModel> transactions = new List<TransactionModel>();
                if (postGetTransactionsRequestDto.WalletId > 0)
                {
                    var consumerWallet = await _consumerWalletRepo.FindOneAsync(x => x.ConsumerCode == postGetTransactionsRequestDto.ConsumerCode
                    && x.WalletId == postGetTransactionsRequestDto.WalletId && x.DeleteNbr == 0);

                    if (consumerWallet == null || consumerWallet.WalletId <= 0)
                    {
                        _transactionLogger.LogError("{className}.{methodName}: ConsumerWallet NotFound For Consumer Code: {consumerCode}, Error Code:{errorCode}", className, methodName, postGetTransactionsRequestDto.ConsumerCode, StatusCodes.Status404NotFound);
                        return new PostGetTransactionsResponseDto()
                        {
                            ErrorCode = StatusCodes.Status404NotFound
                        };
                    }
                    else if (consumerWallet.WalletId != postGetTransactionsRequestDto.WalletId)
                    {
                        _transactionLogger.LogError("{className}.{methodName}: WalletId does not match with consumerCode : {consumerCode},Error Code:{errorCode}", className, methodName, postGetTransactionsRequestDto.ConsumerCode, StatusCodes.Status403Forbidden);
                        return new PostGetTransactionsResponseDto()
                        {
                            ErrorCode = StatusCodes.Status403Forbidden
                        };
                    }
                    transactions = await _transactionRepo.FindAsync(x => x.WalletId == postGetTransactionsRequestDto.WalletId && x.DeleteNbr == 0);
                }

                else
                {
                    transactions = await GetConsumerAllTransactions(postGetTransactionsRequestDto.ConsumerCode, postGetTransactionsRequestDto.IsRewardAppTransactions);
                }

                var transactionDtoList = _mapper.Map<List<TransactionDto>>(transactions);
                var walletTransactions = transactionDtoList.OrderByDescending(x => x.TransactionId).ToList();
                var transactionDetailIdsList = transactionDtoList.Select(x => x.TransactionDetailId).ToList();
                var transactionDetailsList = await _transactionDetailRepo.FindAsync(x => transactionDetailIdsList.Contains(x.TransactionDetailId));
                foreach (var transactionDto in walletTransactions)
                {
                    var transactionDetail = transactionDetailsList.SingleOrDefault(x => x.TransactionDetailId == transactionDto.TransactionDetailId);
                    var transactionDetailMap = _mapper.Map<TransactionDetailDto>(transactionDetail);

                    var transactionEntryDto = new TransactionEntryDto()
                    {
                        TransactionDetail = transactionDetailMap,
                        Transaction = transactionDto,
                        TransactionWalletType = await GetWalletTypeByWalletId(transactionDto.WalletId)
                    };
                    walletResponse.Transactions.Add(transactionEntryDto);

                }
                _transactionLogger.LogInformation("{className}.{methodName}: Retrieved  Data Successfully for ConsumerCode: {consumerCode}", className, methodName, postGetTransactionsRequestDto.ConsumerCode);
                return walletResponse;
            }
            catch (Exception ex)
            {
                _transactionLogger.LogError(ex, "{className}.{methodName}: ERROR - msg : {msg}", className, methodName, ex.Message);
                throw;
            }
        }

        private async Task<IList<TransactionModel>> GetConsumerAllTransactions(string? consumerCode, 
            bool isRewardAppTransactions = false, int? count = null, List<string>? skipTransactionType = null)
        {
            const string methodName = nameof(GetConsumerAllTransactions);
            var consumerWallets = await _consumerWalletRepo.FindAsync(x => x.ConsumerCode == consumerCode && x.DeleteNbr == 0);
            var consumerWalletIds = consumerWallets.Select(x => x.WalletId).ToList();

            if (isRewardAppTransactions)
            {
                var wallets = await _walletRepo.FindAsync(x => consumerWalletIds.Contains(x.WalletId) && x.ActiveStartTs <= DateTime.UtcNow && x.ActiveEndTs >= DateTime.UtcNow);
                var rewardWalletType = await GetRewardWalletType();
                var entriesWalletType = await GetSweepstkesEntriesWalletType();
                var membershipWalletType = await GetMembershipRewardWalletType();
                consumerWalletIds = wallets.Where(x => x.WalletTypeId == rewardWalletType.WalletTypeId || x.WalletTypeId == entriesWalletType.WalletTypeId || x.WalletTypeId == membershipWalletType.WalletTypeId).Select(x => x.WalletId).ToList();
            }

            var allConsumerTransactions = await _transactionRepo.GetConsumerWalletTopTransactions(consumerWalletIds, count, skipTransactionType);
            _transactionLogger.LogInformation("{className}.{methodName}: Retrieved all Consumer transactions For Consumer Code:{consumerCode}", className, methodName, consumerCode);
            return allConsumerTransactions;
        }

        /// <summary>
        /// Revert all transactions for given consumer
        /// </summary>
        /// <param name="revertTransactionsRequestDto"></param>
        /// <returns></returns>
        public async Task<BaseResponseDto> RevertAllTransaction(RevertTransactionsRequestDto revertTransactionsRequestDto)
        {
            const string methodName = nameof(RevertAllTransaction);
            var tenantCode = revertTransactionsRequestDto.TenantCode;
            var consumerCode = revertTransactionsRequestDto.ConsumerCode;
            if (string.IsNullOrEmpty(tenantCode) || string.IsNullOrEmpty(consumerCode))
            {
                _transactionLogger.LogError("{className}.{methodName}: Tenant Code or consumer Code IsNullOrEmpty. ERROR Code: {msg}", className, methodName, StatusCodes.Status400BadRequest);
                return new BaseResponseDto() { ErrorCode = StatusCodes.Status400BadRequest, ErrorMessage = "Invalid input" };
            }

            using var transaction = _session.BeginTransaction();
            try
            {
                var rewardWalletTypeData = await GetRewardWalletType();
                var consumerRewardsWallet = await _walletRepo.GetWalletByConsumerAndWalletType(tenantCode, consumerCode, rewardWalletTypeData.WalletTypeId);
                if (consumerRewardsWallet == null || consumerRewardsWallet.WalletId <= 0)
                {
                    _transactionLogger.LogError("{className}.{methodName}: Consumer primary wallet NotFound: {consumerCode}, ERROR code:{errorCode}", className, methodName, consumerCode, StatusCodes.Status404NotFound);
                    return new PostGetTransactionsResponseDto()
                    {
                        ErrorCode = StatusCodes.Status404NotFound
                    };
                }

                WalletTypeModel sweepstakesEntriesWalletTypeData = await GetSweepstkesEntriesWalletType();
                WalletModel consumerEntriesWallet = await _walletRepo.GetWalletByConsumerAndWalletType(tenantCode, consumerCode, sweepstakesEntriesWalletTypeData.WalletTypeId);
                var consumerEntriesWalletId = consumerEntriesWallet != null ? consumerEntriesWallet.WalletId : 0;

                double totalReward = await _transactionRepo.GetTotalAmountForConsumerByTransactionDetailType(consumerCode, consumerRewardsWallet.WalletId, nameof(TransactionDetailType.REWARD));
                double totalRedeem = await _transactionRepo.GetTotalAmountForConsumerByTransactionDetailType(consumerCode, consumerRewardsWallet.WalletId, nameof(TransactionDetailType.REDEMPTION));
                double totalReturn = await _transactionRepo.GetTotalAmountForConsumerByTransactionDetailType(consumerCode, consumerRewardsWallet.WalletId, nameof(TransactionDetailType.RETURN));
                totalRedeem -= totalReturn;
                double totalSweepstakesEntries = await _transactionRepo.GetTotalAmountForConsumerByTransactionDetailType(consumerCode, consumerEntriesWalletId, nameof(TransactionDetailType.REWARD));

                // Update master reward wallet balance
                await UpdateMasterWalletBalance(tenantCode, rewardWalletTypeData.WalletTypeId, totalReward);

                // Update master redemption wallet balance
                var redemptionWalletType = await GetRedemptionWalletType();
                await UpdateMasterRedmptionWalletBalance(tenantCode, consumerCode, redemptionWalletType.WalletTypeId);

                // Update master sweepstakes entries wallet(secondary wallet) balance
                await UpdateMasterWalletBalance(tenantCode, sweepstakesEntriesWalletTypeData.WalletTypeId, totalSweepstakesEntries);

                // Update consumer reward wallet balance
                await UpdateConsumerWalletBalance(consumerRewardsWallet, totalReward, totalRedeem);

                // Update consumer sweepstakes entries wallet(secondary wallet) balance
                await UpdateConsumerWalletBalance(consumerEntriesWallet, totalSweepstakesEntries, 0);

                // Soft delete consumer wallet transactions
                await SoftDeleteConsumerWalletTransactions(consumerCode);

                // Soft delete consumer wallet transaction details
                await SoftDeleteConsumerWalletTransactionDetails(consumerCode);

                await transaction.CommitAsync();
                _transactionLogger.LogInformation("{className}.{methodName}: All wallet transactions for consumer: {ConsumerCode} successfully reverted.", className, methodName, consumerCode);

                return new BaseResponseDto();
            }
            catch (Exception ex)
            {
                _transactionLogger.LogError(ex, "{className}.{methodName}: ERROR - Message : {Message}", className, methodName, ex.Message);
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="consumerCode"></param>
        /// <returns></returns>
        private async Task SoftDeleteConsumerWalletTransactions(string consumerCode)
        {
            var listOfWalletTransactions = await _transactionRepo.GetConsumerWalletTransactions(consumerCode);
            foreach (var walletTransaction in listOfWalletTransactions)
            {
                walletTransaction.DeleteNbr = walletTransaction.TransactionId;
                walletTransaction.UpdateTs = DateTime.UtcNow;
                await _session.UpdateAsync(walletTransaction);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="consumerCode"></param>
        /// <returns></returns>
        private async Task SoftDeleteConsumerWalletTransactionDetails(string consumerCode)
        {
            var listOfWalletTransactionDetails = await _transactionDetailRepo.FindAsync(x => x.ConsumerCode == consumerCode && x.DeleteNbr == 0);
            foreach (var transactionDetail in listOfWalletTransactionDetails)
            {
                transactionDetail.DeleteNbr = transactionDetail.TransactionDetailId;
                transactionDetail.UpdateTs = DateTime.UtcNow;
                await _session.UpdateAsync(transactionDetail);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tenantCode"></param>
        /// <param name="walletTypeId"></param>
        /// <param name="totalReward"></param>
        /// <returns></returns>
        private async Task UpdateMasterWalletBalance(string tenantCode, long walletTypeId, double totalReward)
        {
            const string methodName = nameof(UpdateMasterWalletBalance);
            var masterWallet = await _walletRepo.FindOneAsync(x => x.TenantCode == tenantCode && x.MasterWallet && x.DeleteNbr == 0 && x.WalletTypeId == walletTypeId);
            if (masterWallet != null)
            {
                _transactionLogger.LogInformation("{className}.{methodName}: Master wallet Balance Updated For Tenant Code:{tenantCode}", className, methodName, tenantCode);
                masterWallet.Balance += totalReward;
                masterWallet.UpdateTs = DateTime.UtcNow;
                await _session.UpdateAsync(masterWallet);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tenantCode"></param>
        /// <param name="consumerCode"></param>
        /// <param name="walletTypeId"></param>
        /// <returns></returns>
        private async Task UpdateMasterRedmptionWalletBalance(string tenantCode, string consumerCode, long walletTypeId)
        {
            var masterWallets = await _walletRepo.FindAsync(x => x.TenantCode == tenantCode && x.MasterWallet && x.DeleteNbr == 0 && x.WalletTypeId == walletTypeId);
            if (masterWallets != null)
            {
                foreach (var masterWallet in masterWallets)
                {
                    double totalConsumerRedeemBalance = await _transactionRepo.GetTotalAmountForConsumerByTransactionDetailType(consumerCode, masterWallet.WalletId, nameof(TransactionDetailType.REDEMPTION));
                    double totalConsumerRedeemReturnBalance = await _transactionRepo.GetTotalAmountForConsumerByTransactionDetailType(consumerCode, masterWallet.WalletId, nameof(TransactionDetailType.RETURN));
                    double consumerRedeemBalance = totalConsumerRedeemBalance - totalConsumerRedeemReturnBalance;
                    if (consumerRedeemBalance > 0)
                    {
                        masterWallet.Balance -= consumerRedeemBalance;
                        masterWallet.UpdateTs = DateTime.UtcNow;
                        await _session.UpdateAsync(masterWallet);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="wallet"></param>
        /// <param name="totalReward"></param>
        /// <param name="totalRedeem"></param>
        /// <returns></returns>
        private async Task UpdateConsumerWalletBalance(WalletModel? wallet, double totalReward, double totalRedeem)
        {
            if (wallet != null)
            {
                wallet.Balance = wallet.Balance + totalRedeem - totalReward;
                wallet.TotalEarned -= totalReward;
                wallet.UpdateTs = DateTime.UtcNow;
                await _session.UpdateAsync(wallet);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private async Task<WalletTypeModel> GetRewardWalletType()
        {
            string? walletTypeCode = _configuration.GetSection("Reward_Wallet_Type_Code").Value;
            return await _walletTypeRepo.FindOneAsync(x => x.WalletTypeCode == walletTypeCode && x.DeleteNbr == 0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private async Task<WalletTypeModel> GetRedemptionWalletType()
        {
            string? walletTypeCode = _configuration.GetSection("Redemption_Wallet_Type_Code").Value;
            return await _walletTypeRepo.FindOneAsync(x => x.WalletTypeCode == walletTypeCode && x.DeleteNbr == 0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private async Task<WalletTypeModel> GetSweepstkesEntriesWalletType()
        {
            string? walletTypeCode = _configuration.GetSection("Sweepstakes_Entries_Wallet_Type_Code").Value;
            return await _walletTypeRepo.FindOneAsync(x => x.WalletTypeCode == walletTypeCode && x.DeleteNbr == 0);
        }

        /// <summary>
        /// Gets the type of the membership reward wallet.
        /// </summary>
        /// <returns></returns>
        private async Task<WalletTypeModel> GetMembershipRewardWalletType()
        {
            string? walletTypeCode = _configuration.GetSection("Health_Actions_Membership_Reward_Wallet_Type_Code").Value;
            return await _walletTypeRepo.FindOneAsync(x => x.WalletTypeCode == walletTypeCode && x.DeleteNbr == 0);
        }

        private async Task<WalletTypeDto> GetWalletTypeByWalletId(long walletId)
        {
            const string methodName = nameof(GetWalletTypeByWalletId);
            try
            {
                var wallet = await _walletRepo.FindOneAsync(x => x.WalletId == walletId);
                if (wallet == null)
                {
                    return new WalletTypeDto();
                }
                var walletType = await _walletTypeRepo.FindOneAsync(x => x.WalletTypeId == wallet.WalletTypeId);
                if (walletType == null)
                {
                    return new WalletTypeDto();
                }
                var response = _mapper.Map<WalletTypeDto>(walletType);
                _transactionLogger.LogInformation("{className}.{methodName}: Retrieved Wallet Type Details Successfully for wallletId : {wallletId}", className, methodName, walletId);

                return response;
            }
            catch (Exception ex)
            {

                _transactionLogger.LogError(ex, "{className}.{methodName}: - Error :{msg}", className, methodName, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Retrieves a specified number of recent wallet transactions and associated wallets for a given consumer.
        /// </summary>
        /// <param name="walletTransactionRequestDto">Request DTO containing the consumer code and the number of transactions to retrieve.</param>
        /// <returns>
        /// A response DTO containing the list of wallet transactions and the associated wallets for the specified consumer.
        /// </returns>
        /// <exception cref="Exception">Throws any unexpected exception that occurs during execution.</exception>
        public async Task<GetWalletTransactionResponseDto> GetRewardWalletTransactions(GetWalletTransactionRequestDto walletTransactionRequestDto)
        {
            const string methodName = nameof(GetRewardWalletTransactions);
            try
            {
                _transactionLogger.LogInformation("{ClassName}.{MethodName}: Started processing for ConsumerCode: {ConsumerCode}", 
                    className, methodName, walletTransactionRequestDto.ConsumerCode);

                var walletTypeCodes = GetConfiguredWalletTypeCodes();

                var wallets = await _consumerWalletRepo.GetConsumerWallets(walletTypeCodes, walletTransactionRequestDto.ConsumerCode);
                var walletIds = wallets.Select(x => x.WalletId).ToList();

                var transactionsQuery = _transactionRepo.GetWalletTransactionsQueryable(walletIds);
                var recentTransactions = walletTransactionRequestDto.Count==0 ? transactionsQuery.ToList() 
                    : transactionsQuery.Take(walletTransactionRequestDto.Count).ToList();

                _transactionLogger.LogInformation("{ClassName}.{MethodName}: Successfully processed for ConsumerCode: {ConsumerCode}",
                    className, methodName, walletTransactionRequestDto.ConsumerCode);
                return new GetWalletTransactionResponseDto
                {
                    Transactions = recentTransactions,
                    Wallets = _mapper.Map<List<WalletDto>>(wallets)
                };
            }
            catch (Exception ex)
            {
                _transactionLogger.LogError(ex, "{ClassName}.{MethodName}:Failed to get wallet transactions for ConsumerCode: {ConsumerCode}", 
                    className,methodName,walletTransactionRequestDto.ConsumerCode);
                throw;
            }
        }

        private List<string> GetConfiguredWalletTypeCodes()
        {
            return new List<string>
            {
                _configuration["Reward_Wallet_Type_Code"] ?? string.Empty,
                _configuration["Sweepstakes_Entries_Wallet_Type_Code"] ?? string.Empty,
                _configuration["Health_Actions_Membership_Reward_Wallet_Type_Code"] ?? string.Empty
            };
        }


        public async Task<CreateTransactionsResponseDto> CreateWalletTransactions(CreateTransactionsRequestDto createTransactions)
        {
            const string methodName = nameof(CreateWalletTransactions);
            try
            {
                _transactionLogger.LogInformation(
                    "{ClassName}.{MethodName}: Started creating transactions for ConsumerCode: {ConsumerCode}",
                    className, methodName, createTransactions.ConsumerCode);

                using (var transaction = _session.BeginTransaction())
                {
                    var transactionTs = DateTime.UtcNow;

                    try
                    {
                        // Source wallet and last transaction
                        var sourceWallet = await _walletRepo.FindOneAsync(x => x.WalletId == createTransactions.RemovedWalletId);
                        var maxSourceTxnId = await _transactionRepo.GetMaxTransactionIdByWallet(createTransactions.RemovedWalletId);
                        var lastSourceTxn = await _transactionRepo.FindOneAsync(t => t.TransactionId == maxSourceTxnId);

                        // Destination wallet and last transaction
                        var destWallet = await _walletRepo.FindOneAsync(x => x.WalletId == createTransactions.AddedWalletId);
                        var maxDestTxnId = await _transactionRepo.GetMaxTransactionIdByWallet(createTransactions.AddedWalletId);
                        var lastDestTxn = await _transactionRepo.FindOneAsync(t => t.TransactionId == maxDestTxnId);

                        // Transaction detail
                        var txnDetail = new TransactionDetailModel
                        {
                            TransactionDetailType = createTransactions.TransactionDetail!.TransactionDetailType,
                            ConsumerCode = createTransactions.TransactionDetail.ConsumerCode,
                            RewardDescription = createTransactions.TransactionDetail.RewardDescription,
                            Notes = createTransactions.TransactionDetail.Notes,
                            RedemptionRef = createTransactions.TransactionDetail.RedemptionRef,
                            RedemptionItemDescription = createTransactions.TransactionDetail.RedemptionItemDescription,
                            CreateTs = transactionTs,
                            CreateUser = Constants.CreateUser,
                            UpdateTs = DateTime.UtcNow,
                            UpdateUser = Constants.CreateUser,
                            DeleteNbr = 0
                        };

                        var txnDetailId = Convert.ToInt64(await _session.SaveAsync(txnDetail));
                        txnDetail.TransactionDetailId = txnDetailId;

                        string newTxnCode = "txn-" + Guid.NewGuid().ToString("N");
                        var fundAmount = Math.Max(createTransactions.TransactionAmount, 0);

                        //  Subtract transaction (source wallet)
                        var subtractTxn = new TransactionModel
                        {
                            WalletId = createTransactions.RemovedWalletId,
                            TransactionCode = newTxnCode,
                            TransactionType = Constants.Subtract,
                            PreviousBalance = sourceWallet?.Balance,
                            TransactionAmount = (double?)fundAmount,
                            Balance = (sourceWallet?.Balance ?? 0) - Convert.ToDouble(fundAmount),
                            PrevWalletTxnCode = $"{sourceWallet?.WalletId}:{(lastSourceTxn?.TransactionCode ?? "init")}",
                            CreateTs = transactionTs,
                            CreateUser = Constants.CreateUser,
                            UpdateTs = DateTime.UtcNow,
                            UpdateUser = Constants.CreateUser,
                            DeleteNbr = 0,
                            TransactionDetailId = txnDetail.TransactionDetailId
                        };
                        await _session.SaveAsync(subtractTxn);

                        // Addition transaction (destination wallet)
                        var addTxn = new TransactionModel
                        {
                            WalletId = createTransactions.AddedWalletId,
                            TransactionCode = newTxnCode,
                            TransactionType = Constants.Addition,
                            PreviousBalance = destWallet?.Balance,
                            TransactionAmount = Convert.ToDouble(fundAmount),
                            Balance = (destWallet?.Balance ?? 0) + Convert.ToDouble(fundAmount),
                            PrevWalletTxnCode = $"{destWallet?.WalletId}:{(lastDestTxn?.TransactionCode ?? "init")}",
                            CreateTs = transactionTs,
                            CreateUser = Constants.CreateUser,
                            UpdateTs = DateTime.UtcNow,
                            UpdateUser = Constants.CreateUser,
                            DeleteNbr = 0,
                            TransactionDetailId = txnDetail.TransactionDetailId
                        };
                        await _session.SaveAsync(addTxn);

                        // Update wallet balances
                        sourceWallet!.Balance = sourceWallet!.Balance - Convert.ToDouble(fundAmount);
                        sourceWallet.UpdateTs = DateTime.UtcNow;

                        destWallet!.Balance = destWallet!.Balance + Convert.ToDouble(fundAmount);
                        destWallet.UpdateTs = DateTime.UtcNow;
                        
                        await _session.UpdateAsync(sourceWallet);
                        await _session.UpdateAsync(destWallet);

                        //Commit if both succeed
                        await transaction.CommitAsync();

                        _transactionLogger.LogInformation(
                            "{ClassName}.{MethodName}: Completed wallet transactions for ConsumerCode: {ConsumerCode}",
                            className, methodName, createTransactions.ConsumerCode);

                        return new CreateTransactionsResponseDto() { TransactionDetailId = txnDetailId };
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();

                        _transactionLogger.LogError(
                            ex,
                            "{ClassName}.{MethodName}: Failed to create transactions for ConsumerCode: {ConsumerCode}",
                            className, methodName, createTransactions.ConsumerCode);

                        return new CreateTransactionsResponseDto { ErrorCode = StatusCodes.Status500InternalServerError, ErrorMessage = "Transaction failed: " + ex.Message };
                    }
                }
            }
            catch (Exception ex)
            {
                _transactionLogger.LogError(
                    ex, "{ClassName}.{MethodName}: Unexpected error creating transactions for ConsumerCode: {ConsumerCode}",
                    className, methodName, createTransactions.ConsumerCode);

                return new CreateTransactionsResponseDto { ErrorCode = StatusCodes.Status500InternalServerError ,  ErrorMessage = "Unexpected failure: " + ex.Message };
            }
        }

        async Task<BaseResponseDto> ITransactionService.RemoveWalletTransactions(RemoveTransactionsRequestDto removeTransactionsRequestDto)
        {
            const string methodName = nameof(CreateWalletTransactions);
            try
            {
                _transactionLogger.LogInformation(
                    "{ClassName}.{MethodName}: Started removing transactions for TraanctionDetailId: {code}",
                    className, methodName, removeTransactionsRequestDto.TransactionDetailId);

                using (var transaction = _session.BeginTransaction())
                {
                    var transactionTs = DateTime.UtcNow;

                    try
                    {

                        // Transaction detail
                        var txnDetail = await _transactionDetailRepo.FindOneAsync(t=> t.TransactionDetailId == removeTransactionsRequestDto.TransactionDetailId);
                        txnDetail.DeleteNbr = txnDetail.TransactionDetailId;
                        txnDetail.UpdateTs = transactionTs;
                        txnDetail.UpdateUser = Constants.CreateUser;
                        await _session.SaveAsync(txnDetail);

                        
                        //Commit if both succeed
                        await transaction.CommitAsync();

                        _transactionLogger.LogInformation(
                            "{ClassName}.{MethodName}: Completed wallet transactions for TransactionDetail: {code}",
                            className, methodName, removeTransactionsRequestDto.TransactionDetailId);

                        return new BaseResponseDto();
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();

                        _transactionLogger.LogError(
                            ex,
                            "{ClassName}.{MethodName}: Failed to remove transactions for TransactionDetail: {code}",
                            className, methodName, removeTransactionsRequestDto.TransactionDetailId);

                        return new CreateTransactionsResponseDto { ErrorCode = StatusCodes.Status500InternalServerError, ErrorMessage = "Transaction failed: " + ex.Message };
                    }
                }
            }
            catch (Exception ex)
            {
                _transactionLogger.LogError(
                    ex, "{ClassName}.{MethodName}: Unexpected error creating transactions for TransactionDetail: {code}",
                    className, methodName, removeTransactionsRequestDto.TransactionDetailId);

                return new CreateTransactionsResponseDto { ErrorCode = StatusCodes.Status500InternalServerError, ErrorMessage = "Unexpected failure: " + ex.Message };
            }
        }
    }
}