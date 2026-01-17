using NHibernate;
using System.ComponentModel.DataAnnotations;

namespace Sunny.Benefits.Bff.Core.Domain.Dtos
{
    public class VerifyMemberDto
    {
        public string? Email { get; set; }
        public DateTime? DOB { get; set; }
        public string? CardLast4 { get; set; }
        public string? LanguageCode { get; set; }
        public string? ComponentCode { get; set; }
        [Required]
        public required string verifyOps { get; set; }
        public string? ConsumerCode { get; set; }
        public string? CardActivationChannel { get; set; }
    }

}