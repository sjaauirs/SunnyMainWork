using SunnyRewards.Helios.Wallet.Core.Domain.Dtos.Json;
using System.ComponentModel.DataAnnotations;

namespace SunnyRewards.Helios.Wallet.Core.Domain.Dtos
{
    public class CreateTenantMasterWalletsRequestDto
    {
        [Required]
        public string[] Apps { get; set; } = null!;
        [Required]
        public string TenantCode { get; set; } = null!;
        [Required]
        public string CustomerCode { get; set; } = null!;
        [Required]
        public string SponsorCode { get; set; } = null!;
        public PurseConfig? PurseConfig { get; set; }
        [Required]
        public string CreateUser { get; set; } = null!;

    }
}
