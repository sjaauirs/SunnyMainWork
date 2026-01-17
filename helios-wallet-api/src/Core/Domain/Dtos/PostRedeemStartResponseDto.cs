using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Wallet.Core.Domain.Dtos
{
    public class PostRedeemStartResponseDto : BaseResponseDto
    {
        public TransactionDto? SubEntry { get; set; }
        public TransactionDto? AddEntry { get; set; }
        public TransactionDetailDto? TransactionDetail { get; set; }
        public RedemptionDto? Redemption { get; set; }

    }
}
