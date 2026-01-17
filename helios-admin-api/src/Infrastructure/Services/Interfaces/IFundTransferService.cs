using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces
{
    public interface IFundTransferService
    {
        Task<BaseResponseDto> TransferFundsAsync(FundTransferToPurseRequestDto request);
    }
}
