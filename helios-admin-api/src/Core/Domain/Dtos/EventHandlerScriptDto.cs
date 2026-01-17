namespace SunnyRewards.Helios.Admin.Core.Domain.Dtos
{
    public class EventHandlerScriptDto
    {
        public long EventHandlerId { get; set; }
        public string? EventHandlerCode { get; set; }
        public string? TenantCode { get; set; }
        public long ScriptId { get; set; }
        public string? EventType { get; set; }
        public string? EventSubType { get; set; }
    }
}
