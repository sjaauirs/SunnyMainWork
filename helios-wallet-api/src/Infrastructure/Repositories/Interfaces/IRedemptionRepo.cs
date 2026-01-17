using SunnyRewards.Helios.Common.Core.Repositories.Interfaces;
using SunnyRewards.Helios.Wallet.Core.Domain.Models;

namespace SunnyRewards.Helios.Wallet.Infrastructure.Repositories.Interfaces
{
    public interface IRedemptionRepo : IBaseRepo<RedemptionModel>
    {
        int UpdateRedemption(DateTime timeStamp, long redemptionId, int xmin);
    }
}
