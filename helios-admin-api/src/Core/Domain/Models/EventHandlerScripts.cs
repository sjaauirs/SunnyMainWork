namespace SunnyRewards.Helios.Admin.Core.Domain.Models
{
    public class EventHandlerScripts
    {
        public EventHandlerScripts(EventHandlerScriptModel EventHandlerScript, ScriptModel Script)
        {
            this.EventHandlerScript = EventHandlerScript;
            this.Script = Script;
        }
        public EventHandlerScriptModel EventHandlerScript { get; set; }
        public ScriptModel Script { get; set; }
    }
}
