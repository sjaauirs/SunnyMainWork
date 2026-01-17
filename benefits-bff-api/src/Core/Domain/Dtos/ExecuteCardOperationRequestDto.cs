using System.ComponentModel.DataAnnotations;

namespace Sunny.Benefits.Bff.Core.Domain.Dtos
{
    public class ExecuteCardOperationRequestDto
    {
        [Required]
        public string? TenantCode { get; set; }

        [Required]
        public string? ConsumerCode { get; set; }

        [Required]
        public string? CardOperation { get; set; }
     
    }
}
