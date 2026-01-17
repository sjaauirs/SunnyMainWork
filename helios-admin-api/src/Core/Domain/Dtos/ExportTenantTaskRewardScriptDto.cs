using SunnyRewards.Helios.Admin.Core.Domain.Models;

namespace SunnyRewards.Helios.Admin.Core.Domain.Dtos
{
    public class ExportTenantTaskRewardScriptDto
    {
        public TenantTaskRewardScriptModel TenantTaskRewardScript { get; set; } = null!;
        public ScriptModel Script { get; set; } = null!;
    }
}
