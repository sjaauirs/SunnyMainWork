using Sunny.Benefits.Bff.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace Sunny.Benefits.Bff.Infrastructure.Services.Interfaces
{
    public interface IFundTransferService
    {
        Task<BaseResponseDto> TransferFundsAsync(FundTransferRequestDto request);
    }
}
