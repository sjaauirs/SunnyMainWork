using System.ComponentModel.DataAnnotations;

namespace Sunny.Benefits.Bff.Core.Domain.Dtos
{
    public class GetUserRequestDto
    {
        public string? UserId { get; set; }
        public string? Email { get; set; }
        public string? DeviceId { get; set; }
    }
}