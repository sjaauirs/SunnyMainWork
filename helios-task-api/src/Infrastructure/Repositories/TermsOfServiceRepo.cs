using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Common.Core.Repositories;
using SunnyRewards.Helios.Task.Core.Domain.Models;
using SunnyRewards.Helios.Task.Infrastructure.Repositories.Interfaces;

namespace SunnyRewards.Helios.Task.Infrastructure.Repositories
{
    public class TermsOfServiceRepo : BaseRepo<TermsOfServiceModel>, ITermsOfServiceRepo
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseLogger"></param>
        /// <param name="session"></param>
        public TermsOfServiceRepo(ILogger<BaseRepo<TermsOfServiceModel>> baseLogger, NHibernate.ISession session) : base(baseLogger, session)
        {
        }
    }
}
