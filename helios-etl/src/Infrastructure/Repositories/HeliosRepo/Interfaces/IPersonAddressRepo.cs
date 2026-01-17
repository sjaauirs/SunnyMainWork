using SunnyRewards.Helios.Common.Core.Repositories.Interfaces;
using SunnyRewards.Helios.ETL.Core.Domain.Models;

namespace SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces
{
    public interface IPersonAddressRepo : IBaseRepo<ETLPersonAddressModel>
    {
        /// <summary>
        /// Retrieves the primary address of person of address type MAILING
        /// </summary>
        /// <param name="personId"></param>
        /// <returns></returns>
        IQueryable<ETLPersonAddressModel> GetPrimaryMailingAddress(long personId);
    }
}
