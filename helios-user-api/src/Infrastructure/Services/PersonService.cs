using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Services;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.enums;
using SunnyRewards.Helios.User.Core.Domain.Models;
using SunnyRewards.Helios.User.Infrastructure.ReadReplica;
using SunnyRewards.Helios.User.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.User.Infrastructure.Services.Interfaces;
using System.Reflection.Metadata;

namespace SunnyRewards.Helios.User.Infrastructure.Services
{
    public class PersonService : BaseService, IPersonService
    {
        private readonly ILogger<PersonService> _personLogger;
        private readonly IMapper _mapper;
        private readonly IPersonRepo _personRepo;
        private readonly IConsumerRepo _consumerRepo;
        private readonly IConsumerService _consumerService;
        private readonly IReadOnlySession? _readOnlySession;

        // Returns the read replica session if available
        private NHibernate.ISession? ReadSession => _readOnlySession?.Session;
        private string DbSource => _readOnlySession != null ? "ReadReplica" : "Primary";

        public PersonService(
            ILogger<PersonService> personLogger,
            IMapper mapper,
            IPersonRepo personRepo,
            IConsumerRepo consumerRepo,
            IConsumerService consumerService,
            IReadOnlySession? readOnlySession = null)
        {
            _personLogger = personLogger;
            _mapper = mapper;
            _personRepo = personRepo;
            _consumerRepo = consumerRepo;
            _consumerService = consumerService;
            _readOnlySession = readOnlySession;
        }
        const string className = nameof(PersonService);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<PersonDto> GetPersonData(long id)
        {
            const string methodName = nameof(GetPersonData);
            var response = new PersonDto();
            try
            {
                _personLogger.LogInformation("{className}.{methodName}: Querying from {dbSource} database for PersonId: {id}", className, methodName, DbSource, id);
                
                PersonModel? person;
                if (_readOnlySession != null)
                {
                    person = await ReadSession!.QueryOver<PersonModel>()
                        .Where(x => x.PersonId == id && x.DeleteNbr == 0)
                        .SingleOrDefaultAsync();
                }
                else
                {
                    person = await _personRepo.FindOneAsync(x => x.PersonId == id && x.DeleteNbr == 0);
                }

                if (person != null && person.PersonId > 0)
                {
                    response = _mapper.Map<PersonDto>(person);
                    _personLogger.LogInformation("{className}.GetPersonData: Retrieved Person Data Successfully for PersonId : {id}", className, id);

                    return response;
                }
                return new PersonDto();
            }
            catch (Exception ex)
            {
                _personLogger.LogError(ex, "{className}.GetPersonData: ERROR - msg: {msg}, Error Code:{errorCode}",className, ex.Message, StatusCodes.Status500InternalServerError);
                return new PersonDto();
            }
        }

        public async Task<GetPersonAndConsumerResponseDto> GetOverAllConsumerDetails(GetConsumerRequestDto consumerRequestDto)
        {
            const string methodName = nameof(GetOverAllConsumerDetails);
            try
            {
                var consumerRecord = await _consumerService.GetConsumerData(consumerRequestDto);
                if (consumerRecord == null || consumerRecord.Consumer == null)
                {
                    _personLogger.LogError("{className}.{methodName}: ERROR - Consumer Record not Found for Consumer code:{consumer} , Error Code:{errorCode}", className, methodName, consumerRequestDto.ConsumerCode, StatusCodes.Status404NotFound);
                    return NotFoundErrorResponse();
                }
                var personData = await GetPersonData(consumerRecord.Consumer.PersonId);
                if (personData == null)
                {
                    _personLogger.LogError("{className}.{methodName}: ERROR - Person details not Found for Consumer code:{consumer}, person Id:{Id} , Error Code:{errorCode}", className, methodName, consumerRequestDto.ConsumerCode, consumerRecord.Consumer.PersonId, StatusCodes.Status404NotFound);
                    return NotFoundErrorResponse();
                }
                _personLogger.LogInformation("{className}.{methodName}: Retrieved Consumer Details Consumer code:{consumer} , Error Code:{errorCode}", className, methodName, consumerRequestDto.ConsumerCode, StatusCodes.Status404NotFound);
                return new GetPersonAndConsumerResponseDto
                {
                    Consumer = consumerRecord.Consumer,
                    Person = personData
                };

            }
            catch (Exception ex)
            {
                _personLogger.LogError(ex, "{className}.{methodName}: Error occurred while fetching consumer and person details. Error: {Message}",className,methodName, ex.Message);
                throw;
            }

        }

        public async Task<PersonResponseDto> UpdatePersonData(UpdatePersonRequestDto updatePersonRequestDto)
        {
            const string methodName = nameof(UpdatePersonData);
            try
            {
                var consumer = await _consumerRepo.FindOneAsync(x => x.ConsumerCode == updatePersonRequestDto.ConsumerCode && x.DeleteNbr == 0);
                if (consumer == null)
                {
                    _personLogger.LogError("{className}.{methodName}: ERROR - Consumer Record not Found for Consumer code:{consumer} , Error Code:{errorCode}", className, methodName, updatePersonRequestDto.ConsumerCode, StatusCodes.Status404NotFound);
                    return new PersonResponseDto { ErrorCode = StatusCodes.Status404NotFound, ErrorMessage = $"Consumer Record not Found for Consumer code:{updatePersonRequestDto.ConsumerCode}" };
                }

                var person = await _personRepo.FindOneAsync(x => x.PersonId == consumer.PersonId && x.DeleteNbr == 0);
                if (person == null)
                {
                    _personLogger.LogError("{className}.{methodName}: ERROR - Person Record not Found for Consumer code:{consumer} , Error Code:{errorCode}", className, methodName, updatePersonRequestDto.ConsumerCode, StatusCodes.Status404NotFound);
                    return new PersonResponseDto { ErrorCode = StatusCodes.Status404NotFound, ErrorMessage = $"Person Record not Found for Consumer code:{updatePersonRequestDto.ConsumerCode}" };
                }

                if (!string.IsNullOrWhiteSpace(updatePersonRequestDto.PhoneNumber))
                {
                    person.PhoneNumber = updatePersonRequestDto.PhoneNumber;
                }

                person.UpdateTs = DateTime.UtcNow;
                person.UpdateUser = updatePersonRequestDto.UpdateUser;
                _personLogger.LogInformation("{className}.{methodName} - Writing to Primary database", className, methodName);
                await _personRepo.CreateAsync(person);

                _personLogger.LogInformation("{className}.{methodName}: Successfully updated person data for ConsumerCode: {consumerCode}", className, methodName, updatePersonRequestDto.ConsumerCode);
               
                return new PersonResponseDto
                {
                    Person = _mapper.Map<PersonDto>(person)
                };
            }
            catch (Exception ex)
            {
                _personLogger.LogError(ex, "{className}.{methodName}: Error occurred while updating person data. Error: {Message}", className, methodName, ex.Message);
                throw;
            }
        }

        private static GetPersonAndConsumerResponseDto NotFoundErrorResponse()
        {
            return new GetPersonAndConsumerResponseDto { ErrorCode = StatusCodes.Status404NotFound };
        }
    }
}


