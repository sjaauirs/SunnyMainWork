namespace SunnyRewards.Helios.Admin.Core.Domain.Dtos
{
    public class ImportAdminRequestDto
    {
        public string TenantCode { get; set; } = null!;
        public List<ScriptDto> Scripts { get; set; } = [];
        public List<TenantTaskRewardScriptDto> TenantTaskRewardScripts { get; set; } = [];
        public List<EventHandlerScriptDto> EventHandlerScripts { get; set; }= [];
    }
}
