using Microsoft.Extensions.Logging;
using Moq;
using NHibernate;
using SunnyRewards.Helios.Common.Core.Repositories;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Models;
using SunnyRewards.Helios.Task.Infrastructure.Repositories;

using Xunit;



namespace SunnyRewards.Helios.Task.UnitTest.Fixtures.MockDtos
{
    public class TenantAdventureMockDto : TenantAdventureDto
    {
        public TenantAdventureMockDto()
        {
            TenantAdventureId = 101;
            TenantAdventureCode = "TA-XYZ123";
            TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4";
            AdventureId = 5;
        }
    }

    public class TenantAdventureMockModel : TenantAdventureModel
    {
        public TenantAdventureMockModel()
        {
            TenantAdventureId = 101;
            TenantAdventureCode = "TA-XYZ123";
            TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4";
            AdventureId = 5;
        }
    }


    public class TenantAdventureTests
    {
        private readonly TenantAdventureMockDto _mockDto;
        private readonly TenantAdventureMockModel _mockModel;

        public TenantAdventureTests()
        {
            _mockDto = new TenantAdventureMockDto();
            _mockModel = new TenantAdventureMockModel();
        }

        [Fact]
        public void TenantAdventureMockDto_Should_Have_Default_Values()
        {
            Assert.Equal(101, _mockDto.TenantAdventureId);
            Assert.Equal("TA-XYZ123", _mockDto.TenantAdventureCode);
            Assert.Equal("ten-ecada21e57154928a2bb959e8365b8b4", _mockDto.TenantCode);
            Assert.Equal(5, _mockDto.AdventureId);
        }

        [Fact]
        public void TenantAdventureMockModel_Should_Have_Default_Values()
        {
            Assert.Equal(101, _mockModel.TenantAdventureId);
            Assert.Equal("TA-XYZ123", _mockModel.TenantAdventureCode);
            Assert.Equal("ten-ecada21e57154928a2bb959e8365b8b4", _mockModel.TenantCode);
            Assert.Equal(5, _mockModel.AdventureId);
        }

        [Fact]
        public void TenantAdventureRepo_Should_Instantiate_Correctly()
        {
            var mockLogger = new Mock<ILogger<BaseRepo<TenantAdventureModel>>>();
            var mockSession = new Mock<ISession>();
            var repo = new TenantAdventureRepo(mockLogger.Object, mockSession.Object);

            Assert.NotNull(repo);
        }
    }
}
