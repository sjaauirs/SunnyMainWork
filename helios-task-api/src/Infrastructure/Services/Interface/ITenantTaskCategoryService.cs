using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Task.Infrastructure.Services.Interface
{
    public interface ITenantTaskCategoryService
    {
        /// <summary>
        /// Creates Tenant task category
        /// </summary>
        /// <param name="tenantTaskCategoryRequestDto">request contains for create tenant task category</param>
        /// <returns></returns>
        Task<BaseResponseDto> CreateTenantTaskCategory(TenantTaskCategoryRequestDto tenantTaskCategoryRequestDto);

        /// <summary>
        /// Updates an existing TenantTaskCategory based on the provided request data.
        /// </summary>
        /// <param name="requestDto">The request data containing the details to update.</param>
        /// <returns>A response DTO indicating success or failure.</returns>
        Task<TenantTaskCategoryResponseDto> UpdateTenantTaskCategory(TenantTaskCategoryDto requestDto);
    }
}
