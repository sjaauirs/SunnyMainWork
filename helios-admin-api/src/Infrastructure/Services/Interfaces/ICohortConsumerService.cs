using SunnyRewards.Helios.Cohort.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces
{
    public interface ICohortConsumerService
    {
        BaseResponseDto AddConsumerCohort(dynamic request);
        BaseResponseDto RemoveConsumerCohort(dynamic request);
    }
}
