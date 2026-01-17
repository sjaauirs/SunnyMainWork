using SunnyRewards.Helios.User.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Wallet.UnitTest.Fixtures.MockDto
{
    public class GetConsumerResponseMockDto : GetConsumerResponseDto
    {
        public GetConsumerResponseMockDto()
        {
            Consumer = new ConsumerDto()
            {
                ConsumerId = 1,
                PersonId = 1,
                TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4",
                ConsumerCode = "cus - 04c211b4339348509eaa870cdea59600",
                Registered = true,
                Eligible = true,
                RegistrationTs = DateTime.Now,
                EligibleStartTs = DateTime.Now,
                EligibleEndTs = DateTime.Now,
                MemberNbr = "8a0eb919-848e-49a1-b497-c61969afc070",
                ConsumerAttribute = "false",
                CreateTs = DateTime.Now,
                UpdateTs = DateTime.Now,
                CreateUser = "sunny",
                UpdateUser = "sunny rewards",
                DeleteNbr = 1,
            };
        }
    }
}
