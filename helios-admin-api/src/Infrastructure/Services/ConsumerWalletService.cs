using SunnyRewards.Helios.Admin.Core.Domain.Constants;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services
{
    public class ConsumerWalletService : IConsumerWalletService
    {
        private readonly IWalletClient _walletClient;

        public ConsumerWalletService(IWalletClient walletClient)
        {
            _walletClient = walletClient;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="consumerWalletByWalletTypeRequestDto"></param>
        /// <returns></returns>
        public async Task<FindConsumerWalletResponseDto> GetConsumerWalletsByWalletType(FindConsumerWalletByWalletTypeRequestDto consumerWalletByWalletTypeRequestDto)
        {
            return await _walletClient.Post<FindConsumerWalletResponseDto>(Constant.GetConsumerWallet, consumerWalletByWalletTypeRequestDto);
        }
        /// <summary>
        /// Handles the posting of consumer wallets.
        /// </summary>
        /// <param name="consumerWalletDataDto">The consumer wallet data transfer object containing wallet details.</param>
        public async Task<List<ConsumerWalletDataResponseDto>> PostConsumerWallets(IList<ConsumerWalletDataDto> consumerWalletDataDto)
        {
            return await _walletClient.Post<List<ConsumerWalletDataResponseDto>>(Constant.PostConsumerWallet, consumerWalletDataDto);
        }
        /// <summary>
        /// Retrieves all wallets associated with a given consumer.
        /// </summary>
        /// <param name="consumerWalletRequestDto">Contains TenantCode and ConsumerCode.</param>
        /// <returns>A list of wallets for the specified consumer.</returns>
        public async Task<ConsumerWalletResponseDto> GetAllConsumerWalletsAsync(GetConsumerWalletRequestDto consumerWalletRequestDto)
        {
            return await _walletClient.Post<ConsumerWalletResponseDto>(Constant.GetAllConsumerWallets, consumerWalletRequestDto);
        }

        public async Task<ConsumerWalletResponseDto> GetAllConsumerRedeemableWalletsAsync(FindConsumerWalletRequestDto consumerWalletRequestDto)
        {
            return await _walletClient.Post<ConsumerWalletResponseDto>(Constant.GetAllConsumerRedeemableWallets, consumerWalletRequestDto);
        }
    }
}
