using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces
{
    public interface ITenantTaskRewardScriptService
    {
        Task<BaseResponseDto> PostTenantTaskRewardScriptRequest(TenantTaskRewardScriptRequestDto postRequestDto);
        Task<BaseResponseDto> UpdateTenantTaskRewardScriptRequest(UpdateTenantTaskRewardScriptRequestDto putRequestDto);
    }
}
