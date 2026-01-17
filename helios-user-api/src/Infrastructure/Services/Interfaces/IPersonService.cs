using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.enums;

namespace SunnyRewards.Helios.User.Infrastructure.Services.Interfaces
{
    public interface IPersonService
    {
        Task<PersonDto> GetPersonData(long id);
        Task<GetPersonAndConsumerResponseDto> GetOverAllConsumerDetails(GetConsumerRequestDto consumerRequestDto);
        Task<PersonResponseDto> UpdatePersonData(UpdatePersonRequestDto updatePersonRequestDto);
    }
}
