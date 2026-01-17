using SunnyRewards.Helios.Tenant.Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Tenant.UnitTest.Fixture.MockModel
{
    public class CustomerMockModel : CustomerModel
    {
        public CustomerMockModel()
        {
            CustomerId = 1;
            CustomerCode = "cus-8d9e6f00eec8436a8251d55ff74b1642";
            CustomerName = "PRIZEOUT";
            CustomerDescription = "";
            CreateTs = DateTime.Now;
            UpdateTs = DateTime.Now;
            CreateUser = "sunny";
            UpdateUser = "sunny rewards";
            DeleteNbr = 0;
        }
    }
}
