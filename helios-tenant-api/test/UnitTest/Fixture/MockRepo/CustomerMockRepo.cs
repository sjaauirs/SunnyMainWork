using Moq;
using SunnyRewards.Helios.Tenant.Core.Domain.Models;
using SunnyRewards.Helios.Tenant.Infrastructure.Repositories;
using SunnyRewards.Helios.Tenant.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Tenant.UnitTest.Fixture.MockModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Tenant.UnitTest.Fixture.MockRepo
{
    public class CustomerMockRepo : Mock<ICustomerRepo>
    {
        public CustomerMockRepo()
        {
            Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<CustomerModel, bool>>>(), false)).ReturnsAsync(new CustomerMockModel());
            Setup(x => x.FindAllAsync()).ReturnsAsync(new List<CustomerModel>());
        }


    }
}
