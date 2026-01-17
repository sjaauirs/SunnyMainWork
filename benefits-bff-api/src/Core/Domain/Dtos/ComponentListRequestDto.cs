using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sunny.Benefits.Bff.Core.Domain.Dtos
{
    public class ComponentListRequestDto
    {
        public string? TenantCode { get; set; }
        public string? ComponentName { get; set; }
        public string? LanguageCode { get; set; }
    }
}
