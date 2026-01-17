using System.ComponentModel.DataAnnotations;

namespace Sunny.Benefits.Bff.Core.Domain.Dtos
{
    public class ValidicCreateUserRequestPayloadDto
    {
        [Required]
        public string uid { get; set; }
    }
}
