using Grpc.Net.Client.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SunnyRewards.Helios.Admin.Core.Domain.Constants;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Core.Domain.Models;
using SunnyRewards.Helios.Admin.Infrastructure.Helpers.Interface;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Tenant.Core.Domain.Models;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services
{
    public class WalletTypeService : IWalletTypeService
    {
        private readonly IWalletClient _walletClient;
        private const string className = nameof(WalletTypeService);
        private readonly IConfiguration _config;
        private readonly IWalletHelper _walletHelper;
        private ILogger<WalletTypeService> _logger;
        private IEventService _eventService;

        public WalletTypeService(ILogger<WalletTypeService> logger, IWalletClient walletClient,
            IConfiguration config, IWalletHelper walletHelper, IEventService eventService)
        {
            _walletClient = walletClient;
            _logger = logger;
            _config = config;
            _walletHelper = walletHelper;
            _eventService = eventService;
        }

        public async Task<GetWalletTypeResponseDto> GetAllWalletTypes()
        {
            var parameters = new Dictionary<string, long>();
            return await _walletClient.Get<GetWalletTypeResponseDto>(Constant.WalletTypes, parameters);
        }

        public async Task<BaseResponseDto> CreateWalletType(WalletTypeDto walletTypeDto)
        {
            return await _walletClient.Post<BaseResponseDto>(Constant.WalletType, walletTypeDto);
        }
        /// <summary>
        /// Creates WalletType with the given data
        /// </summary>
        /// <param name="walletTypeDto">WalletType request to create walletType</param>
        /// <returns>BaseResponse with status codes</returns>
        public async Task<WalletTypeDto> GetWalletTypeCode(WalletTypeDto walletTypeDto)
        {
            return await _walletClient.Post<WalletTypeDto>(Constant.WalletTypeCode, walletTypeDto);
        }
        /// <summary>
        /// Imports a list of wallet types asynchronously by sending them to the wallet client.
        /// </summary>
        /// <param name="walletTypes">The list of wallet types to be imported.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains an 
        /// <see cref="ImportWalletTypeResponseDto"/> indicating the result of the import operation,
        /// including error details if the operation fails.
        /// </returns>
        public async Task<ImportWalletTypeResponseDto> ImportWalletTypesAsync(List<WalletTypeDto> walletTypes)
        {
            const string methodName = nameof(ImportWalletTypesAsync);
            try
            {
                return await _walletClient.Post<ImportWalletTypeResponseDto>(Constant.ImportWalletTypes, new ImportWalletTypeRequestDto { WalletTypes = walletTypes });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: Error Message: {Message}, Error Code: {ErrorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);

                return new ImportWalletTypeResponseDto
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <summary>
        /// Get task reward wallet split configuration
        /// </summary>
        /// <param name="taskRewardDto"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<TaskRewardWalletSplitConfigDto> GetTaskRewardMonetaryDollarWalletSplit(TaskRewardDto taskReward,
            string consumerCode, string tenantCode, bool isLiveTransferToRewardsPurseEnabled)
        {
            const string methodName = nameof(GetTaskRewardMonetaryDollarWalletSplit);
            var response = new TaskRewardWalletSplitConfigDto();
            var defaultWalletSplitConfig = new List<WalletSplitConfig>
                {
                    new WalletSplitConfig
                    {
                        WalletTypeCode = _config.GetSection("Health_Actions_Reward_Wallet_Type_Code").Value,
                        MasterWalletTypeCode = _config.GetSection("Health_Actions_Reward_Wallet_Type_Code").Value,
                        RedemptionWalletTypeCode = _config.GetSection("Healthy_Living_Redemption_Wallet_Type_Code").Value,
                        PurseWalletTypeCode = _config.GetSection("Healthy_Living_Wallet_Type_Code").Value,
                        Percentage = 100
                    }
                };
            try
            {
                var rewardConfigJson = taskReward?.TaskRewardConfigJson ?? string.Empty;

                // Parse wallet split config
                var walletSplitConfig = new List<WalletSplitConfig>();
                if (!string.IsNullOrEmpty(rewardConfigJson))
                {
                    var configObj = JsonConvert.DeserializeObject<TaskRewardWalletSplitConfigDto>(rewardConfigJson);
                    if (configObj?.WalletSplitConfig != null)
                        walletSplitConfig = configObj.WalletSplitConfig;
                }
                walletSplitConfig = walletSplitConfig?.Where(x => !string.IsNullOrEmpty(x.WalletTypeCode)
                        && !string.IsNullOrEmpty(x.RedemptionWalletTypeCode)
                        && !string.IsNullOrEmpty(x.PurseWalletTypeCode)
                        && x.Percentage > 0)?.ToList();

                // Default: single wallet, backward compatible
                if (walletSplitConfig == null || walletSplitConfig.Count == 0)
                {
                    _logger.LogWarning("{className}.{methodName}: Using default wallet split config as none provided.", className, methodName);
                    response.WalletSplitConfig = defaultWalletSplitConfig;
                    return response;
                }

                // Validate total percentage
                var totalPercentage = walletSplitConfig.Sum(x => x.Percentage);
                if (totalPercentage != 100)
                {
                    _logger.LogWarning("{className}.{methodName}: Using default wallet split config as wallet split config percentage does not sum to 100. Actual: {totalPercentage}",
                        className, methodName, totalPercentage);
                    response.WalletSplitConfig = defaultWalletSplitConfig;
                    return response;
                }

                var consumerWallets = await _walletClient.Post<WalletResponseDto>(Constant.ConsumerWallets,
                    new FindConsumerWalletRequestDto { ConsumerCode = consumerCode });

                var parameters = new Dictionary<string, long>();
                var masterWallets = await _walletClient.Get<GetAllMasterWalletsResponseDto>($"{Constant.MasterWallet}/{tenantCode}", parameters);
                foreach (var split in walletSplitConfig)
                {
                    split.MasterWalletTypeCode = split.WalletTypeCode;
                    var consumerWallet =
                       consumerWallets.walletDetailDto
                           .FirstOrDefault(x =>
                               x.WalletType.WalletTypeCode == split.WalletTypeCode);

                    var purseWallet = consumerWallets.walletDetailDto?
                       .FirstOrDefault(x => x.WalletType.WalletTypeCode == split.PurseWalletTypeCode);

                    var redemptionWallet = masterWallets.MasterWallets?
                        .FirstOrDefault(x => x.WalletType.WalletTypeCode == split.RedemptionWalletTypeCode);

                    if (consumerWallet == null || purseWallet == null || redemptionWallet == null)
                    {
                        response.WalletSplitConfig = defaultWalletSplitConfig;
                        return response;
                    }
                    split.WalletName = consumerWallet?.Wallet?.WalletName;
                    split.RedemptionVendorCode = redemptionWallet?.Wallet?.WalletName?.Replace(Constant.TenantMasterRedemptionWalletPrefix, string.Empty);
                }
                response.WalletSplitConfig = walletSplitConfig;

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{className}.{methodName}: Using default wallet split config as Exception occurred while reading task reward wallet split config. Error Message: {Message}",
                    className, methodName, ex.Message);
                response.WalletSplitConfig = defaultWalletSplitConfig;
                return response;
            }
        }

        public async Task<List<WalletSplitConfig>> CreateMissingWalletsAsync(string consumerCode, string tenantCode, bool isLiveTransferToRewardsPurseEnabled, 
               List<WalletSplitConfig> walletSplitConfig)
        {
            string methodName = nameof(CreateMissingWalletsAsync);
            try
            {
                foreach (var split in walletSplitConfig)
                {
                    split.MasterWalletTypeCode = split.WalletTypeCode;
                    var now = DateTime.Now;

                    await _walletHelper.CreateWalletsForConsumer(consumerCode);

                    //find again after wallet creation 

                    var consumerWallets = await _walletClient.Post<WalletResponseDto>(Constant.ConsumerWallets,
                    new FindConsumerWalletRequestDto { ConsumerCode = consumerCode });

                    var consumerWallet =
                        consumerWallets.walletDetailDto
                            .FirstOrDefault(x =>
                                x.WalletType.WalletTypeCode == split.WalletTypeCode &&
                                x.Wallet.ActiveStartTs <= now &&
                                x.Wallet.ActiveEndTs >= now);


                    var purseWallet = consumerWallets.walletDetailDto?
                       .FirstOrDefault(x => x.WalletType.WalletTypeCode == split.PurseWalletTypeCode &&
                       x.Wallet.ActiveStartTs <= now &&
                               x.Wallet.ActiveEndTs >= now);

                    var parameters = new Dictionary<string, long>();
                    var masterWallets = await _walletClient.Get<GetAllMasterWalletsResponseDto>($"{Constant.MasterWallet}/{tenantCode}", parameters);
                    var redemptionWallet = masterWallets.MasterWallets?
                            .FirstOrDefault(x => x.WalletType.WalletTypeCode == split.RedemptionWalletTypeCode);


                    if (consumerWallet == null || redemptionWallet == null)
                    {
                        _logger.LogError("{class}.{method}: Wallet lookup failed even after creation.", className, methodName);

                        // Funded in default wallet
                        _logger.LogError("{class}.{method}: Consumer wallet or Redemption wallet not found for consumer {consumer}." +
                            " Using default wallet [REWARD] .",
                            className, methodName, consumerCode);
                        // raise a Event in Dead letter Queue
                        var consumerErrorEventDto = new ConsumerErrorEventDto()
                        {
                            Header = new PostedEventData()
                            {
                                EventType = "CONSUMER_ERROR",
                                EventSubtype = "WALLET_NOT_FOUND"
                            },
                            Message = new ConsumerErrorEventBodyDto()
                            {
                                ReqDetail = split.ToJson(),
                                Detail = $"Consumer wallet or Redemption wallet not found for consumer {consumerCode} even after creation.",
                            }
                        };
                        await _eventService.PostErrorEvent(consumerErrorEventDto);
                        return walletSplitConfig;  // use default
                    }

                    //we have  cosnumer and redumption wallet, then check  if Live transfer is enabled and we have purse wallet
                    if (isLiveTransferToRewardsPurseEnabled && purseWallet == null)
                    {
                        //we dont have valid Purse, so log and  stop Live transfer
                        _logger.LogError("{class}.{method}: Live transfer is enabled but Purse wallet not found for consumer {consumer}. Stopping live transfer.", className, methodName, consumerCode);
                        split.PurseWalletTypeCode = null;
                        // raise a Event in Dead letter Queue
                    }
                }

                return walletSplitConfig;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{className}.{methodName}: Using default wallet split config as Exception occurred while reading task reward wallet split config. Error Message: {Message}",
                   className, methodName, ex.Message);
                return walletSplitConfig;
            }
        }
    }
}
