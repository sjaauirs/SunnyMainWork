using SunnyRewards.Helios.User.Core.Domain.Dtos;

namespace SunnyRewards.Helios.User.Infrastructure.Services.Interfaces
{
    public interface IPersonAddressService
    {
        Task<GetAllPersonAddressesResponseDto> GetAllPersonAddresses(long personId);
        Task<GetAllPersonAddressesResponseDto> GetPersonAddress(long personId, long? addressTypeId, bool? isPrimary);
        Task<PersonAddressResponseDto> CreatePersonAddress(CreatePersonAddressRequestDto createPersonAddressRequestDto);
        Task<PersonAddressResponseDto> UpdatePersonAddress(UpdatePersonAddressRequestDto updatePersonAddressRequestDto);
        Task<PersonAddressResponseDto> DeletePersonAddress(DeletePersonAddressRequestDto request);
        Task<PersonAddressResponseDto> SetPrimaryAddress(UpdatePrimaryPersonAddressRequestDto request);
    }
}
