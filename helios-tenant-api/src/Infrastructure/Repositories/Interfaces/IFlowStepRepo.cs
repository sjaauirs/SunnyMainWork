using SunnyRewards.Helios.Common.Core.Repositories.Interfaces;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;
using SunnyRewards.Helios.Tenant.Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Tenant.Infrastructure.Repositories.Interfaces
{
    public interface IFlowStepRepo : IBaseRepo<FlowStepModel>
    {
        FlowResponseDto? GetFlowSteps(FlowRequestDto flowRequestDto);
    }
}
