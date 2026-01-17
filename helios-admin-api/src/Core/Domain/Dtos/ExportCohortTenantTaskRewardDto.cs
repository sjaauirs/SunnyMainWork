using SunnyRewards.Helios.Cohort.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Admin.Core.Domain.Dtos
{
    public class ExportCohortTenantTaskRewardDto
    {
        public List<CohortTenantTaskRewardExportDto>? CohortTenantTaskReward { get; set; }
    }
    public class CohortTenantTaskRewardExportDto
    {
        public CohortTenantTaskRewardDto? CohortTenantTaskReward { get; set; }
        public string? TaskExternalCode { get; set; }
    }
}
