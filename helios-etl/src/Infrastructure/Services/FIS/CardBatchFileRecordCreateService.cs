using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SunnyBenefits.Fis.Core.Domain.Dtos;
using SunnyRewards.Helios.Cohort.Core.Domain.Dtos;
using SunnyRewards.Helios.Cohort.Core.Domain.Models;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.ETL.Common.Constants;
using SunnyRewards.Helios.ETL.Common.Helpers.Interfaces;
using SunnyRewards.Helios.ETL.Core.Domain.Constants;
using SunnyRewards.Helios.ETL.Core.Domain.Constants.FIS;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos.Enums;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos.FIS;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos.Json;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces.FIS;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos.Json;
using SunnyRewards.Helios.Tenant.Core.Domain.Models;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using System.Linq.Dynamic.Core;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services.FIS
{
    public partial class CardBatchFileRecordCreateService : ICardBatchFileRecordCreateService
    {
        private static long MAX_SSN = 1000000000;

        private readonly ILogger<CardBatchFileRecordCreateService> _logger;
        private readonly IPersonRepo _personRepo;
        private readonly ITenantAccountRepo _tenantAcountRepo;
        private readonly IFlatFileGenerator _flatFileGenerator;
        private readonly IConfiguration _configuration;
        private FISAccLoadDto? _fISAccLoadDto;
        private IConsumerRepo _consumerRepo;
        private IConsumerAccountRepo _consumerAccountRepo;
        private IPersonAddressRepo _personAddressRepo;
        private IPhoneNumberRepo _phoneNumberRepo;
        private ITenantProgramConfigRepo _tenantProgramConfigRepo;
        private ICohortRepo _cohortRepo;
        private ICohortConsumerRepo _cohortConsumerRepo;

        const string className = nameof(CardBatchFileRecordCreateService);

        public CardBatchFileRecordCreateService(ILogger<CardBatchFileRecordCreateService> logger,
            IPersonRepo personRepo, ITenantAccountRepo tenantAcountRepo,
            IFlatFileGenerator flatFileGenerator, IConfiguration configuration, IConsumerRepo consumerRepo, IConsumerAccountRepo consumerAccountRepo, IPersonAddressRepo personAddressRepo, IPhoneNumberRepo phoneNumberRepo, ITenantProgramConfigRepo tenantProgramConfigRepo, ICohortRepo cohortRepo, ICohortConsumerRepo cohortConsumerRepo)
        {
            _logger = logger;
            _personRepo = personRepo;
            _tenantAcountRepo = tenantAcountRepo;
            _flatFileGenerator = flatFileGenerator;
            _configuration = configuration;
            _consumerRepo = consumerRepo;
            _consumerAccountRepo = consumerAccountRepo;
            _personAddressRepo = personAddressRepo;
            _phoneNumberRepo = phoneNumberRepo;
            _tenantProgramConfigRepo = tenantProgramConfigRepo;
            _cohortRepo = cohortRepo;
            _cohortConsumerRepo = cohortConsumerRepo;
        }

        public string GenerateFileHeader(EtlExecutionContext etlExecutionContext)
        {
            const string methodName = nameof(GenerateFileHeader);
            if (_fISAccLoadDto == null)
            {
                _logger.LogError("{className}.{methodName}: Cannot create FIS card load file header for tenant: {tenantCode} as config not provided", className, methodName,
                    etlExecutionContext.TenantCode);
                return FISBatchConstants.INVALID_RECORD;
            }

            var now = DateTime.UtcNow;
            long companyId = !string.IsNullOrEmpty(_fISAccLoadDto.CompanyId) ? long.Parse(_fISAccLoadDto.CompanyId) : 0;

            var fISFileHeaderDto = new FISFileHeaderDto()
            {
                FileDate = now.ToString("yyyyMMdd"),
                FileTime = now.ToString("HHmmss"),
                CompanyIDExtended = companyId
            };

            var rec = _flatFileGenerator.GenerateFlatFileRecord(fISFileHeaderDto, FISFileHeaderDto.FieldConfigurationMap);
            return rec;
        }

        public string GenerateBatchHeader(EtlExecutionContext etlExecutionContext, long batchSequence, string? proxyIndicatorProcessing = null, string? generateClientUniqueID = null)
        {
            if (_fISAccLoadDto == null)
            {
                _logger.LogError("{className}.GenerateBatchHeader: Cannot create FIS card load batch header for tenant: {tenantCode} as config not provided", className,
                    etlExecutionContext.TenantCode);
                return FISBatchConstants.INVALID_RECORD;
            }

            var clientId = !string.IsNullOrEmpty(_fISAccLoadDto.ClientId) ? long.Parse(_fISAccLoadDto.ClientId) : 0;
            var subprogramId = !string.IsNullOrEmpty(_fISAccLoadDto.SubprogramId) ? long.Parse(_fISAccLoadDto.SubprogramId) : 0;
            var packageId = !string.IsNullOrEmpty(_fISAccLoadDto.PackageId) ? long.Parse(_fISAccLoadDto.PackageId) : 0;

            var fISBatchHeaderDto = new FISBatchHeaderDto()
            {
                BatchSequence = batchSequence,
                ClientIDExtended = clientId,
                SubprogramIDExtended = subprogramId,
                PackageIDExtended = packageId,
                ProxyIndicatorProcessing = FISBatchConstants.FILE_HEADER_PROXY_INDICATOR,
            };

            if (!string.IsNullOrEmpty(proxyIndicatorProcessing))
            {
                fISBatchHeaderDto.ProxyIndicatorProcessing = proxyIndicatorProcessing;
            }
            if (!string.IsNullOrEmpty(generateClientUniqueID))
            {
                fISBatchHeaderDto.GenerateClientUniqueID = generateClientUniqueID;
            }

            var rec = _flatFileGenerator.GenerateFlatFileRecord(fISBatchHeaderDto, FISBatchHeaderDto.FieldConfigurationMap);
            return rec;
        }

        public string GenerateBatchTrailer(EtlExecutionContext etlExecutionContext, long batchSequence,
            long totalRecords, double totalDebit, double totalCredit)
        {
            const string methodName = nameof(GenerateBatchTrailer);
            if (_fISAccLoadDto == null)
            {
                _logger.LogError("{className}.{methodName}: Cannot create FIS card load batch trailer for tenant: {tenantCode} as config not provided", className, methodName,
                    etlExecutionContext.TenantCode);
                return FISBatchConstants.INVALID_RECORD;
            }

            var fisBatchTrailerDto = new FISBatchTrailerDto()
            {
                BatchSequence = batchSequence,
                TotalRecords = totalRecords,
                TotalCredit = new decimal(totalCredit).ToString("0.00"),
                TotalDebit = new decimal(totalDebit).ToString("0.00"),
                TotalProcessed = string.Empty,
                TotalRejected = string.Empty,
                ValueProcessed = string.Empty,
                ValueRejected = string.Empty,
                TotalCashout = string.Empty,
                TotalEscheated = string.Empty
            };

            var rec = _flatFileGenerator.GenerateFlatFileRecord(fisBatchTrailerDto, FISBatchTrailerDto.FieldConfigurationMap);

            _logger.LogInformation("{className}.{methodName}: FIS Batch trailer record generated: Tenant: {tenantCode}, " +
                "Batch seq: {batchSequence}, " +
                "Total records: {totalRec}, Total debit: {totalDebit}", className, methodName, etlExecutionContext.TenantCode,
                batchSequence, totalRecords, totalDebit);

            return rec;
        }

        public string GenerateFileTrailer(EtlExecutionContext etlExecutionContext,
            long totalRecords, long batchCount, long detailCount, double totalDebit, double totalCredit)
        {
            const string methodName = nameof(GenerateFileTrailer);
            if (_fISAccLoadDto == null)
            {
                _logger.LogError("{className}.{methodName}: Cannot create FIS card load file trailer for tenant: {tenantCode} as config not provided", className, methodName,
                    etlExecutionContext.TenantCode);
                return FISBatchConstants.INVALID_RECORD;
            }

            var fisFileTrailerDto = new FISFileTrailerDto()
            {
                TotalRecords = totalRecords,
                BatchCount = batchCount,
                DetailCount = detailCount,
                TotalCredit = new decimal(totalCredit).ToString("0.00"),
                TotalDebit = new decimal(totalDebit).ToString("0.00"),
                TotalProcessed = string.Empty,
                TotalRejected = string.Empty,
                ValueProcessed = string.Empty,
                ValueRejected = string.Empty,
                TotalCashout = string.Empty,
                TotalEscheated = string.Empty
            };

            var rec = _flatFileGenerator.GenerateFlatFileRecord(fisFileTrailerDto, FISFileTrailerDto.FieldConfigurationMap);

            _logger.LogInformation("{className}.{methodName}: FIS Batch trailer record generated: Tenant: {tenantCode}, " +
                "Total records: {totalRec}, Total debit: {totalDebit}", className, methodName, etlExecutionContext.TenantCode,
                totalRecords, totalDebit);

            return rec;
        }

        public async Task<List<string>> GenerateCardHolderData(EtlExecutionContext etlExecutionContext, int skip, int take, TenantOption tenantOption)
        {
            List<string> result = new();
            var persons = _personRepo.GetConsumerPersons(etlExecutionContext.TenantCode, skip, take, tenantOption, etlExecutionContext.CohortCode);
            // loop through valid person records and generate a 30 record for each
            foreach (var person in persons)
            {
                var (req, res) = GenerateCreateCardRecord(person);
                result.Add(res);
                result.Add(GenerateAdditionalCardHolderRecord(person));
                if (tenantOption?.BenefitsOptions?.IncludeDiscretionaryCardData == true)
                {
                    var additionalCardCarrierRes = await GenerateFISCardHolderAdditionalCardCarrierDataRecord(person, etlExecutionContext.TenantCode);
                    if (!string.IsNullOrEmpty(additionalCardCarrierRes))
                    {
                        result.Add(additionalCardCarrierRes);
                    }
                    else
                    {
                        _logger.LogInformation($"Additional Carrier detail is black for person : {person.PersonId}");
                    }
                }
                await SetFisBatchSentStatus(person.PersonId, etlExecutionContext.TenantCode);
            }
            return result;
        }

        public async Task<List<string>> UpdateCardHolderData(EtlExecutionContext etlExecutionContext,int take)
        {
            const string methodName = nameof(UpdateCardHolderData);
            var result = new List<string>();

            // Retrieving consumer accounts
            var consumerAccounts = _consumerAccountRepo.GetConsumerAccounts(etlExecutionContext.TenantCode,take);

            if (!consumerAccounts.Any())
            {
                _logger.LogWarning("{className}.{methodName}: No Updated Consumer Accounts Found For TenantCode: {TenantCode}", className, methodName, etlExecutionContext.TenantCode);
                return result;
            }

            foreach (var consumerAccount in consumerAccounts)
            {
                // Deserializing sync info JSON
                var syncInfo = consumerAccount.SyncInfoJson != null
                    ? JsonConvert.DeserializeObject<SyncInfoJson>(consumerAccount.SyncInfoJson)
                    : new SyncInfoJson { SyncOptions = new List<string>() };

                if (syncInfo?.SyncOptions == null || !syncInfo.SyncOptions.Any())
                {
                    _logger.LogWarning("{className}.{methodName}: SyncInfoJson or SyncOptions is Null or Empty For TenantCode: {TenantCode} and ConsumerCode: {ConsumerCode}", className, methodName, etlExecutionContext.TenantCode, consumerAccount.ConsumerCode);
                    continue;
                }

                // Retrieving Person
                var person = consumerAccount.ConsumerCode != null
                    ? _personRepo.GetConsumerPersonForUpdateInfo(etlExecutionContext.TenantCode, consumerAccount.ConsumerCode)
                    : null;

                if (person?.PersonId <= 0 || person == null)
                {
                    _logger.LogWarning("{className}.{methodName}: Person Not Found For TenantCode: {TenantCode} and ConsumerCode: {ConsumerCode}", className, methodName, etlExecutionContext.TenantCode, consumerAccount.ConsumerCode);
                    continue;
                }

                // Generate cards based on changes
                ProcessCardGeneration(syncInfo.SyncOptions, person, consumerAccount, result);

                // Update consumer account
                if (result.Any())
                {
                    consumerAccount.SyncRequired = false;
                    consumerAccount.SyncInfoJson = null;
                    consumerAccount.UpdateUser = Constants.UpdateUser;
                    var updatedConsumerAccount = await _consumerAccountRepo.UpdateConsumerAccount(consumerAccount);
                    _logger.LogInformation("{className}.{methodName}: UpdateCardHolderData is updated in consumerAccount History table with: {ConsumerCode}", className, methodName, consumerAccount.ConsumerCode);
                    if (updatedConsumerAccount == null)
                    {
                        _logger.LogError("{className}.{methodName}: Error Occurred While Updating ConsumerAccount With TenantCode: {TenantCode} and ConsumerCode: {ConsumerCode}", className, methodName, etlExecutionContext.TenantCode, consumerAccount.ConsumerCode);
                        continue;
                    }
                    _logger.LogInformation("{className}.{methodName}: Successfully Updated Consumer Account! TenantCode: {TenantCode} and ConsumerCode: {ConsumerCode}", className, methodName, etlExecutionContext.TenantCode, consumerAccount.ConsumerCode);
                }
            }
            return result;
        }

        private void ProcessCardGeneration(List<string> syncOptions, ETLPersonModel person, ETLConsumerAccountModel consumerAccount, List<string> result)
        {
            const string methodName = nameof(ProcessCardGeneration);
            bool isNameChanged = syncOptions.Contains(SyncOptions.NAME_CHANGE.ToString());
            bool isAddressChanged = syncOptions.Contains(SyncOptions.ADDRESS_CHANGE.ToString());
            bool isDOBChanged = syncOptions.Contains(SyncOptions.DOB_CHANGE.ToString());

            if (isAddressChanged)
            {
                _logger.LogInformation("{className}.{methodName}: Generating Card30 for ConsumerCode: {ConsumerCode}", className, methodName, consumerAccount.ConsumerCode);
                var (req, res) = GenerateCreateCardRecord(person, true);
                result.Add(res);
                _logger.LogInformation("{className}.{methodName}: Generated Card30 for ConsumerCode: {ConsumerCode}", className, methodName, consumerAccount.ConsumerCode);
            }

            if (isNameChanged || isDOBChanged)
            {
                _logger.LogInformation("{className}.{methodName}: Generating Card31 for ConsumerCode: {ConsumerCode}", className, methodName, consumerAccount.ConsumerCode);
                var res = GenerateAdditionalCardHolderRecord(person, true, isNameChanged);
                result.Add(res);
                _logger.LogInformation("{className}.{methodName}: Generated Card31 for ConsumerCode: {ConsumerCode}", className, methodName, consumerAccount.ConsumerCode);
            }
        }

        public async Task<string> Init(EtlExecutionContext etlExecutionContext)
        {
            bool def = false;

            var cohort = await _cohortRepo.FindOneAsync(x => x.CohortCode == etlExecutionContext.CohortCode && x.DeleteNbr == 0);
            var tenantProgramConfig = await _tenantProgramConfigRepo.FindOneAsync(x => x.TenantCode == etlExecutionContext.TenantCode && x.DeleteNbr == 0);

            if (tenantProgramConfig == null)
            {
                def = true;
            }
            else
            {
                var packageIdMapping = !string.IsNullOrWhiteSpace(tenantProgramConfig?.PackageIdMapping)
                    ? JsonConvert.DeserializeObject<List<PackageIdMappingJson>>(tenantProgramConfig.PackageIdMapping) ?? new()
                    : new List<PackageIdMappingJson>();

                string? selectedPackageId = null;

                var cohortKey = cohort?.CohortName?.Trim();

                if (!string.IsNullOrWhiteSpace(cohortKey))
                {
                    // find first rule whose cohorts contains the cohort name
                    var match = packageIdMapping.FirstOrDefault(r =>
                        r.Cohorts?.Any(c => string.Equals(c?.Trim(), cohortKey, StringComparison.OrdinalIgnoreCase)) == true);

                    if (!string.IsNullOrWhiteSpace(match?.PackageId))
                        selectedPackageId = match.PackageId;
                }

                // Fallbacks: rule without cohorts
                if (string.IsNullOrWhiteSpace(selectedPackageId))
                {
                    selectedPackageId =
                        packageIdMapping.FirstOrDefault(r => (r.Cohorts == null || r.Cohorts.Count == 0) && !string.IsNullOrWhiteSpace(r.PackageId))
                            ?.PackageId
                        ?? "";
                }
                if (string.IsNullOrWhiteSpace(selectedPackageId))
                {
                    def = true;
                }
                else
                {
                    _fISAccLoadDto = new FISAccLoadDto()
                    {
                        CompanyId = tenantProgramConfig.CompanyId,
                        ClientId = tenantProgramConfig.ClientId,
                        SubprogramId = tenantProgramConfig.SubprogramId,
                        PackageId = selectedPackageId
                    };
                }
            }

            if (def)
            {
                _logger.LogError("{className}.Init: Cannot find FIS Tenant Config for Tenant: {tenantCode}",className, etlExecutionContext.TenantCode);

                _fISAccLoadDto = new FISAccLoadDto()
                {
                    CompanyId = "1204185",
                    ClientId = "1217824",
                    SubprogramId = "872817",
                    PackageId = "722896"
                };
            }

            return _fISAccLoadDto.SubprogramId;
        }

        private (FISCardHolderDataDto, string) GenerateCreateCardRecord(ETLPersonModel person, bool isUpdateFISInfo = false)
        {
            var personAddress = _personAddressRepo.GetPrimaryMailingAddress(person.PersonId).FirstOrDefault();
            var phoneNumber = _phoneNumberRepo.GetPrimaryPhoneNumber(person.PersonId).FirstOrDefault();
            var fisCardHolderDto = new FISCardHolderDataDto()
            {
                FirstName = person.FirstName,
                LastName = person.LastName,
                SSN = GenerateSSN(person.PersonId),
                MailingAddr1 = personAddress?.Line1,
                MailingAddr2 = personAddress?.Line2,
                MailingCity = personAddress?.City,
                MailingState = personAddress?.State,
                MailingPostalCode = personAddress?.PostalCode,
                MailingCountryCode = personAddress?.CountryCode,
                HomeNumber = string.IsNullOrWhiteSpace(phoneNumber?.PhoneNumber)
                                ? "9999999999"
                                : DigitsOnly().Replace(phoneNumber.PhoneNumber, ""),
                DeliveryMethod = GetDeliveryMethod(person.PersonId)
            };

            if (isUpdateFISInfo)
            {
                fisCardHolderDto.ActionType = FISBatchConstants.UPDATE_FIS_INFO_ACTION_TYPE;
            }
            var rec = _flatFileGenerator.GenerateFlatFileRecord(fisCardHolderDto, FISCardHolderDataDto.FieldConfigurationMap);
            return (fisCardHolderDto, rec);
        }

        private string GenerateAdditionalCardHolderRecord(ETLPersonModel person, bool isUpdateFISInfo = false, bool nameChange = false)
        {
            var fisCardHolderAdditionalDto = new FISCardHolderAdditionalDataDto()
            {
                LastName = person.LastName,
                SSN = GenerateSSN(person.PersonId),
                DOB = person.DOB.HasValue ? person.DOB.Value.ToString("yyyyMMdd") : string.Empty,
                EmailAddress = person.Email,
                DeliveryMethod = GetDeliveryMethod(person.PersonId)
            };

            if (isUpdateFISInfo)
            {
                fisCardHolderAdditionalDto.ActionType = FISBatchConstants.UPDATE_FIS_INFO_ADDITIONAL_ACTION_TYPE;
                if (nameChange)
                {
                    fisCardHolderAdditionalDto.NameOnCard = person.FirstName + " " + person.LastName;
                }
            }
            var rec = _flatFileGenerator.GenerateFlatFileRecord(fisCardHolderAdditionalDto, FISCardHolderAdditionalDataDto.FieldConfigurationMap);
            return rec;
        }

        [GeneratedRegex("[^0-9]")]
        private static partial Regex DigitsOnly();

        private string GenerateSSN(long personId)
        {
            if (!long.TryParse(_configuration.GetSection("SSNStartFrom").Value, out var sSNStartFrom))
            {
                throw new InvalidOperationException("Invalid configuration value for SSNStartFrom");
            }
            var ssn = (personId % sSNStartFrom) + sSNStartFrom;
            return Convert.ToString(ssn);
        }

        private int GetDeliveryMethod(long personId)
        {
            var consumer = _consumerRepo.FindOneAsync(x => x.PersonId == personId && x.DeleteNbr == 0).Result;
            if (consumer == null)
            {
                _logger.LogError($"Cannot find consumer with person Id: {personId}");
                return FISBatchConstants.CARD_CREATE_DELIVERY_FIRST_CLASS;
            }
            var consumerAttr = !string.IsNullOrEmpty(consumer?.ConsumerAttribute)
                ? JsonConvert.DeserializeObject<ConsumerAttributeDto>(consumer?.ConsumerAttribute)
                : null;
            if (consumerAttr == null || consumerAttr.BenefitsCardOptions == null
                || consumerAttr.BenefitsCardOptions.CardCreateOptions == null
                || !FISBatchConstants.DELIVERY_METHODS.ContainsValue(consumerAttr.BenefitsCardOptions.CardCreateOptions.DeliveryMethod))
                return FISBatchConstants.CARD_CREATE_DELIVERY_FIRST_CLASS;

            return consumerAttr.BenefitsCardOptions.CardCreateOptions.DeliveryMethod;
        }

        private async Task SetFisBatchSentStatus(long personId, string tenantCode)
        {
            var consumer = await _personRepo.GetConsumerByPersonIdAndTenantCode(personId, tenantCode);
            if (consumer != null)
            {
                var consumerAccount = _consumerAccountRepo.FindOneAsync(x => x.ConsumerCode == consumer.ConsumerCode && x.DeleteNbr == 0).Result;
                if (consumerAccount != null)
                {
                    consumerAccount.CardIssueStatus = BenefitsConstants.Card30BatchSentStatus;
                    await _consumerAccountRepo.UpdateConsumerAccount(consumerAccount);
                }
            }
        }

        private async Task<string> GenerateFISCardHolderAdditionalCardCarrierDataRecord(ETLPersonModel person , string tenantCode)
        {

            var tenantProgram =await _tenantProgramConfigRepo.FindOneAsync(x=> x.TenantCode == tenantCode && x.DeleteNbr == 0);
            if (tenantProgram == null)
            {
                _logger.LogError($"Cannot find tenantProgram with tenant: {tenantCode}");
                return string.Empty;
            }


            var consumer = await _consumerRepo.FindOneAsync(x => x.PersonId == person.PersonId && x.DeleteNbr == 0);
            if (consumer == null)
            {
                _logger.LogError($"Cannot find consumer with person Id: {person.PersonId}");
                return string.Empty;
            }

            var Discretionary1Value = GetDiscretaryData1ForConsumer(consumer, tenantProgram.DiscreteDataConfig);

            if (!string.IsNullOrEmpty(Discretionary1Value))
            {
                _logger.LogInformation($"Discretionary1Value : {Discretionary1Value} for conssumer : {consumer.ConsumerCode}, planId : {consumer.PersonId}");

                var fisCardHolderAdditionalCardCarrierDataDto = new FISCardHolderAdditionalCardCarrierDataDto()
                {
                    LastName = person.LastName,
                    SSN = GenerateSSN(person.PersonId),
                    DiscretionaryData1 = Discretionary1Value
                };

                var rec = _flatFileGenerator.GenerateFlatFileRecord(fisCardHolderAdditionalCardCarrierDataDto, FISCardHolderAdditionalCardCarrierDataDto.FieldConfigurationMap);
                return rec;
            }
            return string.Empty;
        }


        public string? GetDiscretaryData1ForConsumer(ETLConsumerModel consumer, string json)
        {
            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var root = System.Text.Json.JsonSerializer.Deserialize<DicretnaryDataRoot>(json, options);

                if (root?.DicretnaryDataMap == null)
                    return string.Empty;

                string defaultValue = string.Empty;

                foreach (var item in root.DicretnaryDataMap)
                {
                    if (item.ConsAttr == null)
                        continue;

                    bool allConditionsMatch = item.ConsAttr.All(condition =>
                    {
                        foreach (var keyValue in condition)
                        {
                            if (keyValue.Key.Equals("default", StringComparison.OrdinalIgnoreCase))
                            {
                                // set the default value to defaultValue
                                defaultValue = item.DiscretaryData1;
                                return false;
                            }

                            // Normalize property path
                            var propertyPath = keyValue.Key.Replace("consumer.", "", StringComparison.OrdinalIgnoreCase);
                            var actualValue = GetPropertyValue(consumer, propertyPath);
                            var expectedValue = keyValue.Value;

                            if (actualValue == null && expectedValue != null)
                                return false;

                            // if we have achual value, convert to string and comapare
                            if (actualValue != null &&
                                !string.Equals(actualValue.ToString(), expectedValue?.ToString(), StringComparison.OrdinalIgnoreCase))
                            {
                                return false;
                            }
                        }

                        return true;
                    });

                    if (allConditionsMatch)
                        return item.DiscretaryData1;
                }
                return defaultValue;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in getting discreate data for the consumer with consumerCode : {consumer.ConsumerCode}");
                return string.Empty;
            }
        }



        private static object? GetPropertyValue(object obj, string path)
        {
            if (obj == null || string.IsNullOrWhiteSpace(path)) return null;
            object? current = obj;

            // Special handling for ConsumerAttribute JSON
            if (path.Equals("ConsumerAttribute.ssbci", StringComparison.OrdinalIgnoreCase))
            {
                if (current is ETLConsumerModel consumerDto)
                {
                    // Return the JSON string (so the next loop part can process)
                    var cattr = consumerDto.ConsumerAttribute;
                    current = GetIsSsbciFlag(cattr);
                    return current;
                }
            }

            foreach (var part in path.Split('.'))
            {
                if (current == null) return null;

                var type = current.GetType();
                var prop = type.GetProperty(part,
                    BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                if (prop == null) return null;

                current = prop.GetValue(current);
            }
            return current;
        }

        //if falg is not there/ or  parse error -- return false
        private static bool GetIsSsbciFlag(string consumerAttributJson)
        {
            if (string.IsNullOrWhiteSpace(consumerAttributJson))
                return false;

            try
            {
                var obj = JObject.Parse(consumerAttributJson);

                // If property not found → default false
                var token = obj["is_ssbci"];
                return token?.Value<bool>() ?? false;
            }
            catch
            {
                return false;
            }
        }


    }
}
