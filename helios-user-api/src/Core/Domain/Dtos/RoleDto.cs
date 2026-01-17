using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.User.Core.Domain.Dtos
{
    public class RoleDto : BaseDto
    {
        public long RoleId { get; set; }
        public string? RoleCode { get; set; }
        public string? RoleName { get; set; }
        public string? RoleDescription { get; set; }
    }
}