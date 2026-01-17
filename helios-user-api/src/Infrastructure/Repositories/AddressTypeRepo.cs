using Microsoft.Extensions.Logging;
using NHibernate;
using SunnyRewards.Helios.Common.Core.Repositories;
using SunnyRewards.Helios.User.Core.Domain.Models;
using SunnyRewards.Helios.User.Infrastructure.Repositories.Interfaces;


namespace SunnyRewards.Helios.User.Infrastructure.Repositories
{
    public class AddressTypeRepo : BaseRepo<AddressTypeModel>, IAddressTypeRepo
    {
        public AddressTypeRepo(ILogger<BaseRepo<AddressTypeModel>> logger, ISession session) : base(logger, session)
        {

        }
    }
}
