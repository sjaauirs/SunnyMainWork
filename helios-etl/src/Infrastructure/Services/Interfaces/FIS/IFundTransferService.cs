using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos.FIS;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces.FIS
{
    public interface IFundTransferService
    {
        Task ExecuteFundTransferAsync(FISFundTransferRequestDto fundTransferRequest, bool? isFundingRuleExecution = true);
        Task<bool> ExecuteRedemptionTransactionAsync(RedemptionRequestDto redemptionRequest);
        Task<(bool,string)> ExecuteRevertRedemptionTransactionAsync(RevertRedemptionRequestDto revertRedemptionRequest);
    }
}
