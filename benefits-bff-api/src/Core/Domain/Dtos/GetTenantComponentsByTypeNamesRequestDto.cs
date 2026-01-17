using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Sunny.Benefits.Bff.Core.Domain.Dtos
{
    public class GetTenantComponentsByTypeNamesRequestDto
    {
        [Required]
        public string TenantCode { get; set; } = null!;

        [Required]
        [MinLength(1, ErrorMessage = "At least one ComponentTypeName is required")]
        public List<string> ComponentTypeNames { get; set; } = new List<string>();

        public string? LanguageCode { get; set; }

        public string? ConsumerCode { get; set; }
        
        [DefaultValue(false)]
        public bool? ApplyCohortFilter { get; set; } = false;
    }
}

