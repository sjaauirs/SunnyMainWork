using SunnyRewards.Helios.Admin.Core.Domain.Models;

namespace SunnyRewards.Helios.Admin.Core.Domain.Dtos
{
    public class ExportEventHandlerScriptDto
    {
        public EventHandlerScriptModel EventHandlerScript { get; set; } = null!;
        public ScriptModel Script { get; set; } = null!;
    }
}
