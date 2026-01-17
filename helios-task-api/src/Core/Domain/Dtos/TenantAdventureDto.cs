namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class TenantAdventureDto
    {
        public long TenantAdventureId { get; set; }
        public string TenantAdventureCode { get; set; } = null!;
        public string TenantCode { get; set; } = null!;
        public long AdventureId { get; set; }
    }

}
