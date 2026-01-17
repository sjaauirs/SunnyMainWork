using SunnyRewards.Helios.ETL.Common.Repositories.Interfaces;
using SunnyRewards.Helios.ETL.Core.Domain.Models;

namespace SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces
{
    public interface IRedemptionRepo : IBaseRepo<ETLRedemptionModel>
    {
        ETLRedemptionModel? GetRedemptionWithRedemptionRef(string redemptionRef);
        int UpdateRedemptionStatus(DateTime transactionTs, string redemptionStatus, string redemptionRef);
    }
}
