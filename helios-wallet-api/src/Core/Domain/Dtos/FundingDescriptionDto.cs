using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Wallet.Core.Domain.Dtos
{
    public class FundingDescriptionDto
    {
        public double? FundAmount { get; set; }
        public DateTime? FundDate { get; set; }
    }
}
