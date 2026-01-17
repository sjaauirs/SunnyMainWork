using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class GetTaskByTenantCodeRequestDto
    {
        public string? TenantCode { get; set; }
        public  string? ConsumerCode { get; set; }
        public string? LanguageCode { get; set; }
    }
}
