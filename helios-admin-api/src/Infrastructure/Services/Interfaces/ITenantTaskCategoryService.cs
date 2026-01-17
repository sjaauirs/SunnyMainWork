using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces
{
    public interface ITenantTaskCategoryService
    {
        /// <summary>
        /// Creates Tenant task category
        /// </summary>
        /// <param name="tenantTaskCategoryRequestDto">request contains for create tenant task category</param>
        /// <returns></returns>
        Task<BaseResponseDto> CreateTenantTaskCategory(TenantTaskCategoryRequestDto tenantTaskCategoryRequestDto);
    }
}
