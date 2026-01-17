using SunnyRewards.Helios.User.Core.Domain.Dtos;

namespace Sunny.Benefits.Bff.Infrastructure.Services.Interfaces
{
    public interface IPersonService
    {
        Task<PersonResponseDto> UpdatePersonData(UpdatePersonRequestDto updatePersonRequestDto);
    }
}
