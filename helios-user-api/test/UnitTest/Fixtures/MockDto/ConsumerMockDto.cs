using SunnyRewards.Helios.User.Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.User.UnitTest.Fixtures.MockDto
{
    public class ConsumerMockDto : ConsumerModel
    {
        public ConsumerMockDto()
        {
            ConsumerId = 1;
            PersonId = 1;
            TenantCode = "ten-91532506c8d468e1d27704";
            ConsumerCode = "cmr--91532578681e1d27704";
            RegistrationTs = DateTime.UtcNow;
            EligibleStartTs = DateTime.UtcNow;
            EligibleEndTs = DateTime.UtcNow.AddDays(30);
            Registered = false;
            Eligible = true;
            MemberNbr = "69-676-815ec8aefa64";
            SubscriberMemberNbr = "69-676-815ec8aefa64";
            ConsumerAttribute = "{\n  \"testing\": {\n \"test\": \"string\"\n  }\n}";
        }

        public static List<ConsumerModel> consumerModel()
        {
            return new List<ConsumerModel>()
            {
                new ConsumerModel()
            };
        }
    }
}
