using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Wallet.UnitTest.Fixtures.MockDto
{
    public class PostGetTransactionsRequestMockDto : PostGetTransactionsRequestDto
    {
        public PostGetTransactionsRequestMockDto()
        {
            ConsumerCode = "cmr-6a516aa13ad44c139511c607e2087cf4";
            WalletId = 3;
        }
    }
}
