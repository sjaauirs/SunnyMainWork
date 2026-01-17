using Microsoft.Extensions.Logging;
using NHibernate;
using SunnyRewards.Helios.Common.Core.Repositories;
using SunnyRewards.Helios.User.Core.Domain.Models;
using SunnyRewards.Helios.User.Infrastructure.Repositories.Interfaces;

namespace SunnyRewards.Helios.User.Infrastructure.Repositories
{
    public class PhoneNumberRepo : BaseRepo<PhoneNumberModel>, IPhoneNumberRepo
    {
        public PhoneNumberRepo(ILogger<BaseRepo<PhoneNumberModel>> logger, ISession session) : base(logger, session)
        {

        }
    }
}
