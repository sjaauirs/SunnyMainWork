using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Moq;
using SunnyRewards.Helios.User.Core.Domain.Models;
using SunnyRewards.Helios.User.Infrastructure.Repositories.Interfaces;

namespace SunnyRewards.Helios.User.UnitTest.Fixtures.MockRepositories
{
    public class PhoneNumberMockRepo : Mock<IPhoneNumberRepo>
    {
        public PhoneNumberMockRepo()
        {
            // Mock for FindOneAsync
            Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<PhoneNumberModel, bool>>>(), false))
                .ReturnsAsync(new PhoneNumberModel
                {
                    PhoneNumberId = 1001,
                    PersonId = 2001,
                    PhoneTypeId = 2,
                    PhoneNumberCode = "pnc-abcdef1234567890",
                    PhoneNumber = "1234567890",
                    IsPrimary = true,
                    IsVerified = false,
                    VerifiedDate = null,
                    Source = "ETL",
                    CreateUser = "unit_test",
                    UpdateUser = "unit_test",
                    CreateTs = DateTime.UtcNow.AddDays(-2),
                    UpdateTs = DateTime.UtcNow,
                    DeleteNbr = 0
                });

            // Mock for FindAsync
            Setup(x => x.FindAsync(It.IsAny<Expression<Func<PhoneNumberModel, bool>>>(), false))
                .ReturnsAsync(new List<PhoneNumberModel>
                {
                    new PhoneNumberModel
                    {
                        PhoneNumberId = 1001,
                        PersonId = 2001,
                        PhoneTypeId = 2,
                        PhoneNumberCode = "pnc-abcdef1234567890",
                        PhoneNumber = "1234567890",
                        IsPrimary = true,
                        IsVerified = false,
                        VerifiedDate = null,
                        Source = "ETL",
                        CreateUser = "unit_test",
                        UpdateUser = "unit_test",
                        CreateTs = DateTime.UtcNow.AddDays(-2),
                        UpdateTs = DateTime.UtcNow,
                        DeleteNbr = 0
                    }
                });
        }
    }
}
