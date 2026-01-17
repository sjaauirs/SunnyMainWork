using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces
{
    public interface IConsumerWalletService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="consumerWalletByWalletTypeRequestDto"></param>
        /// <returns></returns>
        Task<FindConsumerWalletResponseDto> GetConsumerWalletsByWalletType(FindConsumerWalletByWalletTypeRequestDto consumerWalletByWalletTypeRequestDto);

        /// <summary>
        /// Gets all consumer wallets asynchronous.
        /// </summary>
        /// <param name="consumerWalletRequestDto">The consumer wallet request dto.</param>
        /// <returns></returns>
        Task<ConsumerWalletResponseDto> GetAllConsumerWalletsAsync(GetConsumerWalletRequestDto consumerWalletRequestDto);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="consumerWalletDataDto"></param>
        /// <returns></returns>
        Task<List<ConsumerWalletDataResponseDto>> PostConsumerWallets(IList<ConsumerWalletDataDto> consumerWalletDataDto);
        Task<ConsumerWalletResponseDto> GetAllConsumerRedeemableWalletsAsync(FindConsumerWalletRequestDto consumerWalletRequestDto);
    }
}
