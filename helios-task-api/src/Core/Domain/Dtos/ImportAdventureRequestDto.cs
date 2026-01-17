using System.ComponentModel.DataAnnotations;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class ImportAdventureRequestDto
    {
        [Required]
        public required string TenantCode { get; set; }
        public IList<AdventureDto> Adventures { get; set; } = new List<AdventureDto>();
        public IList<TenantAdventureDto> TenantAdventures { get; set; } = new List<TenantAdventureDto>();
    }
}
