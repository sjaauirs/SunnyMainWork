using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SunnyRewards.Helios.User.Core.Domain.Dtos;

namespace SunnyRewards.Helios.User.Infrastructure.Services.Interfaces
{
    public interface IPhoneTypeService
    {
        Task<GetAllPhoneTypesResponseDto> GetAllPhoneTypes();
        Task<GetPhoneTypeResponseDto> GetPhoneTypeById(long phoneTypeId);
    }
}
