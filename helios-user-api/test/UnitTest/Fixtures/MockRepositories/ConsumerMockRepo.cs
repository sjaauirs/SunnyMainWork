using Microsoft.AspNetCore.Mvc;
using Moq;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Models;
using SunnyRewards.Helios.User.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.User.UnitTest.Fixtures.MockDto;
using SunnyRewards.Helios.User.UnitTest.Fixtures.MockModels;
using System.Linq.Expressions;

namespace SunnyRewards.Helios.User.UnitTest.Fixtures.MockRepositories
{
    public class ConsumerMockRepo : Mock<IConsumerRepo>
    {
        public ConsumerMockRepo()
        {
            Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false)).ReturnsAsync(new ConsumerMockModel());
           /// Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<, bool>>>(), false)).ReturnsAsync(new ConsumerAttributesMockModel());
        }
    }
}
