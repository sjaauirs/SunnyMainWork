using SunnyRewards.Helios.ETL.Common.Repositories.Interfaces;
using SunnyRewards.Helios.ETL.Core.Domain.Models;

namespace SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces
{
    public interface ITenantRepo : IBaseRepo<ETLTenantModel>
    {

        Task<long> GetConsumerWalletTypeId();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Task<long> GetSubscriberRoleId();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tenantCode"></param>
        /// <returns></returns>
        Task<(string customerCode, string sponsorCode)> GetCustomerAndSponsorCode(string tenantCode);
    }
}

