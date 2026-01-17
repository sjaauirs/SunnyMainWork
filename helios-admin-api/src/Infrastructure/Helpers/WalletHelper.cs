using FirebaseAdmin.Auth.Multitenancy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SunnyBenefits.Fis.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Core.Domain.Constants;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Infrastructure.Helpers.Interface;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos.Json;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos.Json;

namespace SunnyRewards.Helios.Admin.Infrastructure.Helpers
{
    public class WalletHelper : IWalletHelper
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<WalletHelper> _logger;
        private readonly IConsumerWalletService _consumerWalletService;
        private readonly IConsumerService _consumerService;
        private readonly ITenantService _tenantService;
        private readonly ITenantAccountService _tenantAccountService;
        private readonly IWalletClient _walletClient;
        const string className = nameof(WalletHelper);

        public WalletHelper(IConfiguration config, ILogger<WalletHelper> logger, IConsumerWalletService consumerWalletService,
           IConsumerService consumerService, ITenantService tenantService, ITenantAccountService tenantAccountService, IWalletClient walletClient)
        {
            _configuration = config;
            _logger = logger;
            _consumerWalletService = consumerWalletService;
            _consumerService = consumerService;
            _tenantService = tenantService;
            _tenantAccountService = tenantAccountService;
            _walletClient = walletClient;
        }


        public async Task<BaseResponseDto> CreateWalletsForConsumer(string consumerCode)
        {
            var methodName = nameof(CreateWalletsForConsumer);

            try
            {
                var consumerResponse = await _consumerService.GetConsumerData(new GetConsumerRequestDto() { ConsumerCode = consumerCode });
            var tenantCode = consumerResponse?.Consumer?.TenantCode!;

            var tenant = await _tenantService.GetTenantDetails(tenantCode);
            if (tenant == null || tenant.ErrorCode != null || tenant.Tenant == null)
            {
                _logger.LogError("{ClassName}.{MethodName}  - Invalid tenant code supplied.", className, methodName);
                return new BaseResponseDto
                {
                    ErrorCode = StatusCodes.Status400BadRequest,
                    ErrorMessage = $"Invalid tenant code supplied: {tenantCode}."
                };
            }

            var tenantAccount = await _tenantAccountService.GetTenantAccount(tenantCode);
            if (tenantAccount == null || tenantAccount.ErrorCode != null || tenantAccount.TenantAccount == null)
            {
                _logger.LogError("{ClassName}.{MethodName}  - TenantAccount not found.", className, methodName);
                return new BaseResponseDto
                {
                    ErrorCode = StatusCodes.Status400BadRequest,
                    ErrorMessage = $"Invalid tenant code supplied, Tenant Account : {tenantCode}."
                };
            }

            var walletTypes = await GetWalletTypes(tenant.Tenant, tenantAccount.TenantAccount);

                var tenantAttrs = string.IsNullOrEmpty(tenant.Tenant.TenantAttribute)
                    ? null
                    : JsonConvert.DeserializeObject<TenantAttrs>(tenant.Tenant.TenantAttribute);
                var walletAttributes = ExtractWalletAttributes(tenantAttrs);

           await CreateConsumerWallets(walletTypes,tenant.Tenant, walletAttributes, consumerResponse.Consumer);


            return new BaseResponseDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Class}.{Method} - Unexpected error for ConsumerCode: {ConsumerCode}",
                    className, methodName, consumerCode);

                return new BaseResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError, ErrorMessage = "Internal error while creating consumer wallets." };
            }
        }

        private async System.Threading.Tasks.Task CreateConsumerWallets(Dictionary<string, CreateWalletTypes> walletTypes,TenantDto? tenantDto,
           WalletAttributes walletAttributes , ConsumerDto consumer)
        {
            try
            {
                if (tenantDto == null || consumer == null)
                {
                    _logger.LogWarning("{ClassName}.{MethodName} - Missing required parameters", className, nameof(CreateConsumerWallets));
                    return;
                }

                var consumerWallets = await GetConsumerWallets(tenantDto.TenantCode, consumer.ConsumerCode);
                var (earnMax, membershipWalletEarnMax) = ExtractWalletLimits(tenantDto);

                foreach (var walletType in walletTypes)
                {
                    try
                    {
                        (var exists, var index) = ConsumerWalletExists(consumerWallets,walletType.Value.WalletTypeId,consumer?.ConsumerCode,consumer?.TenantCode);

                        if (walletType.Value.WalletTypeId <= 0 || exists)
                        {
                            continue;
                        }

                        var (wallet, consumerWallet, walletTypeCode) = InitializeWallet(walletType, tenantDto, walletAttributes, consumer);

                        var createWallets = true;
                        if (!string.IsNullOrEmpty(consumer?.MemberNbr) &&
                            !string.IsNullOrEmpty(consumer?.SubscriberMemberNbr) && consumer?.MemberNbr != consumer?.SubscriberMemberNbr)
                        {
                            if (!walletAttributes.IndividualWallet)
                            {
                                createWallets = await HandleDependentConsumerWallet(
                                    tenantDto.TenantCode!,
                                    walletType.Value,
                                    wallet,
                                    consumer,
                                    consumerWallet);
                            }
                            else
                            {
                                //constributer will have there own wallet to redeem
                                consumerWallet.ConsumerRole = "O";

                                if (!(walletType.Key == Constant.MembershipDollars ||
                                           walletType.Key.Contains(Constant.BenefitWalletType) ||
                                           walletType.Key.Contains(Constant.BenefitPurseWalletType) ||
                                           walletType.Key == Constant.SweepstakesReward))
                                {
                                    wallet.EarnMaximum = walletAttributes.ContributorMax;
                                    consumerWallet.EarnMaximum = Convert.ToDecimal(walletAttributes.ContributorMax);
                                }
                            }
                            
                        }

                        if (createWallets)
                        {
                            await CreateConsumerWallet(wallet, consumerWallet);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "{ClassName}.{MethodName} - Error occurred while creating specific consumer wallet for Tenant: {TenantCode},WalletType: {WalletType}. ErrorCode: {Code}, ErrorMessage: {ErrorMessage}",
                        className, nameof(CreateConsumerWallets), tenantDto.TenantCode, walletType, StatusCodes.Status500InternalServerError, ex.Message);
                    }

                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Error occurred while creating consumer wallets for tenant: {TenantCode}. ErrorCode: {Code}, ErrorMessage: {ErrorMessage}",
                     className, nameof(CreateConsumerWallets), tenantDto?.TenantCode, StatusCodes.Status500InternalServerError, ex.Message);
            }

        }


        private (double earnMax, double membershipWalletEarnMax) ExtractWalletLimits(TenantDto tenantDto)
        {
            double earnMax = 500;
            double membershipWalletEarnMax = Constant.MembershipWalletEarnMax;

            if (!string.IsNullOrEmpty(tenantDto.TenantAttribute))
            {
                var tenantAttrs = JsonConvert.DeserializeObject<TenantAttrs>(tenantDto.TenantAttribute);
                earnMax = tenantAttrs?.ConsumerWallet?.OwnerMaximum ?? earnMax;
                membershipWalletEarnMax = tenantAttrs?.MembershipWallet?.EarnMaximum ?? membershipWalletEarnMax;
            }

            return (earnMax, membershipWalletEarnMax);
        }

        private async System.Threading.Tasks.Task CreateConsumerWallet(WalletDto wallet, ConsumerWalletDto consumerWallet)
        {
            var consumerWalletDataDto = new List<ConsumerWalletDataDto>() { new()
                                    {
                                        walletDto = wallet,
                                        consumerWalletDto = consumerWallet
                                    } };
            var consumerWalletResponses = await _consumerWalletService.PostConsumerWallets(consumerWalletDataDto);


            // Handle errors and log success
            HandleConsumerWalletResponses(consumerWalletResponses, consumerCode: consumerWallet?.ConsumerCode);
        }

        private void HandleConsumerWalletResponses(List<ConsumerWalletDataResponseDto> responses, string? consumerCode)
        {
            const string methodName = nameof(HandleConsumerWalletResponses);
            if (responses == null || responses.Count == 0)
            {
                _logger.LogError("{ClassName}.{MethodName} - Could Not create wallet for consumer, consumer code: {ConsumerCode}", className, methodName, consumerCode);
            }
            _logger.LogInformation("{ClassName}.{MethodName} - Successfully created wallet for consumer, consumer code: {ConsumerCode}", className, methodName, consumerCode);
        }

        private (bool, int) ConsumerWalletExists(ConsumerWalletResponseDto consumerWallets, long walletTypeId, string? consumerCode, string? tenantCode)
        {
           var now = DateTime.UtcNow;

            var matchingWallets = consumerWallets.ConsumerWalletDetails
                .Where(x =>
                    x.WalletType?.WalletTypeId == walletTypeId &&
                    x.ConsumerWallet?.ConsumerCode == consumerCode &&
                    x.ConsumerWallet?.TenantCode == tenantCode)
                .OrderBy(x => x.Wallet!.WalletId)
                .ToList();

            if (!matchingWallets.Any())  // no matching wallets found
                return (false, 0);


            for (int i = 0; i < matchingWallets.Count; i++)
            {
                var wallet = matchingWallets[i].Wallet;

                if (wallet == null)
                    continue;

                // ACTIVE wallet
                if (wallet.ActiveStartTs <= now && wallet.ActiveEndTs >= now)
                {
                    _logger.LogInformation(
                        "{Class}.{Method} - Active wallet exists. ConsumerCode: {ConsumerCode}, WalletTypeId: {WalletTypeId}, Index: {Index}",
                        className, nameof(ConsumerWalletExists), consumerCode, walletTypeId, i);

                    return (true, i);
                }

                // INACTIVE wallet
                else
                {
                    _logger.LogWarning(
                        "{Class}.{Method} - Wallet exists but NOT ACTIVE. Consumer: {Consumer}, WalletTypeId: {WalletTypeId}, Start: {Start}, End: {End}, Index: {Index}",
                        className, nameof(ConsumerWalletExists), consumerCode, walletTypeId,
                        wallet.ActiveStartTs, wallet.ActiveEndTs, i);

                    return (false, i + 1);  // next insert index = inactive index + 1
                }
            }

            return (false, 0);
        }

        private (WalletDto wallet, ConsumerWalletDto consumerWallet, string? walletTypeCode)
            InitializeWallet(KeyValuePair<string, SunnyRewards.Helios.Wallet.Core.Domain.Dtos.CreateWalletTypes> walletType, TenantDto tenant, WalletAttributes walletAttributes, ConsumerDto consumer)
        {
            var wallet = new WalletDto
            {
                TenantCode = tenant.TenantCode,
                Balance = 0,
                EarnMaximum = walletAttributes.IndividualWallet ? walletAttributes.OwnerMax : walletAttributes.WalletMax,
                WalletTypeId = walletType.Value.WalletTypeId,
                WalletName = walletType.Value.WalletTypeLabel?.ToUpper(),
                Active = true,
                WalletCode = $"wal-{Guid.NewGuid():N}",
                CustomerCode = $"cus-{Guid.NewGuid():N}",
                ActiveEndTs = walletType.Value.ActiveEndTs,
                ActiveStartTs = walletType.Value.ActiveStartTs,
                RedeemEndTs = walletType.Value.RedeemEndTs
            };

            // Retrieve configuration values once
            var rewardWalletTypeCode = _configuration.GetSection(Constant.RewardWalletTypeCode)?.Value;
            var sweepstakesWalletTypeCode = _configuration.GetSection(Constant.SweepstakesEntriesWalletTypeCode)?.Value;

            var walletTypeCode = rewardWalletTypeCode;

            // Handle specific wallet types
            if (walletType.Key == Constant.SweepstakesReward)
            {
                wallet.WalletName = Constant.WalletSweepstakesReward;
                walletTypeCode = sweepstakesWalletTypeCode;
                wallet.EarnMaximum = Constant.SweepsTasksWalletEarnMax;
            }
            else if (walletType.Key == Constant.MembershipDollars)
            {
                wallet.EarnMaximum = walletAttributes.MembershipWalletEarnMax;
                wallet.WalletName = walletType.Value.WalletTypeLabel;
                walletTypeCode = walletType.Value.WalletTypeCode;
            }
            else if (walletType.Key.Contains(Constant.BenefitWalletType) || walletType.Key.Contains(Constant.BenefitPurseWalletType))
            {
                wallet.WalletName = walletType.Value.WalletTypeLabel;

              //  wallet.EarnMaximum = 0.0;

                walletTypeCode = walletType.Value.WalletTypeCode;
            }


            // Initialize ConsumerWalletDto with assigned ConsumerCode
            var consumerWallet = new ConsumerWalletDto
            {
                ConsumerCode = consumer.ConsumerCode,
                TenantCode = tenant.TenantCode,
                ConsumerRole = "O",
                EarnMaximum = (decimal)wallet.EarnMaximum
            };

            return (wallet, consumerWallet, walletTypeCode);
        }

        private async Task<ConsumerWalletResponseDto> GetConsumerWallets(string? tenantCode, string? consumerCode)
        {
            var getConsumerWalletRequestDto = new GetConsumerWalletRequestDto()
            {
                TenantCode = tenantCode,
                ConsumerCode = consumerCode
            };
            var response = await _consumerWalletService.GetAllConsumerWalletsAsync(getConsumerWalletRequestDto);

            return response;
        }

        private async System.Threading.Tasks.Task<bool> HandleDependentConsumerWallet(
            string tenantCode,
            CreateWalletTypes walletType,
            WalletDto wallet,
            ConsumerDto? consumer,
            ConsumerWalletDto consumerWallet)
        {
            consumerWallet.ConsumerRole = Constant.ConsumerRoleAsContributor;
            consumerWallet.EarnMaximum = Convert.ToDecimal(consumerWallet.EarnMaximum);

            var consumerRequestDto = new GetConsumerByMemIdRequestDto { TenantCode = tenantCode, MemberId = consumer?.MemberId };

            var consumerByMemberNbr = await _consumerService.GetConsumerByMemId(consumerRequestDto);

            if (consumerByMemberNbr?.Consumer == null)
            {
                _logger.LogWarning("{ClassName}.{MethodName} - Unable to find main subscriber through SubscriberMemberNbr: {SubscriberMemberNbr}, skipping dependent wallet association creation",
                    className, nameof(CreateConsumerWallets), consumer?.SubscriberMemberNbr);
                return false;
            }

            var consumerWalletByWalletTypeRequestDto = new FindConsumerWalletByWalletTypeRequestDto { ConsumerCode = consumerByMemberNbr.Consumer.ConsumerCode, WalletTypeCode = walletType.WalletTypeCode };

            var consumerWalletResponse = await _consumerWalletService.GetConsumerWalletsByWalletType(consumerWalletByWalletTypeRequestDto);


            if (consumerWalletResponse?.ConsumerWallets?.Any() == true)
            {
                wallet.WalletId = consumerWalletResponse.ConsumerWallets[0].WalletId;
                consumerWallet.WalletId = consumerWalletResponse.ConsumerWallets[0].WalletId;
                return true;
            }
            return false;
        }

        private async Task<Dictionary<string, CreateWalletTypes>> GetWalletTypes(TenantDto? tenant, TenantAccountRequestDto? tenantAccount)
        {
            const string methodName = nameof(GetWalletTypes);
            var walletTypes = new Dictionary<string, CreateWalletTypes>();
            var tenantOption = tenant?.TenantOption != null ? JsonConvert.DeserializeObject<TenantOption>(tenant.TenantOption) : new TenantOption();

            if (tenantOption?.Apps.FindIndex(x => x?.ToUpper() == Constant.Rewards) > -1)
            {
                WalletTypeDto rewardWalletType = await GetWalletTypeByCode(_configuration.GetSection(Constant.RewardWalletTypeCode).Value!);
                var createWalletRewards = new CreateWalletTypes
                {
                    WalletTypeId = rewardWalletType.WalletTypeId,
                    WalletTypeCode = rewardWalletType.WalletTypeCode,
                    WalletTypeName = rewardWalletType.WalletTypeName,
                    WalletTypeLabel = rewardWalletType.WalletTypeLabel,
                    ShortLabel = rewardWalletType.ShortLabel,
                    IsExternalSync = rewardWalletType.IsExternalSync,
                    ConfigJson = rewardWalletType.ConfigJson,
                    // Add your 3 extra fields here
                    ActiveStartTs = DateTime.UtcNow,
                    ActiveEndTs = GetLastSecondOfYearUtc(),
                    RedeemEndTs = GetLastSecondOfYearUtc()
                };
                walletTypes.Add(Constant.Reward, createWalletRewards);

                WalletTypeDto sweepstakesWalletType = await GetWalletTypeByCode(_configuration.GetSection(Constant.SweepstakesEntriesWalletTypeCode).Value!);

                var createWalletSweepsTakes = new CreateWalletTypes
                {
                    WalletTypeId = sweepstakesWalletType.WalletTypeId,
                    WalletTypeCode = sweepstakesWalletType.WalletTypeCode,
                    WalletTypeName = sweepstakesWalletType.WalletTypeName,
                    WalletTypeLabel = sweepstakesWalletType.WalletTypeLabel,
                    ShortLabel = sweepstakesWalletType.ShortLabel,
                    IsExternalSync = sweepstakesWalletType.IsExternalSync,
                    ConfigJson = sweepstakesWalletType.ConfigJson,
                    // Add your 3 extra fields here
                    ActiveStartTs = DateTime.UtcNow,
                    ActiveEndTs = GetLastSecondOfYearUtc(),
                    RedeemEndTs = GetLastSecondOfYearUtc()
                };
                walletTypes.Add(Constant.SweepstakesReward, createWalletSweepsTakes);

                WalletTypeDto membershipDollarsWalletType = await GetWalletTypeByCode(_configuration.GetSection(Constant.MembershipDollarsWalletTypeCode).Value!);

                var createWalletMemberShip = new CreateWalletTypes
                {
                    WalletTypeId = membershipDollarsWalletType.WalletTypeId,
                    WalletTypeCode = membershipDollarsWalletType.WalletTypeCode,
                    WalletTypeName = membershipDollarsWalletType.WalletTypeName,
                    WalletTypeLabel = membershipDollarsWalletType.WalletTypeLabel,
                    ShortLabel = membershipDollarsWalletType.ShortLabel,
                    IsExternalSync = membershipDollarsWalletType.IsExternalSync,
                    ConfigJson = membershipDollarsWalletType.ConfigJson,
                    // Add your 3 extra fields here
                    ActiveStartTs = DateTime.UtcNow,
                    ActiveEndTs = GetLastSecondOfYearUtc(),
                    RedeemEndTs = GetLastSecondOfYearUtc()
                };

                walletTypes.Add(Constant.MembershipDollars, createWalletMemberShip);
            }

            if (tenantOption?.Apps.FindIndex(x => x?.ToUpper() == Constant.Benefits) > -1)
            {
                if (tenant == null || tenant.TenantCode == null)
                {
                    _logger.LogWarning("{ClassName}.{MethodName} - TenantCode is null or empty for PartnerCode:{PartnerCode}", className, methodName, tenant?.PartnerCode);
                    return walletTypes;
                }

                if (tenantAccount == null || tenantAccount.TenantConfigJson == null)
                {
                    _logger.LogWarning("{ClassName}.{MethodName} - TenantAccount is null or empty for TenantCode:{TenantCode}", className, methodName, tenant.TenantCode);
                    return walletTypes;
                }

                var tenantConfig = JsonConvert.DeserializeObject<TenantConfig>(tenantAccount.TenantConfigJson);


                //Create wallets for each purse which should be active logically

                var purses = tenantConfig?.PurseConfig?.Purses;

                if(purses == null || purses.Count == 0)
                {
                    _logger.LogWarning("{ClassName}.{MethodName} - No purses found in TenantConfig for TenantCode:{TenantCode}", className, methodName, tenant.TenantCode);
                    return walletTypes;
                }

                var validPurses = purses.Where(p => p.ActiveEndTs >= DateTime.UtcNow).ToList();
                if (validPurses.Count == 0)
                {
                    _logger.LogWarning("{ClassName}.{MethodName} - No active purses found in TenantConfig for TenantCode:{TenantCode}", className, methodName, tenant.TenantCode);
                    return walletTypes;
                }


                for (int index = 0; index < validPurses.Count; index++)
                {
                    var purse = validPurses[index];
                    if (purse.WalletType != null)
                    {
                        var benefitWalletType = await GetWalletTypeByCode(purse.WalletType);

                        var createwallet = new CreateWalletTypes
                        {
                            WalletTypeId = benefitWalletType.WalletTypeId,
                            WalletTypeCode = benefitWalletType.WalletTypeCode,
                            WalletTypeName = benefitWalletType.WalletTypeName,
                            ShortLabel = benefitWalletType.ShortLabel,
                            IsExternalSync = benefitWalletType.IsExternalSync,
                            WalletTypeLabel = purse.PurseLabel,
                            ConfigJson = benefitWalletType.ConfigJson,

                            ActiveStartTs = purse.ActiveStartTs!.Value,
                            ActiveEndTs = purse.ActiveEndTs!.Value,
                            RedeemEndTs = purse.RedeemEndTs!.Value
                        };

                        benefitWalletType.WalletTypeLabel = purse.PurseLabel;
                        walletTypes.Add($"{Constant.BenefitWalletType}_{index}", createwallet);
                    }

                    if ( purse.PurseWalletType != null)
                    {
                        var benefitPurseWalletType = await GetWalletTypeByCode(purse.PurseWalletType);
                        var createwallet = new CreateWalletTypes
                        {
                            WalletTypeId = benefitPurseWalletType.WalletTypeId,
                            WalletTypeCode = benefitPurseWalletType.WalletTypeCode,
                            WalletTypeName = benefitPurseWalletType.WalletTypeName,
                            ShortLabel = benefitPurseWalletType.ShortLabel,
                            IsExternalSync = benefitPurseWalletType.IsExternalSync,
                            WalletTypeLabel = purse.PurseLabel,
                            ConfigJson = benefitPurseWalletType.ConfigJson,

                            ActiveStartTs = purse.ActiveStartTs!.Value,
                            ActiveEndTs = purse.ActiveEndTs!.Value,
                            RedeemEndTs = purse.RedeemEndTs!.Value
                        };
                        walletTypes.Add($"{Constant.BenefitPurseWalletType}_{index}", createwallet);
                    }

                }
            }

            return walletTypes;
        }

        private async Task<WalletTypeDto> GetWalletTypeByCode(string? walletTypeCode)
        {
            var getWalletTypeDto = new WalletTypeDto()
            {
                WalletTypeCode = walletTypeCode,
            };
            return await _walletClient.Post<WalletTypeDto>(Constant.WalletTypeCode, getWalletTypeDto);
        }

        private static DateTime GetLastSecondOfYearUtc()
        {
            var currentYear = DateTime.UtcNow.Year;
            return new DateTime(currentYear, 12, 31, 23, 59, 59, DateTimeKind.Utc);
        }

        private static WalletAttributes ExtractWalletAttributes(TenantAttrs? tenantAttrs)
        {
            if (tenantAttrs?.ConsumerWallet != null)
            {
                return new WalletAttributes(
                    tenantAttrs.ConsumerWallet.OwnerMaximum,
                    tenantAttrs.ConsumerWallet.ContributorMaximum,
                    tenantAttrs.ConsumerWallet.WalletMaximum,
                    tenantAttrs.ConsumerWallet.IndividualWallet,
                    tenantAttrs.MembershipWallet?.EarnMaximum ?? Constant.MembershipWalletEarnMax
                );
            }

            return new WalletAttributes(0, 0, 0, false, Constant.MembershipWalletEarnMax);
        }

    }


}
