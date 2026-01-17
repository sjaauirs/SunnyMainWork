using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Models;
using SunnyRewards.Helios.User.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.User.Infrastructure.Services.Interfaces;

namespace SunnyRewards.Helios.User.Infrastructure.Services
{
    public class PhoneNumberService : IPhoneNumberService
    {
        public readonly IPhoneNumberRepo _phoneNumberRepo;
        public readonly ILogger<PhoneNumberService> _logger;
        public readonly IMapper _mapper;
        public readonly IPhoneTypeService _phoneTypeService;
        public readonly IPersonService _personService;
        private const string className = nameof(PhoneNumberService);

        public PhoneNumberService(IPhoneNumberRepo phoneNumberRepo, ILogger<PhoneNumberService> logger, IMapper mapper, IPhoneTypeService phoneTypeService, IPersonService personService)
        {
            _phoneNumberRepo = phoneNumberRepo;
            _logger = logger;
            _mapper = mapper;
            _phoneTypeService = phoneTypeService;
            _personService = personService;
        }

        public async Task<GetAllPhoneNumbersResponseDto> GetAllPhoneNumbers(long personId)
        {
            const string methodName = nameof(GetAllPhoneNumbers);
            try
            {
                _logger.LogInformation("{className}.{methodName} - Fetching all phone numbers for person id {personId}", className, methodName, personId);
                var phoneNumbers = await _phoneNumberRepo.FindOrderedAsync(x => x.PersonId == personId && x.DeleteNbr == 0, x => x.CreateTs, true, false);
                if (phoneNumbers == null || !phoneNumbers.Any())
                {
                    return new GetAllPhoneNumbersResponseDto
                    {
                        ErrorCode = StatusCodes.Status404NotFound,
                        ErrorMessage = $"No phone numbers found for the person id {personId}."
                    };
                }

                _logger.LogInformation("{className}.{methodName} - Successfully fetched phone numbers for person id {personId}", className, methodName, personId);
                return new GetAllPhoneNumbersResponseDto
                {
                    PhoneNumbersList = _mapper.Map<IList<PhoneNumberDto>>(phoneNumbers)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{className}.{methodName} - Error while fetching phone numbers for person id {personId}: {ExceptionMessage}", className, methodName, personId, ex.Message);
                throw;
            }
        }

        public async Task<GetAllPhoneNumbersResponseDto> GetPhoneNumber(long personId, long? phoneTypeId, bool? isPrimary)
        {
            const string methodName = nameof(GetPhoneNumber);
            try
            {
                _logger.LogInformation("{className}.{methodName} - Started fetching phone numbers for person {PersonId}.", className, methodName, personId);

                var phoneNumbers = await _phoneNumberRepo.FindOrderedAsync(
                    x =>
                        x.PersonId == personId &&
                        x.DeleteNbr == 0 &&
                        (!phoneTypeId.HasValue || x.PhoneTypeId == phoneTypeId) &&
                        (!isPrimary.HasValue || x.IsPrimary == isPrimary),
                    x => x.CreateTs,
                    true, false);

                if (phoneNumbers == null || !phoneNumbers.Any())
                {
                    return new GetAllPhoneNumbersResponseDto
                    {
                        ErrorCode = StatusCodes.Status404NotFound,
                        ErrorMessage = "No phone numbers found for the given criteria."
                    };
                }

                _logger.LogInformation("{className}.{methodName} - Successfully fetched {Count} phone numbers for person {PersonId}.", className, methodName, phoneNumbers.Count, personId);

                return new GetAllPhoneNumbersResponseDto
                {
                    PhoneNumbersList = _mapper.Map<IList<PhoneNumberDto>>(phoneNumbers)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{className}.{methodName} - Error occurred while getting phone numbers for person {PersonId}.", className, methodName, personId);
                throw;
            }
        }

        public async Task<PhoneNumberResponseDto> CreatePhoneNumber(CreatePhoneNumberRequestDto request)
        {
            const string methodName = nameof(CreatePhoneNumber);
            try
            {
                _logger.LogInformation("{className}.{methodName} - Creating phone number for person {PersonId}", className, methodName, request.PersonId);

                var existing = await _phoneNumberRepo.FindOneAsync(x =>
                    x.PersonId == request.PersonId &&
                    x.PhoneTypeId == request.PhoneTypeId &&
                    x.DeleteNbr == 0 &&
                    x.PhoneNumber == request.PhoneNumber.Trim());

                if (existing != null)
                {
                    return new PhoneNumberResponseDto
                    {
                        ErrorCode = StatusCodes.Status409Conflict,
                        ErrorMessage = "Phone number already exists."
                    };
                }

                var phoneTypeResult = await _phoneTypeService.GetPhoneTypeById(request.PhoneTypeId);
                if (phoneTypeResult.ErrorCode == StatusCodes.Status404NotFound)
                {
                    return new PhoneNumberResponseDto
                    {
                        ErrorCode = StatusCodes.Status400BadRequest,
                        ErrorMessage = $"Phone type with id {request.PhoneTypeId} does not exist."
                    };
                }

                var personResult = await _personService.GetPersonData(request.PersonId);
                if (personResult.PersonId <= 0)
                {
                    return new PhoneNumberResponseDto
                    {
                        ErrorCode = StatusCodes.Status400BadRequest,
                        ErrorMessage = $"Person with id {request.PersonId} does not exist."
                    };
                }

                var newPhoneNumber = _mapper.Map<PhoneNumberModel>(request);
                newPhoneNumber.PhoneNumberCode = $"pnc-{Guid.NewGuid().ToString("N")}";
                newPhoneNumber.CreateTs = DateTime.UtcNow;

                await _phoneNumberRepo.CreateAsync(newPhoneNumber);

                _logger.LogInformation("{className}.{methodName} - Phone number created for person {PersonId}", className, methodName, request.PersonId);

                return new PhoneNumberResponseDto
                {
                    PhoneNumber = _mapper.Map<PhoneNumberDto>(newPhoneNumber)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{className}.{methodName} - Error creating phone number for person {PersonId}", className, methodName, request.PersonId);
                throw;
            }
        }

        public async Task<PhoneNumberResponseDto> UpdatePhoneNumber(UpdatePhoneNumberRequestDto request)
        {
            const string methodName = nameof(UpdatePhoneNumber);
            try
            {
                _logger.LogInformation("{className}.{methodName} - Updating phone number {PhoneNumberId}", className, methodName, request.PhoneNumberId);

                var existing = await _phoneNumberRepo.FindOneAsync(x => x.PhoneNumberId == request.PhoneNumberId && x.DeleteNbr == 0);
                if (existing == null)
                {
                    return new PhoneNumberResponseDto
                    {
                        ErrorCode = StatusCodes.Status404NotFound,
                        ErrorMessage = $"Phone number with id {request.PhoneNumberId} does not exist."
                    };
                }

                var phoneTypeResult = await _phoneTypeService.GetPhoneTypeById(request.PhoneTypeId);
                if (phoneTypeResult.ErrorCode == StatusCodes.Status404NotFound)
                {
                    return new PhoneNumberResponseDto
                    {
                        ErrorCode = StatusCodes.Status400BadRequest,
                        ErrorMessage = $"Phone type with id {request.PhoneTypeId} does not exist."
                    };
                }

                existing.PhoneTypeId = request.PhoneTypeId;
                if (!string.IsNullOrWhiteSpace(request.PhoneNumber)) existing.PhoneNumber = request.PhoneNumber;
                if (!string.IsNullOrWhiteSpace(request.Source)) existing.Source = request.Source;
                existing.IsVerified = request.IsVerified;
                if (request.VerifiedDate.HasValue) existing.VerifiedDate = request.VerifiedDate.Value;
                existing.UpdateUser = request.UpdateUser;
                existing.UpdateTs = DateTime.UtcNow;

                await _phoneNumberRepo.CreateAsync(existing);

                _logger.LogInformation("{className}.{methodName} - Successfully updated phone number {PhoneNumberId}", className, methodName, request.PhoneNumberId);

                return new PhoneNumberResponseDto
                {
                    PhoneNumber = _mapper.Map<PhoneNumberDto>(existing)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{className}.{methodName} - Error updating phone number {PhoneNumberId}", className, methodName, request.PhoneNumberId);
                throw;
            }
        }

        public async Task<PhoneNumberResponseDto> DeletePhoneNumber(DeletePhoneNumberRequestDto request)
        {
            const string methodName = nameof(DeletePhoneNumber);
            try
            {
                _logger.LogInformation("{className}.{methodName} - Deleting phone number with ID {PhoneNumberId}", className, methodName, request.PhoneNumberId);

                var phone = await _phoneNumberRepo.FindOneAsync(x =>
                    x.PhoneNumberId == request.PhoneNumberId && x.DeleteNbr == 0);

                if (phone == null)
                {
                    return new PhoneNumberResponseDto
                    {
                        ErrorCode = StatusCodes.Status404NotFound,
                        ErrorMessage = $"Phone number with id {request.PhoneNumberId} does not exist."
                    };
                }

                phone.DeleteNbr = phone.PhoneNumberId;
                phone.UpdateTs = DateTime.UtcNow;
                phone.UpdateUser = request.UpdateUser;

                await _phoneNumberRepo.CreateAsync(phone);

                _logger.LogInformation("{className}.{methodName} - Phone number with ID {PhoneNumberId} marked as deleted.", className, methodName, request.PhoneNumberId);

                return new PhoneNumberResponseDto
                {
                    PhoneNumber = _mapper.Map<PhoneNumberDto>(phone)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{className}.{methodName} - Error occurred while deleting phone number {PhoneNumberId}", className, methodName, request.PhoneNumberId);
                throw;
            }
        }

        public async Task<PhoneNumberResponseDto> SetPrimaryPhoneNumber(UpdatePrimaryPhoneNumberRequestDto request)
        {
            const string methodName = nameof(SetPrimaryPhoneNumber);
            try
            {
                _logger.LogInformation("{className}.{methodName} - Setting phone number {PhoneNumberId} as primary.", className, methodName, request.PhoneNumberId);

                var phoneToUpdate = await _phoneNumberRepo.FindOneAsync(x => x.PhoneNumberId == request.PhoneNumberId && x.DeleteNbr == 0);
                if (phoneToUpdate == null)
                {
                    return new PhoneNumberResponseDto
                    {
                        ErrorCode = StatusCodes.Status404NotFound,
                        ErrorMessage = $"Phone number with id {request.PhoneNumberId} does not exist."
                    };
                }

                var personId = phoneToUpdate.PersonId;

                // Resetting current primary phone
                var currentPrimary = await _phoneNumberRepo.FindOneAsync(x => x.PersonId == personId && x.IsPrimary && x.DeleteNbr == 0);
                if (currentPrimary != null)
                {
                    currentPrimary.IsPrimary = false;
                    currentPrimary.UpdateTs = DateTime.UtcNow;
                    currentPrimary.UpdateUser = request.UpdateUser;
                    await _phoneNumberRepo.CreateAsync(currentPrimary);
                }

                phoneToUpdate.IsPrimary = true;
                phoneToUpdate.UpdateTs = DateTime.UtcNow;
                phoneToUpdate.UpdateUser = request.UpdateUser;
                await _phoneNumberRepo.CreateAsync(phoneToUpdate);

                _logger.LogInformation("{className}.{methodName} - Primary phone number set for person {PersonId}.", className, methodName, personId);

                return new PhoneNumberResponseDto
                {
                    PhoneNumber = _mapper.Map<PhoneNumberDto>(phoneToUpdate)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{className}.{methodName} - Error while setting primary phone number.", className, methodName);
                throw;
            }
        }
    }
}
