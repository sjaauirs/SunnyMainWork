using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class DataQueryRequestDto
    {
        public required List<SearchAttributeDto> SearchAttributes { get; set; } = new List<SearchAttributeDto>();
        public required string ConsumerCode { get; set; }
        public required string TenantCode { get; set; }
        public required string LanguageCode { get; set; }
    }

    public class SearchAttributeDto
    {
        public required string Column { get; set; }
        public required string Operator { get; set; }          // e.g., '=', '>=', 'IN', 'BETWEEN', '<'
        public required string DataType { get; set; }          // e.g., int, string, datetime, decimal
        public required object Value { get; set; }             // Single value or array (e.g., [10, 20])
        public string? Criteria { get; set; }          // Optional: 'AND' or 'OR'
    }
}
