using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Wallet.UnitTest.Fixtures.MockDto
{
    public class GetMasterWalletRequestMockDto: GetMasterWalletRequestDto
    {
        public GetMasterWalletRequestMockDto()
        {
            WalletTypeName = "Health Actions Reward";
            TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4";
            SponsorCode = "spo-c008f49aa31f4acd9aa6e2114bfb820e";
            CustomerCode = "cus-04c211b4339348509eaa870cdea59600";
        }
    }
}
