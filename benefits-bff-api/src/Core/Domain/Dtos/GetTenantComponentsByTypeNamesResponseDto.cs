using System.Collections.Generic;
using Sunny.Benefits.Cms.Core.Domain.Dtos;

namespace Sunny.Benefits.Bff.Core.Domain.Dtos
{
    public class GetTenantComponentsByTypeNamesResponseDto
    {
        public Dictionary<string, List<ComponentDto>> ComponentsByType { get; set; } = new Dictionary<string, List<ComponentDto>>();
        
        public int? ErrorCode { get; set; }
        
        public string? ErrorMessage { get; set; }
    }
}

