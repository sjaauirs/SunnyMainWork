using Microsoft.Extensions.Logging;
using NHibernate;
using SunnyRewards.Helios.Common.Core.Repositories;
using SunnyRewards.Helios.User.Core.Domain.Models;
using SunnyRewards.Helios.User.Infrastructure.Repositories.Interfaces;

namespace SunnyRewards.Helios.User.Infrastructure.Repositories
{
    public class PersonAddressRepo : BaseRepo<PersonAddressModel>, IPersonAddressRepo
    {
        public PersonAddressRepo(ILogger<BaseRepo<PersonAddressModel>> logger, ISession session) : base(logger, session)
        {

        }
    }
}
