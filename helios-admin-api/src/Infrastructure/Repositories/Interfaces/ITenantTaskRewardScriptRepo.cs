using SunnyRewards.Helios.Common.Core.Repositories.Interfaces;
using SunnyRewards.Helios.Admin.Core.Domain.Models;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Infrastructure.Repositories.Interfaces
{
    public interface ITenantTaskRewardScriptRepo : IBaseRepo<TenantTaskRewardScriptModel>
    {
        List<ExportTenantTaskRewardScriptDto> GetTenantTaskRewardScripts(string tenantCode);
    }
}

