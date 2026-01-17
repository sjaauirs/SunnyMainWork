using SunnyBenefits.Fis.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces
{
    public interface IConsumerPurseAssignmentService
    {
        Task<BaseResponseDto> ConsumerPurseAssignment(string tenantCode, string consumerCode,
            string purseWalletTypeCode, int purseNumber, string action, List<PlanCohortPurseMappingDto>? flexMapping = null);
    }
}
