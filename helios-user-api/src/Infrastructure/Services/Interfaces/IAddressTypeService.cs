using SunnyRewards.Helios.User.Core.Domain.Dtos;

namespace SunnyRewards.Helios.User.Infrastructure.Services.Interfaces
{
    public interface IAddressTypeService
    {
        Task<GetAllAddressTypesResponseDto> GetAllAddressTypes();
        Task<GetAddressTypeResponseDto> GetAddressTypeById(long addressTypeId);
    }
}
