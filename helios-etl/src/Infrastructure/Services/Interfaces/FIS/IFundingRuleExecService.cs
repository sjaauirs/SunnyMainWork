using SunnyRewards.Helios.ETL.Core.Domain.Dtos.FIS;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces.FIS
{
    public interface IFundingRuleExecService
    {
        Task ExecuteFundingRuleAsync(FISFundingRuleDto fundingRule, FISFundTransferRequestDto fundTransferRequest);
        Task ExecuteFundTransferAsync(FISFundTransferRequestDto fISFundTransferRequest);

    }
}
