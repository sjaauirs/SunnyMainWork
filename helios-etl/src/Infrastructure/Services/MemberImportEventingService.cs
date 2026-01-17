using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Core.Domain.Constants;
using System.Text.Json;
using System.ComponentModel.DataAnnotations;
using SunnyRewards.Helios.ETL.Common.Constants;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Common.Domain.Enums;
using AutoMapper;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services
{
    public class MemberImportEventingService : IMemberImportEventingService
    {
        private readonly string _redshiftConnectionString;
        private readonly IRedshiftDataReader _redshiftDataReaderService;
        private readonly IVault _vault;
        private readonly ILogger<MemberImportEventingService> _logger;
        private readonly IAwsQueueService _awsQueueService;
        private readonly ITenantRepo _tenantRepo;
        private readonly IConsumerService _consumerService;
        private readonly IConsumerRepo _consumerRepo;
        private readonly IEventService _eventService;
        private readonly IEventingWrapperService _eventingWrapperService;
        private readonly IMapper _mapper;
        private const string ClassName = nameof(MemberImportEventingService);

        public MemberImportEventingService(
            ILogger<MemberImportEventingService> logger,
            Helpers.Interfaces.ISecretHelper secretHelper,
            IRedshiftDataReader redshiftDataReaderService,
            IVault vault,
            IAwsQueueService awsQueueService,
            ITenantRepo tenantRepo,
            IConsumerService consumerService,
            IConsumerRepo consumerRepo,
            IEventService eventService,
            IMapper mapper,
            IEventingWrapperService eventingWrapperService)
        {
            _logger = logger;
            _redshiftConnectionString = secretHelper.GetRedshiftConnectionString().Result;
            _redshiftDataReaderService = redshiftDataReaderService;
            _vault = vault;
            _tenantRepo = tenantRepo;
            _awsQueueService = awsQueueService;
            _consumerService = consumerService;
            _consumerRepo = consumerRepo;
            _eventService = eventService;
            _mapper = mapper;
            _eventingWrapperService = eventingWrapperService;
        }

        public async Task MemberImportEventingAsync(EtlExecutionContext etlExecutionContext, string jobId)
        {
            const string methodName = nameof(MemberImportEventingAsync);
            try
            {
                int totalProcessed = 0;
                int iteration = 0;

                while (true)
                {
                    iteration++;
                    
                    // Fetch unclaimed rows from redshift for the latest file
                    var unclaimedRows = await _redshiftDataReaderService.FetchAndClaimBatchAsync(
                        _redshiftConnectionString,
                        etlExecutionContext.PartnerCode,
                        jobId,
                        etlExecutionContext.BatchSize);

                    if (unclaimedRows == null || !unclaimedRows.Any())
                    {
                        _logger.LogInformation("{ClassName}.{MethodName}: No more unclaimed rows found after {Iterations} iterations. Total processed: {Count}.",
                            ClassName, methodName, iteration, totalProcessed);
                        break;
                    }

                    _logger.LogInformation("{ClassName}.{MethodName}: Processing batch #{BatchNumber} with {Count} claimed rows.",
                        ClassName, methodName, iteration, unclaimedRows.Count);

                    // Step 1: Prepare messages
                    var messages = new List<(string EventMessage, long RowId, string EventId, string EventType, string PersonUniqueIdentifier)>();

                    foreach (var row in unclaimedRows)
                    {
                        totalProcessed++;

                        try
                        {
                            var eventDataDto = CreateMemberDtos(row);
                            var tenant = await _tenantRepo.FindOneAsync(x => x.PartnerCode == row.PartnerCode && x.DeleteNbr == 0);
                            

                            eventDataDto.EnrollmentDetail.CustomerCode = etlExecutionContext.CustomerCode;
                            eventDataDto.EnrollmentDetail.CustomerLabel = etlExecutionContext.CustomerLabel;
                            eventDataDto.EnrollmentDetail.Action = row.Action;
                            eventDataDto.EnrollmentDetail.TenantCode = tenant?.TenantCode ?? string.Empty;

                            if (!ValidateConsumer(eventDataDto))
                            {
                                _logger.LogError("{ClassName}.{MethodName}: Validation failed for MemberId: {MemberId}, marking as ERROR.",
                                    ClassName, methodName, row.MemberId);

                                await _redshiftDataReaderService.MarkPublishStatusAsync(
                                    _redshiftConnectionString,
                                    row.MemberImportFileDataId,
                                    "VALIDATION_ERROR");

                                eventDataDto.EnrollmentDetail.PublishStatus = "VALIDATION_ERROR";
                                await _awsQueueService.PushToMemberImportEventDlqQueue(eventDataDto);
                                continue;
                            }

                            // Handle insert/update enrichment
                            if (eventDataDto.EnrollmentDetail.Action is ActionTypes.InsertDescription or
                                ActionTypes.InsertCode or
                                ActionTypes.UpdateDescription or
                                ActionTypes.UpdateCode)
                            {
                                var memberList = new List<MemberEnrollmentDetailDto> { eventDataDto };
                                var memberDetails = await _consumerService.GetUpdatedInsurancePeriod(memberList);
                                if (memberDetails != null && memberDetails.Any())
                                    eventDataDto = memberDetails.First();
                            }

                            // Handle cancel/delete enrollment
                            if (eventDataDto.EnrollmentDetail.Action is ActionTypes.CancelDescription or
                                ActionTypes.CancelCode or
                                ActionTypes.DeleteDescription or
                                ActionTypes.DeleteCode)
                            {
                                await UpdateConsumerEnrollment(eventDataDto, eventDataDto.EnrollmentDetail.Action);
                            }

                            // Build event header and prepare the SNS message
                            var eventHeaderDto = BuildEventHeaderDto();

                            string eventMessage = JsonSerializer.Serialize(new EventDto<MemberEnrollmentDetailDto>
                            {
                                Header = eventHeaderDto,
                                Data = eventDataDto
                            });

                            messages.Add((eventMessage, row.MemberImportFileDataId, eventHeaderDto.EventId, eventHeaderDto.EventType, eventDataDto.MemberDetail.PersonUniqueIdentifier));
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "{ClassName}.{MethodName}: Error preparing event for MemberId: {MemberId}",
                                ClassName, methodName, row.MemberId);

                            await _redshiftDataReaderService.MarkPublishStatusAsync(
                                _redshiftConnectionString,
                                row.MemberImportFileDataId,
                                "ERROR");
                        }
                    }

                    // Step 2: Publish messages using wrapper service
                    var results = await _eventingWrapperService.PublishMessagesInParallelAsync(
                        messages, 
                        jobId,
                        AdminConstants.MemberImportEventTopicName);

                    // Step 3: Update publish statuses
                    var dbUpdates = results
                        .Select(r => (r.RowId, r.Published ? "PUBLISHED" : "ERROR"))
                        .ToList();

                    await _redshiftDataReaderService.MarkPublishStatusBatchAsync(_redshiftConnectionString, dbUpdates);

                    _logger.LogInformation(
                        "{ClassName}.{MethodName}: Completed batch #{BatchNumber}. Total processed so far: {Total}.",
                        ClassName, methodName, iteration, totalProcessed);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: Error: {ErrorMessage}",
                    ClassName, methodName, ex.Message);
                throw;
            }
        }

        private MemberEnrollmentDetailDto CreateMemberDtos(RedShiftMemberImportFileDataDto record, bool subscriberOnly = false)
        {
            const string methodName = nameof(CreateMemberDtos);
            var isSSOUser = record.IsSsoUser ?? false;;
            DateTime dob = record.Dob ?? DateTime.MinValue;
            // Determine Age
            int age;
            if (int.TryParse(record.Age, out int parsedAge) && parsedAge >= 18)
            {
                age = parsedAge;
            }
            else
            {
                age = CalculateAge(dob);

                if (age < 18)
                {
                    _logger.LogError("{ClassName}.{MethodName} - Member age < 18, record: {record}",
                                     ClassName, methodName, record);
                    return null; // Skip underage members
                }
            }
            var memberDto = new MemberEnrollmentDetailDto
            {
                MemberDetail = new MemberDetailDto
                {
                    FirstName = record.FirstName ?? string.Empty,
                    LastName = record.LastName ?? string.Empty,
                    LanguageCode = string.IsNullOrWhiteSpace(record.LanguageCode) ? "en-US" : record.LanguageCode,
                    MemberSince = DateTime.UtcNow,
                    Email = record.Email,
                    City = record.City ?? string.Empty,
                    Country = record.Country ?? "US",
                    PostalCode = record.PostalCode ?? string.Empty,
                    PhoneNumber = record.MobilePhone ?? string.Empty,
                    Region = " ",
                    Dob = dob,
                    Gender = GetGenderString(record.Gender ?? string.Empty),
                    MailingAddressLine1 = record.MailingAddressLine1 ?? string.Empty,
                    MailingAddressLine2 = record.MailingAddressLine2 ?? string.Empty,
                    MailingState = record.MailingState ?? string.Empty,
                    MailingCountryCode = record.MailingCountryCode ?? string.Empty,
                    HomePhoneNumber = record.HomePhoneNumber ?? string.Empty,
                    MiddleName = record.MiddleName ?? string.Empty,
                    HomeAddressLine1 = record.HomeAddressLine1 ?? string.Empty,
                    HomeAddressLine2 = record.HomeAddressLine2 ?? string.Empty,
                    HomeCity = record.HomeCity ?? string.Empty,
                    HomeState = record.HomeState ?? string.Empty,
                    HomePostalCode = record.HomePostalCode ?? string.Empty,
                    Source = "ETL",
                    PersonUniqueIdentifier = record.PersonUniqueIdentifier,
                    Age = age
                },
                EnrollmentDetail = new EnrollmentDetailDto
                {
                    PartnerCode = record.PartnerCode ?? string.Empty,
                    MemberNbr = record.MemNbr ?? string.Empty,
                    SubscriberMemberNbr = record.SubscriberMemNbr ?? string.Empty,
                    RegistrationTs = DateTime.UtcNow,
                    EligibleStartTs = record.EligibilityStart ?? DateTime.MinValue,
                    EligibleEndTs = record.EligibilityEnd ?? DateTime.MaxValue,
                    SubscriberOnly = subscriberOnly,
                    SubsciberMemberNbrPrefix = record.SubscriberMemNbrPrefix ?? string.Empty,
                    MemberNbrPrefix = record.MemNbrPrefix ?? string.Empty,
                    RegionCode = record.RegionCode ?? string.Empty,
                    PlanId = record.PlanId ?? string.Empty,
                    PlanType = record.PlanType ?? string.Empty,
                    SubgroupId = record.SubgroupId ?? string.Empty,
                    IsSSOUser = record.IsSsoUser ?? false,
                    MemberId = record.MemberId ?? string.Empty,
                    MemberType = record.MemberType ?? string.Empty,
                    ConsumerAttribute = record.RawDataJson ?? string.Empty
                }
            };
            return memberDto;
        }

        private int CalculateAge(DateTime dob)
        {
            var today = DateTime.Today;
            var age = today.Year - dob.Year;

            if (dob.Date > today.AddYears(-age)) age--;

            return age;
        }
        private string GetGenderString(string gender)
        {
            return gender switch
            {
                "M" => "MALE",
                "F" => "FEMALE",
                "O" => "OTHER",
                "U" => "UNKNOWN",
                _ => string.Empty
            };
        }

        private EventHeaderDto BuildEventHeaderDto()
        {
            var dto = new EventHeaderDto
            {
                EventId = Guid.NewGuid().ToString("N"),
                EventType = AdminConstants.MemberImportEvent,
                EventSubtype = AdminConstants.MemberImportEventSubType,
                PublishTs = DateTime.UtcNow,
                SourceModule = "ETL"
            };

            _logger.LogInformation("{ClassName}.{MethodName}: EventHeaderDto created: {@EventHeaderDto}",
                ClassName, nameof(BuildEventHeaderDto), dto);

            return dto;
        }
        private bool ValidateConsumer(MemberEnrollmentDetailDto consumerCsvDto)
        {
            const string methodName = nameof(ValidateConsumer);

            try
            {
                if (!consumerCsvDto.EnrollmentDetail.IsSSOUser)
                {
                    if (string.IsNullOrWhiteSpace(consumerCsvDto.MemberDetail.Email))
                    {
                        _logger.LogInformation("{ClassName}.{MethodName}: Email is required when SSO is not enabled for member {memberId}.", ClassName, methodName, consumerCsvDto.EnrollmentDetail.MemberId);
                        return false;
                    }
                    else
                    {
                        var emailAttribute = new EmailAddressAttribute();
                        if (!emailAttribute.IsValid(consumerCsvDto.MemberDetail.Email))
                        {
                            _logger.LogInformation("{ClassName}.{MethodName}: Invalid email format for member {memberId}.", ClassName, methodName, consumerCsvDto.EnrollmentDetail.MemberId);
                            return false;
                        }
                    }
                }
                else if (string.IsNullOrEmpty(consumerCsvDto.EnrollmentDetail.Action))
                {
                    _logger.LogInformation("{ClassName}.{MethodName}: Action missing for member {memberId}."
                                           , ClassName, methodName, consumerCsvDto.EnrollmentDetail.MemberId);
                    return false;
                }
                else if (string.IsNullOrEmpty(consumerCsvDto.EnrollmentDetail.TenantCode))
                {
                    _logger.LogInformation("{ClassName}.{MethodName}: Tenant Code missing for member {memberId}."
                                           , ClassName, methodName, consumerCsvDto.EnrollmentDetail.MemberId);
                    return false;
                }
                else if (consumerCsvDto.EnrollmentDetail.EligibleEndTs <= consumerCsvDto.EnrollmentDetail.EligibleStartTs)
                {
                    _logger.LogInformation("{ClassName}.{MethodName}: Invalid EligibleEndTs or EligibleStartTs for member {memberId}."
                                           , ClassName, methodName, consumerCsvDto.EnrollmentDetail.MemberId);
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogInformation("{ClassName}.{MethodName}: Exception during validation: {ExceptionMessage}", ClassName, methodName, ex.Message);
                return false;
            }
        }
        private async Task UpdateConsumerEnrollment(MemberEnrollmentDetailDto consumerDto, string actionType)
        {
            const string methodName = nameof(UpdateConsumerEnrollment);
            _logger.LogInformation("{ClassName}.{MethodName} - Started updating consumer for TenantCode: {TenantCode}, MemberId: {MemberId}",
                ClassName, methodName, consumerDto.EnrollmentDetail.TenantCode, consumerDto.EnrollmentDetail.MemberId);

            var updatedConsumers = new List<ETLConsumerModel>();

            try
            {
                var consumerModel = await _consumerRepo.FindOneAsync(
                    x => x.MemberId == consumerDto.EnrollmentDetail.MemberId &&
                         x.TenantCode == consumerDto.EnrollmentDetail.TenantCode && x.DeleteNbr == 0);

                if (actionType is ActionTypes.CancelDescription or ActionTypes.CancelCode)
                {
                    consumerModel.EnrollmentStatus = EnrollmentStatus.ENROLLMENT_CANCELLED.ToString();
                }
                else if (actionType is ActionTypes.DeleteDescription or ActionTypes.DeleteCode)
                {
                    consumerModel.EnrollmentStatus = EnrollmentStatus.ENROLLMENT_TERMINATED.ToString();
                }
                consumerModel.EnrollmentStatusSource = EnrollmentStatusSource.ELIGIBILITY_FILE.ToString();
                consumerModel.UpdateTs = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
                consumerModel.UpdateUser = Constants.UpdateUser;
                var updatedConsumer = await _consumerRepo.UpdateAsync(consumerModel);
                updatedConsumers.Add(updatedConsumer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Consumer Enrollment Status not updated for MemberId: {MemberId}",
                    consumerDto.EnrollmentDetail.MemberId);
            }

            var updatedDtoList = _mapper.Map<List<ConsumerDto>>(updatedConsumers);
            await _eventService.CreateConsumerHistoryEvent(updatedDtoList, methodName);
        }

    }
}
