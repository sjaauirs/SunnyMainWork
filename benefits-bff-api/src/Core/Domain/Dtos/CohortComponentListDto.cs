using Sunny.Benefits.Cms.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sunny.Benefits.Bff.Core.Domain.Dtos
{
    public class CohortComponentListDto
    {
        public ComponentDto componentDto { get; set; }= new ComponentDto();
        public List<string> CohortName { get; set; }
    }
}
