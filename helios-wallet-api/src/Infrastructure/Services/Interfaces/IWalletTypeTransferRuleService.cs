using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Wallet.Infrastructure.Services.Interfaces
{
    public interface IWalletTypeTransferRuleService
    {
        Task<ExportWalletTypeTransferRuleResponseDto> ExportWalletTypeTransferRules(ExportWalletTypeTransferRuleRequestDto request);

        Task<BaseResponseDto> ImportWalletTypeTransferRules(ImportWalletTypeTransferRuleRequestDto importWalletTypeTransferRuleRequest);
    }
}
