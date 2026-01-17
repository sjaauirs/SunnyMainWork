using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Common.Core.Repositories;
using SunnyRewards.Helios.Admin.Core.Domain.Models;
using SunnyRewards.Helios.Admin.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using NHibernate;

namespace SunnyRewards.Helios.Admin.Infrastructure.Repositories
{
    public class TenantTaskRewardScriptRepo : BaseRepo<TenantTaskRewardScriptModel>, ITenantTaskRewardScriptRepo
    {
        private readonly ISession _session;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseLogger"></param>
        /// <param name="session"></param>
        public TenantTaskRewardScriptRepo(ILogger<BaseRepo<TenantTaskRewardScriptModel>> baseLogger, NHibernate.ISession session) : base(baseLogger, session)
        {
            _session = session;
        }

        public List<ExportTenantTaskRewardScriptDto> GetTenantTaskRewardScripts(string tenantCode)
        {
            var scripts = (from taskRewardScript in _session.Query<TenantTaskRewardScriptModel>()
                           join script in _session.Query<ScriptModel>()
                           on taskRewardScript.ScriptId equals script.ScriptId
                          where taskRewardScript.DeleteNbr == 0
                                && taskRewardScript.TenantCode == tenantCode
                                 && script.DeleteNbr == 0
                          select new ExportTenantTaskRewardScriptDto
                          {
                              TenantTaskRewardScript = taskRewardScript,
                              Script = script
                          }).ToList();
            return scripts;
        }
    }
}