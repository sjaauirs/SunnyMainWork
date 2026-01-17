using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SunnyRewards.Helios.ETL.Common.CustomException;
using SunnyRewards.Helios.ETL.Core.Domain.Constants;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos.FIS;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos.Json;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Services.FIS;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces.FIS;


public class BenefitsFundingService : IBenefitsFundingService
{
    private readonly ILogger<BenefitsFundingService> _logger;
    private readonly ITenantAccountRepo _tenantAcountRepo;
    private readonly ITenantRepo _tenantRepo;
    private readonly IWalletRepo _walletRepo;
    private readonly IWalletTypeRepo _walletTypeRepo;
    private readonly IConsumerWalletRepo _consumerWalletRepo;
    private readonly IEnumerable<IFundingRuleExecService> _fundingRuleExecServices;
    private readonly IAwsS3Service _awsS3Service;
    private readonly IConsumerAccountRepo _consumerAccountRepo;
    private readonly ICohortConsumerRepo _cohortConsumerRepo;
    private readonly ICohortRepo _cohortRepo;
    const string className = nameof(BenefitsFundingService);

    public BenefitsFundingService(ILogger<BenefitsFundingService> logger, ITenantAccountRepo tenantAccountRepo,
        ITenantRepo tenantRepo, IWalletRepo walletRepo, IWalletTypeRepo walletTypeRepo,
        IConsumerWalletRepo consumerWalletRepo, IEnumerable<IFundingRuleExecService> fundingRuleExecServices,
        IAwsS3Service awsS3Service, IConsumerAccountRepo consumerAccountRepo, ICohortConsumerRepo cohortConsumerRepo, ICohortRepo cohortRepo    )
    {
        _logger = logger;
        _tenantAcountRepo = tenantAccountRepo;
        _tenantRepo = tenantRepo;
        _walletRepo = walletRepo;
        _walletTypeRepo = walletTypeRepo;
        _consumerWalletRepo = consumerWalletRepo;
        _fundingRuleExecServices = fundingRuleExecServices;
        _awsS3Service = awsS3Service;
        _consumerAccountRepo = consumerAccountRepo;
        _cohortConsumerRepo = cohortConsumerRepo;
        _cohortRepo = cohortRepo;
    }
    /// <summary>
    /// Executes the benefits funding rules for a given tenant.
    /// </summary>
    /// <param name="executionContext"></param>
    public async Task ExecuteFundingRules(EtlExecutionContext etlExecutionContext)
    {
        const string methodName = nameof(ExecuteFundingRules);
        var tenantCode = etlExecutionContext.TenantCode;
        if (string.IsNullOrEmpty(tenantCode))
        {
            _logger.LogWarning("{className}.{methodName}:: No tenant code provided for processing benefits fund.ErrorCode:{error}", className, methodName, StatusCodes.Status400BadRequest);
            throw new ETLException(ETLExceptionCodes.NullValue, "No tenant code provided for processing benefits fund");
        }
        try
        {
            var Today = DateTime.UtcNow;
            _logger.LogInformation("{className}.{methodName}: Starting to process funding rules for TenantCode: {TenantCode}", className, methodName, tenantCode);

            // Retrieve tenant information
            var tenant = await _tenantRepo.FindOneAsync(x => x.TenantCode == tenantCode && x.DeleteNbr == 0);
            if (tenant == null)
            {
                _logger.LogWarning("{className}.{methodName}: Invalid tenant code: {TenantCode}, Error Code:{errorCode}", className, methodName, tenantCode, StatusCodes.Status404NotFound);
                throw new ETLException(ETLExceptionCodes.NotFoundInDb, $"Invalid tenant code : {tenantCode}");
            }

            var tenantAttributes = !string.IsNullOrEmpty(tenant.TenantAttribute)
                        ? JsonConvert.DeserializeObject<TenantAttribute>(tenant.TenantAttribute)
                        : new TenantAttribute();

            // Retrieve tenant account information and funding configuration
            var tenantAccount = await _tenantAcountRepo.FindOneAsync(x => x.TenantCode == tenantCode && x.DeleteNbr == 0);
            if (tenantAccount == null || tenantAccount.FundingConfigJson == null)
            {
                _logger.LogWarning("{className}.{methodName}: Funding configuration not available for Tenant: {TenantCode}, Error Code:{errorCode}", className, methodName, tenantCode, StatusCodes.Status404NotFound);
                throw new ETLException(ETLExceptionCodes.NotFoundInDb, $" Funding configuration not available in tenant account for Tenant code : {tenantCode}");
            }
            //Read consumer list from the file if exists and execute funding rules only for the listed consumers
            var adhocConsumerCodesList = await _awsS3Service.GetConsumerListFromFile(etlExecutionContext.ConsumerListFile);

            // Deserialize funding configuration
            FISFundingConfigDto fundingConfig = JsonConvert.DeserializeObject<FISFundingConfigDto>(tenantAccount.FundingConfigJson);
            if (fundingConfig != null && fundingConfig.FundingRules != null && fundingConfig.FundingRules.Any())
            {
                //ignore “ONBOARDING” funding rules
                fundingConfig.FundingRules = fundingConfig.FundingRules.Where(x => x.RecurrenceType != null && x.RecurrenceType.ToUpper() != BenefitsConstants.FISOnBoardingRecurrenceType).ToList();
                //ignore funding rules which are not in effective date range
                fundingConfig.FundingRules = fundingConfig.FundingRules
                .Where(x =>
                {
                    bool isValidStart = DateTime.TryParse(x.EffectiveStartDate, out var startDate);
                    bool isValidEnd = DateTime.TryParse(x.EffectiveEndDate, out var endDate);

                    if (isValidStart && isValidEnd)
                    {
                        return startDate <= Today && endDate >= Today;
                    }

                    // Log and skip invalid dates
                    _logger.LogInformation(
                        "{className}.{methodName}: Funding Rule Invalid for date validation - RuleNumber: {RuleNumber}, StartDate: {StartDate}, EndDate: {EndDate}, Today: {Today}",
                        className, methodName, x.RuleNumber, x.EffectiveStartDate, x.EffectiveEndDate, Today
                    );

                    return false; // skip invalid dates
                })
                .ToList();

                if(fundingConfig.FundingRules.Count == 0)
                {
                    _logger.LogInformation("{className}.{methodName}: No funding rules to process after filtering for TenantCode: {TenantCode}", className, methodName, tenantCode);
                    return;
                }

                foreach (var fundingRule in fundingConfig.FundingRules)
                {
                    // Retrieve master wallet type
                    var masterWalletType = await _walletTypeRepo.FindOneAsync(x => x.WalletTypeCode == fundingRule.MasterWalletType && x.DeleteNbr == 0);
                    if (masterWalletType == null)
                    {
                        _logger.LogWarning("{className}.{methodName}: Master wallet type not found.For Wallet Type:{type}", className, methodName, fundingRule.MasterWalletType);
                        continue;
                    }

                    // Retrieve master wallet
                    var masterWallet = await _walletRepo.FindOneAsync(w => w.TenantCode == tenantCode && w.DeleteNbr == 0
                    && w.WalletTypeId == masterWalletType.WalletTypeId && w.MasterWallet == true);
                    if (masterWallet == null)
                    {
                        _logger.LogWarning($"{className}.{methodName}: Master wallet not found for tenant, TenantCode: {tenantCode}, RuleNumber:{fundingRule.RuleNumber}");
                        continue;
                    }

                    // Retrieve consumer wallet type
                    var consumerWalletType = await _walletTypeRepo.FindOneAsync(x => x.WalletTypeCode == fundingRule.ConsumerWalletType && x.DeleteNbr == 0);
                    if (consumerWalletType == null)
                    {
                        _logger.LogWarning($"{className}.{methodName}: Consumer wallet type not found. TenantCode:{tenantCode}, RuleNumber: {fundingRule.RuleNumber}");
                        continue;
                    }

                    // Retrieve consumer wallets and wallets
                    var consumerWalletsAndWalletModels = _walletRepo.GetAllConsumerWalletsAndWallets(tenantCode, consumerWalletType.WalletTypeId, adhocConsumerCodesList);

                    var WalletList = consumerWalletsAndWalletModels.Select(e => e.WalletModel).ToList();

                    var consumerWalletList = consumerWalletsAndWalletModels.Select(e => e.ConsumerWalletModel).ToList();

                    //select Active wallets in funding rule execution
                    WalletList = WalletList.Where(w => w.ActiveStartTs <= DateTime.UtcNow &&
                                            w.ActiveEndTs >= DateTime.UtcNow).ToList();

                    foreach (var wallet in WalletList)
                    {
                        if (fundingRule.RecurrenceType == BenefitsConstants.FISPeriodRecurrenceType && fundingRule.PeriodConfig != null)
                        {
                            var consumerWallet = consumerWalletList.FirstOrDefault(x =>
                                                        x.WalletId == wallet.WalletId &&
                                                        x.TenantCode == tenantCode &&
                                                        x.DeleteNbr == 0 &&
                                                        x.ConsumerRole == BenefitsConstants.ConsumerOwnerRole);

                            if (consumerWallet == null)
                            {
                                _logger.LogWarning($"{className}.{methodName}: Consumer wallet not found. TenantCode:{tenantCode}, WalletId: {wallet.WalletId}");
                                continue;
                            }

                            var consumerAccount = await _consumerAccountRepo.FindOneAsync(x =>
                                                                x.TenantCode == tenantCode &&
                                                                x.ConsumerCode == consumerWallet.ConsumerCode &&
                                                                x.DeleteNbr == 0);

                            // get enabled wallet types
                            var enabledWalletTypes = GetEnabledWalletTypes(consumerAccount, tenantAccount);

                            // If enabledWalletTypes is empty or ConsumerWalletType is not in the list, skip executing funding rules
                            bool shouldSkip = enabledWalletTypes?.Count == 0 || enabledWalletTypes != null && !enabledWalletTypes.Contains(fundingRule.ConsumerWalletType);

                            if (shouldSkip)
                            {
                                _logger.LogError(
                                    "{ClassName}.{MethodName}: Consumer Wallet type not found in enabled wallet types with wallet type code:{code}. Skipping current consumer wallet. TenantCode: {TenantCode}, ConsumerCode: {ConsumerCode}",
                                    className, methodName, fundingRule.ConsumerWalletType, tenantCode, consumerWallet.ConsumerCode);
                                continue;
                            }


                            //if consumer list is available, skip executing funding rules for consumers other than listed
                            if (adhocConsumerCodesList != null && adhocConsumerCodesList.Count > 0 && consumerWallet != null
                                        && consumerWallet.ConsumerCode != null && !adhocConsumerCodesList.Contains(consumerWallet.ConsumerCode))
                            {
                                _logger.LogError($"{className}.{methodName}: Consumer not found in the adhoc consumer list. TenantCode:{tenantCode}, consumerCode: {consumerWallet.ConsumerCode}");
                                continue;
                            }


                            // if funding rule has cohort codes, check if consumer belongs to All of the cohorts
                            if (fundingRule.CohortCodes.Count> 0)
                            {
                                //get consumer cohorts
                                var consumerCohort = await _cohortConsumerRepo.FindAsync(c =>
                                    c.TenantCode == tenantCode &&
                                    c.ConsumerCode == consumerWallet!.ConsumerCode &&
                                    c.DeleteNbr == 0 
                                   );

                                var cohortIds = consumerCohort.Select(c => c.CohortId).ToList();

                                var cohorts = await _cohortRepo.FindAsync(x => cohortIds.Contains(x.CohortId) && x.DeleteNbr == 0);

                                var consumerEnrolledCohorts = cohorts.Select(c => c.CohortCode).ToList();

                                //match all rules
                                bool isMatch = fundingRule.CohortCodes.All(cohort => consumerEnrolledCohorts.Contains(cohort, StringComparer.OrdinalIgnoreCase));

                                if (!isMatch)
                                {
                                    _logger.LogInformation("{className}.{methodName}: Consumer {ConsumerCode} does not belong to the required cohorts for funding rule execution. TenantCode:{TenantCode}, RuleNumber: {RuleNumber}",
                                        className, methodName, consumerWallet.ConsumerCode, tenantCode, fundingRule.RuleNumber);
                                    continue;
                                }
                            }


                            var fundTransferRequest = new FISFundTransferRequestDto
                            {
                                TenantCode = tenantCode,
                                ConsumerCode = consumerWallet.ConsumerCode,
                                MasterWallet = masterWallet,
                                ConsumerWallet = wallet,
                                TransactionDetailType = BenefitsConstants.BenefitTransactionDetailType,
                                Amount = fundingRule.Amount,
                                RewardDescription = fundingRule.RuleDescription,
                                RuleNumber = fundingRule.RuleNumber
                            };


                            //if consumer list is available, execute funding rules only for the consumers in the list as adhoc irrespective of interval(MONTH/QUARTER)
                            if (adhocConsumerCodesList != null && adhocConsumerCodesList.Count > 0 && consumerWallet != null
                                && consumerWallet.ConsumerCode != null && adhocConsumerCodesList.Contains(consumerWallet.ConsumerCode))
                            {
                                var periodMonthFundingRuleService = _fundingRuleExecServices.FirstOrDefault(s => s.GetType() == typeof(PeriodMonthFundingRuleService));
                                if (periodMonthFundingRuleService != null)
                                {
                                    await periodMonthFundingRuleService.ExecuteFundTransferAsync(fundTransferRequest);
                                }
                            }
                            //If interval is ADHOC, funding rules will be executed irrespective of interval(MONTH/QUARTER)
                            else if (string.Equals(fundingRule.PeriodConfig.Interval, BenefitsConstants.AdhocPeriodType, StringComparison.OrdinalIgnoreCase))
                            {
                                var periodMonthFundingRuleService = _fundingRuleExecServices.FirstOrDefault(s => s.GetType() == typeof(PeriodMonthFundingRuleService));
                                if (periodMonthFundingRuleService != null)
                                {
                                    await periodMonthFundingRuleService.ExecuteFundTransferAsync(fundTransferRequest);
                                }
                            }
                            //~~~~~~~~~Monthly execute funding rules~~~~~~~~~~~~~~
                            //Verify if just-in funding is enabled and currentDate should equal to fundDate-1 then execute funding rules.
                            //Example: currentDay = 28/02/2025 and fundDate = 01 (March 1st). (currentDay + 1 == fundDate) evaluates to true
                            //Verify if just-in funding is disabled and currentDate should equal to fundDate then execute funding rules.
                            else if (((tenantAttributes?.JustInTimeFunding == false && fundingRule.PeriodConfig.FundDate == DateTime.UtcNow.Day) ||
                                (tenantAttributes?.JustInTimeFunding == true && DateTime.UtcNow.AddDays(1).Day == fundingRule?.PeriodConfig?.FundDate))
                                && fundingRule.PeriodConfig.Interval == BenefitsConstants.MonthlyPeriodType)
                            {
                                var periodMonthFundingRuleService = _fundingRuleExecServices.FirstOrDefault(s => s.GetType() == typeof(PeriodMonthFundingRuleService));
                                if (periodMonthFundingRuleService != null)
                                {
                                    await periodMonthFundingRuleService.ExecuteFundingRuleAsync(fundingRule, fundTransferRequest);
                                }
                            }
                            //~~~~~~~~Quarterly execute funding rules~~~~~~~~~~~~~~
                            //Verify if just-in funding is enabled and currentDate should equal to fundDate-1 then execute funding rules.
                            //Example: currentDay = 28/02/2025 and fundDate = 01 (March 1st). (currentDay + 1 == fundDate) evaluates to true
                            //Verify if just-in funding is disabled and currentDate should equal to fundDate then execute funding rules.
                            else if (((tenantAttributes?.JustInTimeFunding == false && fundingRule.PeriodConfig.FundDate == DateTime.UtcNow.Day) ||
                                (tenantAttributes?.JustInTimeFunding == true && DateTime.UtcNow.AddDays(1).Day == fundingRule?.PeriodConfig?.FundDate))
                                && string.Equals(fundingRule.PeriodConfig.Interval, BenefitsConstants.QuarterlyPeriodType, StringComparison.OrdinalIgnoreCase))
                            {
                                var periodQuarterFundingRuleService = _fundingRuleExecServices.FirstOrDefault(s => s.GetType() == typeof(PeriodQuarterFundingRuleService));
                                if (periodQuarterFundingRuleService != null)
                                {
                                    await periodQuarterFundingRuleService.ExecuteFundingRuleAsync(fundingRule, fundTransferRequest);
                                }
                            }
                        }
                    }
                }
            }
            var isPathFullyQualified = Path.IsPathFullyQualified(etlExecutionContext.ConsumerListFile);

            if (adhocConsumerCodesList != null && adhocConsumerCodesList.Count > 0 && !isPathFullyQualified)
            {
                await _awsS3Service.MoveFileFromProcessingToArchive(etlExecutionContext.ConsumerListFile);
            }

            _logger.LogInformation("{className}.{methodName}: Funding rules processing completed successfully for TenantCode: {TenantCode}", className, methodName, tenantCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{className}.{methodName}: Error occurred while executing funding rules, Error: {Message}", className, methodName, ex.Message);
            throw;
        }
    }

    private static List<string> GetEnabledWalletTypes(ETLConsumerAccountModel consumerAccount, ETLTenantAccountModel tenantAccount)
    {
        if (consumerAccount == null || string.IsNullOrEmpty(consumerAccount.ConsumerAccountConfigJson))
        {
            return null;
        }

        var consumerAccountConfig = JsonConvert.DeserializeObject<ConsumerAccountConfigJson>(consumerAccount.ConsumerAccountConfigJson);

        if (consumerAccountConfig?.ConsumerAccountPurseConfigDto?.Purses == null || !consumerAccountConfig.ConsumerAccountPurseConfigDto.Purses.Any())
        {
            return null;
        }

        var tenantConfig = string.IsNullOrEmpty(tenantAccount.TenantConfigJson)
            ? new FISTenantConfigDto()
            : JsonConvert.DeserializeObject<FISTenantConfigDto>(tenantAccount.TenantConfigJson);

        if (tenantConfig?.PurseConfig?.Purses == null || !tenantConfig.PurseConfig.Purses.Any())
        {
            return null;
        }

        // Extract enabled consumer purse labels
        var enabledPurseLabels = consumerAccountConfig?.ConsumerAccountPurseConfigDto?.Purses
            .Where(purse => purse.Enabled)
            .Select(purse => purse.PurseLabel?.ToUpper())
            .ToHashSet();

        // Map tenant purses to wallet types based on enabled labels
        return tenantConfig.PurseConfig.Purses
            .Where(purse => purse?.PurseLabel != null && enabledPurseLabels.Contains(purse.PurseLabel.ToUpper()))
            .Select(purse => purse.WalletType)
            .ToList();
    }

}
