using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using NHibernate;
using NHibernate.Mapping.ByCode.Impl;
using SunnyRewards.Helios.Common.Core.Domain;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Common.Core.Services;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos.Json;
using SunnyRewards.Helios.User.Core.Domain.Constant;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Dtos.Json;
using SunnyRewards.Helios.User.Core.Domain.enums;
using SunnyRewards.Helios.User.Core.Domain.Models;
using SunnyRewards.Helios.User.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.User.Infrastructure.Repositories;
using SunnyRewards.Helios.User.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.User.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Security.Cryptography;
using SunnyRewards.Helios.Tenant.Core.Domain.Models;
using SunnyRewards.Helios.User.Infrastructure.ReadReplica;

namespace SunnyRewards.Helios.User.Infrastructure.Services
{
    public class ConsumerService : BaseService, IConsumerService
    {
        private readonly ILogger<ConsumerService> _consumerLogger;
        private readonly IMapper _mapper;
        private readonly IConsumerRepo _consumerRepo;
        private readonly IPersonRepo _personRepo;
        private readonly IRoleRepo _roleRepo;
        private readonly IPersonRoleRepo _personRoleRepo;
        private readonly IPersonAddressRepo _personAddressRepo;
        private readonly IPhoneNumberRepo _phoneNumberRepo;
        private readonly NHibernate.ISession _session;
        private readonly IReadOnlySession? _readOnlySession;
        private readonly ITenantClient _tenantClient;
        private readonly IAddressTypeService _addressTypeService;
        private readonly IUploadAgreementPDFService _uploadAgreementPDFService;
        private readonly IMemberImportFileDataRepo _memberImportFileDataRepo;
        private readonly IConsumerETLRepo _consumerETLRepo;
        private readonly IEventService _eventService;
        private readonly IHeliosEventPublisher<AgreementsVerifiedEventDto> _heliosEventPublisher;
        private const string className = nameof(ConsumerService);

        // Returns the read replica session if available, otherwise falls back to primary
        private NHibernate.ISession ReadSession => _readOnlySession?.Session ?? _session;
        
        // Returns database source name for logging
        private string DbSource => _readOnlySession != null ? "ReadReplica" : "Primary";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="consumerLogger"></param>
        /// <param name="mapper"></param>
        /// <param name="consumerRepo"></param>
        /// <param name="personRepo"></param>
        /// <param name="roleRepo"></param>
        /// <param name="session"></param>
        public ConsumerService(
            ILogger<ConsumerService> consumerLogger,
            IMapper mapper,
            IConsumerRepo consumerRepo,
            IPersonRepo personRepo,
            IRoleRepo roleRepo,
            NHibernate.ISession session,
            IPersonRoleRepo personRoleRepo,
            ITenantClient tenantClient,
            IAddressTypeService addressTypeService,
            IUploadAgreementPDFService uploadAgreementPDFService,
            IPersonAddressRepo personAddressRepo,
            IPhoneNumberRepo phoneNumberRepo,
            IMemberImportFileDataRepo memberImportFileDataRepo,
            IEventService eventService,
            IConsumerETLRepo consumerETLRepo,
            IHeliosEventPublisher<AgreementsVerifiedEventDto> heliosEventPublisher,
            IReadOnlySession? readOnlySession = null)
        {
            _consumerLogger = consumerLogger;
            _mapper = mapper;
            _consumerRepo = consumerRepo;
            _personRepo = personRepo;
            _roleRepo = roleRepo;
            _session = session;
            _readOnlySession = readOnlySession;
            _personRoleRepo = personRoleRepo;
            _tenantClient = tenantClient;
            _addressTypeService = addressTypeService;
            _uploadAgreementPDFService = uploadAgreementPDFService;
            _personAddressRepo = personAddressRepo;
            _phoneNumberRepo = phoneNumberRepo;
            _memberImportFileDataRepo = memberImportFileDataRepo;
            _eventService = eventService;
            _consumerETLRepo = consumerETLRepo;
            _heliosEventPublisher = heliosEventPublisher;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="consumerRequestDto"></param>
        /// <returns></returns>
        public async Task<GetConsumerResponseDto> GetConsumerData(GetConsumerRequestDto consumerRequestDto)
        {
            const string methodName = nameof(GetConsumerData);
            var response = new GetConsumerResponseDto();
            try
            {
                _consumerLogger.LogInformation("{className}.{methodName}: Querying from {dbSource} database", className, methodName, DbSource);
                
                ConsumerModel? consumer;
                if (_readOnlySession != null)
                {
                    consumer = await ReadSession.QueryOver<ConsumerModel>()
                        .Where(x => x.ConsumerCode == consumerRequestDto.ConsumerCode && x.DeleteNbr == 0)
                        .SingleOrDefaultAsync();
                }
                else
                {
                    consumer = await _consumerRepo.FindOneAsync(x => x.ConsumerCode == consumerRequestDto.ConsumerCode && x.DeleteNbr == 0);
                }

                if (consumer != null && consumer.ConsumerId > 0)
                {
                    response = new GetConsumerResponseDto()
                    {
                        Consumer = _mapper.Map<ConsumerDto>(consumer)
                    };
                    _consumerLogger.LogInformation("{className}.{methodName}: Retrieved Consumer Data Successfully for ConsumerCode : {ConsumerCode}", className, methodName, consumerRequestDto.ConsumerCode);

                    return response;
                }
                return new GetConsumerResponseDto { Consumer = null };
            }
            catch (Exception ex)
            {
                _consumerLogger.LogError(ex, "{className}.{methodName}: ERROR - msg: {msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return new GetConsumerResponseDto() { ErrorMessage = ex.Message, ErrorDescription = ex.InnerException?.Message };
            }
        }

        /// <summary>
        /// IConsumerService.GetConsumerByMemNbr impl
        /// </summary>
        /// <param name="consumerRequestDto"></param>
        /// <returns></returns>
        public async Task<GetConsumerByMemIdResponseDto> GetConsumerByMemId(GetConsumerByMemIdRequestDto consumerRequestDto)
        {
            var response = new GetConsumerByMemIdResponseDto();
            const string methodName = nameof(GetConsumerByMemId);
            try
            {
                _consumerLogger.LogInformation("{className}.{methodName}: Querying from {dbSource} database", className, methodName, DbSource);
                
                ConsumerModel? consumer;
                if (_readOnlySession != null)
                {
                    consumer = await ReadSession.QueryOver<ConsumerModel>()
                        .Where(x => x.TenantCode == consumerRequestDto.TenantCode
                            && x.MemberId == consumerRequestDto.MemberId && x.DeleteNbr == 0)
                        .SingleOrDefaultAsync();
                }
                else
                {
                    consumer = await _consumerRepo.FindOneAsync(x => x.TenantCode == consumerRequestDto.TenantCode
                        && x.MemberId == consumerRequestDto.MemberId && x.DeleteNbr == 0);
                }

                if (consumer != null && consumer.ConsumerId > 0)
                {
                    response = _mapper.Map<GetConsumerByMemIdResponseDto>(consumer);
                    _consumerLogger.LogInformation("{className}.{methodName}: Retrieved Consumer Data Successfully for Tenant: {tenant}, MemId: {MemId}", className, methodName,
                        consumerRequestDto.TenantCode, consumerRequestDto.MemberId);

                    return response;
                }
                return new GetConsumerByMemIdResponseDto();
            }
            catch (Exception ex)
            {
                _consumerLogger.LogError(ex, "{className}.{methodName}: ERROR - msg: {msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return new GetConsumerByMemIdResponseDto() { ErrorMessage = ex.Message, ErrorDescription = ex.InnerException?.Message };
            }
        }

        public async Task<ConsumersAndPersonsListResponseDto> GetConsumerByDOB(GetConsumerByTenantCodeAndDOBRequestDto consumerRequestDto)
        {
            var response = new ConsumersAndPersonsListResponseDto();
            const string methodName = nameof(GetConsumerByDOB);
            try
            {
                var consumerRecords = await _personRepo.GetConsumerPersonsByDOB(consumerRequestDto);

                if (consumerRecords == null || consumerRecords.Count == 0)
                {
                    _consumerLogger.LogError("{className}.{methodName}: ERROR - Consumer Records not Found for Tenant code:{TenantCode} , Error Code:{errorCode}", className, methodName, consumerRequestDto.TenantCode, StatusCodes.Status404NotFound);
                    return new ConsumersAndPersonsListResponseDto() { ErrorCode = StatusCodes.Status404NotFound };
                }

                foreach (var record in consumerRecords)
                {
                    var consumerAndPerson = new ConsumersAndPersons()
                    {
                        Consumer = _mapper.Map<ConsumerDto>(record.ConsumerModel),
                        Person = _mapper.Map<PersonDto>(record.PersonModel),
                    };
                    response.ConsumerAndPersons.Add(consumerAndPerson);
                }
                return response;
            }
            catch (Exception ex)
            {
                var errorMessage = $"Error occurred while fetching consumer and person details for TenantCode: {consumerRequestDto.TenantCode}.";
                _consumerLogger.LogError(ex, "{className}.{methodName}: ERROR - msg: {msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                throw new InvalidOperationException(errorMessage, ex);
            }
        }

        /// <summary>
        /// CreateConsumers
        /// </summary>
        /// <param name="consumersCreateRequestDto"></param>
        /// <returns></returns>
        public async Task<List<ConsumerDataResponseDto>> CreateConsumers(IList<ConsumerDataDto> consumersCreateRequestDto)
        {

            var consumerDataResponseList = new List<ConsumerDataResponseDto>();
            const string methodName = nameof(CreateConsumers);
            _consumerLogger.LogInformation("{ClassName}.{MethodName}: Started processing.", className, methodName);

            if (consumersCreateRequestDto == null || consumersCreateRequestDto.Count == 0)
            {
                _consumerLogger.LogError("{ClassName}.{MethodName}: Data is null or empty.", className, methodName);
                throw new InvalidOperationException("Data is null or empty.");
            }
            try
            {

                var tenantResponseCache = new Dictionary<string, TenantDto>();
                foreach (var item in consumersCreateRequestDto)
                {
                    var response = new ConsumerDataResponseDto();
                    using var transaction = _session.BeginTransaction();

                    try
                    {
                        // Get tenant response with caching
                        if (!tenantResponseCache.TryGetValue(item.Consumer.TenantCode!, out var tenantResponse))
                        {
                            var getTenantCodeRequestDto = new GetTenantCodeRequestDto { TenantCode = item.Consumer.TenantCode };
                            tenantResponse = await _tenantClient.Post<TenantDto>($"{Constant.GetTenantByTenantCode}", getTenantCodeRequestDto);

                            if (tenantResponse != null)
                                tenantResponseCache[item.Consumer.TenantCode!] = tenantResponse;
                        }

                        bool isIndividualWallet = false;
                        //Check to if deleted consumer reactivation enabled
                        bool reactivateDeletedConsumer = false;

                        if (tenantResponse?.TenantOption != null)
                        {
                            var tenantOption = JsonConvert.DeserializeObject<TenantOption>(tenantResponse.TenantOption);
                            reactivateDeletedConsumer = tenantOption?.BenefitsOptions?.ReactivateDeletedConsumer ?? false;
                        }

                        var tenantAttrs = string.IsNullOrEmpty(tenantResponse?.TenantAttribute)
                            ? null
                            : JsonConvert.DeserializeObject<TenantAttributeDto>(tenantResponse.TenantAttribute);

                        isIndividualWallet = tenantAttrs?.ConsumerWallet?.IndividualWallet ?? false;

                        var sourceMemberNbr = string.Empty;
                        var sourceSubscriberMemberNbr = string.Empty;
                        if (item.Consumer.SubscriberOnly)
                        {
                            sourceMemberNbr = item.Consumer.MemberNbr;
                            item.Consumer.MemberNbr = item.Consumer.SubscriberMemberNbr;
                        }
                        if (item.Consumer.IsSsoAuthenticated)
                        {
                            sourceSubscriberMemberNbr = item.Consumer.SubscriberMemberNbr;
                            item.Consumer.SubscriberMemberNbr = item.Consumer.MemberNbr;
                        }
                        // Only validate dependent consumer if it's not an individual wallet and consumer is different from subscriber
                        if (!isIndividualWallet && item.Consumer.MemberNbr != item.Consumer.SubscriberMemberNbr)
                        {
                            var consumerDataList = await _consumerRepo.FindAsync(x => x.MemberNbr == item.Consumer.SubscriberMemberNbr && x.DeleteNbr == 0 && x.TenantCode == item.Consumer.TenantCode);
                            var consumerData = consumerDataList?.OrderByDescending(x => x.ConsumerId)?.FirstOrDefault();
                            if (consumerData == null)
                            {
                                var errorMessage = $"CreateConsumers: Unable to find consumer through main subscriber, subscriber member number: {item.Consumer.SubscriberMemberNbr}, skipping dependent consumer creation";
                                _consumerLogger.LogError(errorMessage);
                                response.ErrorCode = StatusCodes.Status400BadRequest;
                                response.ErrorMessage = errorMessage;
                                consumerDataResponseList.Add(response);
                                continue;
                            }
                            item.Person.IsDependent = true;
                        }
                        else
                        {
                            item.Person.IsDependent = false;
                        }

                        // Check person exist
                        PersonModel? personModel = null;
                        var personUniqueIdentifier = item.Person!.PersonUniqueIdentifier?.Trim().ToLower();
                        personModel = await _personRepo.FindOneAsync(p => p.PersonUniqueIdentifier != null && p.PersonUniqueIdentifier == personUniqueIdentifier && p.DeleteNbr == 0);
                        if (personModel != null)
                        {
                            // Check person associated consumer exist
                            var consumer = await _consumerRepo.FindOneAsync(c => c.PersonId == personModel.PersonId && c.DeleteNbr == 0);
                            if (consumer != null)
                            {
                                consumerDataResponseList.Add(new ConsumerDataResponseDto()
                                {
                                    ErrorCode = 409,
                                    ErrorMessage = $"Member Number: {item.Consumer.MemberNbr}, PersonUniqueIdentifier: {item.Person.PersonUniqueIdentifier} is already exist"
                                });
                            }
                            _consumerLogger.LogInformation("Person already exists with PersonUniqueIdentifier: {PersonUniqueIdentifier}, PersonId: {PersonId}", personModel.PersonUniqueIdentifier, personModel.PersonId);
                        }

                        var consumerResponse = VerifyConsumerExist(item, reactivateDeletedConsumer);
                        // Member number already exists
                        if (consumerResponse?.ErrorCode == StatusCodes.Status202Accepted && item != null)
                        {
                            var request = new List<ConsumerDataDto>() { item };
                            var updateConsumerResponse = await UpdateConsumers(request, reactivateDeletedConsumer: reactivateDeletedConsumer);
                            updateConsumerResponse[0].ErrorCode = consumerResponse.ErrorCode;
                            updateConsumerResponse[0].ErrorMessage = consumerResponse.ErrorMessage;
                            _consumerLogger.LogInformation($"Updated Consumer to {updateConsumerResponse.ToJson()} as consumer already exists");
                            consumerDataResponseList.AddRange(updateConsumerResponse);
                            continue;
                        }

                        if (personModel == null)
                        {
                            personModel = await CreatePerson(item.Person);
                        }

                        if (personModel == null || personModel.PersonId <= 0)
                        {
                            var errorMessage = $"Unable to create person for MemberNbr: {item.Consumer.MemberNbr}";
                            _consumerLogger.LogError(errorMessage);
                            response.ErrorCode = StatusCodes.Status409Conflict;
                            response.ErrorMessage = errorMessage;
                            consumerDataResponseList.Add(response);
                            continue;
                        }
                        response.Person = _mapper.Map<PersonDto>(personModel);

                        var personAddresses = await CreatePersonAddresses(item.PersonAddresses, personModel.PersonId);
                        response.PersonAddresses = _mapper.Map<List<PersonAddressDto>>(personAddresses);

                        var phoneNumbers = await CreatePhoneNumbers(item.PhoneNumbers, personModel.PersonId);
                        response.PhoneNumbers = _mapper.Map<List<PhoneNumberDto>>(phoneNumbers);

                        var roleSubscriber = await _roleRepo.FindOneAsync(x => x.RoleName == "subscriber" && x.DeleteNbr == 0);

                        // Check person role exist
                        PersonRoleModel? personRole = null;
                        personRole = await _personRoleRepo.FindOneAsync(x => x.PersonId == personModel.PersonId && x.RoleId == roleSubscriber.RoleId);
                        if (personRole == null)
                        {
                            var tenanntSponsorCustomerResponse = await _tenantClient.Get<TenantSponsorCustomerResponseDto>($"{Constant.GetTenantSponsorCustomer}{item?.Consumer.TenantCode}", new Dictionary<string, long>());
                            if (item?.Consumer.TenantCode != null && tenanntSponsorCustomerResponse != null)
                            {
                                var personRoleModel = new PersonRoleModel()
                                {
                                    PersonId = personModel.PersonId,
                                    RoleId = roleSubscriber.RoleId,
                                    TenantCode = item?.Consumer.TenantCode,
                                    CustomerCode = tenanntSponsorCustomerResponse?.Customer?.CustomerCode,
                                    SponsorCode = tenanntSponsorCustomerResponse?.Sponsor?.SponsorCode,
                                    CreateTs = DateTime.UtcNow,
                                    CreateUser = Constants.CreateUser,
                                    DeleteNbr = 0,
                                };
                                personRole = await CreatePersonRole(personRoleModel);
                            }
                            else
                            {
                                personRole = await CreatePersonRole(personModel, roleSubscriber.RoleId);
                            }
                        }

                        if (personRole == null || personRole.RoleId <= 0)
                        {
                            var errorMessage = $"Unable to create person role for PersonId: {personModel.PersonId}, RoleId: {roleSubscriber.RoleId}";
                            _consumerLogger.LogError(errorMessage);
                            response.ErrorCode = StatusCodes.Status409Conflict;
                            response.ErrorMessage = errorMessage;
                            consumerDataResponseList.Add(response);
                            continue;
                        }

                        ConsumerModel consumerModel = await CreateConsumer(personModel, item.Consumer, sourceMemberNbr, sourceSubscriberMemberNbr);
                        if (consumerModel == null || consumerModel.ConsumerId <= 0)
                        {
                            var errorMessage = $"Unable to create consumer for MemberNbr: {item.Consumer.MemberNbr}";
                            _consumerLogger.LogError(errorMessage);
                            response.ErrorCode = StatusCodes.Status409Conflict;
                            response.ErrorMessage = errorMessage;
                            consumerDataResponseList.Add(response);
                            continue;
                        }
                        response.Consumer = _mapper.Map<ConsumerDto>(consumerModel);

                        await transaction.CommitAsync();
                        consumerDataResponseList.Add(response);

                        //await _eventService.PublishCohortEventToSNSTopic(consumerModel.TenantCode ?? string.Empty, consumerModel.ConsumerCode ?? string.Empty);
                    }
                    catch (Exception ex)
                    {
                        var errorMessage = $"Member Number {item.Consumer.MemberNbr}, Internal Server Error";
                        _consumerLogger.LogError(ex, errorMessage);
                        response.ErrorMessage = errorMessage;
                        response.ErrorCode = StatusCodes.Status500InternalServerError;
                        consumerDataResponseList.Add(response);

                        if (transaction != null && transaction.IsActive)
                        {
                            await transaction.RollbackAsync();
                        }
                        _session.Clear();
                    }
                }

                _consumerLogger.LogInformation($"Batch consumer list: {consumerDataResponseList.ToJson()}");
                var createdConsumers = consumerDataResponseList.Where(x => x.ErrorCode == null).Select(x => x.Consumer).ToList();
                _consumerLogger.LogInformation($"Created consumer list for event: {createdConsumers.ToJson()}");
                await _eventService.CreateConsumerHistoryEvent(createdConsumers, methodName);


            }
            catch (Exception ex)
            {
                _consumerLogger.LogError(ex, "{ClassName}.{MethodName}: Failed processing.", className, methodName);
                throw;
            }
            finally
            {
                _consumerLogger.LogInformation("{ClassName}.{MethodName}: Ended processing.", className, methodName);
            }
            return consumerDataResponseList;
        }


        public async Task<List<ConsumerDataResponseDto>> UpdateConsumers(IList<ConsumerDataDto> consumersUpdateRequestDto, bool isCancel = false, bool isDelete = false)
        {
            bool reactivateDeletedConsumer = false;

            //Check to if deleled consumer reactivation enabled
            if (!isCancel && !isDelete)
            {
                var tenantCode = consumersUpdateRequestDto.FirstOrDefault()?.Consumer.TenantCode;
                var getTenantCodeRequestDto = new GetTenantCodeRequestDto { TenantCode = tenantCode };
                var tenantResponse = await _tenantClient.Post<TenantDto>($"{Constant.GetTenantByTenantCode}", getTenantCodeRequestDto);
                if (tenantResponse?.TenantOption != null)
                {
                    var tenantOption = JsonConvert.DeserializeObject<TenantOption>(tenantResponse.TenantOption);
                    if (tenantOption?.BenefitsOptions != null)
                    {
                        reactivateDeletedConsumer = tenantOption.BenefitsOptions.ReactivateDeletedConsumer;
                    }
                }
            }
            var updatedConsumer = await UpdateConsumers(consumersUpdateRequestDto, isCancel, isDelete, reactivateDeletedConsumer);
            var ConsumerLst = updatedConsumer.Where(x => x.ErrorCode == null).Select(x => x.Consumer).ToList();
            await _eventService.CreateConsumerHistoryEvent(ConsumerLst, nameof(UpdateConsumers));
            return updatedConsumer;
        }

        /// <summary>
        /// UpdateConsumers
        /// </summary>
        /// <param name="consumersUpdateRequestDto"></param>
        /// <returns></returns>
        private async Task<List<ConsumerDataResponseDto>> UpdateConsumers(IList<ConsumerDataDto> consumersUpdateRequestDto, bool isCancel = false, bool isDelete = false, bool reactivateDeletedConsumer = false)
        {
            var consumerDataResponseDtoList = new List<ConsumerDataResponseDto>();
            const string methodName = nameof(UpdateConsumers);
            _consumerLogger.LogInformation("{ClassName}.{MethodName}: Started processing.", className, methodName);

            try
            {
                foreach (var item in consumersUpdateRequestDto)
                {
                    _consumerLogger.LogInformation($"Dto received for updating consumer {consumersUpdateRequestDto.ToJson()}");
                    var consumerDto = item.Consumer;
                    var personDto = item.Person;
                    var personAddressDto = item.PersonAddresses;
                    var phoneNumberDto = item.PhoneNumbers;
                    var consumerDataResponseDto = new ConsumerDataResponseDto();
                    using var transaction = _session.BeginTransaction();

                    try
                    {
                        if (consumerDto.SubscriberOnly)
                        {
                            consumerDto.MemberNbr = consumerDto.SubscriberMemberNbr;
                        }
                        if (consumerDto.IsSsoAuthenticated)
                        {
                            consumerDto.SubscriberMemberNbr = consumerDto.MemberNbr;
                        }
                        var (consumerModel, statusCode, message) = await PrepareConsumerUpdateRequest(consumerDto, isCancel, isDelete, reactivateDeletedConsumer);
                        if (consumerModel == null || consumerModel.ConsumerId <= 0)
                        {
                            consumerDataResponseDto.ErrorCode = statusCode;
                            consumerDataResponseDto.ErrorMessage = message;
                            consumerDataResponseDtoList.Add(consumerDataResponseDto);
                            continue;
                        }
                        await _session.UpdateAsync(consumerModel);

                        // If PersonId is less then or equal to zero then get it from consumer model 
                        personDto.PersonId = personDto.PersonId <= 0 ? consumerModel.PersonId : personDto.PersonId;

                        var (personModel, syncOptions, errorCode, errorMessage) = await PreparePersonUpdateRequest(personDto, isCancel, isDelete, consumerDto.IsSsoAuthenticated, reactivateDeletedConsumer);

                        if (personModel == null || personModel.PersonId <= 0 || (errorCode != 200 && !isCancel))
                        {
                            consumerDataResponseDto.ErrorCode = errorCode;
                            consumerDataResponseDto.ErrorMessage = errorMessage;
                            consumerDataResponseDtoList.Add(consumerDataResponseDto);
                            _consumerLogger.LogError(errorMessage);
                            continue;
                        }

                        await _session.UpdateAsync(personModel);
                        var (personAddressModel, personAddressStatusCode, personAddressMessage, addressSyncOptions) = await PreparePersonAddressUpdateRequest(personAddressDto, personDto.PersonId, isCancel, isDelete, reactivateDeletedConsumer);
                        var (phoneNumberModel, phoneNumberStatusCode, phoneNumberMessage) = await PreparePhoneNumberUpdateRequest(phoneNumberDto, personDto.PersonId, isCancel, isDelete , reactivateDeletedConsumer);

                        if (personAddressModel != null && personAddressModel.Count > 0 && personAddressStatusCode == 200)
                        {
                            foreach (var model in personAddressModel)
                            {
                                await _session.UpdateAsync(model);
                            }
                        }

                        if (phoneNumberModel != null && phoneNumberModel.Count > 0 && phoneNumberStatusCode == 200)
                        {
                            foreach (var number in phoneNumberModel)
                            {
                                await _session.UpdateAsync(number);
                            }
                        }

                        await CancelDependentConsumers(isCancel, consumerDto, consumerModel, personModel);

                        await transaction.CommitAsync();
                        if (!string.IsNullOrEmpty(addressSyncOptions))
                        {
                            syncOptions?.Add(addressSyncOptions);
                        }

                        consumerDataResponseDto.Person = _mapper.Map<PersonDto>(personModel);
                        consumerDataResponseDto.Consumer = _mapper.Map<ConsumerDto>(consumerModel);
                        consumerDataResponseDto.PersonAddresses = _mapper.Map<List<PersonAddressDto>>(personAddressModel);
                        consumerDataResponseDto.PhoneNumbers = _mapper.Map<List<PhoneNumberDto>>(phoneNumberModel);
                        consumerDataResponseDto.Person.SyncOptions = syncOptions;
                        consumerDataResponseDto.Person.SyncRequired = syncOptions?.Count > 0;
                        _consumerLogger.LogInformation($"Updated Consumer to {consumerDataResponseDto.ToJson()}");
                        consumerDataResponseDtoList.Add(consumerDataResponseDto);

                        //await _eventService.PublishCohortEventToSNSTopic(consumerModel.TenantCode ?? string.Empty, consumerModel.ConsumerCode ?? string.Empty);
                    }
                    catch (Exception ex)
                    {
                        consumerDataResponseDto.ErrorCode = StatusCodes.Status500InternalServerError;
                        consumerDataResponseDto.ErrorMessage = $"Member Number {consumerDto.MemberNbr}, Internal Server Error";
                        consumerDataResponseDtoList.Add(consumerDataResponseDto);
                        await transaction.RollbackAsync();
                        _session.Clear();
                        _consumerLogger.LogError(ex, "{ClassName}.{MethodName}: Failed processing.", className, methodName);
                    }
                }
            }
            catch (Exception ex)
            {
                _consumerLogger.LogError(ex, "{ClassName}.{MethodName}: Failed processing.", className, methodName);
                throw;
            }
            finally
            {
                _consumerLogger.LogInformation("{ClassName}.{MethodName}: Ended processing.", className, methodName);
            }
            return consumerDataResponseDtoList;
        }

        private (bool IsValid, string? ErrorMessage) ValidateUpdateAgreementStatusDto(UpdateAgreementStatusDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.TenantCode))
                return (false, "TenantCode is required.");

            if (string.IsNullOrWhiteSpace(dto.ConsumerCode))
                return (false, "ConsumerCode is required.");

            if (!Enum.IsDefined(typeof(AgreementStatus), dto.AgreementStatus))
                return (false, $"Invalid AgreementStatus value. Allowed values: {string.Join(", ", Enum.GetNames(typeof(AgreementStatus)))}");

            return (true, null);
        }

        private async Task CancelDependentConsumers(bool isCancel, ConsumerDto consumerDto, ConsumerETLModel? consumerModel, PersonModel? personModel)
        {
            var methodName = nameof(CancelDependentConsumers);
            var cancelledDependentConsumers = new List<ConsumerModel>();
            if (personModel != null && consumerModel != null && !personModel.IsSpouse && !personModel.IsDependent && isCancel)
            {
                var dependentConsumers = await _consumerRepo.FindAsync(consumer => consumer.TenantCode == consumerModel.TenantCode && consumer.SubscriberMemberNbr == consumerModel.MemberNbr && consumer.DeleteNbr == 0);
                if (dependentConsumers == null || dependentConsumers.Count < 1)
                {
                    _consumerLogger.LogInformation($"UpdateDependentsDeactivatioRequest:Consumer record not found  for  consumerId {consumerDto.ConsumerId}");

                }
                else
                {
                    foreach (var consumerdata in dependentConsumers)
                    {
                        var person = await _personRepo.FindOneAsync(x => x.PersonId == consumerdata.PersonId);

                        // Update person properties
                        person.UpdateTs = DateTime.UtcNow;
                        person.UpdateUser = Constants.CreateUser;
                        consumerdata.UpdateTs = DateTime.UtcNow;
                        consumerdata.UpdateUser = Constants.CreateUser;
                        person.DeleteNbr = personModel.PersonId;
                        consumerdata.DeleteNbr = consumerdata.ConsumerId;

                        var (personAddressModel, statusCode, message) = await PreparePersonAddressCancelRequest(consumerdata.PersonId);
                        if (personAddressModel != null && personAddressModel.Count > 0 && statusCode == 200)
                        {
                            foreach (var model in personAddressModel)
                            {
                                await _session.UpdateAsync(model);
                            }
                        }

                        var (phoneNumberModel, phoneNumberStatusCode, phoneNumberMessage) = await PreparePhoneNumberCancelRequest(consumerdata.PersonId);


                        if (phoneNumberModel != null && phoneNumberModel.Count > 0 && phoneNumberStatusCode == 200)
                        {
                            foreach (var number in phoneNumberModel)
                            {
                                await _session.UpdateAsync(number);
                            }
                        }
                        await _session.UpdateAsync(person);
                        await _session.UpdateAsync(consumerdata);
                        cancelledDependentConsumers.Add(consumerdata);
                        _consumerLogger.LogInformation($"Updating  dependent for consumercode: {consumerModel.ConsumerCode}");
                    }
                    var ConsumerLst = _mapper.Map<List<ConsumerDto>>(cancelledDependentConsumers);
                    await _eventService.CreateConsumerHistoryEvent(ConsumerLst, methodName);
                }

            }
        }

        private ConsumerDataResponseDto VerifyConsumerExist(ConsumerDataDto item, bool reactivateDeletedConsumer = false)
        {
            ConsumerDataResponseDto consumerDataResponseDto = new ConsumerDataResponseDto();
            var memnbrExist = IsMemberIdExist(item.Consumer, reactivateDeletedConsumer).Result;
            if (memnbrExist)
            {
                consumerDataResponseDto.ErrorCode = 202;
                consumerDataResponseDto.ErrorMessage = $"Member Number: {item.Consumer.MemberId} is already exist.Hence Updated consumer and person";
                return consumerDataResponseDto;
            }

            return consumerDataResponseDto;

        }

        /// <summary>
        /// PrepareConsumerUpdateRequest
        /// </summary>
        /// <param name="consumerDto"></param>
        /// <param name="isCancel"></param>
        /// <returns></returns>
        private async Task<(ConsumerETLModel?, int, string)> PrepareConsumerUpdateRequest(ConsumerDto consumerDto, bool isCancel = false, bool isDelete = false, bool reactivateDeletedConsumer = false)
        {
            ConsumerETLModel? consumerModel = null;
            int statusCode = StatusCodes.Status200OK;
            string message = string.Empty;

            try
            {
                // Attempt to find the consumer in the repository
                consumerModel = await _consumerETLRepo.FindOneAsync(x => x.TenantCode == consumerDto.TenantCode && x.MemberId == consumerDto.MemberId, reactivateDeletedConsumer);
                _consumerLogger.LogInformation($"Found consumerModel {consumerModel?.ToJson()} for MemberId: {consumerDto.MemberId}, reactivateDeletedConsumer: {reactivateDeletedConsumer}");
                if (consumerModel == null || consumerModel.ConsumerId <= 0)
                {
                    var consumerModelList = await _consumerETLRepo.FindAsync(x => x.TenantCode == consumerDto.TenantCode && x.MemberId == consumerDto.MemberId, true);
                    consumerModel = consumerModelList?.Where(x => x.DeleteNbr != 0)?.FirstOrDefault();
                    statusCode = StatusCodes.Status404NotFound;
                    message = $"Unable to find consumer for MemberId: {consumerDto.MemberId}";
                    _consumerLogger.LogError(message);
                    return (consumerModel, statusCode, message); // Return immediately on error
                }


                // Update consumer properties
                consumerModel.UpdateTs = DateTime.UtcNow;
                consumerModel.UpdateUser = Constants.CreateUser;
                consumerModel.IsSSOUser = consumerDto.IsSSOUser;
                if (isCancel)
                {
                    consumerModel.DeleteNbr = consumerModel.ConsumerId;
                }
                else if (reactivateDeletedConsumer)
                {
                    consumerModel.DeleteNbr = 0;
                }
                if (isDelete)
                {
                    return DeleteConsumer(consumerDto, consumerModel, ref statusCode, ref message);
                }
                else
                {
                    // existing JSON from DB
                    var existingJson = string.IsNullOrWhiteSpace(consumerModel.ConsumerAttribute)
                        ? new JObject()
                        : SafeParse(consumerModel.ConsumerAttribute);

                    // incoming update JSON (from API request)
                    var updateJson = SafeParse(consumerDto.ConsumerAttribute);

                    // Merge: only updates keys that are present in updateJson
                    existingJson.Merge(updateJson, new JsonMergeSettings
                    {
                        MergeArrayHandling = MergeArrayHandling.Replace,
                        MergeNullValueHandling = MergeNullValueHandling.Ignore
                    });

                    // Save back
                    consumerModel.ConsumerAttribute = existingJson.ToString();

                    consumerModel.EligibleStartTs = consumerDto.EligibleStartTs;
                    consumerModel.EligibleEndTs = consumerDto.EligibleEndTs;
                    consumerModel.RegionCode = consumerDto.RegionCode;
                    consumerModel.PlanId = consumerDto.PlanId;
                    consumerModel.SubgroupId = consumerDto.SubgroupId;
                    consumerModel.PlanType = consumerDto.PlanType;
                    consumerModel.MemberNbrPrefix = consumerDto.MemberNbrPrefix;
                    consumerModel.SubsciberMemberNbrPrefix = consumerDto.SubsciberMemberNbrPrefix;
                    consumerModel.MemberId = consumerDto.MemberId;
                    consumerModel.MemberNbr = consumerDto.MemberNbr;   // Updated for KP leading zeros 
                    consumerModel.MemberType = consumerDto.MemberType;
                    consumerModel.SubscriptionStatusJson = consumerDto.SubscriptionStatusJson;
                }
            }
            catch (Exception ex)
            {
                statusCode = StatusCodes.Status500InternalServerError;
                message = $"Failed to process for consumer for MemberId: {consumerDto.MemberId}";
                _consumerLogger.LogError(ex, message);
            }
            return (consumerModel, statusCode, message);
        }



        JObject SafeParse(string? json)
        {
            _consumerLogger.LogInformation("Parsing JSON: {Json}", json);
            if (string.IsNullOrWhiteSpace(json))
                return new JObject();

            try
            {
                var token = JToken.Parse(json);

                // If parsed JSON is not an object, convert to object
                if (token is JObject obj)
                    return obj;

                return new JObject(); // or wrap primitive
            }
            catch
            {
                _consumerLogger.LogError("Failed to parse JSON: {Json}", json);
                return new JObject(); // fallback on any parse failure
            }
        }


        private (ConsumerETLModel?, int, string) DeleteConsumer(ConsumerDto consumerDto, ConsumerETLModel? consumerModel, ref int statusCode, ref string message)
        {
            if (consumerDto.EligibleEndTs >= consumerModel?.EligibleStartTs)
            {
                consumerModel.UpdateTs = DateTime.UtcNow;
                consumerModel.UpdateUser = Constants.CreateUser;
                consumerModel.EligibleEndTs = consumerDto.EligibleEndTs;
                return (consumerModel, statusCode, message);
            }
            else
            {
                statusCode = StatusCodes.Status400BadRequest;
                message = $"EligibleEndTs should be grater than EligibleStartTs, MemberId - {consumerDto.MemberId}, EligibleStartTs - {consumerModel?.EligibleStartTs}, EligibleEndTs - {consumerDto.EligibleEndTs}";
                _consumerLogger.LogError(message);
                return (consumerModel, statusCode, message);
            }
        }

        /// <summary>
        /// PreparePersonUpdateRequest
        /// </summary>
        /// <param name="personDto"></param>
        /// <param name="isCancel"></param>
        /// <param name="isDelete"></param>
        /// <returns></returns>
        private async Task<(PersonModel?, List<string>?, int, string)> PreparePersonUpdateRequest(PersonDto personDto, bool isCancel = false, bool isDelete = false, bool IsSsoAuthenticated = false, bool reactivateDeletedConsumer = false)
        {
            int statusCode = StatusCodes.Status200OK;
            string message = string.Empty;
            PersonModel? personModel = null;
            List<string>? syncOptions = null;

            try
            {
                // Attempt to find the person in the repository
                personModel = await _personRepo.FindOneAsync(x => x.PersonId == personDto.PersonId , reactivateDeletedConsumer);
                
                if(personModel == null || personModel.PersonId <= 0)
                {
                    statusCode = StatusCodes.Status404NotFound;
                    message = $"Unable to find consumer for PersonId: {personDto.PersonId}";
                    _consumerLogger.LogError(message);
                    return (personModel, syncOptions, statusCode, message);
                }


                // Update person properties
                personModel.UpdateTs = DateTime.UtcNow;
                personModel.UpdateUser = Constants.CreateUser;

                if (isCancel)
                {
                    personModel.DeleteNbr = personModel.PersonId;
                }
                if (reactivateDeletedConsumer)
                {
                    personModel.DeleteNbr = 0;
                }
                if (isDelete)
                {
                    return (personModel, syncOptions, statusCode, message);
                }
                else
                {
                    // Get only sync options for the update name, dob, address
                    syncOptions = GetSyncOptions(personDto, personModel);

                    // Update person properties
                    personModel.FirstName = personDto.FirstName;
                    personModel.LastName = personDto.LastName;
                    personModel.LanguageCode = personDto.LanguageCode;
                    personModel.Gender = personDto.Gender;
                    personModel.DOB = personDto.DOB;
                    personModel.YearOfBirth = Convert.ToInt32(personDto.DOB.Year);
                    personModel.PhoneNumber = personDto.PhoneNumber;
                    personModel.MailingAddressLine1 = personDto.MailingAddressLine1;
                    personModel.MailingAddressLine2 = personDto.MailingAddressLine2;
                    personModel.MailingState = personDto.MailingState;
                    personModel.MailingCountryCode = personDto.MailingCountryCode;
                    personModel.HomePhoneNumber = personDto.HomePhoneNumber;
                    personModel.IsDependent = personDto.IsDependent;
                    personModel.IsSpouse = personDto.IsSpouse;
                    personModel.City = personDto.City;
                    personModel.Country = personDto.Country;
                    personModel.PostalCode = personDto.PostalCode;
                    personModel.UpdateTs = DateTime.UtcNow;
                    personModel.UpdateUser = Constants.CreateUser;
                    personModel.SyntheticUser = personDto.SyntheticUser;
                    personModel.MiddleName = personDto.MiddleName;
                    personModel.Email = personDto.Email?.ToLower();
                    personModel.Age = personDto.Age;
                }
            }
            catch (Exception ex)
            {
                statusCode = StatusCodes.Status500InternalServerError;
                message = $"Failed to process for PersonId: {personDto.PersonId}";
                _consumerLogger.LogError(ex, message);
            }
            return (personModel, syncOptions, statusCode, message);
        }

        /// <summary>
        /// Compares the properties of a PersonDto and a PersonModel to determine if any
        /// sync options should be applied based on detected changes.
        /// </summary>
        /// <param name="personDto">The DTO containing the updated person updateResponse.</param>
        /// <param name="person">The model representing the current person updateResponse.</param>
        /// <returns>A list of sync options indicating what changes were detected, or null if no changes were found.</returns>
        private List<string> GetSyncOptions(PersonDto personDto, PersonModel person)
        {
            List<string> syncOptions = [];

            if (personDto.DOB != person.DOB)
            {
                syncOptions.Add(SyncOptions.DOB_CHANGE.ToString());
            }

            if (personDto.FirstName != person.FirstName || personDto.LastName != person.LastName)
            {
                syncOptions.Add(SyncOptions.NAME_CHANGE.ToString());
            }

            return syncOptions;
        }

        private string GetSyncOptionsForAddressChange(PersonAddressDto personAddressDto, PersonAddressModel personAddressModel)
        {
            var syncOptions = string.Empty;
            bool isAddressChanged = personAddressDto.Line1 != personAddressModel.Line1
                                 || personAddressDto.Line2 != personAddressModel.Line2
                                 || personAddressDto.State != personAddressModel.State
                                 || personAddressDto.CountryCode != personAddressModel.CountryCode
                                 || personAddressDto.City != personAddressModel.City
                                 || personAddressDto.Country != personAddressModel.Country
                                 || personAddressDto.PostalCode != personAddressModel.PostalCode;

            if (isAddressChanged)
            {
                syncOptions = SyncOptions.ADDRESS_CHANGE.ToString();
            }

            return syncOptions;
        }

        private async Task<(List<PersonAddressModel>?, int, string, string)> PreparePersonAddressUpdateRequest(List<PersonAddressDto> personAddresses, long personId, bool isCancel = false, bool isDelete = false, bool reactivateDeletedConsumer = false)
        {
            int statusCode = StatusCodes.Status200OK;
            string message = string.Empty;
            List<PersonAddressModel>? personAddressModels = new List<PersonAddressModel>();
            var syncOptions = string.Empty;
            try
            {
                if (personAddresses == null || personAddresses.Count == 0)
                {
                    return (personAddressModels, statusCode, message, syncOptions);
                }

                if (isCancel)
                {
                    (personAddressModels, statusCode, message) = await PreparePersonAddressCancelRequest(personId);
                    return (personAddressModels, statusCode, message, syncOptions);
                }
                else if (!isDelete)
                {
                    foreach (var address in personAddresses)
                    {
                        var personAddressModel = await _personAddressRepo.FindOneAsync(x =>
                            x.PersonId == personId &&
                            x.AddressTypeId == address.AddressTypeId &&
                            x.Source == address.Source , reactivateDeletedConsumer);

                        if (personAddressModel != null)
                        {
                            personAddressModel.Line1 = address.Line1;
                            personAddressModel.Line2 = address.Line2;
                            personAddressModel.City = address.City;
                            personAddressModel.State = address.State;
                            personAddressModel.PostalCode = address.PostalCode;
                            personAddressModel.CountryCode = address.CountryCode;
                            personAddressModel.UpdateTs = DateTime.UtcNow;
                            personAddressModel.UpdateUser = Constants.CreateUser;
                            personAddressModel.DeleteNbr = 0;
                            personAddressModels.Add(personAddressModel);

                            if (personAddressModel.AddressTypeId == (long)AddressTypeEnum.MAILING && personAddressModel.IsPrimary)
                            {
                                syncOptions = GetSyncOptionsForAddressChange(address, personAddressModel);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                statusCode = StatusCodes.Status500InternalServerError;
                message = $"Failed to process addresses for PersonId: {personId}";
                _consumerLogger.LogError(ex, message);
            }
            return (personAddressModels, statusCode, message, syncOptions);
        }

        private async Task<(List<PersonAddressModel>?, int, string)> PreparePersonAddressCancelRequest(long personId)
        {
            int statusCode = StatusCodes.Status200OK;
            string message = string.Empty;
            List<PersonAddressModel> personAddressModels = new List<PersonAddressModel>();
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
                            personAddressModels.Add(address);
                        }
                    }
                }

                return (personAddressModels, statusCode, message);
            }
            catch (Exception ex)
            {
                statusCode = StatusCodes.Status500InternalServerError;
                message = $"Failed to process cancel request for addresses with PersonId: {personId}";
                _consumerLogger.LogError(ex, message);
            }
            return (personAddressModels, statusCode, message);
        }

        private async Task<(List<PhoneNumberModel>?, int, string)> PreparePhoneNumberUpdateRequest(List<PhoneNumberDto> phoneNumbers, long personId, bool isCancel = false, bool isDelete = false , bool reactivateDeletedConsumer = false)
        {
            int statusCode = StatusCodes.Status200OK;
            string message = string.Empty;
            List<PhoneNumberModel>? phoneNumberModels = new List<PhoneNumberModel>();
            try
            {
                if (phoneNumbers == null || phoneNumbers.Count == 0)
                {
                    return (phoneNumberModels, statusCode, message);
                }


                if (isCancel)
                {
                    (phoneNumberModels, statusCode, message) = await PreparePhoneNumberCancelRequest(personId);
                    return (phoneNumberModels, statusCode, message);
                }
                else if (!isDelete)
                {
                    foreach (var number in phoneNumbers)
                    {
                        var phoneNumberModel = await _phoneNumberRepo.FindOneAsync(x =>
                            x.PersonId == personId &&
                            x.PhoneTypeId == number.PhoneTypeId &&
                            x.Source == number.Source , reactivateDeletedConsumer);

                        if (phoneNumberModel != null)
                        {
                            phoneNumberModel.DeleteNbr = 0;
                            phoneNumberModel.PhoneNumber = number.PhoneNumber;
                            phoneNumberModel.UpdateTs = DateTime.UtcNow;
                            phoneNumberModel.UpdateUser = Constants.CreateUser;
                            phoneNumberModels.Add(phoneNumberModel);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                statusCode = StatusCodes.Status500InternalServerError;
                message = $"Failed to process phone numbers for PersonId: {personId}";
                _consumerLogger.LogError(ex, message);
            }
            return (phoneNumberModels, statusCode, message);
        }

        private async Task<(List<PhoneNumberModel>?, int, string)> PreparePhoneNumberCancelRequest(long personId)
        {
            int statusCode = StatusCodes.Status200OK;
            string message = string.Empty;
            List<PhoneNumberModel> phoneNumberModels = new List<PhoneNumberModel>();
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
                            phoneNumberModels.Add(number);
                        }
                    }
                }

                return (phoneNumberModels, statusCode, message);
            }
            catch (Exception ex)
            {
                statusCode = StatusCodes.Status500InternalServerError;
                message = $"Failed to process cancel request for phone number with PersonId: {personId}";
                _consumerLogger.LogError(ex, message);
            }
            return (phoneNumberModels, statusCode, message);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="person"></param>
        /// <param name="subscriberRoleId"></param>
        /// <returns></returns>
        private async Task<PersonRoleModel> CreatePersonRole(PersonModel person, long subscriberRoleId)
        {
            try
            {
                var personRole = new PersonRoleModel()
                {
                    PersonId = person.PersonId,
                    RoleId = subscriberRoleId,
                    CreateTs = DateTime.UtcNow,
                    CreateUser = Constants.CreateUser,
                    UpdateTs = DateTime.UtcNow,
                    DeleteNbr = 0,
                };
                _consumerLogger.LogInformation($"CreatePersonRole with role={personRole.ToJson()}");
                await _session.SaveAsync(personRole);

                return personRole;
            }
            catch (Exception ex)
            {
                _consumerLogger.LogError(ex, "CreatePersonRole Error");
                throw;
            }
        }

        private async Task<PersonRoleModel> CreatePersonRole(PersonRoleModel personRoleModel)
        {
            await _session.SaveAsync(personRoleModel);
            return personRoleModel;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="person"></param>
        /// <param name="enrollment"></param>
        /// <param name="tenantCode"></param>
        /// <returns></returns>
        private async Task<ConsumerModel> CreateConsumer(PersonModel person, ConsumerDto consumerDto, string? sourceMemberNbr = null, string? sourceSubscriberMemberNbr = null)
        {
            try
            {

                string? consumerCode = consumerDto.ConsumerCode;
                string? anonymousCode = consumerDto.AnonymousCode;
                _consumerLogger.LogInformation($"CreateConsumer with consumerDto={consumerDto.ToJson()}");
                if (string.IsNullOrEmpty(consumerCode))
                    consumerCode = $"cmr-{Guid.NewGuid().ToString().Replace("-", "")}";

                if (string.IsNullOrEmpty(anonymousCode))
                    anonymousCode = "anc-" + Guid.NewGuid().ToString("N");

                var consumer = new ConsumerModel()
                {
                    PersonId = person.PersonId,
                    TenantCode = consumerDto.TenantCode,
                    ConsumerCode = consumerCode,
                    Eligible = false,
                    EligibleStartTs = consumerDto.EligibleStartTs,
                    EligibleEndTs = consumerDto.EligibleEndTs,
                    Registered = false,
                    RegistrationTs = consumerDto.RegistrationTs,
                    CreateTs = DateTime.UtcNow,
                    CreateUser = Constants.CreateUser,
                    MemberNbr = consumerDto.MemberNbr,
                    SubscriberMemberNbr = consumerDto.SubscriberMemberNbr,
                    AnonymousCode = anonymousCode,
                    EnrollmentStatus = EnrollmentStatus.UNKNOWN.ToString(),
                    EnrollmentStatusSource = EnrollmentStatusSource.UNKNOWN.ToString(),
                    OnBoardingState = OnboardingState.NOT_STARTED.ToString(),
                    AgreementStatus = Constant.NotVerified.ToString(),
                    RegionCode = consumerDto.RegionCode,
                    PlanId = consumerDto.PlanId,
                    PlanType = consumerDto.PlanType,
                    SubgroupId = consumerDto.SubgroupId,
                    SubsciberMemberNbrPrefix = consumerDto.SubsciberMemberNbrPrefix,
                    MemberNbrPrefix = consumerDto.MemberNbrPrefix,
                    IsSSOUser = consumerDto.IsSSOUser,
                    Auth0UserName = consumerDto.Auth0UserName,
                    MemberId = consumerDto.MemberId,
                    MemberType = consumerDto.MemberType,
                    ConsumerAttribute = consumerDto.ConsumerAttribute,
                    SubscriptionStatusJson = consumerDto.SubscriptionStatusJson
                };
                if (consumerDto.SubscriberOnly || consumerDto.IsSsoAuthenticated)
                {
                    JObject consumerAttribute;

                    if (string.IsNullOrWhiteSpace(consumerDto.ConsumerAttribute))
                    {
                        // Case 1: No JSON -> create new JObject
                        consumerAttribute = new JObject
                        {
                            ["SourceMemberNbr"] = sourceMemberNbr,
                            ["SourceSubscriberMemberNbr"] = sourceSubscriberMemberNbr
                        };
                    }
                    else
                    {
                        // Deserialize existing JSON (handles {} or populated JSON)
                        consumerAttribute = JObject.Parse(consumerDto.ConsumerAttribute);

                        // Add or update fields dynamically
                        consumerAttribute["SourceMemberNbr"] = sourceMemberNbr;
                        consumerAttribute["SourceSubscriberMemberNbr"] = sourceSubscriberMemberNbr;
                    }

                    consumer.ConsumerAttribute = consumerAttribute.ToString(Formatting.None);
                }


                _consumerLogger.LogInformation($"CreateConsumer with consumer={consumer.ToJson()}");
                await _session.SaveAsync(consumer);

                return consumer;
            }
            catch (Exception ex)
            {
                _consumerLogger.LogError(ex, "{className}.CreateConsumer Error", className);
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="person"></param>
        /// <returns></returns>
        private async Task<PersonModel> CreatePerson(PersonDto person)
        {
            try
            {
                // Create the person_code
                string? personCode = person.PersonCode;
                if (string.IsNullOrEmpty(personCode))
                {
                    personCode = "per-" + Guid.NewGuid().ToString("N");
                }

                var personModel = new PersonModel
                {
                    PersonCode = personCode,
                    FirstName = person.FirstName,
                    LastName = person.LastName,
                    LanguageCode = person.LanguageCode,
                    MemberSince = DateTime.UtcNow,
                    CreateTs = DateTime.UtcNow,
                    CreateUser = Constants.CreateUser,
                    Email = person.Email?.ToLower(),
                    City = person.City,
                    Country = person.Country ?? "US",
                    YearOfBirth = Convert.ToInt32(person.DOB.Year),
                    PostalCode = person.PostalCode,
                    PhoneNumber = person.PhoneNumber,
                    Region = " ",
                    Gender = person.Gender,
                    DOB = person.DOB,
                    IsSpouse = person.IsSpouse,
                    IsDependent = person.IsDependent,
                    SSN = "test",
                    SSNLast4 = person.SSNLast4,
                    MailingAddressLine1 = person.MailingAddressLine1,
                    MailingAddressLine2 = person.MailingAddressLine2,
                    MailingState = person.MailingState,
                    MailingCountryCode = person.MailingCountryCode,
                    HomePhoneNumber = person.HomePhoneNumber,
                    OnBoardingState = person.OnBoardingState,
                    SyntheticUser = person.SyntheticUser,
                    MiddleName = person.MiddleName,
                    PersonUniqueIdentifier = person.PersonUniqueIdentifier,
                    Age = person.Age,

                };

                await _session.SaveAsync(personModel);
                _consumerLogger.LogInformation($"CreatePerson with person={personModel.ToJson()}");

                return personModel;
            }
            catch (Exception ex)
            {
                _consumerLogger.LogError(ex, "{className}.CreatePerson Error", className);
                throw;
            }
        }

        private async Task<List<PersonAddressModel>> CreatePersonAddresses(List<PersonAddressDto> personAddresses, long personId)
        {
            try
            {
                List<PersonAddressModel> personAddressModels = new List<PersonAddressModel>();
                if (personAddresses == null || personAddresses.Count == 0)
                {
                    return personAddressModels;
                }
                foreach (var address in personAddresses)
                {
                    if (!await ValidateAddressType(address.AddressTypeId))
                    {
                        _consumerLogger.LogError("{className}.CreatePersonAddresses Error - Invalid Address Type", className);
                        continue;
                    }
                    var personAddressModel = _mapper.Map<PersonAddressModel>(address);
                    personAddressModel.PersonId = personId;
                    personAddressModel.CreateTs = DateTime.UtcNow;
                    personAddressModel.CreateUser = Constants.CreateUser;
                    await _session.SaveAsync(personAddressModel);
                    _consumerLogger.LogInformation($"CreatePersonAddresses with personAddress={personAddressModel.ToJson()}");
                    personAddressModels.Add(personAddressModel);
                }
                return personAddressModels;
            }
            catch (Exception ex)
            {
                _consumerLogger.LogError(ex, "{className}.CreatePersonAddresses Error", className);
                throw;
            }
        }

        private async Task<bool> ValidateAddressType(long addressTypeId)
        {
            try
            {
                var addressType = await _addressTypeService.GetAddressTypeById(addressTypeId);
                if (addressType.ErrorCode != null)
                {
                    _consumerLogger.LogError("Invalid address type.");
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                _consumerLogger.LogError(ex, "{className}.ValidateAddressType Error", className);
                throw;
            }
        }

        private async Task<List<PhoneNumberModel>> CreatePhoneNumbers(List<PhoneNumberDto> phoneNumbers, long personId)
        {
            try
            {
                List<PhoneNumberModel> phoneNumberModels = new List<PhoneNumberModel>();
                if (phoneNumbers == null || phoneNumbers.Count == 0)
                {
                    return phoneNumberModels;
                }
                foreach (var number in phoneNumbers)
                {
                    var phoneNumberModel = _mapper.Map<PhoneNumberModel>(number);
                    phoneNumberModel.PersonId = personId;
                    phoneNumberModel.PhoneNumberCode = number.PhoneNumberCode ?? "pnc-" + Guid.NewGuid().ToString("N");
                    phoneNumberModel.CreateTs = DateTime.UtcNow;
                    phoneNumberModel.CreateUser = Constants.CreateUser;
                    await _session.SaveAsync(phoneNumberModel);
                    _consumerLogger.LogInformation($"CreatePhoneNumber with phoneNumber={phoneNumberModel.ToJson()}");
                    phoneNumberModels.Add(phoneNumberModel);
                }
                return phoneNumberModels;
            }
            catch (Exception ex)
            {
                _consumerLogger.LogError(ex, "{className}.CreatePhoneNumber Error", className);
                throw;
            }
        }

        public async Task<ConsumerModel> updateRegisterFlag(ConsumerDto consumer)
        {
            const string methodName = nameof(updateRegisterFlag);
            try
            {
                var Consumer = await _consumerRepo.FindOneAsync(x => x.ConsumerId == consumer.ConsumerId && x.DeleteNbr == 0);

                using (var transaction = _session.BeginTransaction())
                {
                    try
                    {
                        Consumer.Registered = true;
                        await _session.UpdateAsync(Consumer);
                        await transaction.CommitAsync();
                        _consumerLogger.LogInformation("{className}.{methodName}: Registered Flag Updated Successfully for Registered : {Registered}", className, methodName, consumer.Registered);
                        var ConsumerLst = new List<ConsumerDto>() { consumer };
                        await _eventService.CreateConsumerHistoryEvent(ConsumerLst, methodName);
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        _consumerLogger.LogError(ex, "{className}.{methodName}: Error updating Registered Flag for Registered: {Registered}", className, methodName, consumer.Registered);
                        throw;
                    }

                }

                return Consumer;
            }
            catch (Exception ex)
            {
                _consumerLogger.LogError(ex, "{className}.{methodName}: Error", className, methodName);
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="consumerAttributesRequestDto"></param>
        /// <returns></returns>
        public async Task<ConsumerAttributesResponseDto> ConsumerAttributes(ConsumerAttributesRequestDto consumerAttributesRequestDto)
        {
            ConsumerDto? consumer = new();
            List<ConsumerDto> consumersList = new();
            const string methodName = nameof(ConsumerAttributes);
            try
            {

                List<ConsumerAttributeDetailDto>? currentConsumerRecs = null;
                string? currentConsumerCode = null;

                _consumerLogger.LogInformation("{className}.{methodName}: Started With ConsumerCode : {ConsumerCode}, TenantCode : {TenantCode}", className, methodName,
                    consumerAttributesRequestDto.ConsumerAttributes[0].ConsumerCode, consumerAttributesRequestDto.TenantCode);
                foreach (var consumerAttrDto in consumerAttributesRequestDto.ConsumerAttributes)
                {
                    if (consumerAttrDto == null) continue;

                    if (consumerAttrDto.ConsumerCode == currentConsumerCode)
                    {
                        currentConsumerRecs?.Add(consumerAttrDto);
                    }
                    else if (currentConsumerRecs != null && currentConsumerRecs.Any())
                    {
                        _consumerLogger.LogInformation("{className}.{methodName}: Process Start With If ConsumerCode is null : {ConsumerCode}", className, methodName,
                    consumerAttributesRequestDto.ConsumerAttributes[0].ConsumerCode);
                        consumer = await Process(consumerAttributesRequestDto.TenantCode, currentConsumerCode, currentConsumerRecs);

                        if (consumer != null && !consumersList.Any(c => c.ConsumerCode == consumer.ConsumerCode))
                        {
                            consumersList.Add(consumer);
                        }
                        currentConsumerRecs = null;
                    }

                    if (currentConsumerRecs == null)
                    {
                        currentConsumerRecs = new List<ConsumerAttributeDetailDto>();
                        currentConsumerRecs?.Add(consumerAttrDto);
                    }

                    currentConsumerCode = consumerAttrDto.ConsumerCode;
                }

                // Process the last set of ConsumerAttributes outside the loop
                if (currentConsumerRecs != null && currentConsumerRecs.Any())
                {
                    _consumerLogger.LogInformation("{className}.{methodName}: Process Start With If ConsumerCode is not null : {ConsumerCode}", className, methodName,
                consumerAttributesRequestDto.ConsumerAttributes[0].ConsumerCode);
                    consumer = await Process(consumerAttributesRequestDto.TenantCode, currentConsumerCode, currentConsumerRecs);
                    if (consumer != null && !consumersList.Any(c => c.ConsumerCode == consumer.ConsumerCode))
                    {
                        consumersList.Add(consumer);
                    }
                }

                _consumerLogger.LogInformation("{className}.{methodName}: Request Success For ConsumerCode : {ConsumerCode}, TenantCode: {TenantCode}", className, methodName,
                    consumerAttributesRequestDto.ConsumerAttributes[0].ConsumerCode, consumerAttributesRequestDto.TenantCode);

                //Consumer Attribute
                await _eventService.CreateConsumerHistoryEvent(consumersList, methodName);

                return new ConsumerAttributesResponseDto
                {
                    Consumers = consumersList
                };
            }
            catch (Exception ex)
            {
                _consumerLogger.LogError(ex, "{className}.{methodName}: ERROR - msg : {msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return new ConsumerAttributesResponseDto() { ErrorMessage = ex.Message, ErrorDescription = ex.InnerException?.Message };

            }
        }

        private async Task<ConsumerDto?> Process(string tenantCode, string? consumerCode, List<ConsumerAttributeDetailDto> currentConsumerRecs)
        {
            Dictionary<string, List<ConsumerAttributeDetailDto>> groupRecs = new();
            foreach (var consumerRec in currentConsumerRecs)
            {
                if (!groupRecs.ContainsKey(consumerRec.GroupName))
                {
                    groupRecs.Add(consumerRec.GroupName, new List<ConsumerAttributeDetailDto>());
                }

                groupRecs[consumerRec.GroupName].Add(consumerRec);
            }
            var consumer = await _consumerRepo.FindOneAsync(x => x.TenantCode == tenantCode && x.ConsumerCode == consumerCode && x.DeleteNbr == 0);

            if (consumer == null) return null;

            var transaction = _session.BeginTransaction();
            try
            {
                var consumerAttrs = consumer.Attr ?? new JObject();

                foreach (var keyValuePair in groupRecs)
                {
                    // If group name is null or empty → flatten attributes directly under root
                    if (string.IsNullOrWhiteSpace(keyValuePair.Key))
                    {
                        foreach (var item in keyValuePair.Value)
                        {
                            // Convert types
                            if (int.TryParse(item.AttributeValue, out var intVal))
                            {
                                consumerAttrs[item.AttributeName] = intVal;
                            }
                            else if (bool.TryParse(item.AttributeValue, out var boolVal))
                            {
                                consumerAttrs[item.AttributeName] = boolVal;
                            }
                            else
                            {
                                consumerAttrs[item.AttributeName] = item.AttributeValue;
                            }
                        }

                        continue; // skip grouped logic
                    }

                    // Existing grouped behavior
                    if (!consumerAttrs.ContainsKey(keyValuePair.Key))
                    {
                        consumerAttrs[keyValuePair.Key] = new JObject();
                    }

                    var consumerAttrObj = consumerAttrs[keyValuePair.Key];

                    if (consumerAttrObj is JObject obj)
                    {
                        foreach (var item in keyValuePair.Value)
                        {
                            if (int.TryParse(item.AttributeValue, out var intVal))
                                obj[item.AttributeName] = intVal;
                            else if (bool.TryParse(item.AttributeValue, out var boolVal))
                                obj[item.AttributeName] = boolVal;
                            else
                                obj[item.AttributeName] = item.AttributeValue;
                        }
                    }
                    else if (consumerAttrObj is JArray arr)
                    {
                        foreach (var item in keyValuePair.Value)
                        {
                            JToken newValue;
                            if (int.TryParse(item.AttributeValue, out var intVal))
                                newValue = intVal;
                            else if (bool.TryParse(item.AttributeValue, out var boolVal))
                                newValue = boolVal;
                            else
                                newValue = item.AttributeValue;

                            var existing = arr
                                .OfType<JObject>()
                                .FirstOrDefault(o => o.ContainsKey(item.AttributeName));

                            if (existing != null)
                            {
                                existing[item.AttributeName] = newValue;
                            }
                            else
                            {
                                var newObj = new JObject
                                {
                                    [item.AttributeName] = newValue
                                };
                                arr.Add(newObj);
                            }
                        }
                    }
                }


                consumer.ConsumerAttribute = consumerAttrs.ToJson();

                await _session.SaveAsync(consumer);
                await transaction.CommitAsync();
                ConsumerDto consumerDto = new();
                consumerDto = _mapper.Map<ConsumerDto>(consumer);

                await _eventService.PublishCohortEventToSNSTopic(consumerDto.TenantCode ?? string.Empty, consumerDto.ConsumerCode ?? string.Empty);


                return consumerDto;
            }
            catch (Exception ex)
            {
                _consumerLogger.LogError(ex, $"Process : Error while saving consumer = {ex.Message}");
                await transaction.RollbackAsync();
                return null;
            }
        }

        public async Task<GetConsumerByEmailResponseDto> GetConsumerByEmail(string email)
        {
            try
            {
                return await GetPersonByEmail(email);
            }

            catch (Exception ex)
            {
                _consumerLogger.LogError(ex, "{className}.GetConsumerByEmail: ERROR - msg: {msg}, Error Code:{errorCode}", className, ex.Message, StatusCodes.Status500InternalServerError);
                return new GetConsumerByEmailResponseDto() { ErrorMessage = ex.Message, ErrorDescription = ex.InnerException?.Message };
            }

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="email"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        private async Task<GetConsumerByEmailResponseDto> GetPersonByEmail(string email)
        {
            const string methodName = nameof(GetPersonByEmail);
            var response = new GetConsumerByEmailResponseDto();

            _consumerLogger.LogInformation("{className}.{methodName}: Querying from {dbSource} database", className, methodName, DbSource);
            
            IList<PersonModel> getAllPersonList;
            if (_readOnlySession != null)
            {
                getAllPersonList = await ReadSession.QueryOver<PersonModel>()
                    .Where(x => x.Email != null && x.DeleteNbr == 0)
                    .AndRestrictionOn(x => x.Email).IsInsensitiveLike(email)
                    .ListAsync();
            }
            else
            {
                getAllPersonList = await _personRepo.FindAsync(x => x.Email != null && x.Email.ToLower() == email.ToLower() && x.DeleteNbr == 0);
            }

            if (getAllPersonList.Count == 0)
            {
                _consumerLogger.LogError("{className}.{methodName}: Email not found for given Email.", className, methodName);
                return new GetConsumerByEmailResponseDto()
                {
                    Person = null,
                    ErrorCode = StatusCodes.Status404NotFound
                };

            }
            _consumerLogger.LogInformation("{className}.{methodName}: Retrieved  Person Data Successfully for ", className, methodName);
            var personData = getAllPersonList.First();

            var personDto = _mapper.Map<PersonDto>(personData);

            var personId = personDto.PersonId;

            IList<ConsumerModel> consumerList;
            if (_readOnlySession != null)
            {
                consumerList = await ReadSession.QueryOver<ConsumerModel>()
                    .Where(x => x.PersonId == personId)
                    .ListAsync();
            }
            else
            {
                consumerList = await _consumerRepo.FindAsync(x => x.PersonId == personId);
            }

            if (consumerList.Count == 0)
            {
                _consumerLogger.LogError("{className}.{methodName}: personId doesn't exist in Consumer Table : {personId}", className, methodName, personDto.PersonId);
                return new GetConsumerByEmailResponseDto()
                {
                    Person = null,
                    ErrorCode = StatusCodes.Status404NotFound
                };
            }

            var consumerDtoList = _mapper.Map<List<ConsumerDto>>(consumerList.ToList());
            response.Person = personDto;
            response.Consumer = consumerDtoList.ToArray();
            _consumerLogger.LogInformation("{className}.{methodName}: Retrieved  Consumer Data Successfully for PersonId: {personId}", className, methodName, personDto.PersonId);
            return response;
        }
        private async Task<bool> IsMemberIdExist(ConsumerDto consumerDto, bool reactivateDeletedConsumer = false)
        {
            var consumer = await _consumerRepo.FindOneAsync(consumer => consumer.TenantCode == consumerDto.TenantCode && consumer.MemberId == consumerDto.MemberId, reactivateDeletedConsumer);
            return consumer != null;
        }

        /// <summary>
        /// Retrieves consumer and person details based on the tenant code provided.
        /// </summary>
        /// <param name="consumerByTenantRequestDto">Contains tenant code, search term, and pagination parameters.</param>
        /// <returns>A paginated list of consumer and person details that match the search criteria.</returns>
        /// <remarks>This method performs optional search, and pagination.</remarks>
        public async Task<ConsumersAndPersonsListResponseDto> GetConsumersByTenantCode(GetConsumerByTenantRequestDto consumerByTenantRequestDto)
        {
            const string methodName = nameof(GetConsumersByTenantCode);
            try
            {
                var skip = (consumerByTenantRequestDto.PageNumber - 1) * consumerByTenantRequestDto.PageSize;

                var response = new ConsumersAndPersonsListResponseDto();
                var consumerRecords = await _personRepo.GetConsumerPersons(consumerByTenantRequestDto.TenantCode!, consumerByTenantRequestDto.SearchTerm, skip, consumerByTenantRequestDto.PageSize);

                if (consumerRecords == null || consumerRecords.Count == 0)
                {
                    _consumerLogger.LogError("{className}.{methodName}: ERROR - Consumer Records not Found for Tenant code:{TenantCode} , Error Code:{errorCode}", className, methodName, consumerByTenantRequestDto.TenantCode, StatusCodes.Status404NotFound);
                    return new ConsumersAndPersonsListResponseDto() { ErrorCode = StatusCodes.Status404NotFound };
                }

                foreach (var record in consumerRecords)
                {
                    var consumerAndPerson = new ConsumersAndPersons()
                    {
                        Consumer = _mapper.Map<ConsumerDto>(record.ConsumerModel),
                        Person = _mapper.Map<PersonDto>(record.PersonModel),
                    };
                    response.ConsumerAndPersons.Add(consumerAndPerson);
                }
                return response;
            }
            catch (Exception ex)
            {
                var errorMessage = $"Error occurred while fetching consumer and person details for TenantCode: {consumerByTenantRequestDto.TenantCode}.";
                _consumerLogger.LogError(ex, "{className}.{methodName}: {ErrorMessage}", className, methodName, errorMessage);
                throw new InvalidOperationException(errorMessage, ex);
            }
        }
        /// <summary>
        /// Updates the consumer details asynchronously in the database.
        /// </summary>
        /// <param name="consumerRequestDto">The data transfer object containing consumer details to update.</param>
        /// <returns>
        /// A <see cref="ConsumerResponseDto"/> containing the updated consumer details or error information if the update fails.
        /// </returns>

        public async Task<ConsumerResponseDto> UpdateConsumerAsync(long consumerId, ConsumerRequestDto consumerRequestDto)
        {
            const string methodName = nameof(UpdateConsumerAsync);
            try
            {
                _consumerLogger.LogInformation("{ClassName}.{MethodName} : Started updating consumer with ConsumerCode:{Code} and TenantCode:{Tenant}",
                    className, methodName, consumerRequestDto.ConsumerCode, consumerRequestDto.TenantCode);

                // Retrieve the consumer from the repository
                var consumerModel = await _consumerRepo.FindOneAsync(x => x.ConsumerId == consumerId && x.DeleteNbr == 0);

                if (consumerModel == null || consumerModel.ConsumerId <= 0)
                {
                    _consumerLogger.LogWarning("{ClassName}.{MethodName}: Consumer not found with ConsumerCode: {Code} and TenantCode: {Tenant}",
                        className, methodName, consumerRequestDto.ConsumerCode, consumerRequestDto.TenantCode);
                    return new ConsumerResponseDto()
                    {
                        ErrorCode = StatusCodes.Status404NotFound,
                        ErrorMessage = $"Consumer not found with ConsumerId:{consumerId}",
                        Consumer = _mapper.Map<ConsumerDto>(consumerRequestDto)
                    };
                }
                // Update consumer attributes
                if (!string.IsNullOrEmpty(consumerRequestDto.ConsumerAttribute))
                {
                    consumerModel.ConsumerAttribute = consumerRequestDto.ConsumerAttribute;
                }
                if (!string.IsNullOrWhiteSpace(consumerRequestDto.Auth0UserName))
                {
                    consumerModel.Auth0UserName = consumerRequestDto.Auth0UserName;
                }
                consumerModel.UpdateTs = DateTime.UtcNow;
                consumerModel.UpdateUser = Constants.CreateUser;
                // Update the consumer in the repository
                _consumerLogger.LogInformation("{ClassName}.{MethodName} - Writing to Primary database", className, methodName);
                await _consumerRepo.UpdateAsync(consumerModel);

                _consumerLogger.LogInformation("{ClassName}.{MethodName} : Successfully updated consumer with ConsumerCode:{Code} and TenantCode:{Tenant}",
                    className, methodName, consumerRequestDto.ConsumerCode, consumerRequestDto.TenantCode);
                var consumer = _mapper.Map<ConsumerDto>(consumerModel);
                await _eventService.CreateConsumerHistoryEvent(new List<ConsumerDto>() { consumer }, methodName);
                return new ConsumerResponseDto() { Consumer = consumer };
            }
            catch (Exception ex)
            {
                _consumerLogger.LogError(ex, "{ClassName}.{MethodName} : Error occurred while updating consumer with ConsumerCode:{Code} and TenantCode:{Tenant},ERROR:{Msg}",
                    className, methodName, consumerRequestDto.ConsumerCode, consumerRequestDto.TenantCode, ex.Message);
                return new ConsumerResponseDto()
                {
                    Consumer = _mapper.Map<ConsumerDto>(_mapper.Map<ConsumerDto>(consumerRequestDto)),
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<ConsumerResponseDto> UpdateOnboardingState(UpdateOnboardingStateDto updateOnboardingStateDto)
        {
            var methodName = nameof(UpdateOnboardingState);
            try
            {
                _session.Clear();
                var consumer = await _consumerRepo.FindOneAsync(x => x.ConsumerCode == updateOnboardingStateDto.ConsumerCode && x.TenantCode == updateOnboardingStateDto.TenantCode && x.DeleteNbr == 0);
                _consumerLogger.LogInformation("{className}.{methodName}: Started updating onboarding state for Consumer :{0} , TenantCode : {1}", className, methodName, consumer.ToJson(), updateOnboardingStateDto.TenantCode);
                if (consumer == null)
                {
                    _consumerLogger.LogError("Consumer not found for ConsumerCode :{0} , TenantCode : {1}", updateOnboardingStateDto.ConsumerCode, updateOnboardingStateDto.TenantCode);
                    throw new InvalidDataException("Consumer not found");
                }
                if (string.IsNullOrEmpty(updateOnboardingStateDto.LanguageCode))
                {
                    updateOnboardingStateDto.LanguageCode = Constant.DefaultLanguageCode;
                }

                var presentState = Enum.Parse<OnboardingState>(consumer.OnBoardingState!);

                if (presentState == updateOnboardingStateDto.OnboardingState && consumer.AgreementStatus == Constant.NotVerified && presentState != OnboardingState.VERIFIED)
                {
                    _consumerLogger.LogInformation("{className}.{methodName}: Consumer onboarding state is already {presentState}", className, methodName, presentState);
                    return new ConsumerResponseDto()
                    {
                        Consumer = _mapper.Map<ConsumerDto>(consumer)
                    };
                }

                //update EnrollmentStatus
                if (updateOnboardingStateDto.OnboardingState == OnboardingState.AGREEMENT_VERIFIED
                    && updateOnboardingStateDto.HtmlFileName?.Count > 0
                    )
                {
                    var uploadedFileNames = await _uploadAgreementPDFService.UploadAgreementPDf(updateOnboardingStateDto, consumer.TenantCode, consumer.ConsumerCode);
                    if (uploadedFileNames.Count <= 0)
                    {
                        _consumerLogger.LogError("{ClassName}.{MethodName} - Agreement pdf upload failed for consumer with verify Member updateOnboardingStateDto request Dto : {updateOnboardingStateDto}", className, methodName, updateOnboardingStateDto.ToJson());
                        return new ConsumerResponseDto()
                        {
                            ErrorCode = StatusCodes.Status500InternalServerError,
                            ErrorMessage = "Agreement pdf upload failed"
                        };
                    }
                    else
                    {

                        consumer.AgreementStatus = OnboardingState.VERIFIED.ToString();
                        consumer.AgreementFileName = JsonConvert.SerializeObject(
                            uploadedFileNames,
                            new JsonSerializerSettings
                            {
                                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                                Formatting = Formatting.None
                            });
                    }
                }
                if (updateOnboardingStateDto.OnboardingState == OnboardingState.VERIFIED
                    )
                {
                    consumer.OnBoardingState = OnboardingState.VERIFIED.ToString();
                    consumer.EnrollmentStatus = EnrollmentStatus.ENROLLED.ToString();
                    consumer.EnrollmentStatusSource = EnrollmentStatusSource.ONBOARDING_FLOW.ToString();

                }
                else if (updateOnboardingStateDto.OnboardingState != OnboardingState.VERIFIED)
                {
                    consumer.OnBoardingState = updateOnboardingStateDto.OnboardingState.ToString();
                }
                else
                {
                    _consumerLogger.LogError("{ClassName}.{MethodName} - Agreement url is invalid for consumer with verify Member updateOnboardingStateDto request Dto : {updateOnboardingStateDto}", className, methodName, updateOnboardingStateDto.ToJson());

                }
                consumer.UpdateTs = DateTime.UtcNow;
                consumer.UpdateUser = Constants.CreateUser;
                var updatedConusmer = await _consumerRepo.UpdateAsync(consumer);
                _consumerLogger.LogInformation($"{className}-{methodName}: Updated consumer to {updatedConusmer.ToJson()}");
                var consumerUpdated = _mapper.Map<ConsumerDto>(updatedConusmer);
                await _eventService.CreateConsumerHistoryEvent(new List<ConsumerDto>() { consumerUpdated }, methodName);
                
                // Publish AgreementsVerified event if onboarding state is AGREEMENT_VERIFIED
                if (updateOnboardingStateDto.OnboardingState == OnboardingState.AGREEMENT_VERIFIED)
                {
                    await PublishAgreementsVerifiedEvent(consumerUpdated);
                }
                
                return new ConsumerResponseDto() { Consumer = consumerUpdated };
            }
            catch (Exception ex)
            {
                _consumerLogger.LogError(ex, "{className}.{methodName}: ERROR - msg: {Msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return new ConsumerResponseDto()
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = ex.Message
                };
            }
        }


        public async Task<GetConsumerByPersonUniqueIdentifierResponseDto> GetConsumerByPersonUniqueIdentifier(string personUniqueIdentifier)
        {
            var methodName = nameof(GetConsumerByPersonUniqueIdentifier);
            var response = new GetConsumerByPersonUniqueIdentifierResponseDto();

            if (string.IsNullOrWhiteSpace(personUniqueIdentifier))
            {
                _consumerLogger.LogError("{ClassName}.{MethodName}: PersonUniqueIdentifier is null or empty", className, methodName);
                response.ErrorCode = StatusCodes.Status400BadRequest;
                response.ErrorMessage = "PersonUniqueIdentifier cannot be null or empty.";
                return response;
            }

            _consumerLogger.LogInformation("{className}.{methodName}: Querying from {dbSource} database", className, methodName, DbSource);
            
            PersonModel? person;
            if (_readOnlySession != null)
            {
                person = await ReadSession.QueryOver<PersonModel>()
                    .Where(x => x.PersonUniqueIdentifier == personUniqueIdentifier && x.DeleteNbr == 0)
                    .SingleOrDefaultAsync();
            }
            else
            {
                person = await _personRepo.FindOneAsync(x =>
                    x.PersonUniqueIdentifier == personUniqueIdentifier &&
                    x.DeleteNbr == 0);
            }

            if (person == null)
            {
                _consumerLogger.LogWarning("{ClassName}.{MethodName}: No person found for PersonUniqueIdentifier: {Identifier}", className, methodName, personUniqueIdentifier);
                response.ErrorCode = StatusCodes.Status404NotFound;
                response.ErrorMessage = "Person not found.";
                return response;
            }

            var personDto = _mapper.Map<PersonDto>(person);
            response.Person = personDto;

            IList<ConsumerModel> consumers;
            if (_readOnlySession != null)
            {
                consumers = await ReadSession.QueryOver<ConsumerModel>()
                    .Where(x => x.PersonId == personDto.PersonId)
                    .ListAsync();
            }
            else
            {
                consumers = await _consumerRepo.FindAsync(x => x.PersonId == personDto.PersonId);
            }

            if (consumers == null || consumers.Count == 0)
            {
                _consumerLogger.LogWarning("{ClassName}.{MethodName}: No consumers found for PersonId: {PersonId}", className, methodName, personDto.PersonId);
                response.ErrorCode = StatusCodes.Status404NotFound;
                response.ErrorMessage = "No consumers found for this person.";
                return response;
            }

            response.Consumer = _mapper.Map<List<ConsumerDto>>(consumers.ToList()).ToArray();

            _consumerLogger.LogInformation("{ClassName}.{MethodName}: Successfully retrieved consumer data for PersonId: {PersonId}", className, methodName, personDto.PersonId);

            return response;
        }

        public async Task<ConsumerPersonResponseDto> GetConsumersByMemberNbrAndRegionCode(string memberNbr, string regionCode)
        {
            const string methodName = nameof(GetConsumersByMemberNbrAndRegionCode);
            var response = new ConsumerPersonResponseDto();

            if (string.IsNullOrWhiteSpace(memberNbr) || string.IsNullOrWhiteSpace(regionCode))
            {
                _consumerLogger.LogError("{ClassName}.{MethodName}: MemberNbr or RegionCode is null or empty", className, methodName);
                response.ErrorCode = StatusCodes.Status400BadRequest;
                response.ErrorMessage = "MemberNbr and RegionCode are required.";
                return response;
            }

            _consumerLogger.LogInformation("{className}.{methodName}: Querying from {dbSource} database", className, methodName, DbSource);
            
            IList<MemberImportFileDataModel> members;
            if (_readOnlySession != null)
            {
                members = await ReadSession.QueryOver<MemberImportFileDataModel>()
                    .Where(x => x.MemNbr == memberNbr && x.RegionCode == regionCode && x.DeleteNbr == 0)
                    .ListAsync();
            }
            else
            {
                members = await _memberImportFileDataRepo.FindAsync(x => x.MemNbr == memberNbr && x.RegionCode == regionCode && x.DeleteNbr == 0);
            }
            var member = members?.OrderByDescending(x => x.MemberImportFileDataId)?.FirstOrDefault();

            if (member == null)
            {
                _consumerLogger.LogError("{ClassName}.{MethodName}: Member not found. MemberNbr: {MemberNbr}, RegionCode: {RegionCode}", className, methodName, memberNbr, regionCode);
                response.ErrorCode = StatusCodes.Status404NotFound;
                response.ErrorMessage = "Member not found.";
                return response;
            }

            IList<ConsumerModel> consumers;
            if (_readOnlySession != null)
            {
                consumers = await ReadSession.QueryOver<ConsumerModel>()
                    .Where(x => x.MemberId == member.MemberId && x.DeleteNbr == 0)
                    .ListAsync();
            }
            else
            {
                consumers = await _consumerRepo.FindAsync(x => x.MemberId == member.MemberId && x.DeleteNbr == 0);
            }
            var latestConsumer = consumers?.OrderByDescending(x => x.ConsumerId).FirstOrDefault();

            if (consumers == null || !consumers.Any())
            {
                _consumerLogger.LogWarning("{ClassName}.{MethodName}: No consumers found for MemberNbr: {MemberNbr}, RegionCode: {RegionCode}",
                    className, methodName, memberNbr, regionCode);

                response.ErrorCode = StatusCodes.Status404NotFound;
                response.ErrorMessage = "No consumers found.";
                return response;
            }

            response.Consumer = _mapper.Map<List<ConsumerDto>>(consumers.ToList()).ToArray();

            PersonModel? person;
            if (_readOnlySession != null)
            {
                person = await ReadSession.QueryOver<PersonModel>()
                    .Where(p => p.PersonId == latestConsumer!.PersonId && p.DeleteNbr == 0)
                    .SingleOrDefaultAsync();
            }
            else
            {
                person = await _personRepo.FindOneAsync(p => p.PersonId == latestConsumer!.PersonId && p.DeleteNbr == 0);
            }

            if (person == null)
            {
                _consumerLogger.LogWarning("{ClassName}.{MethodName}: Person not found for PersonId: {PersonId}", className, methodName, latestConsumer!.PersonId);
                response.ErrorCode = StatusCodes.Status404NotFound;
                response.ErrorMessage = "Person not found.";
                return response;
            }

            response.Person = _mapper.Map<PersonDto>(person);

            _consumerLogger.LogInformation("{ClassName}.{MethodName}: Successfully retrieved consumers and person for MemberNbr: {MemberNbr}, RegionCode: {RegionCode}",
                className, methodName, memberNbr, regionCode);

            return response;
        }

        /// <summary>
        /// Retrieves consumer and person details based on the tenant code provided.
        /// </summary>
        /// <param name="consumerByTenantRequestDto">Contains tenant code, search term, and pagination parameters.</param>
        /// <returns>A paginated list of consumer and person details that match the search criteria.</returns>
        /// <remarks>This method performs optional search, and pagination.</remarks>
        public async Task<ConsumersAndPersonsListResponseDto> GetConsumersByConsumerCodes(GetConsumerByConsumerCodes getConsumerByConsumerCodes)
        {
            const string methodName = nameof(GetConsumersByConsumerCodes);
            try
            {

                var response = new ConsumersAndPersonsListResponseDto();
                var consumerRecords = await _personRepo.GetConsumerPersons(getConsumerByConsumerCodes.ConsumerCodes, getConsumerByConsumerCodes.TenantCode!);

                if (consumerRecords == null || consumerRecords.Count == 0)
                {
                    _consumerLogger.LogError("{className}.{methodName}: ERROR - Consumer Records not Found for Tenant code:{TenantCode} , Error Code:{errorCode}", className, methodName, getConsumerByConsumerCodes.TenantCode, StatusCodes.Status404NotFound);
                    return new ConsumersAndPersonsListResponseDto() { ErrorCode = StatusCodes.Status404NotFound };
                }

                foreach (var record in consumerRecords)
                {
                    var consumerAndPerson = new ConsumersAndPersons()
                    {
                        Consumer = _mapper.Map<ConsumerDto>(record.ConsumerModel),
                        Person = _mapper.Map<PersonDto>(record.PersonModel),
                    };
                    response.ConsumerAndPersons.Add(consumerAndPerson);
                }
                return response;
            }
            catch (Exception ex)
            {
                var errorMessage = $"Error occurred while fetching consumer and person details for TenantCode: {getConsumerByConsumerCodes.TenantCode}.";
                _consumerLogger.LogError(ex, "{className}.{methodName}: {ErrorMessage}", className, methodName, errorMessage);
                throw new InvalidOperationException(errorMessage, ex);
            }
        }


        public async Task<ConsumerResponseDto> UpdateAgreementStatus(UpdateAgreementStatusDto dto)
        {
            var methodName = nameof(UpdateAgreementStatus);
            var (isValid, errorMessage) = ValidateUpdateAgreementStatusDto(dto);
            if (!isValid)
            {
                return new ConsumerResponseDto
                {
                    ErrorCode = StatusCodes.Status400BadRequest,
                    ErrorMessage = errorMessage
                };
            }
            var consumer = await _consumerRepo.FindOneAsync(x => x.TenantCode == dto.TenantCode && x.ConsumerCode == dto.ConsumerCode && x.DeleteNbr == 0);
            if (consumer == null)
            {
                return new ConsumerResponseDto
                {
                    ErrorCode = 404,
                    ErrorMessage = "Consumer not found"
                };
            }
            consumer.AgreementStatus = dto.AgreementStatus.ToString();

            _consumerLogger.LogInformation("{className}.{methodName} - Writing to Primary database", className, methodName);
            var updatedConusmer = await _consumerRepo.UpdateAsync(consumer);

            var consumerUpdated = _mapper.Map<ConsumerDto>(updatedConusmer);
            await _eventService.CreateConsumerHistoryEvent(new List<ConsumerDto>() { consumerUpdated }, methodName);
            return new ConsumerResponseDto() { Consumer = consumerUpdated };

        }

        public async Task<ConsumerResponseDto> UpdateEnrollmentStatus(UpdateEnrollmentStatusRequestDto requestDto)
        {
            var methodName = nameof(UpdateAgreementStatus);
            if (!Enum.TryParse<EnrollmentStatus>(requestDto.EnrollmentStatus, true, out _))
            {
                return new ConsumerResponseDto
                {
                    ErrorCode = StatusCodes.Status400BadRequest,
                    ErrorMessage = "EnrollmentStatus must be a valid value."
                };
            }

            var consumer = await _consumerRepo.FindOneAsync(x =>
                x.TenantCode == requestDto.TenantCode &&
                x.ConsumerCode == requestDto.ConsumerCode &&
                x.DeleteNbr == 0);

            if (consumer == null)
            {
                return new ConsumerResponseDto
                {
                    ErrorCode = StatusCodes.Status404NotFound,
                    ErrorMessage = "Consumer not found"
                };
            }

            consumer.EnrollmentStatus = requestDto.EnrollmentStatus;
            consumer.UpdateTs = DateTime.UtcNow;
            consumer.UpdateUser = Constants.CreateUser;
            _consumerLogger.LogInformation("{className}.{methodName} - Writing to Primary database", className, methodName);
            var updatedConusmer = await _consumerRepo.UpdateAsync(consumer);

            var consumerUpdated = _mapper.Map<ConsumerDto>(updatedConusmer);
            await _eventService.CreateConsumerHistoryEvent(new List<ConsumerDto>() { consumerUpdated }, methodName);
            return new ConsumerResponseDto() { Consumer = consumerUpdated };

        }

        /// <summary>
        /// Publishes the AgreementsVerified event to SNS topic.
        /// </summary>
        /// <param name="consumerDto">The consumer DTO.</param>
        private async Task PublishAgreementsVerifiedEvent(ConsumerDto consumerDto)
        {
            const string methodName = nameof(PublishAgreementsVerifiedEvent);
            try
            {
                _consumerLogger.LogInformation("{className}.{methodName}: Publishing AgreementsVerified event for ConsumerCode: {ConsumerCode}, TenantCode: {TenantCode}",
                    className, methodName, consumerDto.ConsumerCode, consumerDto.TenantCode);

                var eventHeaderDto = BuildAgreementsVerifiedEventHeaderDto(consumerDto.TenantCode!, consumerDto.ConsumerCode!);
                var eventDataDto = BuildAgreementsVerifiedEventDataDto(consumerDto);

                var publishResultDto = await _heliosEventPublisher.PublishMessage(eventHeaderDto, eventDataDto);

                if (!string.IsNullOrEmpty(publishResultDto.ErrorCode))
                {
                    _consumerLogger.LogError(
                        "{className}.{methodName}: Consumer agreements verified event publish failed. Request: {eventDto}, Response: {responseDto}",
                        className, methodName, eventHeaderDto.ToJson(), publishResultDto.ToJson());
                }
            }
            catch (Exception ex)
            {
                _consumerLogger.LogError(ex, "{className}.{methodName}: Exception occurred while publishing AgreementsVerified event. ConsumerCode: {ConsumerCode}, TenantCode: {TenantCode}",
                    className, methodName, consumerDto.ConsumerCode, consumerDto.TenantCode);
            }
        }

        /// <summary>
        /// Builds the event header DTO for AgreementsVerified event publishing.
        /// </summary>
        /// <param name="tenantCode">The tenant code.</param>
        /// <param name="consumerCode">The consumer code.</param>
        /// <returns>An instance of <see cref="EventHeaderDto"/>.</returns>
        private EventHeaderDto BuildAgreementsVerifiedEventHeaderDto(string tenantCode, string consumerCode)
        {
            var dto = new EventHeaderDto
            {
                EventId = Guid.NewGuid().ToString("N"),
                EventType = Constant.AgreementsVerifiedEventType,
                EventSubtype = Constant.AgreementsVerifiedEventSubType,
                PublishTs = DateTime.UtcNow,
                TenantCode = tenantCode,
                ConsumerCode = consumerCode,
                SourceModule = Constant.UserService
            };

            _consumerLogger.LogInformation("{className}.{methodName}: EventHeaderDto created: {@EventHeaderDto}",
                className, nameof(BuildAgreementsVerifiedEventHeaderDto), dto);

            return dto;
        }

        /// <summary>
        /// Builds the event data DTO for AgreementsVerified event publishing.
        /// </summary>
        /// <param name="consumerDto">The consumer DTO.</param>
        /// <returns>An instance of <see cref="AgreementsVerifiedEventDto"/>.</returns>
        private AgreementsVerifiedEventDto BuildAgreementsVerifiedEventDataDto(ConsumerDto consumerDto)
        {
            var dto = new AgreementsVerifiedEventDto
            {
                AgreementStatus = consumerDto.AgreementStatus
            };

            _consumerLogger.LogInformation("{className}.{methodName}: AgreementsVerifiedEventDto created: {@AgreementsVerifiedEventDto}",
                className, nameof(BuildAgreementsVerifiedEventDataDto), dto);

            return dto;
        }

        /// <summary>
        /// Updates the subscription status of a consumer based on the provided request DTO.
        /// </summary>
        /// <param name="requestDto"></param>
        /// <returns></returns>
        public async Task<BaseResponseDto> UpdateConsumerSubscriptionStatus(ConsumerSubscriptionStatusRequestDto requestDto)
        {
            const string methodName = nameof(UpdateConsumerSubscriptionStatus);

            try
            {
                if (requestDto == null || string.IsNullOrWhiteSpace(requestDto.TenantCode))
                {
                    _consumerLogger.LogWarning("{className}.{methodName}: Invalid request. TenantCode or Request body is null.", className, methodName);
                    return new BaseResponseDto
                    {
                        ErrorMessage = "Invalid request.",
                        ErrorDescription = "TenantCode or request body is missing."
                    };
                }

                if (requestDto.ConsumerSubscriptionStatuses == null || !requestDto.ConsumerSubscriptionStatuses.Any())
                {
                    _consumerLogger.LogInformation("{className}.{methodName}: No subscription status records to process. TenantCode:{tenantCode}",
                        className, methodName, requestDto.TenantCode);

                    return new BaseResponseDto
                    {
                        ErrorMessage = "No subscription status records to process.",
                        ErrorDescription = "ConsumerSubscriptionStatuses list is empty."
                    };
                }

                // Validate Tenant
                var getTenantRequestDto = new GetTenantCodeRequestDto { TenantCode = requestDto.TenantCode };
                var tenantResponse = await _tenantClient.Post<TenantDto>(Constant.GetTenantByTenantCode, getTenantRequestDto);

                if (tenantResponse == null)
                {
                    _consumerLogger.LogWarning("{className}.{methodName}: Tenant not found for TenantCode: {tenantCode}", className, methodName, requestDto.TenantCode);
                    return new BaseResponseDto
                    {
                        ErrorMessage = "Tenant not found.",
                        ErrorDescription = $"No tenant exists for TenantCode: {requestDto.TenantCode}"
                    };
                }

                // Extract tenantOption.subscriptionStatus list
                List<ConsumerSubscriptionStatusDetailDto> tenantSubscriptionOptions = new();
                if (tenantResponse?.TenantOption != null)
                {
                    var tenantOption = JsonConvert.DeserializeObject<TenantOption>(tenantResponse.TenantOption);
                    if (tenantOption?.SubscriptionStatus != null)
                        tenantSubscriptionOptions = tenantOption.SubscriptionStatus;
                }

                if (tenantSubscriptionOptions == null || !tenantSubscriptionOptions.Any())
                {
                    _consumerLogger.LogWarning("{className}.{methodName}: No subscriptionStatus configuration found in tenantOption. TenantCode:{tenantCode}",
                        className, methodName, requestDto.TenantCode);
                    return new BaseResponseDto
                    {
                        ErrorMessage = "Invalid tenant configuration.",
                        ErrorDescription = "subscriptionStatus configuration missing in tenant options."
                    };
                }

                var consumer = await _consumerRepo.FindOneAsync(x => x.TenantCode == requestDto.TenantCode && x.ConsumerCode == requestDto.ConsumerCode);

                if (consumer == null)
                {
                    _consumerLogger.LogWarning("{className}.{methodName}: Consumer not found for TenantCode: {tenantCode}, ConsumerCode: {consumerCode}",
                        className, methodName, requestDto.TenantCode, requestDto.ConsumerCode);
                    return new BaseResponseDto
                    {
                        ErrorMessage = "Consumer not found.",
                        ErrorDescription = $"No consumer exists for TenantCode: {requestDto.TenantCode}, ConsumerCode: {requestDto.ConsumerCode}"
                    };
                }

                SubscriptionStatusDto consumerStatuses = new SubscriptionStatusDto();

                // Deserialize existing subscription status JSON (wrapped object)
                if (!string.IsNullOrWhiteSpace(consumer.SubscriptionStatusJson))
                {
                    try
                    {
                        // Try to parse existing JSON
                        consumerStatuses = JsonConvert.DeserializeObject<SubscriptionStatusDto>(consumer.SubscriptionStatusJson!) ?? new SubscriptionStatusDto();
                    }
                    catch (Exception ex)
                    {
                        _consumerLogger.LogWarning(ex, "{className}.{methodName}: Invalid JSON in consumer.subscription_status_json for TenantCode:{tenantCode}, ConsumerCode:{consumerCode}",
                            className, methodName, requestDto.TenantCode, requestDto.ConsumerCode);
                    }
                }

                // Ensure list is always initialized
                consumerStatuses.subscriptionStatus ??= new List<ConsumerSubscriptionStatusDetailDto>();

                // Process each incoming status
                foreach (var statusDto in requestDto.ConsumerSubscriptionStatuses)
                {
                    // Only consider features defined in tenantOption
                    var tenantFeature = tenantSubscriptionOptions.FirstOrDefault(t =>
                        t.Feature.Equals(statusDto.Feature, StringComparison.OrdinalIgnoreCase));

                    if (tenantFeature == null)
                    {
                        _consumerLogger.LogInformation("{className}.{methodName}: Ignored feature '{feature}' not configured in tenantOption. TenantCode:{tenantCode}",
                            className, methodName, statusDto.Feature, requestDto.TenantCode);
                        continue;
                    }

                    // Find if feature already exists for consumer
                    var existing = consumerStatuses.subscriptionStatus.FirstOrDefault(c =>
                        c.Feature.Equals(statusDto.Feature, StringComparison.OrdinalIgnoreCase));

                    if (existing != null)
                    {
                        // Update status
                        existing.Status = statusDto.Status;
                    }
                    else
                    {
                        // Add new valid feature
                        consumerStatuses.subscriptionStatus.Add(new ConsumerSubscriptionStatusDetailDto
                        {
                            Feature = statusDto.Feature,
                            Status = statusDto.Status
                        });
                    }
                }

                
                string subscriptionStatusJson = JsonConvert.SerializeObject(consumerStatuses);
                consumer.SubscriptionStatusJson = subscriptionStatusJson;
                await _consumerRepo.UpdateAsync(consumer);

                _consumerLogger.LogInformation("{className}.{methodName}: Updated consumer subscription status successfully. TenantCode:{tenantCode}, ConsumerCode:{consumerCode}",
                    className, methodName, requestDto.TenantCode, requestDto.ConsumerCode);

                return new BaseResponseDto();
            }
            catch (Exception ex)
            {
                _consumerLogger.LogError(ex, "{className}.{methodName}: ERROR - Msg:{msg}, Code:{code}",
                    className, methodName, ex.Message, StatusCodes.Status500InternalServerError);

                throw;
            }
        }

    }
}