using FluentNHibernate.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NHibernate.Loader.Custom;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using SunnyRewards.Helios.ETL.Common.Constants;
using SunnyRewards.Helios.ETL.Common.CustomException;
using SunnyRewards.Helios.ETL.Common.Extensions;
using SunnyRewards.Helios.ETL.Common.Helpers.Interfaces;
using SunnyRewards.Helios.ETL.Core.Domain.Constants;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos.Json;
using SunnyRewards.Helios.ETL.Core.Domain.Enums;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using SunnyRewards.Helios.ETL.Infrastructure.AwsConfig;
using SunnyRewards.Helios.ETL.Infrastructure.Helpers.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos.Json;
using SunnyRewards.Helios.Tenant.Core.Domain.Enum;
using System.Transactions;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services
{
    /// <summary>
    /// Sweepstakes Import Service
    /// </summary>
    public class SweepstakesImportService : AwsConfiguration, ISweepstakesImportService
    {
        private readonly ILogger<SweepstakesImportService> _logger;
        private readonly ISweepstakesInstanceRepo _sweepstakesInstanceRepo;
        private readonly ITenantSweepstakesRepo _tenantSweepstakesRepo;
        private readonly IConsumerRepo _consumerRepo;
        private readonly IConsumerWalletRepo _consumerwalletRepo;
        private readonly IConsumerAccountRepo _consumerAccountRepo;
        private readonly IPersonRepo _personRepo;
        private readonly ITenantRepo _tenantRepo;
        private readonly IWalletTypeRepo _walletTypeRepo;
        private readonly IWalletRepo _walletRepo;
        private readonly ICsvWrapper _csvHelper;
        private readonly IS3Helper _s3Helper;
        private readonly ITransactionRepo _transactionRepo;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly NHibernate.ISession _session;

        private const string className = nameof(SweepstakesImportService);

        public SweepstakesImportService(
            ILogger<SweepstakesImportService> logger,
            IConfiguration configuration,
            IVault vault,
            ISweepstakesInstanceRepo sweepstakesInstanceRepo,
            ITenantSweepstakesRepo tenantSweepstakesRepo,
            IConsumerRepo consumerRepo,
            IWalletTypeRepo walletTypeRepo,
            ICsvWrapper csvHelper,
            IS3Helper s3Helper,
            ITenantRepo tenantRepo,
            IWalletRepo walletRepo,
            ITransactionRepo transactionRepo,
            IDateTimeHelper dateTimeHelper,
            IPersonRepo personRepo,
            IConsumerAccountRepo consumerAccountRepo,
            IConsumerWalletRepo consumerWalletRepo,
            NHibernate.ISession session
        ) : base(vault, configuration)
        {
            _logger = logger;
            _sweepstakesInstanceRepo = sweepstakesInstanceRepo;
            _tenantSweepstakesRepo = tenantSweepstakesRepo;
            _consumerRepo = consumerRepo;
            _walletTypeRepo = walletTypeRepo;
            _csvHelper = csvHelper;
            _s3Helper = s3Helper;
            _tenantRepo = tenantRepo;
            _walletRepo = walletRepo;
            _transactionRepo = transactionRepo;
            _dateTimeHelper = dateTimeHelper;
            _personRepo = personRepo;
            _consumerAccountRepo = consumerAccountRepo;
            _consumerwalletRepo = consumerWalletRepo;
            _session = session;
        }

        /// <summary>
        /// Sweeptakes Import
        /// </summary>
        public async Task<List<SweepstakesWalletBalancesReportDto>> GenerateSweepstakesEntriesReport(EtlExecutionContext etlExecutionContext)
        {
            const string methodName = nameof(GenerateSweepstakesEntriesReport);
            var response = new List<SweepstakesWalletBalancesReportDto>();
            var sweepstakesInstance = new ETLSweepstakesInstanceModel();

            var jobManager = new JobManager
            {
                JobName = "GenerateSweepstakesEntriesReport",
                JobRunId = Guid.NewGuid().ToString(),
                LastProcessedKey = "ConsumerWalletId",
                OutputArtifact = "S3File",
            };

            _logger.LogInformation(
                $"{className}.{methodName} - Started processing for SweepstakesInstanceId: {etlExecutionContext.SweepstakesInstanceId}, Format: {etlExecutionContext.Format}, WalletTypeCode: {etlExecutionContext.WalletTypeCode}"
            );

            if (!etlExecutionContext.Format.Equals(Constants.RealtimeMedia, StringComparison.OrdinalIgnoreCase))
            {
                jobManager.ErrorMessage = $"{Constants.Invalid} {Constants.MediaType}";
                _logger.LogError($"{className}.{methodName}: {Constants.Invalid} {Constants.MediaType}");
                throw new ETLException(ETLExceptionCodes.InValidValue, $"{Constants.Invalid} {Constants.MediaType}");
            }

            try
            {
                sweepstakesInstance = await _sweepstakesInstanceRepo.FindOneAsync(
                  x => x.SweepstakesInstanceId == etlExecutionContext.SweepstakesInstanceId && x.DeleteNbr == 0);

                if (sweepstakesInstance == null)
                {
                    jobManager.ErrorMessage = $"Invalid SweepstakesInstanceId:{etlExecutionContext.SweepstakesInstanceId}";
                    _logger.LogError("{Class}.{Method} - Invalid SweepstakesInstanceId:{Id}", className, methodName, etlExecutionContext.SweepstakesInstanceId);
                    throw new ETLException(ETLExceptionCodes.InValidValue, $"Invalid SweepstakesInstanceId:{etlExecutionContext.SweepstakesInstanceId}");
                }

                var tenantSweepstake = await _tenantSweepstakesRepo.FindOneAsync(
                   x =>
                        x.TenantSweepstakesId == sweepstakesInstance.TenantSweepstakesId &&
                        x.DeleteNbr == 0);

                if (tenantSweepstake == null)
                {
                    jobManager.ErrorMessage = $"Invalid SweepstakesId:{sweepstakesInstance.SweepstakesId}";
                    _logger.LogError("{Class}.{Method} - Invalid SweepstakesId:{Id}", className, methodName, sweepstakesInstance.SweepstakesId);
                    throw new ETLException(ETLExceptionCodes.InValidValue, $"Invalid SweepstakesId:{sweepstakesInstance.SweepstakesId}");
                }
                var walletType = await _walletTypeRepo.FindOneAsync(x => x.WalletTypeCode == etlExecutionContext.WalletTypeCode);
                var tenant = await _tenantRepo.FindOneAsync(x => x.TenantCode == tenantSweepstake.TenantCode);

                if (walletType == null)
                {
                    jobManager.ErrorMessage = $"Invalid Wallet type code:{etlExecutionContext.WalletTypeCode}";
                    _logger.LogError("{Class}.{Method} - Invalid Wallet type code:{Code}", className, methodName, etlExecutionContext.WalletTypeCode);
                    throw new ETLException(ETLExceptionCodes.InValidValue, $"Invalid Wallet type code:{etlExecutionContext.WalletTypeCode}");
                }

                if (tenant == null)
                {
                    jobManager.ErrorMessage = $"Invalid tenant code:{etlExecutionContext.TenantCode}";
                    _logger.LogError("{Class}.{Method} - Invalid tenant code:{Code}", className, methodName, etlExecutionContext.TenantCode);
                    throw new ETLException(ETLExceptionCodes.InValidValue, $"Invalid tenant code:{etlExecutionContext.TenantCode}");
                }

                var tenantAttr = JsonConvert.DeserializeObject<TenantAttributeDto>(tenant.TenantAttribute);

                var lastSweepstakesRan = await _sweepstakesInstanceRepo.GetLatestSweepstakesInstance(
                    tenant.TenantCode,
                    sweepstakesInstance.SweepstakesInstanceId,
                    tenantSweepstake.TenantSweepstakesId);

                if (lastSweepstakesRan != null && tenantAttr?.EntriesRule != null && lastSweepstakesRan.Status != Constants.SWEEPSTAKES_ENTRIES_REPORT_ERROR_STATUS &&
                    !IsValidFrequencyRange(lastSweepstakesRan.InstanceTs, sweepstakesInstance.InstanceTs, tenantAttr.EntriesRule.ResetFrequency))
                {
                    jobManager.ErrorMessage = $"Invalid Sweepstakes instance date frequency validation:{etlExecutionContext.SweepstakesInstanceId}";
                    _logger.LogError("{Class}.{Method} - Invalid Sweepstakes instance date frequency validation:{Id}", className, methodName, etlExecutionContext.SweepstakesInstanceId);

                    sweepstakesInstance.Status = Constants.SWEEPSTAKES_ENTRIES_REPORT_DUPLICATE_STATUS;
                    sweepstakesInstance.UpdateUser = Constants.UpdateUser;
                    sweepstakesInstance.UpdateTs = DateTime.UtcNow;
                    sweepstakesInstance.TenantSweepstakesId = tenantSweepstake.TenantSweepstakesId;


                    await _sweepstakesInstanceRepo.UpdateAsync(sweepstakesInstance);

                    return response;
                }

                if (sweepstakesInstance.Status != Constants.SWEEPSTAKES_ENTRIES_REPORT_STARTED_STATUS &&
                    sweepstakesInstance.Status != Constants.SWEEPSTAKES_ENTRIES_REPORT_ERROR_STATUS &&
                    sweepstakesInstance.Status != Constants.SWEEPSTAKES_ENTRIES_REPORT_DUPLICATE_STATUS)
                {
                    jobManager.ErrorMessage = $"Invalid sweepstakes Instance status:{sweepstakesInstance.Status} id:{sweepstakesInstance.SweepstakesInstanceId}";
                    _logger.LogError("{Class}.{Method} - Invalid sweepstakes Instance status: {Status} Id:{Id}",
                        className, methodName, sweepstakesInstance.Status, sweepstakesInstance.SweepstakesInstanceId);

                    throw new ETLException(
                        ETLExceptionCodes.InValidValue,
                        $"Invalid sweepstakes Instance status:{sweepstakesInstance.Status} id:{sweepstakesInstance.SweepstakesInstanceId}");
                }

                sweepstakesInstance.Status = Constants.SWEEPSTAKES_ENTRIES_REPORT_INPROGRESS_STATUS;
                await _sweepstakesInstanceRepo.UpdateAsync(sweepstakesInstance);

                int skip = 0;
                int take = etlExecutionContext.BatchSize;
                IQueryable<ETLConsumerWalletAggregate> batch;

                DateTime? cutoffTsUtc = null;
                if (!string.IsNullOrEmpty(etlExecutionContext.CutoffDate) && !string.IsNullOrEmpty(etlExecutionContext.CutoffTz))
                {
                    // Convert the cutoff date and time zone to UTC
                    cutoffTsUtc = _dateTimeHelper.GetUtcDateTime(etlExecutionContext.CutoffDate, etlExecutionContext.CutoffTz);
                }

                else if (lastSweepstakesRan != null)

                {
                    cutoffTsUtc = lastSweepstakesRan.InstanceTs;
                }

                var walletsToUpdate = new List<ETLWalletModel>();
                do
                {
                    batch = _consumerRepo.GetConsumersAndWalletsByWalletTypeIdByCutOffDate(
                        tenantSweepstake.TenantCode,
                        walletType.WalletTypeId,
                        skip,
                        take,
                        cutoffTsUtc ?? DateTime.UtcNow);

                    foreach (var item in batch)
                    {
                        try
                        {
                            if (item == null|| item.Consumer == null || item.Wallet == null || item.Person == null)
                                throw new ETLException(ETLExceptionCodes.InValidValue, "Invalid consumer or wallet in batch");

                            double entries;
                            double balance;

                            if (cutoffTsUtc != null)
                            {
                                var transactions = await _transactionRepo.FindAsync(x =>
                                    x.WalletId == item.Wallet.WalletId &&
                                    x.CreateTs >= cutoffTsUtc &&
                                    x.CreateTs <= DateTime.UtcNow);

                                var txAmount = transactions.Sum(x => x.TransactionAmount) ?? 0;
                                (entries, balance) = reportEntries(tenantAttr, txAmount);
                            }
                            else
                            {
                                (entries, balance) = reportEntries(tenantAttr, item.Wallet.Balance);
                            }

                            var person = item.Person;
                            var consumerAccount = item.ConsumerAccount;

                            var reportDto = new SweepstakesWalletBalancesReportDto
                            {
                                userId = item.Consumer.AnonymousCode,
                                entries = entries,
                                phoneNumber = person.PhoneNumber,
                                firstName = person.FirstName,
                                lastName = person.LastName,
                                secondaryPhoneNumber = person.HomePhoneNumber,
                                languageCode = person.LanguageCode,
                                cardOrdered =
                                    consumerAccount != null &&
                                    !string.IsNullOrEmpty(consumerAccount.CardIssueStatus) &&
                                    consumerAccount.CardIssueStatus.In(
                                        BenefitsConstants.EligibleForActivationCardIssueStatus,
                                        BenefitsConstants.EligibleCardIssueStatus,
                                        BenefitsConstants.IssuedCardRequestStatus)
                            };

                            if (entries > 0)
                                response.Add(reportDto);

                            if (cutoffTsUtc != null)
                                item.Wallet.Balance = balance;
                            else
                                item.Wallet.TotalEarned = balance;

                            item.Wallet.UpdateTs = DateTime.UtcNow;
                            item.Wallet.UpdateUser = Constants.UpdateUser;

                            //await _walletRepo.UpdateAsync(item.Wallet);
                            walletsToUpdate.Add(item.Wallet);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(
                                ex,
                                "{Class}.{Method} - Error processing batch for SweepstakesInstanceId:{Id}, Skip:{Skip}, Take:{Take}",
                                className, methodName, etlExecutionContext.SweepstakesInstanceId, skip, take);

                            
                            jobManager.failCollector.failedRecords.Add(
                                CreateFailedRecord(
                                    recordKey: item?.Wallet?.WalletId.ToString() ?? "UNKNOWN",
                                    recordPayload: item,
                                    exception: ex));

                            throw;
                        }
                    }

                    skip += take;

                } while (batch.Any());


                if (jobManager.failCollector.failedRecords.Count == 0)
                {
                    _session.Clear();
                    using var stateless = _session.SessionFactory.OpenStatelessSession();
                    using var tx = stateless.BeginTransaction();

                    try
                    {
                        foreach (var wallet in walletsToUpdate)
                        {
                            await stateless.UpdateAsync(wallet);
                        }

                        tx.Commit();
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }


                    var fileName = $"{Constants.Outbound}/rtm_sweepstakes_entries_report_{DateTime.UtcNow:yyyyMMdd_HHmmss}.{sweepstakesInstance.SweepstakesInstanceCode}.txt";
                    var sweepstakesSmtpS3BucketName = GetAwsSweepstakesSmtpS3BucketName();

                    await _s3Helper.UploadCsvFileToS3<SweepstakesWalletBalancesReportDto>(response, sweepstakesSmtpS3BucketName, fileName);
                    sweepstakesInstance.Status = Constants.SWEEPSTAKES_ENTRIES_REPORT_SUCCESS_STATUS;
                    await _sweepstakesInstanceRepo.UpdateAsync(sweepstakesInstance);
                    _logger.LogInformation("{Class}.{Method} - Completed processing.", className, methodName);
                }
            }


            catch (Exception ex)
            {
                if (sweepstakesInstance != null && sweepstakesInstance.SweepstakesInstanceId != 0)
                {
                    sweepstakesInstance.Status = Constants.SWEEPSTAKES_ENTRIES_REPORT_ERROR_STATUS;
                    await _sweepstakesInstanceRepo.UpdateAsync(sweepstakesInstance);
                }




                _logger.LogError(
                    ex,
                    "{Class}.{Method} - Failed processing, ErrorCode:{Code}, ERROR:{Msg}, ErrorObj :{}",
                    className,
                    methodName,
                    StatusCodes.Status500InternalServerError,
                    ex.Message, jobManager.ToJson()
                );

                throw;
            }

            return response;
        }

        public bool IsValidFrequencyRange(
     DateTime startDate,
     DateTime endDate,
     string frequencyString)
        {
            ResetFrequencyType frequency = ParseResetFrequency(frequencyString);
            return ValidateRange(startDate, endDate, frequency);
        }

        /// <summary>
        /// Maps tenant text values to enum type, removing spaces and normalizing
        /// </summary>
        private static ResetFrequencyType ParseResetFrequency(string tenantValue)
        {
            if (string.IsNullOrWhiteSpace(tenantValue))
                throw new ArgumentException("Reset frequency cannot be null or empty.", nameof(tenantValue));

            string normalized = tenantValue.Replace(" ", "", StringComparison.OrdinalIgnoreCase)
                                           .ToLowerInvariant();

            return normalized switch
            {
                "daily" => ResetFrequencyType.Daily,
                "weekly" => ResetFrequencyType.Weekly,
                "monthly" => ResetFrequencyType.Monthly,
                "every3months" => ResetFrequencyType.Quarterly,     // 3 months
                "every4months" => ResetFrequencyType.FourMonthly,    // 4 months
                "every6months" => ResetFrequencyType.HalfYearly,     // 6 months

                _ => throw new ArgumentOutOfRangeException(nameof(tenantValue),
                        $"Invalid reset frequency value: {tenantValue}")
            };
        }

        /// <summary>
        /// Validates the date rule for the frequency
        /// </summary>
        private static bool ValidateRange(DateTime lastRunDate, DateTime currentDate, ResetFrequencyType frequency)
        {
            lastRunDate = lastRunDate.Date;
            currentDate = currentDate.Date;

            switch (frequency)
            {
                case ResetFrequencyType.Daily:
                    // Can run next day only
                    return currentDate >= lastRunDate.AddDays(1);

                case ResetFrequencyType.Weekly:
                    // Run again after 7 full days
                    return currentDate >= lastRunDate.AddDays(7);

                case ResetFrequencyType.Monthly:
                    // Calendar month logic using AddMonths(1) which automatically adjusts dates like 31 -> 28/30
                    return currentDate >= lastRunDate.AddMonths(1);

                case ResetFrequencyType.Quarterly:
                    // Every 3 calendar months
                    return currentDate >= lastRunDate.AddMonths(3);

                case ResetFrequencyType.FourMonthly:
                    // Every 4 calendar months
                    return currentDate >= lastRunDate.AddMonths(4);

                case ResetFrequencyType.HalfYearly:
                    // Every 6 calendar months
                    return currentDate >= lastRunDate.AddMonths(6);

                default:
                    return false;
            }
        }

        /// <summary>
        /// Returns quarter number (1 to 4)
        /// </summary>
        private static int GetQuarter(DateTime date)
        {
            return (date.Month - 1) / 3 + 1;
        }

        /// <summary>
        /// Checks if two dates fall within the same offset month period group
        /// (4 months or 6 months)
        /// </summary>
        private static bool IsSamePeriod(DateTime start, DateTime end, int months)
        {
            int startPeriod = (start.Month - 1) / months;
            int endPeriod = (end.Month - 1) / months;

            return startPeriod == endPeriod && start.Year == end.Year;
        }

        private static (double reportEntries, double balance) reportEntries(TenantAttributeDto? tenantAttr, double? entries)
        {
            if (tenantAttr?.EntriesRule == null)
            {
                return (entries ?? 0, 0);
            }
            else if (entries == null)
            {
                return (0, 0);

            }
            else if (!tenantAttr.EntriesRule.RolloverEnabled)
                return (entries ?? 0, 0);

            else if (tenantAttr.EntriesRule.EntryCap > 0)
            {
                double reportEntry = entries != null && entries > tenantAttr.EntriesRule.EntryCap ? tenantAttr.EntriesRule.EntryCap : entries ?? 0;
                double balance = (entries ?? 0) > tenantAttr.EntriesRule.EntryCap
                    ? (entries ?? 0) - tenantAttr.EntriesRule.EntryCap
                    : 0;
                return (reportEntry, balance);
            }

            return (entries ?? 0, 0);
        }

        private FailedRecords CreateFailedRecord(
    string recordKey,
    object? recordPayload,
    Exception exception)
        {
            return new FailedRecords
            {
                RecordKey = recordKey,           
                RecordPayload = recordPayload != null
                    ? JsonConvert.SerializeObject(recordPayload)
                    : string.Empty,

                ErrorMessage = exception.GetBaseException().Message,
                FailedTs = DateTime.UtcNow,

            };
        }

    }


}

    
