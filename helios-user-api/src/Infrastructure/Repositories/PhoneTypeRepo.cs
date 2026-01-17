using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Common.Core.Repositories;
using SunnyRewards.Helios.User.Core.Domain.Models;
using SunnyRewards.Helios.User.Infrastructure.Repositories.Interfaces;
using NHibernate;

namespace SunnyRewards.Helios.User.Infrastructure.Repositories
{
    public class PhoneTypeRepo : BaseRepo<PhoneTypeModel>, IPhoneTypeRepo
    {
        public PhoneTypeRepo(ILogger<BaseRepo<PhoneTypeModel>> logger, ISession session) : base(logger, session)
        {

        }
    }
}