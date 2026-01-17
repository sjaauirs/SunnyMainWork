using SunnyRewards.Helios.ETL.Common.Repositories.Interfaces;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos.Json;

namespace SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces
{
    public interface IPersonRepo : IBaseRepo<ETLPersonModel>
    {
        /// <summary>
        /// Returns valid persons for the given tenant
        /// </summary>
        /// <param name="tenantCode"></param>
        /// <returns></returns>
        IQueryable<ETLPersonModel> GetConsumerPersons(string tenantCode, int skip, int take, TenantOption tenantOption, string cohortCode);

        Task<ETLConsumerModel?> GetConsumerByPersonUniqueIdentifierAndTenantCode(string? personUniqueIdentifier, string? tenantCode);

        /// <summary>
        /// Get consumer by given email and tenant code
        /// </summary>
        /// <param name="email"></param>
        /// <param name="tenantCode"></param>
        /// <returns></returns>
        Task<ETLConsumerModel?> GetConsumerByEmailAndTenantCode(string? email, string? tenantCode);

        ETLPersonModel? GetConsumerPersonForUpdateInfo(string tenantCode, string consumerCode);
        Task<ETLConsumerModel?> GetConsumerByPersonIdAndTenantCode(long personId, string? tenantCode);
    }
}

