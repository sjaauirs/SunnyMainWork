using SunnyRewards.Helios.Common.Core.Repositories.Interfaces;
using SunnyRewards.Helios.User.Core.Domain.Models;

namespace SunnyRewards.Helios.User.Infrastructure.Repositories.Interfaces
{
    public interface IConsumerLoginRepo : IBaseRepo<ConsumerLoginModel>
    {
        Task<DateTime?> GetFirstLoginDateAsync(long consumerId);
    }
}