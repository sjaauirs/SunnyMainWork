namespace Sunny.Benefits.Bff.Core.Domain.Dtos
{
    public class PatchUserRequestDto
    {
        public string? Email { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string? DeviceId { get; set; }
        public string? DeviceType { get; set; }
        public string? DeviceAttrJson { get; set; }
    }
}
