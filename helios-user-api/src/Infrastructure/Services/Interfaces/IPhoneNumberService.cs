using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SunnyRewards.Helios.User.Core.Domain.Dtos;

namespace SunnyRewards.Helios.User.Infrastructure.Services.Interfaces
{
    public interface IPhoneNumberService
    {
        Task<GetAllPhoneNumbersResponseDto> GetAllPhoneNumbers(long personId);
        Task<GetAllPhoneNumbersResponseDto> GetPhoneNumber(long personId, long? phoneTypeId, bool? isPrimary);
        Task<PhoneNumberResponseDto> CreatePhoneNumber(CreatePhoneNumberRequestDto request);
        Task<PhoneNumberResponseDto> UpdatePhoneNumber(UpdatePhoneNumberRequestDto request);
        Task<PhoneNumberResponseDto> DeletePhoneNumber(DeletePhoneNumberRequestDto request);
        Task<PhoneNumberResponseDto> SetPrimaryPhoneNumber(UpdatePrimaryPhoneNumberRequestDto request);
    }
}
