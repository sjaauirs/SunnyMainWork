using SunnyBenefits.Fis.Core.Domain.Dtos;

namespace Sunny.Benefits.Bff.Infrastructure.Services.Interfaces
{
    public interface ITenantAccountService
    {
        /// <summary>
        /// Retrive the tenant account details by using the tenant code
        /// </summary>
        /// <param name="createRequestDto">request contains tenant code</param>
        /// <returns>TenantAccountDto</returns>
        Task<ExportTenantAccountResponseDto> GetTenantAccount(ExportTenantAccountRequestDto requestDto);

        /// <summary>
        /// Get TenantAccount By Tenant code
        /// </summary>
        /// <param name="requestDto"></param>
        /// <returns></returns>
        Task<TenantAccountDto> GetTenantAccount(TenantAccountCreateRequestDto requestDto);
    }
}
