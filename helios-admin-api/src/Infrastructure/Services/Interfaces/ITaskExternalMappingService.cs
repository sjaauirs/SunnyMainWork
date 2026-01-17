using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces
{
    public interface ITaskExternalMappingService
    {
        Task<BaseResponseDto> CreateTaskExternalMapping(TaskExternalMappingRequestDto requestDto);

    }
}
