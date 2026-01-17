using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Common.Core.Repositories;
using SunnyRewards.Helios.ETL.Core.Domain.Enums;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces;

namespace SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo
{
    public class PersonAddressRepo : BaseRepo<ETLPersonAddressModel>, IPersonAddressRepo 
    {
        private readonly ILogger<BaseRepo<ETLPersonAddressModel>> _logger;
        private readonly NHibernate.ISession _session;
        private const string _className = nameof(PersonAddressRepo);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="session"></param>
        public PersonAddressRepo(ILogger<BaseRepo<ETLPersonAddressModel>> logger, NHibernate.ISession session) : base(logger, session)
        {
            _logger = logger;
            _session = session;
        }

        /// <summary>
        /// Retrieves the primary address of person of address type MAILING
        /// </summary>
        /// <param name="personId"></param>
        /// <returns></returns>
        public IQueryable<ETLPersonAddressModel> GetPrimaryMailingAddress(long personId)
        {
            try
            {
                var query = from address in _session.Query<ETLPersonAddressModel>()
                            where address.PersonId == personId
                                  && address.AddressTypeId == (long)AddressTypeEnum.MAILING
                                  && address.IsPrimary
                                  && address.DeleteNbr == 0
                            select address;
                return query;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{_className} - GetPrimaryMailingAddress - Error: {ex.Message}");
                throw;
            }
        }
    }
}
