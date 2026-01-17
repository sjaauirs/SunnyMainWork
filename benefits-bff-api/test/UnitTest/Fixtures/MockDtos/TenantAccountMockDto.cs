using Newtonsoft.Json;
using SunnyBenefits.Fis.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sunny.Benefits.Bff.UnitTest.Fixtures.MockDtos
{
    public class TenantAccountMockDto : TenantAccountDto
    {
        public TenantAccountMockDto()
        {
            TenantAccountCode = "MockTenantAccountCode";
            TenantCode = "MockTenantCode";
            AccLoadConfig = "MockAccLoadConfig";
            TenantConfigJson = JsonConvert.SerializeObject(new TenantConfigDto
            {
                PurseConfig = new PurseConfigDto
                {
                    Purses = new List<PurseDto>
                    { new PurseDto{
                        WalletType = "test",
                        PurseNumber=2,
                        PurseWalletType = "test1" }
                    }
                }
            });
            FundingConfigJson = "{\"key\":\"value\"}"; // Example JSON string
            LastMonetaryTransactionId = 123456789; // Example long value
        }
    }
}
