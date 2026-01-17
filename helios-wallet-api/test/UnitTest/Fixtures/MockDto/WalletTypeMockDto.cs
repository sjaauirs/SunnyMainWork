using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Wallet.UnitTest.Fixtures.MockDto
{
    public class WalletTypeMockDto : WalletTypeDto
    {
        public WalletTypeMockDto()
        {
            WalletTypeId = 1;
            WalletTypeCode = "test";
            WalletTypeName = "Health Actions Reward"; 
            WalletTypeLabel = "test";
        }
    }
}
