using Sunny.Benefits.Bff.Core.Domain.Dtos;
using SunnyBenefits.Fis.Core.Domain.Dtos;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;

namespace Sunny.Benefits.Bff.Infrastructure.Services.Interfaces
{
    public interface IWalletService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="findConsumerWalletRequestDto"></param>
        /// <returns></returns>
        Task<WalletResponseDto> GetWallets(FindConsumerWalletRequestDto findConsumerWalletRequestDto, TenantDto? tenant = null);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="postGetTransactionsRequestDto"></param>
        /// <returns></returns>
        Task<TransactionBySectionResponseDto> GetTransactions(PostGetTransactionsRequestDto postGetTransactionsRequestDto);

        /// <summary>
        /// Gets the consumer benefits wallet types asynchronous.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        Task<ConsumerBenefitsWalletTypesResponseDto> GetConsumerBenefitsWalletTypesAsync(ConsumerBenefitsWalletTypesRequestDto request);
    }
}
