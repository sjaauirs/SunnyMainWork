using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sunny.Benefits.Bff.UnitTest.Fixtures.MockDtos
{
    public class FindConsumerWalletRequestMockDto : FindConsumerWalletRequestDto
    {
        public FindConsumerWalletRequestMockDto()
        {
            ConsumerCode = "cmr-f9c419da974c4bbb99eab99fd3b490e0";
        }
    }
}
