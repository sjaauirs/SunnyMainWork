

using SunnyRewards.Helios.ETL.Core.Domain.Models;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces
{
    public interface IRedemptionService
    {
        ETLRedemptionModel? GetRedemptionWithRedemptionRef(string redemptionRef);
        int UpdateRedemptionStatus(string redemptionRef, string redemptionStatus);
    }
}
