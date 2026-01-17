using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Infrastructure.Helpers.Interface
{
    public interface IWalletHelper
    {
        Task<BaseResponseDto> CreateWalletsForConsumer(string consumerCode);
    }
}
