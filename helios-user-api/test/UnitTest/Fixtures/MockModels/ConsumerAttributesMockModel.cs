using Microsoft.AspNetCore.Mvc;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.User.UnitTest.Fixtures.MockDto
{
    public class ConsumerAttributesResponseMockDto : ConsumerAttributesResponseDto
    {
        public ConsumerAttributesResponseMockDto()
        {
            Consumers = new List<ConsumerDto>
        {
            new ConsumerDto
            {
                ConsumerId = 3,
                PersonId = 120,
                TenantCode = "ten-ecada21e68764928a2bb959e8365b8b4",
                ConsumerCode = "cmr-bjuebdf-492f-0987-bh55-5a2b0134cbc",
                Registered = true,
                Eligible = true,
                MemberNbr = "78b1bhf61-09y6-4029-8e81-3jjy47654a8",
                ConsumerAttribute = "ConsumerAttribute",
                RegistrationTs = DateTime.Now,
                EligibleStartTs = DateTime.Now,
                EligibleEndTs = DateTime.Now,
                CreateTs = DateTime.Now,
                UpdateTs = DateTime.Now,
                CreateUser = "sunny",
                UpdateUser = "sunny rewards",
                DeleteNbr = 1,
            },
            
        };
        }
    }
}

