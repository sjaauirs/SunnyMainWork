using System;
using global::SunnyRewards.Helios.Common.Core.Repositories;
using global::SunnyRewards.Helios.Task.Core.Domain.Dtos;
using global::SunnyRewards.Helios.Task.Core.Domain.Models;
using SunnyRewards.Helios.Task.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using NHibernate;

using Xunit;


namespace SunnyRewards.Helios.Task.UnitTest.Fixtures.MockDtos
{
    public class AdventureMockDto : AdventureDto
    {
        public AdventureMockDto()
        {
            AdventureId = 201;
            AdventureCode = "ADV-12345";
            AdventureConfigJson = "{\"key\": \"value\"}";
            CmsComponentCode = "CMS-002";
            LanguageCode = "en-US";
        }
    }

    public class AdventureMockModel : AdventureModel
    {
        public AdventureMockModel()
        {
            AdventureId = 201;
            AdventureCode = "ADV-12345";
            AdventureConfigJson = "{\"cohorts\":[\"adventure:fitness_and_exercise\"]}";
            CmsComponentCode = "CMS-002";
        }
    }
    
    public class AdventureTests
    {
        private readonly AdventureMockDto _mockDto;
        private readonly AdventureMockModel _mockModel;

        public AdventureTests()
        {
            _mockDto = new AdventureMockDto();
            _mockModel = new AdventureMockModel();
        }

        [Fact]
        public void AdventureMockDto_Should_Have_Default_Values()
        {
            Assert.Equal(201, _mockDto.AdventureId);
            Assert.Equal("ADV-12345", _mockDto.AdventureCode);
        }

        [Fact]
        public void AdventureMockModel_Should_Have_Default_Values()
        {
            Assert.Equal(201, _mockModel.AdventureId);
            Assert.Equal("ADV-12345", _mockModel.AdventureCode);
        }

        [Fact]
        public void AdventureRepo_Should_Instantiate_Correctly()
        {
            var mockLogger = new Mock<ILogger<BaseRepo<AdventureModel>>>();
            var mockSession = new Mock<ISession>();
            var repo = new AdventureRepo(mockLogger.Object, mockSession.Object);

            Assert.NotNull(repo);
        }
    }
}


