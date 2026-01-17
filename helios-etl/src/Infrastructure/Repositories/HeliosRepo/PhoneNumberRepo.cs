using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Common.Core.Repositories;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces;

namespace SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo
{
    public class PhoneNumberRepo : BaseRepo<ETLPhoneNumberModel>, IPhoneNumberRepo
    {
        private readonly ILogger<BaseRepo<ETLPhoneNumberModel>> _logger;
        private readonly NHibernate.ISession _session;
        private const string _className = nameof(PhoneNumberRepo);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="session"></param>
        public PhoneNumberRepo(ILogger<BaseRepo<ETLPhoneNumberModel>> logger, NHibernate.ISession session) : base(logger, session)
        {
            _logger = logger;
            _session = session;
        }

        /// <summary>
        /// Retrieves the primary phone number of person
        /// </summary>
        /// <param name="personId"></param>
        /// <returns></returns>
        public IQueryable<ETLPhoneNumberModel> GetPrimaryPhoneNumber(long personId)
        {
            try
            {
                var query = from number in _session.Query<ETLPhoneNumberModel>()
                            where number.PersonId == personId
                                  && number.IsPrimary
                                  && number.DeleteNbr == 0
                            select number;
                return query;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{_className} - GetPrimaryPhoneNumber - Error: {ex.Message}");
                throw;
            }
        }
    }
}
