using SunnyRewards.Helios.Wallet.Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Wallet.Core.Domain.Dtos
{
    public class ExportWalletTypeTransferRuleDto : WalletTypeTransferRuleModel
    {
        public string SourceWalletTypeCode { get; set; } = string.Empty;
        public string TargetWalletTypeCode { get; set; } = string.Empty;
    }
}
