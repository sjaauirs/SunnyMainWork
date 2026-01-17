using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sunny.Benefits.Bff.Core.Domain.Dtos
{
    public class FaqRetriveRequestDto
    {
        [Required]
        public string? TenantCode { get; set; }
        public string? LanguageCode { get; set; }
        public string? ConsumerCode { get; set; }
        [DefaultValue(false)]
        public bool? ApplyCohortFilter { get; set; } = false;
        [DefaultValue(false)]
        public bool? richContentEnabled { get; set; } = false;

    }
}
