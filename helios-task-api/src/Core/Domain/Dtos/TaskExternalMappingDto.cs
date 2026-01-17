using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class TaskExternalMappingDto: BaseDto
    {
        public long TaskExternalMappingId { get; set; }
        public string? TenantCode { get; set; }
        public string? TaskThirdPartyCode { get; set; }
        public string? TaskExternalCode { get; set; }
    }
}
