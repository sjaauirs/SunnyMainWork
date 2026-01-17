using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Wallet.Core.Domain.Dtos
{
    public class ExportWalletTypeTransferRuleResponseDto : BaseResponseDto
    {
        public List<ExportWalletTypeTransferRuleDto> WalletTypeTransferRules { get; set; } = new List<ExportWalletTypeTransferRuleDto>();
    }
}
