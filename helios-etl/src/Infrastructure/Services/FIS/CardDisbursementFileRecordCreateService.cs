using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.ETL.Common.CustomException;
using SunnyRewards.Helios.ETL.Common.Helpers.Interfaces;
using SunnyRewards.Helios.ETL.Core.Domain.Constants;
using SunnyRewards.Helios.ETL.Core.Domain.Constants.FIS;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos.Enums;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos.FIS;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos.Json;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces.FIS;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services.FIS
{
    public class CardDisbursementFileRecordCreateService : ICardDisbursementFileRecordCreateService
    {
        private static int DISBURSEMENT_RECORDS_CHUNK_SIZE = 100;
        private static int MAX_DISBURSEMENT_RECORDS = 30000;

        private readonly ILogger<CardDisbursementFileRecordCreateService> _logger;
        private readonly IConsumerRepo _consumerRepo;
        private readonly IWalletTypeRepo _walletTypeRepo;
        private readonly IFundTransferService _fundTransferService;
        private readonly IConsumerAccountRepo _consumerAccountRepo;
        private readonly IFlatFileGenerator _flatFileGenerator;
        private readonly IConfiguration _configuration;
        private readonly IWalletRepo _walletRepo;
        private readonly IAwsS3Service _awsS3Service;
        const string className = nameof(CardDisbursementFileRecordCreateService);
        public CardDisbursementFileRecordCreateService(ILogger<CardDisbursementFileRecordCreateService> logger,
            IConsumerRepo consumerRepo, IWalletTypeRepo walletTypeRepo, IFundTransferService fundTransferService,
            IConsumerAccountRepo consumerAccountRepo, IFlatFileGenerator flatFileGenerator, IConfiguration configuration,
            IWalletRepo walletRepo, IAwsS3Service awsS3Service)
        {
            _logger = logger;

            _consumerRepo = consumerRepo;
            _walletTypeRepo = walletTypeRepo;
            _fundTransferService = fundTransferService;
            _consumerAccountRepo = consumerAccountRepo;
            _flatFileGenerator = flatFileGenerator;
            _configuration = configuration;
            _walletRepo = walletRepo;
            _awsS3Service = awsS3Service;
        }

        public async Task<(List<string>, double)> GenerateDisbursementRecords(EtlExecutionContext etlExecutionContext, List<FISPurseDto> fisPurses,
            bool justInTimeFunding)
        {
            const string methodName = nameof(GenerateDisbursementRecords);
            var disbursementRecordsList = new List<string>();
            double disbursementRecordsTotal = 0;

            var consumersList = await _awsS3Service.GetConsumerListFromFileForCard60(etlExecutionContext.ConsumerListFile);

            var consumerCodes = new List<string>();
            var consumersWithWalletType = new List<ETLCard60ConsumerInputDto>();

            if (!string.IsNullOrEmpty(etlExecutionContext.ConsumerListFile))
            {
                if (consumersList?.Count() == 0)
                {
                    _logger.LogInformation($"{className}.{methodName}: No consumers found in the consumer list file: {etlExecutionContext.ConsumerListFile}. Exiting process.");
                    return (disbursementRecordsList, disbursementRecordsTotal);

                }
                if (consumersList is List<object> objectList)
                {
                    // If consumersList is a List<object>, we assume it might contain both types
                    consumerCodes = objectList.OfType<string>().ToList();
                    consumersWithWalletType = objectList.OfType<ETLCard60ConsumerInputDto>().ToList();
                }
                else
                {
                    _logger.LogError($"{className} {methodName}: Unexpected list type. Unable to cast consumersList {etlExecutionContext.ConsumerListFile}.");
                    throw new ETLException(ETLExceptionCodes.InValidValue, "Unexpected list type. Unable to cast consumersList.");
                }
            }


            var groupedConsumers = consumersWithWalletType?.GroupBy(c => c.ConsumerWalletTypeCode)?.ToList();

            foreach (var purse in fisPurses)
            {
                try
                {
                    var periodConfig = purse.PeriodConfig;
                    var todayUtcDate = DateTime.UtcNow;
                    var purseConsumers = new List<ETLCard60ConsumerInputDto>();
                    //When consumer list is available skip checking fund date is today and execute as ADHOC
                    if (periodConfig != null && !string.IsNullOrEmpty(etlExecutionContext.ConsumerListFile))
                    {
                        periodConfig.Interval = BenefitsConstants.AdhocPeriodType;
                        if (groupedConsumers?.Any() == true)
                        {
                            var match = groupedConsumers.FirstOrDefault(g => g.Key == purse.WalletType);
                            if (match == null)
                            {
                                _logger.LogInformation($"{className}.{methodName}: Skipping purse {purse.PurseNumber} - no consumers for wallet type {purse.WalletType}");
                                continue;
                            }
                            purseConsumers = match.ToList();
                            consumerCodes = purseConsumers.Select(c => c.ConsumerCode).ToList();

                            if (consumerCodes.Count == 0)
                            {
                                _logger.LogInformation($"{className}.{methodName}: Skipping purse {purse.PurseNumber} - empty consumer list");
                                continue;
                            }
                        }
                    }
                    if (periodConfig == null)
                    {
                        _logger.LogInformation($"{className}.{methodName}: Skip processing for this purse: {purse.PurseNumber} because purseConfig is not set to the purse in tenant account");
                        continue;
                    }
                    //if interval is ADHOC, execute the card 60 creation process without checking justInTimeFunding or FundDate
                    if (!string.Equals(periodConfig.Interval, BenefitsConstants.AdhocPeriodType, StringComparison.OrdinalIgnoreCase))
                    {
                        //if interval is not ADHOC and justInTimeFunding is ENABLED, then execute card 60 creation only if current date is equal to fundDate-1
                        //Example: todayUtcDate = 28/02/2025 and fundDate = 01 (March 1st). (todayUtcDate + 1 == fundDate) evaluates to true
                        if (justInTimeFunding && (todayUtcDate.AddDays(1).Day != periodConfig?.FundDate))
                        {
                            _logger.LogInformation($"{className}.{methodName}: Skip processing for this purse: {purse.PurseNumber} because today is not the fund date-1 : {periodConfig?.FundDate}");
                            continue;
                        }
                        //if interval is not ADHOC and justInTimeFunding is DISABLED, then execute card 60 creation only if current date is equal to fundDate
                        if (!justInTimeFunding && todayUtcDate.Day != periodConfig.FundDate)
                        {
                            _logger.LogInformation($"{className}.{methodName}: Skip processing for this purse: {purse.PurseNumber} because today is not the fund date : {periodConfig?.FundDate}");
                            continue;
                        }
                    }
                    var applyDate = GetApplyDate(periodConfig, todayUtcDate, justInTimeFunding);
                    if (string.IsNullOrEmpty(applyDate))
                    {
                        _logger.LogInformation($"{className}.{methodName}: Skip processing for this purse: {purse.PurseNumber} due to invalid apply date config, period config: {periodConfig?.ToJson()}");
                        continue;
                    }
                    int skip = 0;
                    int take = DISBURSEMENT_RECORDS_CHUNK_SIZE;
                    int max = MAX_DISBURSEMENT_RECORDS;
                    if (disbursementRecordsList.Count >= max)
                    {
                        break;
                    }
                    var walletType = await _walletTypeRepo.FindOneAsync(x => x.WalletTypeCode == purse.WalletType && x.DeleteNbr == 0);
                    if (walletType == null)
                    {
                        _logger.LogWarning($"{className}.{methodName}: Purse Wallet type not found. WalletTypeCode: {purse.WalletType}, Purse Number: {purse.PurseNumber}, Error Code:{StatusCodes.Status404NotFound}");
                        continue;
                    }

                    var masterRedemptionWalletType = await _walletTypeRepo.FindOneAsync(x => x.WalletTypeCode == purse.MasterRedemptionWalletType && x.DeleteNbr == 0);
                    if (masterRedemptionWalletType == null)
                    {
                        _logger.LogWarning($"{className}.{methodName}: Master Redemption Wallet type not found. WalletTypeCode: {purse.MasterRedemptionWalletType}, Purse Number: {purse.PurseNumber}, Error Code:{StatusCodes.Status404NotFound}");
                        continue;
                    }
                    var masterRedemptionWallet = new Core.Domain.Models.ETLWalletModel();
                    var rewardRemdeptionWalletTypeCode = GetRedemptionWalletTypeCode();
                    if (purse.MasterRedemptionWalletType == rewardRemdeptionWalletTypeCode)
                    {
                        masterRedemptionWallet = await _walletRepo.FindOneAsync(x => x.WalletTypeId == masterRedemptionWalletType.WalletTypeId &&
                                    x.TenantCode == etlExecutionContext.TenantCode && x.WalletName == BenefitsConstants.RewardSuspenseWalletName && x.MasterWallet && x.DeleteNbr == 0);
                    }
                    else
                    {
                        masterRedemptionWallet = await _walletRepo.FindOneAsync(x => x.WalletTypeId == masterRedemptionWalletType.WalletTypeId &&
                            x.TenantCode == etlExecutionContext.TenantCode && x.MasterWallet && x.DeleteNbr == 0);
                    }
                    if (masterRedemptionWallet == null || masterRedemptionWallet.WalletTypeId == 0)
                    {
                        _logger.LogWarning($"{className}.{methodName}: Master Redemption Wallet not found. WalletTypeCode: {purse.MasterRedemptionWalletType}, Purse Number: {purse.PurseNumber}, Error Code:{StatusCodes.Status404NotFound}");
                        continue;
                    }

                    while (true)
                    {
                        var consumersAndWallets = _consumerRepo.GetConsumersAndWalletsByWalletTypeId(etlExecutionContext.TenantCode,
                            walletType.WalletTypeId, skip, take, consumerCodes);
                        if (!consumersAndWallets.Any())
                        {
                            break;
                        }
                        foreach (var consumersAndWallet in consumersAndWallets)
                        {
                            try
                            {
                                var consumerWallet = consumersAndWallet.Wallet;
                                var consumer = consumersAndWallet.Consumer;
                                var input = purseConsumers.FirstOrDefault(x => x.ConsumerCode == consumer?.ConsumerCode);

                                var consumerAccountData = await _consumerAccountRepo.FindOneAsync(x =>
                                                               x.TenantCode == etlExecutionContext.TenantCode &&
                                                               x.ConsumerCode == consumer.ConsumerCode &&
                                                               x.DeleteNbr == 0);
                                // get enabled wallet types
                                var enabledWalletTypes = GetEnabledWalletTypes(consumerAccountData, fisPurses);

                                // If enabledWalletTypes is empty or ConsumerWalletType is not in the list, skip executing funding rules
                                bool shouldSkip = enabledWalletTypes == null || enabledWalletTypes?.Count == 0 || enabledWalletTypes != null && !enabledWalletTypes.Contains(purse.WalletType);

                                if (shouldSkip)
                                {
                                    _logger.LogError(
                                        "{ClassName}.{MethodName}: Consumer Wallet type not found in enabled wallet types with wallet type code:{code}. Skipping current consumer wallet. TenantCode: {TenantCode}, ConsumerCode: {ConsumerCode}",
                                        className, methodName, purse.WalletType, etlExecutionContext.TenantCode, consumer.ConsumerCode);
                                    continue;
                                }

                                if (consumerWallet?.Balance <= 0)
                                {
                                    _logger.LogInformation($"{className}.{methodName}: Consumer wallet does not have sufficient funds, Consumer: {consumer?.ConsumerCode}, Purse Number: {purse.PurseNumber}");
                                    continue;
                                }

                                if (input != null && (input.RedemptionAmount <= 0 || consumerWallet?.Balance < input.RedemptionAmount))
                                {
                                    _logger.LogWarning($"{className}.{methodName}: Wallet balance < redemption amount for {consumer?.ConsumerCode} with walletId: {consumerWallet?.WalletId}");
                                    continue;
                                }

                                var consumerAccount = await _consumerAccountRepo.FindOneAsync(x => x.TenantCode == etlExecutionContext.TenantCode &&
                                x.ConsumerCode == consumer.ConsumerCode && x.DeleteNbr == 0);
                                if (consumerAccount == null)
                                {
                                    _logger.LogInformation($"{className}.{methodName}: Consumer account not found, ConsuemrCode: {consumer?.ConsumerCode}, TenantCode: {etlExecutionContext.TenantCode}, Error Code:{StatusCodes.Status404NotFound}");
                                    continue;
                                }

                                var transactionCode = "txn-" + Guid.NewGuid().ToString("N");
                                var redemptionRequestDto = new RedemptionRequestDto
                                {
                                    TenantCode = etlExecutionContext.TenantCode,
                                    ConsumerCode = consumer.ConsumerCode,
                                    ConsumerWallet = consumerWallet,
                                    RedemptionWalletTypeCode = masterRedemptionWalletType.WalletTypeCode,
                                    RedemptionAmount = input != null ? input.RedemptionAmount : consumerWallet.Balance,
                                    RedemptionVendorCode = BenefitsConstants.RedemptionVendorCode + "_" + masterRedemptionWalletType.WalletTypeLabel,
                                    RedemptionRef = GenerateUniqueCode(),
                                    TransactionDetailType = BenefitsConstants.RedemptionTransactionDetailType,
                                    NewTransactionCode = transactionCode,
                                    MasterRedemptionWalletId = masterRedemptionWallet.WalletId,
                                    RedemptionItemDescription = input?.CustomTransactionDescription ?? string.Empty
                                };
                                var isRedemptionExecuted = await _fundTransferService.ExecuteRedemptionTransactionAsync(redemptionRequestDto);

                                if (!isRedemptionExecuted)
                                {
                                    _logger.LogInformation($"{className}.{methodName}: Redemption transaction failed,  Redemption Request: {redemptionRequestDto.ToJson()}");
                                    continue;
                                }

                                var cardDisbursementRecordDto = new FISCardAdditionalDisbursementRecordDto
                                {
                                    PaymentAmount = new decimal(input?.RedemptionAmount ?? consumerWallet.Balance).ToString("0.00"),
                                    Purse = purse.PurseNumber,
                                    ClientReferenceNumber = redemptionRequestDto.RedemptionRef,
                                    PANProxyClientUniqueID = consumerAccount.ProxyNumber,
                                    CustomTransactionDescription = input?.CustomTransactionDescription ?? FISBatchConstants.MerchantNameForFundTransfer
                                };
                                var disbursementRecord = _flatFileGenerator.GenerateFlatFileRecord(cardDisbursementRecordDto, FISCardAdditionalDisbursementRecordDto.FieldConfigurationMap);
                                disbursementRecordsList.Add(disbursementRecord);
                                disbursementRecordsTotal += input?.RedemptionAmount ?? consumerWallet.Balance;
                                if (disbursementRecordsList.Count >= max)
                                {
                                    break;
                                }

                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "{className}.{methodName}: Error occurred while creating disbursement record, ConsumerCode:{consumerCode}, Error Msg:{msg}", className, methodName, consumersAndWallet?.Consumer?.ConsumerCode, ex.Message);
                                continue;
                            }
                        }
                        skip += take;
                        if (disbursementRecordsList.Count >= max)
                        {
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "{className}.{methodName}: Error occurred while creating disbursement record, Purse wallet type:{walletType}, Error Msg:{msg}", className, methodName, purse.WalletType, ex.Message);
                    continue;
                }
            }

            if (consumersList != null && !string.IsNullOrEmpty(etlExecutionContext.ConsumerListFile))
            {
                await _awsS3Service.MoveFileFromProcessingToArchive(etlExecutionContext.ConsumerListFile);
            }

            return (disbursementRecordsList, disbursementRecordsTotal);
        }

        private static List<string>? GetEnabledWalletTypes(ETLConsumerAccountModel consumerAccount, List<FISPurseDto> fisPurses)
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

            if (fisPurses == null || !fisPurses.Any())
            {
                return null;
            }

            // Extract enabled consumer purse labels
            var enabledPurseLabels = consumerAccountConfig?.ConsumerAccountPurseConfigDto?.Purses
                .Where(purse => purse.Enabled)
                .Select(purse => purse.PurseLabel?.ToUpper())
                .ToHashSet();

            // Map tenant purses to wallet types based on enabled labels
            return fisPurses
                .Where(purse => purse?.PurseLabel != null && enabledPurseLabels.Contains(purse.PurseLabel.ToUpper()))
                .Select(purse => purse.WalletType!)
                .ToList();
        }
        private string? GetRedemptionWalletTypeCode()
        {
            return _configuration.GetSection("Redemption_Wallet_Type_Code").Value;
        }

        private string? GetApplyDate(PeriodConfigDto? periodConfig, DateTime todayUtcDate, bool justInTimeFunding)
        {
            try
            {
                var applyDateConfig = periodConfig?.ApplyDateConfig;
                if (applyDateConfig == null)
                {
                    return null;
                }
                if (string.Equals(periodConfig?.Interval, BenefitsConstants.AdhocPeriodType, StringComparison.OrdinalIgnoreCase)
                    || justInTimeFunding)
                {
                    return FISBatchConstants.APPLY_DATE;
                }
                DateTime applyDate;
                if (applyDateConfig?.ApplyDateType == ApplyDateType.AT_FUND_DATE.ToString())
                {
                    applyDate = todayUtcDate;
                }
                else if (applyDateConfig?.ApplyDateType == ApplyDateType.NEXT_MONTH.ToString())
                {
                    int day = applyDateConfig.ApplyDate;
                    int month = todayUtcDate.Month == 12 ? 1 : todayUtcDate.Month + 1;
                    int year = todayUtcDate.Month == 12 ? todayUtcDate.Year + 1 : todayUtcDate.Year;

                    applyDate = new DateTime(year, month, day);
                }
                else
                {
                    return null;
                }
                return applyDate.ToString("yyyyMMdd");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{className}.GetApplyDate: Invalid Apply date, ErrorMessage - {error}", className, ex.Message);
                return null;
            }

        }

        private string GenerateUniqueCode()
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] uniqueBytes = Guid.NewGuid().ToByteArray();

                byte[] hashBytes = sha256.ComputeHash(uniqueBytes);

                string base64String = Convert.ToBase64String(hashBytes);

                // Replace special characters with an empty string
                string result = Regex.Replace(base64String, BenefitsConstants.RegexForAlphaNumeric, "");
                string uniqueCode = result.Substring(0, 25);

                return uniqueCode;
            }
        }



    }
}
