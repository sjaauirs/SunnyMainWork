using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Core.Domain.Models;
using SunnyRewards.Helios.Common.Core.Repositories.Interfaces;

namespace SunnyRewards.Helios.Admin.Infrastructure.Repositories.Interfaces
{
    public interface IEventHandlerScriptRepo : IBaseRepo<EventHandlerScriptModel>
    {
        List<ExportEventHandlerScriptDto> GetEventHandlerScripts(string tenantCode);
    }

}
