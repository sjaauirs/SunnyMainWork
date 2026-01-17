using SunnyRewards.Helios.User.Core.Domain.Dtos;

namespace Sunny.Benefits.Bff.Infrastructure.Services.Interfaces
{
    public interface IPersonAddressService
    {
        Task<GetAllPersonAddressesResponseDto> GetAllPersonAddresses(long personId);
        Task<PersonAddressResponseDto> CreatePersonAddress(CreatePersonAddressRequestDto request);
        Task<PersonAddressResponseDto> UpdatePersonAddress(UpdatePersonAddressRequestDto request, bool markAsPrimary);
    }
}
