using Microsoft.Extensions.Logging;
using NHibernate;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Core.Domain.Models;
using SunnyRewards.Helios.Admin.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Common.Core.Repositories;

namespace SunnyRewards.Helios.Admin.Infrastructure.Repositories
{
    public class EventHandlerScriptRepo : BaseRepo<EventHandlerScriptModel>, IEventHandlerScriptRepo
    {
        private readonly ISession _session;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseLogger"></param>
        /// <param name="session"></param>
        public EventHandlerScriptRepo(ILogger<BaseRepo<EventHandlerScriptModel>> baseLogger, NHibernate.ISession session) : base(baseLogger, session)
        {
            _session = session;
        }

        public List<ExportEventHandlerScriptDto> GetEventHandlerScripts(string tenantCode)
        {
            var scripts = (from eventHandlerScript in _session.Query<EventHandlerScriptModel>()
                           join script in _session.Query<ScriptModel>()
                           on eventHandlerScript.ScriptId equals script.ScriptId
                           where eventHandlerScript.DeleteNbr == 0
                                 && script.DeleteNbr == 0
                                 && eventHandlerScript.TenantCode == tenantCode
                           select new ExportEventHandlerScriptDto
                           {
                               EventHandlerScript = eventHandlerScript,
                               Script = script
                           }).ToList();
            return scripts;
        }
    }
}
