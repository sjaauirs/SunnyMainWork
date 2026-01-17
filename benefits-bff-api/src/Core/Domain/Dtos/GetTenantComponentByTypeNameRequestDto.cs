using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sunny.Benefits.Bff.Core.Domain.Dtos
{
    public class GetTenantComponentByTypeNameRequestDto
    {
        [Required]
        public string TenantCode { get; set; } = null!;

        [Required]
        public string ComponentTypeName { get; set; } = null!;

        public string? LanguageCode { get; set; }

        public string? ConsumerCode { get; set; }
        [DefaultValue(false)]
        public bool? ApplyCohortFilter { get; set; } = false;
    }
}
