using SunnyRewards.Helios.Common.Core.Repositories.Interfaces;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Models;

namespace SunnyRewards.Helios.Task.Infrastructure.Repositories.Interfaces
{
    public interface IAdventureRepo : IBaseRepo<AdventureModel>
    {
        Task<IList<AdventureModel>> GetAllAdventures(string tenantCode);

        Task<ExportAdventureResponseDto>GetTenantAdventures(string tenantCode);
    }
}
