using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.ETL.Common.Constants;
using SunnyRewards.Helios.ETL.Common.Domain.Enums;
using SunnyRewards.Helios.ETL.Common.CustomException;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;
using System.Transactions;
using NHibernate.Util;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using SunnyRewards.Helios.User.Core.Domain.Models;
using Newtonsoft.Json;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos.Json;
using SunnyRewards.Helios.ETL.Infrastructure.HttpClients.Interfaces;
using SunnyBenefits.Fis.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Infrastructure.Helpers.Interfaces;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services
{
    public class ConsumerService : IConsumerService
    {
        private readonly ITenantRepo _tenantRepo;
        private readonly IConsumerRepo _consumerRepo;
        private readonly IPersonRepo _personRepo;
        private readonly IPersonAddressRepo _personAddressRepo;
        private readonly IPhoneNumberRepo _phoneNumberRepo;
        private readonly ILogger<ConsumerService> _logger;
        private readonly IEventService _eventService;
        private readonly NHibernate.ISession _session;
        private readonly IMapper _mapper;
        private readonly IConsumerAccountRepo _consumerAccountRepo;
        private readonly ISecretHelper _secretHelper;
        private readonly IBenefitBFFClient _benefitBFFClient;
        private const string className = nameof(ConsumerService);

        private const string _className = nameof(ConsumerService);
        public ConsumerService(ITenantRepo tenantRepo, IConsumerRepo consumerRepo, IPersonRepo personRepo, ILogger<ConsumerService> logger, NHibernate.ISession session
            , IMapper mapper, IPersonAddressRepo personAddressRepo, IPhoneNumberRepo phoneNumberRepo, IEventService eventService, IConsumerAccountRepo consumerAccountRepo, ISecretHelper secretHelper, IBenefitBFFClient benefitBFFClient)
        {
            _tenantRepo = tenantRepo;
            _consumerRepo = consumerRepo;
            _personRepo = personRepo;
            _logger = logger;
            _session = session;
            _mapper = mapper;
            _personAddressRepo = personAddressRepo;
            _phoneNumberRepo = phoneNumberRepo;
            _eventService = eventService;
            _consumerAccountRepo = consumerAccountRepo;
            _secretHelper = secretHelper;
            _benefitBFFClient = benefitBFFClient;
        }

        public async Task DeleteConsumers(EtlExecutionContext etlExecutionContext)
        {
            const string methodName = nameof(DeleteConsumers);
            _logger.LogInformation("{ClassName}.{MethodName}  started processing for TenantCode: {TenantCode}", className, methodName, etlExecutionContext.TenantCode);

            try
            {
                var tenantCode = etlExecutionContext.TenantCode;
                if (string.IsNullOrEmpty(tenantCode))
                {
                    _logger.LogWarning("{ClassName}.{MethodName}  - No tenant code supplied.", className, methodName);
                    throw new ETLException(ETLExceptionCodes.NullValue, "No tenant code supplied.");
                }

                var tenant = await _tenantRepo.FindOneAsync(x => x.TenantCode == tenantCode && x.DeleteNbr == 0);
                if (tenant == null)
                {
                    _logger.LogWarning("{ClassName}.{MethodName}  - Invalid tenant code supplied.", className, methodName);
                    throw new ETLException(ETLExceptionCodes.NotFoundInDb, $"Tenant not found in DB with tenant_code: {tenantCode}.");
                }

                await FreezeIneligibleConsumerCards(tenant);

                var consumers = await _consumerRepo.FindAsync(x => x.TenantCode == tenantCode && x.DeleteNbr == 0 && x.EligibleEndTs != null && x.EligibleEndTs < DateTime.UtcNow.Date);
                if (consumers == null || consumers.Count == 0)
                {
                    _logger.LogError("{ClassName}.{MethodName}  - No consumers found for TenantCode: {TenantCode}", className, methodName, etlExecutionContext.TenantCode);
                    throw new ETLException(ETLExceptionCodes.NotFoundInDb, $"No consumers found with tenant_code: {tenantCode} and EligibleEndTs < .{DateTime.UtcNow.Date}");
                }

                foreach (var consumer in consumers)
                {
                    if (consumer?.SubscriberMemberNbr != null)
                    {
                        await ProcessConsumerAsync(consumer, tenantCode);
                    }
                    else
                    {
                        _logger.LogWarning("{ClassName}.{MethodName}  - ConsumerCode: {ConsumerCode} has a null SubscriberMemberNbr.", className, methodName, consumer.ConsumerCode);
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}  - Failed processing for delete consumers. ErrorCode:{Code},ERROR: {Message}", className, methodName, StatusCodes.Status500InternalServerError, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Update consumer Enrollment state
        /// </summary>
        /// <param name="membersResponseDto"></param>
        /// <param name="actionType"></param>
        /// <returns></returns>
        public async Task UpdateConsumerEnrollment(MembersResponseDto membersResponseDto, String actionType)
        {
            const string methodName = nameof(UpdateConsumerEnrollment);
            _logger.LogInformation("{ClassName}.{MethodName}  started updated consumers : {TenantCode}", className, methodName, membersResponseDto);

            var updatedConsumers = new List<ETLConsumerModel>();
            foreach (var consumer in membersResponseDto.Consumers)
            {
                try
                {
                    if (consumer.ErrorMessage == null && consumer.ErrorCode == null)
                    {
                        _session.Clear();
                        var consumerModel = await _consumerRepo.FindOneAsync(x => x.ConsumerCode == consumer.Consumer.ConsumerCode, true);

                        if (actionType == ActionTypes.CancelDescription)
                        {
                            consumerModel.EnrollmentStatus = EnrollmentStatus.ENROLLMENT_CANCELLED.ToString();
                            consumerModel.EnrollmentStatusSource = EnrollmentStatusSource.ELIGIBILITY_FILE.ToString();
                            consumerModel.UpdateTs = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
                            consumerModel.UpdateUser = Constants.UpdateUser;

                           var updatedConsumer =  await _consumerRepo.UpdateAsync(consumerModel);
                            updatedConsumers.Add(updatedConsumer);
                        }
                        else if (actionType == ActionTypes.DeleteDescription)
                        {
                            consumerModel.EnrollmentStatus = EnrollmentStatus.ENROLLMENT_TERMINATED.ToString();
                            consumerModel.EnrollmentStatusSource = EnrollmentStatusSource.ELIGIBILITY_FILE.ToString();
                            consumerModel.UpdateTs = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
                            consumerModel.UpdateUser = Constants.UpdateUser;

                            var updatedConsumer = await _consumerRepo.UpdateAsync(consumerModel);
                            updatedConsumers.Add(updatedConsumer);
                            
                        }

                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Consumer Entrollment Status not updated for Consumer code : { Consumercode}, Error : {Error}", consumer.Consumer.ConsumerCode, ex.Message);
                }
            }

            var updatedDtoList = _mapper.Map<List<ConsumerDto>>(updatedConsumers);
            await _eventService.CreateConsumerHistoryEvent(updatedDtoList, nameof(methodName));
        }

        private async Task ProcessConsumerAsync(ETLConsumerModel consumer, string tenantCode)
        {
            const string methodName = nameof(ProcessConsumerAsync);

            using var transaction = _session.BeginTransaction();
            try
            {
                if (await IsPrimarySubscriber(consumer.PersonId))
                {
                    var dependentConsumers = await _consumerRepo.FindAsync(x => x.TenantCode == tenantCode && x.SubscriberMemberNbr == consumer.MemberNbr && x.DeleteNbr == 0);

                    if (dependentConsumers == null || dependentConsumers.Count == 0)
                    {
                        _logger.LogWarning("{ClassName}.{MethodName}  - No dependent consumers found for ConsumerCode: {ConsumerCode}", className, methodName, consumer.ConsumerCode);
                    }
                    else
                    {
                        foreach (var dependentConsumer in dependentConsumers)
                        {
                            await DeleteConsumerAndPerson(dependentConsumer);
                        }
                    }
                }
                else
                {
                    await DeleteConsumerAndPerson(consumer);
                }

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _session.Clear();
                _logger.LogError(ex, "{ClassName}.{MethodName}  - Failed processing ConsumerCode: {ConsumerCode}.ErrorCode:{Code},ERROR: {ex.Message}",
                    className, methodName, consumer.ConsumerCode, StatusCodes.Status500InternalServerError, ex.Message);
                throw;
            }
        }

        private async Task DeleteConsumerAndPerson(ETLConsumerModel consumer)
        {
            const string methodName = nameof(DeleteConsumerAndPerson);

            if (consumer == null)
            {
                _logger.LogError("{ClassName}.{MethodName}  - Consumer is null.", className, methodName);
                return;
            }

            var person = await _personRepo.FindOneAsync(x => x.PersonId == consumer.PersonId);

            if (person == null)
            {
                _logger.LogError("{ClassName}.{MethodName}  - No person found with PersonId: {PersonId}", className, methodName, consumer.PersonId);
                return;
            }

            person.UpdateTs = DateTime.UtcNow;
            person.UpdateUser = Constants.CreateUser;
            person.DeleteNbr = person.PersonId;

            consumer.DeleteNbr = consumer.ConsumerId;
            consumer.UpdateTs = DateTime.UtcNow;
            consumer.UpdateUser = Constants.CreateUser;

            await _session.UpdateAsync(person);
            await _session.UpdateAsync(consumer);

            var updatedDto = _mapper.Map<ConsumerDto>(consumer);
            await _eventService.CreateConsumerHistoryEvent(new List<ConsumerDto>() { updatedDto }, nameof(methodName));

            await DeletePersonAddresses(person.PersonId);
            await DeletePhoneNumber(person.PersonId);
        }

        private async Task FreezeIneligibleConsumerCards(ETLTenantModel tenant)
        {
            const string methodName = nameof(FreezeConsumerCard);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName}  - Started freezing cards for TenantCode: {TenantCode}", className, methodName, tenant.TenantCode);
                var today = DateTime.UtcNow;
                var tenantOption = tenant.TenantOption != null ? JsonConvert.DeserializeObject<TenantOption>(tenant.TenantOption) : new TenantOption();
                if (tenantOption != null && tenantOption.Apps != null && tenantOption.Apps.Contains(Constants.Benefits) && tenantOption.BenefitsOptions != null &&
                   tenantOption.BenefitsOptions.ShouldFreezeCardOnTermination == true)
                {
                    var consumers = await _consumerRepo.FindAsync(x =>
                        x.TenantCode == tenant.TenantCode &&
                        x.DeleteNbr != 0 &&
                        x.EligibleEndTs != null &&
                        x.EligibleEndTs.Value.Date == today.AddDays(-tenantOption.BenefitsOptions.ValidCardActiveDays).Date, includeDeleted: true
                    );
                    if (consumers == null || consumers.Count == 0)
                    {
                        _logger.LogError("{ClassName}.{MethodName}  - No consumers found for TenantCode: {TenantCode} with {ValidCardActiveDays} days beyond eligibility end date", className, methodName, tenant.TenantCode, tenantOption.BenefitsOptions.ValidCardActiveDays);
                        return;
                    }
                    _logger.LogInformation("{ClassName}.{MethodName}  - Found {ConsumerCount} consumers to freeze cards for TenantCode: {TenantCode}", className, methodName, consumers.Count, tenant.TenantCode);
                    
                    // Get X-API-Key secret for the tenant when consumers are found
                    var apiKey = await _secretHelper.GetTenantSecret(tenant.TenantCode, Constants.XApiKeySecret);
                    _logger.LogInformation("{ClassName}.{MethodName}  - Retrieved X-API-Key secret for TenantCode: {TenantCode}", className, methodName, tenant.TenantCode);
                    
                    foreach (var consumer in consumers)
                    {
                        await FreezeConsumerCard(consumer.ConsumerCode, tenant.TenantCode, apiKey);
                    }
                }
                else
                {
                    _logger.LogInformation("{ClassName}.{MethodName}  - Skipping card freeze for TenantCode: {TenantCode} as the option is not enabled.", className, methodName, tenant.TenantCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}  - Error occurred while freezing cards for TenantCode: {TenantCode}. Error: {Message}", className, methodName, tenant.TenantCode, ex.Message);
            }
        }

        private async Task FreezeConsumerCard(string consumerCode, string tenantCode, string apiKey)
        {
            const string methodName = nameof(FreezeConsumerCard);
            _logger.LogInformation("{ClassName}.{MethodName}  - Freezing card for ConsumerCode: {ConsumerCode} in TenantCode: {TenantCode}", className, methodName, consumerCode, tenantCode);
            try
            {
                var consumerAccount = await _consumerAccountRepo.FindOneAsync(x => x.ConsumerCode == consumerCode && x.TenantCode == tenantCode && x.DeleteNbr == 0);
                if (consumerAccount == null || string.IsNullOrWhiteSpace(consumerAccount.ProxyNumber) || consumerAccount.ProxyNumber.Equals("0000", StringComparison.Ordinal))
                {
                    _logger.LogError("{ClassName}.{MethodName}  - No consumer account found or missing proxy number for ConsumerCode: {ConsumerCode} in TenantCode: {TenantCode}", className, methodName, consumerCode, tenantCode);
                    return;
                }
                
                var headers = new Dictionary<string, string>();
                headers.Add("X_API_KEY", apiKey);

                var executeCardOperationRequestDto = new ExecuteCardOperationRequestDto
                {
                    TenantCode = tenantCode,
                    ConsumerCode = consumerCode,
                    CardOperation = Constants.FreezeCardOperation
                };

                var benefitResponse = await _benefitBFFClient.Post<ExecuteCardOperationResponseDto>(Constants.BenefitsCardOperation, executeCardOperationRequestDto, headers);
                if (benefitResponse.ErrorCode != null)
                {
                    _logger.LogError("{ClassName}.{MethodName} -  Failed to freeze card for ConsumerCode: {ConsumerCode}. ErrorCode:{ErrorCode}, ERROR:{Msg}", className, methodName, consumerCode, benefitResponse.ErrorCode, benefitResponse.ErrorMessage);
                }
                else
                {
                    _logger.LogInformation("{ClassName}.{MethodName}  - Card frozen successfully for ConsumerCode: {ConsumerCode}", className, methodName, consumerCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}  - Error occurred while freezing card for ConsumerCode: {ConsumerCode}. Error: {Message}", className, methodName, consumerCode, ex.Message);
            }
        }

        private async Task DeletePersonAddresses(long personId)
        {
            try
            {
                var fetchedToCancelAddresses = await _personAddressRepo.FindAsync(x =>
                         x.PersonId == personId && x.DeleteNbr == 0);
                if (fetchedToCancelAddresses != null && fetchedToCancelAddresses.Any())
                {
                    foreach (var address in fetchedToCancelAddresses)
                    {
                        if (address != null)
                        {
                            address.DeleteNbr = address.PersonAddressId;
                            address.UpdateTs = DateTime.UtcNow;
                            address.UpdateUser = Constants.CreateUser;
                            await _session.UpdateAsync(address);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to process cancel request for addresses with PersonId: {personId}");
            }
        }

        private async Task DeletePhoneNumber(long personId)
        {
            List<ETLPhoneNumberModel> phoneNumberModels = new List<ETLPhoneNumberModel>();
            try
            {
                var fetchedToCancelNumber = await _phoneNumberRepo.FindAsync(x =>
                         x.PersonId == personId && x.DeleteNbr == 0);

                if (fetchedToCancelNumber != null && fetchedToCancelNumber.Any())
                {
                    foreach (var number in fetchedToCancelNumber)
                    {
                        if (number != null)
                        {
                            number.DeleteNbr = number.PhoneNumberId;
                            number.UpdateTs = DateTime.UtcNow;
                            number.UpdateUser = Constants.CreateUser;
                            await _session.UpdateAsync(number);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(StatusCodes.Status500InternalServerError, $"Failed to process delete request for phone number with PersonId: {personId}");
            }
        }

        private async Task<bool> IsPrimarySubscriber(long personId)
        {
            const string methodName = nameof(IsPrimarySubscriber);

            var person = await _personRepo.FindOneAsync(x => x.PersonId == personId && !x.IsDependent && !x.IsSpouse && x.DeleteNbr == 0);

            if (person == null)
            {
                _logger.LogWarning("{ClassName}.{MethodName}  - Person with ID: {personId} is not found or is not a primary subscriber.", className, methodName, personId);
                return false;
            }

            return true;
        }
        public async Task<List<MemberEnrollmentDetailDto>> GetUpdatedInsurancePeriod(List<MemberEnrollmentDetailDto> memberCsvDtoList)
        {
            const string methodName = nameof(GetUpdatedInsurancePeriod);
            _logger.LogInformation("[{Class}] : [{Method}] : Started processing...", _className, methodName);

            try
            {
                // Group CSV entries by composite key and get min/max eligible dates
                var csvGroups = memberCsvDtoList
                    .GroupBy(x => new MemberGroupByKey
                    {         
                        //MemberNbr = x.EnrollmentDetail.MemberNbr,
                        MemberId = x.EnrollmentDetail.MemberId,
                        PartnerCode = x.EnrollmentDetail.PartnerCode
                    })
                    .ToDictionary(
                        g => g.Key,
                        g => (
                            Start: g.Min(x => x.EnrollmentDetail.EligibleStartTs),
                            End: g.Max(x => x.EnrollmentDetail.EligibleEndTs)
                        ));

                var keys = csvGroups.Keys.ToList();

                // Fetch db periods from repo by the keys (assumed GetInsurancePeriod accepts List<MemberGroupByKey>)
                var dbPeriods = await _consumerRepo.GetInsurancePeriod(keys);

                // Group DB periods by the same composite key
                var dbPeriodMap = dbPeriods
                    .GroupBy(x => new MemberGroupByKey
                    {
                        //MemberNbr = x.MemberNbr,
                        MemberId = x.MemberId,
                        PartnerCode = x.PartnerCode
                    })
                    .ToDictionary(
                        g => g.Key,
                        g => (
                            Start: g.Min(x => x.EligibleStartTs),
                            End: g.Max(x => x.EligibleEndTs),
                            ConsumerCode: g.FirstOrDefault()?.ConsumerCode ?? string.Empty
                        ));

                // Merge CSV and DB periods, preferring wider date ranges
                var mergedPeriods = csvGroups.Select(kvp =>
                {
                    var key = kvp.Key;
                    var csvPeriod = kvp.Value;

                    var hasDb = dbPeriodMap.TryGetValue(key, out var dbPeriod);

                    // Calculate merged start/end dates
                    var start = hasDb && dbPeriod.Start < csvPeriod.Start ? dbPeriod.Start : csvPeriod.Start;
                    var end = hasDb && dbPeriod.End > csvPeriod.End ? dbPeriod.End : csvPeriod.End;
                    var consumerCode = hasDb ? dbPeriod.ConsumerCode : string.Empty;

                    return new MemberInsurancePeriodDto
                    {
                        //MemberNbr = key.MemberNbr,
                        MemberId = key.MemberId,
                        PartnerCode = key.PartnerCode,
                        EligibleStartTs = start,
                        EligibleEndTs = end,
                        ConsumerCode = consumerCode
                    };
                }).ToList();

                // Update DB in batch
                //Change

               var updatedconsumers =  await _consumerRepo.UpdateInsurancePeriodsAsync(mergedPeriods);
                var updatedConsumerLst = _mapper.Map<List<ConsumerDto>>(updatedconsumers);
                await _eventService.CreateConsumerHistoryEvent(updatedConsumerLst);


                // Update the CSV list dates accordingly (using the composite key)
                var mergedDict = mergedPeriods.ToDictionary(
                    p => new MemberGroupByKey { //MemberNbr = p.MemberNbr,
                        MemberId = p.MemberId, PartnerCode = p.PartnerCode });

                foreach (var csvEntry in memberCsvDtoList)
                {
                    var key = new MemberGroupByKey
                    {
                        //MemberNbr = csvEntry.EnrollmentDetail.MemberNbr,
                        MemberId = csvEntry.EnrollmentDetail.MemberId,
                        PartnerCode = csvEntry.EnrollmentDetail.PartnerCode
                    };

                    if (mergedDict.TryGetValue(key, out var merged))
                    {
                        csvEntry.EnrollmentDetail.EligibleStartTs = merged.EligibleStartTs?? DateTime.MinValue;
                        csvEntry.EnrollmentDetail.EligibleEndTs = merged.EligibleEndTs ?? DateTime.MinValue;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{Class}] : [{Method}] : Error occurred during processing.", _className, methodName);
            }

            _logger.LogInformation("[{Class}] : [{Method}] : Completed processing.", _className, methodName);
            return memberCsvDtoList;
        }

    }
}
