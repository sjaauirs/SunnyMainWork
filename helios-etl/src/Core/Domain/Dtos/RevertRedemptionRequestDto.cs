using SunnyRewards.Helios.ETL.Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos
{
    public class RevertRedemptionRequestDto
    {
        public long MasterWalletId { get; set; }
        public long ConsumerWalletId { get; set; }
        public string? TenantCode { get; set; }
        public string? ConsumerCode { get; set; }
        public double TransactionAmount { get; set; }
        public string? RedemptionRef { get; set; }
        public string? Notes { get; set; }
        public string? RedemptionItemDescription { get; set; }
        public string? TransactionDetailType { get; set; }
        public string? NewTransactionCode { get; set; }
        public ETLRedemptionModel? Redemption { get; set; }
    }
}
