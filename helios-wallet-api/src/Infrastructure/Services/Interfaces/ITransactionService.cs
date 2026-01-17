using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Wallet.Infrastructure.Services.Interfaces
{
    public interface ITransactionService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="recentTransactionRequestDto"></param>
        /// <returns></returns>
        Task<GetRecentTransactionResponseDto> GetTransactionDetails(GetRecentTransactionRequestDto recentTransactionRequestDto, GetConsumerResponseDto? consumerDto = null);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="postGetTransactionsRequestDto"></param>
        /// <returns></returns>
        Task<PostGetTransactionsResponseDto> GetTransaction(PostGetTransactionsRequestDto postGetTransactionsRequestDto);

        /// <summary>
        /// Revert all transactions for given consumer
        /// </summary>
        /// <param name="revertTransactionsRequestDto"></param>
        /// <returns></returns>
        Task<BaseResponseDto> RevertAllTransaction(RevertTransactionsRequestDto revertTransactionsRequestDto);
        Task<GetWalletTransactionResponseDto> GetRewardWalletTransactions(GetWalletTransactionRequestDto walletTransactionRequestDto);
        Task<CreateTransactionsResponseDto> CreateWalletTransactions(CreateTransactionsRequestDto createTransactionsRequestDto);
        Task<BaseResponseDto> RemoveWalletTransactions(RemoveTransactionsRequestDto removeTransactionsRequestDto);
    }
}
