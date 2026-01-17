using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Admin.Core.Domain.Models;
using SunnyRewards.Helios.Admin.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Common.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Admin.Infrastructure.Repositories
{
    public class ScriptRepo : BaseRepo<ScriptModel>, IScriptRepo
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseLogger"></param>
        /// <param name="session"></param>
        public ScriptRepo(ILogger<BaseRepo<ScriptModel>> baseLogger, NHibernate.ISession session) : base(baseLogger, session)
        {
        }
    }
}
