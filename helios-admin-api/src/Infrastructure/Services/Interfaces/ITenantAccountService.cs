using SunnyBenefits.Fis.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces
{
    public interface ITenantAccountService
    {
        /// <summary>
        /// Create the tenant account if not exists else returns 409 error
        /// </summary>
        /// <param name="createTenantAccountRequestDto"></param>
        /// <returns>A base response dto </returns>
        Task<BaseResponseDto> CreateTenantAccount(CreateTenantAccountRequestDto createTenantAccountRequestDto);
        Task<GetTenantAccountResponseDto> GetTenantAccount(string tenantCode);
        Task<TenantAccountUpdateResponseDto> UpdateTenantAccount(string tenantCode, TenantAccountRequestDto tenantAccountDto);
        Task<BaseResponseDto> SaveTenantAccount(TenantAccountRequestDto tenantAccountDto);
        Task<BaseResponseDto> CreateMasterWallets(TenantAccountRequestDto tenantAccountRequestDto, string customerCode, string sponsorCode, string createUser);
    }
}
