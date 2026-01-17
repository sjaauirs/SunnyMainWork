using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.enums;
using SunnyRewards.Helios.User.Core.Domain.Models;
using SunnyRewards.Helios.User.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.User.Infrastructure.Services.Interfaces;

namespace SunnyRewards.Helios.User.Infrastructure.Services
{
    public class PersonAddressService : IPersonAddressService
    {
        public readonly IPersonAddressRepo _personAddressRepo;
        public readonly ILogger<PersonAddressService> _logger;
        public readonly IMapper _mapper;
        public readonly IAddressTypeService _addressTypeService;
        public readonly IPersonService _personService;
        private const string className = nameof(PersonAddressService);

        public PersonAddressService(IPersonAddressRepo personAddressRepo, ILogger<PersonAddressService> logger, IMapper mapper, IAddressTypeService addressTypeService, IPersonService personService)
        {
            _personAddressRepo = personAddressRepo;
            _logger = logger;
            _mapper = mapper;
            _addressTypeService = addressTypeService;
            _personService = personService;
        }

        public async Task<GetAllPersonAddressesResponseDto> GetAllPersonAddresses(long personId)
        {
            const string methodName = nameof(GetAllPersonAddresses);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName} - Fetching all addresses for person id {personId}", className, methodName, personId);
                var personAddresses = await _personAddressRepo.FindOrderedAsync(x => x.PersonId == personId && x.DeleteNbr == 0, x => x.CreateTs, true, false);
                if (personAddresses == null || !personAddresses.Any()) 
                {
                    return new GetAllPersonAddressesResponseDto()
                    {
                        ErrorCode = StatusCodes.Status404NotFound,
                        ErrorMessage = $"No addresses found for the person id {personId}."
                    };
                }
                _logger.LogInformation("{ClassName}.{MethodName} - Successfully fetched addresses for person id {personId}", className, methodName, personId);
                return new GetAllPersonAddressesResponseDto()
                {
                    PersonAddressesList = _mapper.Map<IList<PersonAddressDto>>(personAddresses)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - An error occurred while fetching addresses for person id {personId}: {ExceptionMessage}", className, methodName, personId, ex.Message);
                throw;
            }
        }
        public async Task<GetAllPersonAddressesResponseDto> GetPersonAddress(long personId, long? addressTypeId, bool? isPrimary)
        {
            const string methodName = nameof(GetPersonAddress);
            try
            {
                _logger.LogInformation("{className}.{methodName} - Started fetching addresses for person {PersonId}.", className, methodName, personId);

                var personAddresses = await _personAddressRepo.FindOrderedAsync(x =>
                    x.PersonId == personId &&
                    x.DeleteNbr == 0 &&
                    (!addressTypeId.HasValue || x.AddressTypeId == addressTypeId) &&
                    (!isPrimary.HasValue || x.IsPrimary == isPrimary), x => x.CreateTs, true, false);

                if (personAddresses == null || !personAddresses.Any())
                {
                    return new GetAllPersonAddressesResponseDto
                    {
                        ErrorCode = StatusCodes.Status404NotFound,
                        ErrorMessage = "No addresses found for the given criteria."
                    };
                }

                _logger.LogInformation("{className}.{methodName} - Successfully fetched {Count} addresses for person {PersonId}.", className, methodName, personAddresses.Count, personId);

                return new GetAllPersonAddressesResponseDto
                {
                    PersonAddressesList = _mapper.Map<IList<PersonAddressDto>>(personAddresses)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{className}.{methodName} - Error occurred while getting addresses for person {PersonId}.", className, methodName, personId);
                throw;
            }
        }

        public async Task<PersonAddressResponseDto> CreatePersonAddress(CreatePersonAddressRequestDto request)
        {
            const string methodName = nameof(CreatePersonAddress);
            try
            {
                _logger.LogInformation("{className}.{methodName} - Creating address for person {PersonId}", className, methodName, request.PersonId);

                var existingAddress = await _personAddressRepo.FindOneAsync(x =>
                    x.PersonId == request.PersonId &&
                    x.AddressTypeId == request.AddressTypeId &&
                    x.DeleteNbr == 0 &&
                    x.Line1.Trim().ToLower() == request.Line1.Trim().ToLower() &&
                    x.State.Trim().ToLower() == request.State.Trim().ToLower() &&
                    x.CountryCode.Trim().ToLower() == request.CountryCode.Trim().ToLower() &&
                    x.PostalCode.Trim().ToLower() == request.PostalCode.Trim().ToLower());

                if (existingAddress != null)
                {
                    return new PersonAddressResponseDto()
                    {
                        ErrorCode = StatusCodes.Status409Conflict,
                        ErrorMessage = "Address already exists."
                    };
                }

                var existingAddressType = await _addressTypeService.GetAddressTypeById(request.AddressTypeId);

                if (existingAddressType.ErrorCode == StatusCodes.Status404NotFound)
                {
                    return new PersonAddressResponseDto()
                    {
                        ErrorCode = StatusCodes.Status400BadRequest,
                        ErrorMessage = $"Address type with id {request.AddressTypeId} does not exist."
                    };
                }

                var existingPerson = await _personService.GetPersonData(request.PersonId);

                if (existingPerson.PersonId <= 0)
                {
                    return new PersonAddressResponseDto()
                    {
                        ErrorCode = StatusCodes.Status400BadRequest,
                        ErrorMessage = $"Person with id {request.PersonId} does not exist."
                    };
                }

                var newAddress = _mapper.Map<PersonAddressModel>(request);
                newAddress.CreateTs = DateTime.UtcNow;

                await _personAddressRepo.CreateAsync(newAddress);

                _logger.LogInformation("{className}.{methodName} - Address created successfully for person {PersonId}", className, methodName, request.PersonId);

                return new PersonAddressResponseDto()
                {
                    PersonAddress = _mapper.Map<PersonAddressDto>(newAddress)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{className}.{methodName} - Error creating person address for person {PersonId}", className, methodName, request.PersonId);
                throw;
            }
        }

        public async Task<PersonAddressResponseDto> UpdatePersonAddress(UpdatePersonAddressRequestDto request)
        {
            const string methodName = nameof(UpdatePersonAddress);
            try
            {
                _logger.LogInformation("{className}.{methodName} - Updating address {PersonAddressId}", className, methodName, request.PersonAddressId);

                var existing = await _personAddressRepo.FindOneAsync(x => x.PersonAddressId == request.PersonAddressId && x.DeleteNbr == 0);
                if (existing == null)
                {
                    return new PersonAddressResponseDto()
                    {
                        ErrorCode = StatusCodes.Status404NotFound,
                        ErrorMessage = $"Address with id {request.PersonAddressId} does not exist."
                    };
                }

                var existingAddressType = await _addressTypeService.GetAddressTypeById(request.AddressTypeId);

                if (existingAddressType.ErrorCode == StatusCodes.Status404NotFound)
                {
                    return new PersonAddressResponseDto()
                    {
                        ErrorCode = StatusCodes.Status400BadRequest,
                        ErrorMessage = $"Address type with id {request.AddressTypeId} does not exist."
                    };
                }

                existing.AddressTypeId = request.AddressTypeId;
                if (!string.IsNullOrWhiteSpace(request.AddressLabel)) existing.AddressLabel = request.AddressLabel;
                if (!string.IsNullOrWhiteSpace(request.Line1)) existing.Line1 = request.Line1;
                if (!string.IsNullOrWhiteSpace(request.Line2)) existing.Line2 = request.Line2;
                if (!string.IsNullOrWhiteSpace(request.City)) existing.City = request.City;
                if (!string.IsNullOrWhiteSpace(request.State)) existing.State = request.State;
                if (!string.IsNullOrWhiteSpace(request.PostalCode)) existing.PostalCode = request.PostalCode;
                if (!string.IsNullOrWhiteSpace(request.Region)) existing.Region = request.Region;
                if (!string.IsNullOrWhiteSpace(request.CountryCode)) existing.CountryCode = request.CountryCode;
                if (!string.IsNullOrWhiteSpace(request.Country)) existing.Country = request.Country;
                if (!string.IsNullOrWhiteSpace(request.Source)) existing.Source = request.Source;

                existing.UpdateUser = request.UpdateUser;
                existing.UpdateTs = DateTime.UtcNow;

                await _personAddressRepo.CreateAsync(existing);

                _logger.LogInformation("{className}.{methodName} - Successfully updated address {PersonAddressId}", className, methodName, request.PersonAddressId);

                return new PersonAddressResponseDto()
                {
                    PersonAddress = _mapper.Map<PersonAddressDto>(existing)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{className}.{methodName} - Error updating person address {PersonAddressId}", className, methodName, request.PersonAddressId);
                throw;
            }
        }

        public async Task<PersonAddressResponseDto> DeletePersonAddress(DeletePersonAddressRequestDto request)
        {
            const string methodName = nameof(DeletePersonAddress);
            try
            {
                _logger.LogInformation("{className}.{methodName} - Deleting address with ID {PersonAddressId}", className, methodName, request.PersonAddressId);

                var address = await _personAddressRepo.FindOneAsync(x =>
                    x.PersonAddressId == request.PersonAddressId && x.DeleteNbr == 0);

                if (address == null)
                {
                    return new PersonAddressResponseDto()
                    {
                        ErrorCode = StatusCodes.Status404NotFound,
                        ErrorMessage = $"Address with id {request.PersonAddressId} does not exist."
                    };
                }

                address.DeleteNbr = address.PersonAddressId;
                address.UpdateTs = DateTime.UtcNow;
                address.UpdateUser = request.UpdateUser;

                await _personAddressRepo.CreateAsync(address);

                _logger.LogInformation("{className}.{methodName} - Address with ID {PersonAddressId} marked as deleted.", className, methodName, request.PersonAddressId);

                return new PersonAddressResponseDto()
                {
                    PersonAddress = _mapper.Map<PersonAddressDto>(address)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{className}.{methodName} - Error occurred while deleting address {PersonAddressId}", className, methodName, request.PersonAddressId);
                throw;
            }
        }

        public async Task<PersonAddressResponseDto> SetPrimaryAddress(UpdatePrimaryPersonAddressRequestDto request)
        {
            const string methodName = nameof(SetPrimaryAddress);
            try
            {
                _logger.LogInformation("{className}.{methodName} - Setting address {PersonAddressId} as primary.", className, methodName, request.PersonAddressId);

                var addressToUpdate = await _personAddressRepo.FindOneAsync(x =>
                    x.PersonAddressId == request.PersonAddressId && x.DeleteNbr == 0);

                if (addressToUpdate == null)
                {
                    return new PersonAddressResponseDto()
                    {
                        ErrorCode = StatusCodes.Status404NotFound,
                        ErrorMessage = $"Address with id {request.PersonAddressId} does not exist."
                    };
                }

                if (addressToUpdate.AddressTypeId != (long)AddressTypeEnum.MAILING)
                {
                    return new PersonAddressResponseDto()
                    {
                        ErrorCode = StatusCodes.Status400BadRequest,
                        ErrorMessage = $"Address type {addressToUpdate.AddressTypeId} cannot be set to primary address."
                    };
                }

                var personId = addressToUpdate.PersonId;

                // Setting current primary address to non-primary
                var currentPrimaryAddress = await _personAddressRepo.FindOneAsync(x =>
                    x.PersonId == personId && x.IsPrimary && x.DeleteNbr == 0);
                if (currentPrimaryAddress != null)
                {
                    currentPrimaryAddress.IsPrimary = false;
                    currentPrimaryAddress.UpdateTs = DateTime.UtcNow;
                    currentPrimaryAddress.UpdateUser = request.UpdateUser;
                    await _personAddressRepo.CreateAsync(currentPrimaryAddress);
                }

                addressToUpdate.IsPrimary = true;
                addressToUpdate.UpdateTs = DateTime.UtcNow;
                addressToUpdate.UpdateUser = request.UpdateUser;
                await _personAddressRepo.CreateAsync(addressToUpdate);

                _logger.LogInformation("{className}.{methodName} - Primary address set for person {PersonId}.", className, methodName, personId);

                return new PersonAddressResponseDto
                {
                    PersonAddress = _mapper.Map<PersonAddressDto>(addressToUpdate)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{className}.{methodName} - Error while setting primary address.", className, methodName);
                throw;
            }
        }

    }
}
