using Microsoft.Extensions.Logging;
using NHibernate;
using NHibernate.Linq;
using SunnyRewards.Helios.Common.Core.Repositories;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Models;
using SunnyRewards.Helios.Task.Infrastructure.Repositories.Interfaces;
using System.Diagnostics.CodeAnalysis;

namespace SunnyRewards.Helios.Task.Infrastructure.Repositories
{
    [ExcludeFromCodeCoverage]
    public class AdventureRepo : BaseRepo<AdventureModel>, IAdventureRepo
    {
        private readonly ISession _session;
        public AdventureRepo(ILogger<BaseRepo<AdventureModel>> baseLogger, ISession session)
            : base(baseLogger, session)
        {
            _session = session;
        }
        /// <summary>
        /// Retrieves all adventure records associated with a specific tenant.
        /// </summary>
        /// <param name="tenantCode">The unique code identifying the tenant.</param>
        /// <returns>
        /// A task that represents the asynchronous operation, containing a list of <see cref="AdventureModel"/>.
        /// If no adventures are found, an empty list is returned.
        /// </returns>
        public async Task<IList<AdventureModel>> GetAllAdventures(string tenantCode)
        {
            return await _session.Query<AdventureModel>()
                .Join(_session.Query<TenantAdventureModel>(),
                      adv => adv.AdventureId,
                      tenantAdv => tenantAdv.AdventureId,
                      (adv, tenantAdv) => new { adv, tenantAdv })
                .Where(joined => joined.tenantAdv.TenantCode == tenantCode &&
                                 joined.tenantAdv.DeleteNbr == 0 &&
                                 joined.adv.DeleteNbr == 0)
                .Select(joined => joined.adv)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves all adventures associated with a specific tenant code.
        /// </summary>
        /// <param name="tenantCode"></param>
        /// <returns></returns>
        public async Task<ExportAdventureResponseDto> GetTenantAdventures(string tenantCode)
        {
            var query = await (from tenantAdventure in _session.Query<TenantAdventureModel>()
                               join adventure in _session.Query<AdventureModel>()
                               on tenantAdventure.AdventureId equals adventure.AdventureId
                               where tenantAdventure.TenantCode == tenantCode && tenantAdventure.DeleteNbr == 0 && adventure.DeleteNbr == 0
                               select new
                               {
                                   Adventures = adventure,
                                   GetTenantAdventures = tenantAdventure
                               }).ToListAsync();

            var exportAdventureResponseDto = new ExportAdventureResponseDto();
            if (query != null)
            {
                exportAdventureResponseDto.Adventures = query.Select(x => new AdventureDto
                {
                    AdventureId = x.Adventures.AdventureId,
                    AdventureCode = x.Adventures.AdventureCode,
                    AdventureConfigJson = x.Adventures.AdventureConfigJson,
                    CmsComponentCode = x.Adventures.CmsComponentCode
                }).ToList();
                exportAdventureResponseDto.TenantAdventures = query.Select(x => new TenantAdventureDto
                {
                    TenantAdventureId = x.GetTenantAdventures.TenantAdventureId,
                    TenantAdventureCode = x.GetTenantAdventures.TenantAdventureCode,
                    TenantCode = x.GetTenantAdventures.TenantCode,
                    AdventureId = x.GetTenantAdventures.AdventureId
                }).ToList();
            }
            return exportAdventureResponseDto;
        }
    }
}
