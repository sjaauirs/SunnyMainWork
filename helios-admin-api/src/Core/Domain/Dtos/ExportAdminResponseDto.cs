using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Core.Domain.Dtos
{
    public class ExportAdminResponseDto:BaseResponseDto
    {
        public List<ScriptDto> Scripts { get; set; } = [];
        public List<EventHandlerScriptDto> EventHandlerScripts { get; set; } = [];
        public List<TenantTaskRewardScriptDto> TenantTaskRewardScripts { get; set; } = [];
    }
}
