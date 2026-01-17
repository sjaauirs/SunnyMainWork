using SunnyBenefits.Fis.Core.Domain.Models;
using SunnyRewards.Helios.ETL.Common.Repositories.Interfaces;
using SunnyRewards.Helios.ETL.Core.Domain.Models;

namespace SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces
{
    public interface IConsumerAccountRepo : IBaseRepo<ETLConsumerAccountModel>
    {
        List<ETLConsumerAccountModel> GetConsumerAccounts(string tenantCode,int take);
        /// <summary>
        /// When ever we are updating consumer account don't use updateAsync instead use this method.
        /// It will update consumerAccount and with the same object will Update ConsumerAccountHistory also.
        /// </summary>
        /// <param name="consumerAccountModel"></param>
        /// <returns></returns>
        Task<ETLConsumerAccountModel> UpdateConsumerAccount(ETLConsumerAccountModel consumerAccountModel);
    }
}
