using Amazon.Runtime.Internal.Transform;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SunnyBenefits.Fis.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Common.Constants;
using SunnyRewards.Helios.ETL.Common.CustomException;
using SunnyRewards.Helios.ETL.Core.Domain.Constants;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos.Json;
using SunnyRewards.Helios.ETL.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos.Json;
using SunnyRewards.Helios.Tenant.Core.Domain.Models;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos.Json;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services
{
    public class WalletSyncService : IWalletSyncService
    {
        private readonly ILogger<WalletSyncService> _logger;
        private readonly IAdminClient _adminClient;
        private readonly IConfiguration _configuration;
        private const string className = nameof(WalletSyncService);
        public WalletSyncService(ILogger<WalletSyncService> logger, IAdminClient adminClient, IConfiguration configuration)
        {
            _logger = logger;
            _adminClient = adminClient;
            _configuration = configuration;

        }
        /// <summary>
        /// Synchronizes the wallets for tenant asynchronous.
        /// </summary>
        /// <param name="tenantDto">The tenant dto.</param>
        /// <param name="tenantAccountRequestDto">The tenant account request dto.</param>
        /// <param name="consumerCodes">The consumer codes.</param>
        public async Task SyncWalletsForTenantAsync(TenantDto? tenantDto, TenantAccountRequestDto? tenantAccountRequestDto, string consumerCodes)
        {
            const string methodName = nameof(SyncWalletsForTenantAsync);
            try
            {
                await SyncMasterWallets(tenantDto, tenantAccountRequestDto, methodName);
                await SyncConsumerWallets(tenantDto, tenantAccountRequestDto, consumerCodes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}  - Failed processing wallet sync. ErrorCode:{Code},ERROR: {Message}", className, methodName, StatusCodes.Status500InternalServerError, ex.Message);
                throw;
            }

        }

        /// <summary>
        /// Synchronizes the master wallets.
        /// </summary>
        /// <param name="tenantDto">The tenant dto.</param>
        /// <param name="tenantAccountRequestDto">The tenant account request dto.</param>
        /// <param name="methodName">Name of the method.</param>
        /// <exception cref="SunnyRewards.Helios.ETL.Common.CustomException.ETLException">Error fetching sponsor and customer details for TenantCode: {tenantCode}. ErrorCode: {sponsorCustomerResponse.ErrorCode}, ERROR: {sponsorCustomerResponse.ErrorMessage}</exception>
        private async Task SyncMasterWallets(TenantDto? tenantDto, TenantAccountRequestDto? tenantAccountRequestDto, string methodName)
        {
            var tenantCode = tenantDto?.TenantCode;
            try
            {
                var sponsorCustomerResponse = await GetTenantSponsorCustomer(tenantCode);

                if (sponsorCustomerResponse.ErrorCode != null)
                {
                    _logger.LogError("{ClassName}.{MethodName} - Error fetching sponsor and customer details for TenantCode: {TenantCode}. ErrorCode: {Code}, ERROR: {ErrorMessage}",
                        className, methodName, tenantCode, sponsorCustomerResponse.ErrorCode, sponsorCustomerResponse.ErrorMessage);

                    throw new ETLException(ETLExceptionCodes.ErrorFromAPI,
                        $"Error fetching sponsor and customer details for TenantCode: {tenantCode}. ErrorCode: {sponsorCustomerResponse.ErrorCode}, ERROR: {sponsorCustomerResponse.ErrorMessage}");
                }

                var tenantConfig = string.IsNullOrEmpty(tenantAccountRequestDto?.TenantConfigJson)
                    ? new TenantConfig()
                    : JsonConvert.DeserializeObject<TenantConfig>(tenantAccountRequestDto.TenantConfigJson);

                // Create Master Wallets for Rewards
                await CreateMasterWallets(new CreateTenantMasterWalletsRequestDto
                {
                    CustomerCode = sponsorCustomerResponse.Customer?.CustomerCode ?? string.Empty,
                    SponsorCode = sponsorCustomerResponse.Sponsor?.SponsorCode ?? string.Empty,
                    TenantCode = tenantCode ?? string.Empty,
                    CreateUser = Constants.CreateUserAsETL,
                    Apps = [AdminConstants.Apps.Rewards]
                });

                // Create Master Wallets for Benefits
                await CreateMasterWallets(new CreateTenantMasterWalletsRequestDto
                {
                    CustomerCode = sponsorCustomerResponse.Customer?.CustomerCode ?? string.Empty,
                    SponsorCode = sponsorCustomerResponse.Sponsor?.SponsorCode ?? string.Empty,
                    TenantCode = tenantCode ?? string.Empty,
                    CreateUser = Constants.CreateUserAsETL,
                    Apps = [AdminConstants.Apps.Benefits],
                    PurseConfig = tenantConfig?.PurseConfig
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Error occurred while syncing master wallets for tenant: {TenantCode}", nameof(SyncConsumerWallets), methodName, tenantCode);
                throw;
            }
          
        }

        /// <summary>
        /// Creates the master wallets.
        /// </summary>
        /// <param name="createMasterWalletsRequest">The create master wallets request.</param>
        private async Task CreateMasterWallets(CreateTenantMasterWalletsRequestDto createMasterWalletsRequest)
        {
            var response = await _adminClient.Post<BaseResponseDto>(AdminConstants.CreateTenantMasterWalletsAPIUrl, createMasterWalletsRequest);

            if (response.ErrorCode != null)
            {
                _logger.LogWarning("{ClassName}.CreateMasterWallets - Error creating master wallets for TenantCode: {TenantCode}. ErrorCode: {ErrorCode}",
                    className, createMasterWalletsRequest.TenantCode, response.ErrorCode);
            }
        }

        /// <summary>
        /// Synchronizes the consumer wallets.
        /// </summary>
        /// <param name="tenantDto">The tenant dto.</param>
        /// <param name="tenantAccountRequestDto">The tenant account request dto.</param>
        /// <param name="consumerCodes">The consumer codes.</param>
        private async Task SyncConsumerWallets(TenantDto? tenantDto, TenantAccountRequestDto? tenantAccountRequestDto, string consumerCodes)
        {
            const string methodName = nameof(SyncWalletsForTenantAsync);
            var tenantCode = tenantDto?.TenantCode;

            try
            {
                if (string.IsNullOrWhiteSpace(consumerCodes))
                {
                    LogAndThrowInvalidConsumerCode(methodName, tenantCode);
                }

                var walletTypes = await GetWalletTypes(tenantDto, tenantAccountRequestDto);
                if (consumerCodes.Trim().ToUpper() == AdminConstants.ConsumerCodesAll)
                {
                    await SyncAllConsumers(tenantDto, methodName, tenantCode, walletTypes);
                }
                else
                {
                    await SyncSpecificConsumers(tenantDto, methodName, tenantCode, walletTypes, consumerCodes);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Error syncing consumer wallets for tenant: {TenantCode}", nameof(SyncConsumerWallets), methodName, tenantCode);
                throw;
            }
        }

        private void LogAndThrowInvalidConsumerCode(string methodName, string? tenantCode)
        {
            _logger.LogError("{ClassName}.{MethodName} - Invalid consumer codes in job params: {TenantCode}. ErrorCode: {Code}",
                nameof(SyncConsumerWallets), methodName, tenantCode, StatusCodes.Status500InternalServerError);

            throw new ETLException(ETLExceptionCodes.NullValue, $"Invalid consumers code in job params: {tenantCode}. ErrorCode: {StatusCodes.Status500InternalServerError}");
        }

        /// <summary>
        /// Synchronizes all consumers.
        /// </summary>
        /// <param name="tenantDto">The tenant dto.</param>
        /// <param name="methodName">Name of the method.</param>
        /// <param name="tenantCode">The tenant code.</param>
        /// <param name="walletTypes">The wallet types.</param>
        private async Task SyncAllConsumers(TenantDto? tenantDto, string methodName, string? tenantCode, Dictionary<string, CreateWalletTypes> walletTypes)
        {
            try
            {
                int pageNumber = 1;

                while (true)
                {
                    var request = new GetConsumerByTenantRequestDto
                    {
                        TenantCode = tenantCode,
                        PageNumber = pageNumber,
                        SearchTerm = string.Empty,
                        PageSize = AdminConstants.GetConsumersByTenantCodePageSize
                    };

                    var response = await _adminClient.Post<ConsumersAndPersonsListResponseDto>(AdminConstants.GetConsumersByTenantCode, request);
                    if (response?.ConsumerAndPersons == null || response.ConsumerAndPersons.Count() == 0)
                    {
                        break;
                    }

                    foreach (var consumerAndPerson in response.ConsumerAndPersons)
                    {
                        await CreateConsumerWallets(tenantDto, tenantCode, walletTypes, consumerAndPerson.Consumer);
                    }

                    pageNumber++;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Error occurred while syncing consumer wallets for tenant: {TenantCode}. ErrorCode: {Code}, ErrorMessage: {ErrorMessage}",
              className, nameof(SyncAllConsumers), tenantCode, StatusCodes.Status500InternalServerError, ex.Message);
                
            }
            
        }
        /// <summary>
        /// Synchronizes the specific consumers.
        /// </summary>
        /// <param name="tenantDto">The tenant dto.</param>
        /// <param name="methodName">Name of the method.</param>
        /// <param name="tenantCode">The tenant code.</param>
        /// <param name="walletTypes">The wallet types.</param>
        /// <param name="consumerCodes">The consumer codes.</param>
        private async Task SyncSpecificConsumers(TenantDto? tenantDto, string methodName, string? tenantCode, Dictionary<string, CreateWalletTypes> walletTypes, string consumerCodes)
        {
            try
            {
                var consumerCodeArray = consumerCodes.Split(',');
                foreach (var consumerCode in consumerCodeArray)
                {
                    var request = new GetConsumerRequestDto { ConsumerCode = consumerCode };
                    var consumerData = await GetConsumerData(request);

                    if (consumerData?.Consumer != null)
                    {
                        await CreateConsumerWallets(tenantDto, tenantCode, walletTypes, consumerData.Consumer);
                    }
                    else
                    {
                        _logger.LogWarning("{ClassName}.{MethodName} - Consumer not found: {ConsumerCode}", className, nameof(SyncConsumerWallets), consumerCode);
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Error occurred while syncing specific consumer wallets for tenant: {TenantCode}. ErrorCode: {Code}, ErrorMessage: {ErrorMessage}",
             className, nameof(SyncSpecificConsumers), tenantCode, StatusCodes.Status500InternalServerError, ex.Message);
            }

           
        }

        /// <summary>
        /// Creates the consumer wallets.
        /// </summary>
        /// <param name="tenantDto">The tenant dto.</param>
        /// <param name="tenantCode">The tenant code.</param>
        /// <param name="walletTypes">The wallet types.</param>
        /// <param name="consumer">The consumer.</param>
        private async Task CreateConsumerWallets(TenantDto? tenantDto, string? tenantCode,
            Dictionary<string, CreateWalletTypes> walletTypes, ConsumerDto? consumer)
        {
            try
            {
                if (tenantDto == null || string.IsNullOrEmpty(tenantCode) || consumer == null)
                {
                    _logger.LogWarning("{ClassName}.{MethodName} - Missing required parameters", className, nameof(CreateConsumerWallets));
                    return;
                }

                var consumerWallets = await GetConsumerWallets(tenantCode, consumer.ConsumerCode);
                var (earnMax, membershipWalletEarnMax) = ExtractWalletLimits(tenantDto);

                foreach (var walletType in walletTypes.Values)
                {
                    try
                    {
                        (var exists, var index) = ConsumerWalletExists(consumerWallets, walletType.WalletTypeId, consumer?.ConsumerCode, consumer?.TenantCode);

                        if (walletType.WalletTypeId <= 0 || exists)
                        {
                            continue;
                        }

                        var wallet = InitializeWallet(tenantCode, walletType, earnMax, membershipWalletEarnMax);
                        var consumerWallet = new ConsumerWalletDto
                        {
                            ConsumerCode = consumer?.ConsumerCode,
                            TenantCode = tenantCode,
                            ConsumerRole = AdminConstants.ConsumerRoleAsOwner
                        };

                        if (!string.IsNullOrEmpty(consumer?.MemberNbr) &&
                            !string.IsNullOrEmpty(consumer?.SubscriberMemberNbr) && consumer?.MemberNbr != consumer?.SubscriberMemberNbr)
                        {
                            await HandleDependentConsumerWallet(tenantCode, walletType, wallet, consumer, consumerWallet);
                            if (wallet.WalletId <= 0)
                            {
                                continue;
                            }
                        }

                        await CreateConsumerWallet(wallet, consumerWallet);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "{ClassName}.{MethodName} - Error occurred while creating specific consumer wallet for Tenant: {TenantCode},WalletType: {WalletType}. ErrorCode: {Code}, ErrorMessage: {ErrorMessage}",
                        className, nameof(CreateConsumerWallets), tenantCode,walletType, StatusCodes.Status500InternalServerError, ex.Message);
                    }
                   
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Error occurred while creating consumer wallets for tenant: {TenantCode}. ErrorCode: {Code}, ErrorMessage: {ErrorMessage}",
                     className, nameof(CreateConsumerWallets), tenantCode, StatusCodes.Status500InternalServerError, ex.Message);
            }
           
        }

        private (double earnMax, double membershipWalletEarnMax) ExtractWalletLimits(TenantDto tenantDto)
        {
            double earnMax = 500;
            double membershipWalletEarnMax = AdminConstants.MembershipWalletEarnMax;

            if (!string.IsNullOrEmpty(tenantDto.TenantAttribute))
            {
                var tenantAttrs = JsonConvert.DeserializeObject<TenantAttribute>(tenantDto.TenantAttribute);
                earnMax = tenantAttrs?.ConsumerWallet?.OwnerMaximum ?? earnMax;
                membershipWalletEarnMax = tenantAttrs?.MembershipWallet?.EarnMaximum ?? membershipWalletEarnMax;
            }

            return (earnMax, membershipWalletEarnMax);
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

        private WalletDto InitializeWallet(string tenantCode, CreateWalletTypes walletType, double earnMax, double membershipWalletEarnMax)
        {
            var wallet = new WalletDto
            {
                TenantCode = tenantCode,
                Balance = 0,
                EarnMaximum = earnMax,
                WalletTypeId = walletType.WalletTypeId,
                WalletName = walletType.WalletTypeLabel?.ToUpper() ?? "",
                Active = true,
                WalletCode = $"wal-{Guid.NewGuid():N}",
                CustomerCode = $"cus-{Guid.NewGuid():N}",
                ActiveEndTs = walletType.ActiveEndTs,
                ActiveStartTs = walletType.ActiveStartTs,
                RedeemEndTs = walletType.RedeemEndTs
                
            };

            string? walletTypeCode = _configuration.GetSection(AdminConstants.RewardWalletTypeCode).Value;

            if (walletType?.WalletTypeCode == AdminConstants.SweepstakesReward)
            {
                wallet.WalletName = AdminConstants.WalletSweepstakesReward;
                walletTypeCode = _configuration.GetSection(AdminConstants.SweepstakesEntriesWalletTypeCode).Value;
            }
            else if (walletType?.WalletTypeCode == AdminConstants.MembershipDollars)
            {
                wallet.EarnMaximum = membershipWalletEarnMax;
                wallet.WalletName = walletType.WalletTypeLabel;
                walletTypeCode = walletType.WalletTypeCode;
            }
            else if (walletType.WalletTypeCode.Contains(AdminConstants.BenefitWalletType) ||
                     walletType.WalletTypeCode.Contains(AdminConstants.BenefitPurseWalletType))
            {
                walletTypeCode = walletType.WalletTypeCode;
                wallet.WalletName = walletType.WalletTypeLabel;
               // wallet.EarnMaximum = 0.0;
            }

            return wallet;
        }

        private async Task HandleDependentConsumerWallet(
            string tenantCode,
            CreateWalletTypes walletType,
            WalletDto wallet,
            ConsumerDto? consumer,
            ConsumerWalletDto consumerWallet)
        {
            consumerWallet.ConsumerRole = AdminConstants.ConsumerRoleAsContributor;

            var consumerByMemberNbr = await _adminClient.Post<GetConsumerByMemIdResponseDto>(
                AdminConstants.GetConsumerByMemberNumber,
                new GetConsumerByMemIdRequestDto { TenantCode = tenantCode, MemberId = consumer?.MemberId });

            if (consumerByMemberNbr?.Consumer == null)
            {
                _logger.LogWarning("{ClassName}.{MethodName} - Unable to find main subscriber through SubscriberMemberNbr: {SubscriberMemberNbr}, skipping dependent wallet association creation",
                    className, nameof(CreateConsumerWallets), consumer?.SubscriberMemberNbr);
                return;
            }

            var consumerWalletResponse = await _adminClient.Post<FindConsumerWalletResponseDto>(
                AdminConstants.GetConsumerWalletByWalletType,
                new FindConsumerWalletByWalletTypeRequestDto { ConsumerCode = consumerByMemberNbr.Consumer.ConsumerCode, WalletTypeCode = walletType.WalletTypeCode });

            if (consumerWalletResponse?.ConsumerWallets?.Any() == true)
            {
                wallet.WalletId = consumerWalletResponse.ConsumerWallets[0].WalletId;
                consumerWallet.WalletId = consumerWalletResponse.ConsumerWallets[0].WalletId;
            }
            else
            {
                await CreateNewDependentWallet(tenantCode, wallet, consumerWallet, consumerByMemberNbr?.Consumer?.ConsumerCode);
            }
        }

        private async Task CreateNewDependentWallet(string tenantCode, WalletDto wallet, ConsumerWalletDto consumerWallet, string? mainConsumerCode)
        {
            var newWalletRequest = new List<ConsumerWalletDataDto>
            {
                new()
                {
                    walletDto = new WalletDto
                    {
                        TenantCode = tenantCode,
                        Balance = wallet.Balance,
                        EarnMaximum = wallet.EarnMaximum,
                        WalletTypeId = wallet.WalletTypeId,
                        WalletName = wallet.WalletName,
                        Active = true,
                        WalletCode = $"wal-{Guid.NewGuid():N}",
                        CustomerCode = $"cus-{Guid.NewGuid():N}",
                        ActiveEndTs = wallet.ActiveEndTs,
                        ActiveStartTs = wallet.ActiveStartTs,
                        RedeemEndTs = wallet.RedeemEndTs
                    },
                    consumerWalletDto = new ConsumerWalletDto
                    {
                        ConsumerCode = mainConsumerCode,
                        TenantCode = tenantCode,
                        ConsumerRole = AdminConstants.ConsumerRoleAsOwner
                    }
                }
            };

            var consumerWalletResponses = await _adminClient.Post<List<ConsumerWalletDataResponseDto>>(
                AdminConstants.PostConsumerWallets, newWalletRequest);

            if (consumerWalletResponses?.Any() != true)
            {
                _logger.LogError("{ClassName}.{MethodName} - Could not create wallet for consumer, consumer code: {ConsumerCode}",
                   className, nameof(CreateConsumerWallets), mainConsumerCode);
                return;
            }

            _logger.LogInformation("{ClassName}.{MethodName} - Successfully created wallet for consumer, consumer code: {ConsumerCode}",
               className, nameof(CreateConsumerWallets), mainConsumerCode);
            wallet.WalletId = consumerWalletResponses[0].Wallet.WalletId;
            consumerWallet.WalletId = consumerWalletResponses[0].Wallet.WalletId;
        }

        public async Task<GetConsumerResponseDto> GetConsumerData(GetConsumerRequestDto consumerRequestDto)
        {
            return await _adminClient.Post<GetConsumerResponseDto>(AdminConstants.GetConsumer, consumerRequestDto);
        }

        private async Task CreateConsumerWallet(WalletDto wallet, ConsumerWalletDto consumerWallet)
        {
            // Call the API for each consumer and collect the response
            var consumerWalletResponses = await _adminClient.Post<List<ConsumerWalletDataResponseDto>>(AdminConstants.PostConsumerWallets,
                new List<ConsumerWalletDataDto>() { new()
                                    {
                                        walletDto = wallet,
                                        consumerWalletDto = consumerWallet
                                    }
                });

            // Handle errors and log success
            HandleConsumerWalletResponses(consumerWalletResponses, consumerCode: consumerWallet?.ConsumerCode);
        }

        private async Task<TenantSponsorCustomerResponseDto> GetTenantSponsorCustomer(string? tenantCode)
        {
            IDictionary<string, long> parameters = new Dictionary<string, long>();
            return await _adminClient.Get<TenantSponsorCustomerResponseDto>($"{AdminConstants.GetTenantSponsorCustomer}/{tenantCode}", parameters);
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

        private async Task<Dictionary<string, CreateWalletTypes>> GetWalletTypes(TenantDto? tenant, TenantAccountRequestDto? tenantAccount)
        {
            const string methodName = nameof(GetWalletTypes);
            var walletTypes = new Dictionary<string, CreateWalletTypes>();
            var tenantOption = tenant?.TenantOption != null ? JsonConvert.DeserializeObject<TenantOption>(tenant.TenantOption) : new TenantOption();

            if (tenantOption?.Apps.FindIndex(x => x?.ToUpper() == Constants.Rewards) > -1)
            {
                WalletTypeDto rewardWalletType = await GetWalletTypeByCode(_configuration.GetSection(AdminConstants.RewardWalletTypeCode).Value!);
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
                walletTypes.Add(AdminConstants.Reward, createWalletRewards);

                WalletTypeDto sweepstakesWalletType = await GetWalletTypeByCode(_configuration.GetSection(AdminConstants.SweepstakesEntriesWalletTypeCode).Value!);

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
                walletTypes.Add(AdminConstants.SweepstakesReward, createWalletSweepsTakes);

                WalletTypeDto membershipDollarsWalletType = await GetWalletTypeByCode(_configuration.GetSection(AdminConstants.MembershipDollarsWalletTypeCode).Value!);

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

                walletTypes.Add(AdminConstants.MembershipDollars, createWalletMemberShip);
            }

            if (tenantOption?.Apps.FindIndex(x => x?.ToUpper() == Constants.Benefits) > -1)
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


                var purses = tenantConfig?.PurseConfig?.Purses;

                if (purses == null || purses.Count == 0)
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
                        walletTypes.Add($"{AdminConstants.BenefitWalletType}_{index}", createwallet);
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
                        walletTypes.Add($"{AdminConstants.BenefitPurseWalletType}_{index}", createwallet);
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
            var walletTypeResponse = await _adminClient.Post<WalletTypeResponseDto>(AdminConstants.GetWalletTypeByCode, getWalletTypeDto);
            if (walletTypeResponse == null || walletTypeResponse.ErrorCode != null || walletTypeResponse.WalletTypeDto == null || walletTypeResponse.WalletTypeDto.WalletTypeId <= 0)
            {
                _logger.LogError("{ClassName}.{MethodName} - Wallet Type not found for WalletTypeCode:{walletTypeCode}, ErrorCode: {ErrorCode}", className, nameof(GetWalletTypeByCode), walletTypeCode, StatusCodes.Status404NotFound);
            }
            return walletTypeResponse?.WalletTypeDto!;
        }

        private async Task<ConsumerWalletResponseDto> GetConsumerWallets(string? tenantCode, string? consumerCode)
        {
            var getConsumerWalletRequestDto = new GetConsumerWalletRequestDto()
            {
                TenantCode = tenantCode,
                ConsumerCode = consumerCode
            };
            return await _adminClient.Post<ConsumerWalletResponseDto>(AdminConstants.GetAllConsumerWallets, getConsumerWalletRequestDto);
        }

        private static DateTime GetLastSecondOfYearUtc()
        {
            var currentYear = DateTime.UtcNow.Year;
            return new DateTime(currentYear, 12, 31, 23, 59, 59, DateTimeKind.Utc);
        }
    }
}
