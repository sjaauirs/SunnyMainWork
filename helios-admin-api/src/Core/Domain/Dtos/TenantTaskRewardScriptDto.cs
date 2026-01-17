using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Core.Domain.Dtos
{
    public class TenantTaskRewardScriptDto : BaseDto
    {
        public  long TenantTaskRewardScriptId { get; set; }
        public  string? TenantTaskRewardScriptCode { get; set; }
        public  string? TenantCode { get; set; }
        public  string? TaskRewardCode { get; set; }
        public  string? ScriptType { get; set; }
        public  long ScriptId { get; set; }
    }
}