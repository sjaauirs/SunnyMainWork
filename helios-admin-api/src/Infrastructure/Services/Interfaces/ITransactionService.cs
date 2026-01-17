using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces
{
    public interface ITransactionService
    {
        Task<RewardsRecentActivityResponseDto> GetWalletTransactions(GetWalletTransactionRequestDto walletTransactionRequestDto);
    }
}
