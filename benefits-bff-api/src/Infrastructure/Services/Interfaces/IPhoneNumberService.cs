using SunnyRewards.Helios.User.Core.Domain.Dtos;

namespace Sunny.Benefits.Bff.Infrastructure.Services.Interfaces
{
    public interface IPhoneNumberService
    {
        Task<GetAllPhoneNumbersResponseDto> GetAllPhoneNumbers(long personId);
        Task<PhoneNumberResponseDto> CreatePhoneNumber(CreatePhoneNumberRequestDto request);
        Task<PhoneNumberResponseDto> UpdatePhoneNumber(UpdatePhoneNumberRequestDto request, bool markAsPrimary);
    }
}
