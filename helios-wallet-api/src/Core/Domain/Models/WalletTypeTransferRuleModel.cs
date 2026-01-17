using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Domain.Models;
using System.Diagnostics.CodeAnalysis;

namespace SunnyRewards.Helios.Wallet.Core.Domain.Models
{

    public class WalletTypeTransferRuleModel : BaseModel
    {
        public virtual long WalletTypeTransferRuleId { get; set; }
        public virtual string? WalletTypeTransferRuleCode { get; set; }
        public virtual string? TenantCode { get; set; }
        public virtual long SourceWalletTypeId { get; set; }
        public virtual long TargetWalletTypeId { get; set; }
        public virtual string? TransferRule { get; set; }
    }

    public class WalletTransferRequest
    {
        public required string TenantCode { get; set; }
        public required  long SourceWalletTypeId { get; set; }

        public required string ConsumerCode { get; set; }
    }

    public class WalletTypeRuleResult : BaseResponseDto
    {
        public  WalletModel? ConsumerWallet { get; set; }
        public  WalletModel? MasterWallet { get; set; }
        public  double TransferRule { get; set; }

        public string? ConsumerWalletTypeCode { get; set; }
        public string? WalletTypeCurrency { get; set; }
    }

    public class TransferRule
    {
        public double TransferRatio { get; set; }
    }
}
