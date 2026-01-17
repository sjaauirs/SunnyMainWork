using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Wallet.Infrastructure.Services.Interfaces
{
    public interface ICsaWalletTransactionsService
    {
        Task<BaseResponseDto> HandleCsaWalletTransactions(CsaWalletTransactionsRequestDto csaWalletRequestDto);
    }
}
