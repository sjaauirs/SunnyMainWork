using SunnyRewards.Helios.ETL.Common.Repositories.Interfaces;
using SunnyRewards.Helios.ETL.Core.Domain.Models;

namespace SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces
{
    public interface ISweepstakesInstanceRepo : IBaseRepo<ETLSweepstakesInstanceModel>
    {
       Task<ETLSweepstakesInstanceModel?> GetLatestSweepstakesInstance(string tenantCode, long sweepstakesInstanceId, long sweepstakesId);
    }
}

