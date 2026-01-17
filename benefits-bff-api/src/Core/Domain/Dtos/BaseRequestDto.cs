using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sunny.Benefits.Bff.Core.Domain.Dtos
{
    public class BaseRequestDto
    {
        public string? consumerCode { get; set; }
        public string? email { get; set; }
    }
}
