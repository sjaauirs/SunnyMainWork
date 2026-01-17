using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sunny.Benefits.Bff.Core.Domain.Dtos
{
    public class FaqSectionDto
    {
        public string? HeaderText { get; set; }
        public List<FaqItemDto>? FaqItems { get; set; }
    }
    public class FaqItemDto
    {
        public string? HeaderText { get; set; }
        public string? DescriptionText { get; set; }
    }
}
