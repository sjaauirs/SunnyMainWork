using System.Linq.Expressions;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Models;
using SunnyRewards.Helios.User.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.User.Infrastructure.Services;
using SunnyRewards.Helios.User.Infrastructure.Services.Interfaces;
using Xunit;

namespace SunnyRewards.Helios.User.UnitTest.ReadReplica
{
    public class PersonServiceReadReplicaTests
    {
        private readonly Mock<IPersonRepo> _personRepo;
        private readonly Mock<IConsumerRepo> _consumerRepo;
        private readonly Mock<IConsumerService> _consumerService;
        private readonly Mock<ILogger<PersonService>> _logger;
        private readonly Mock<IMapper> _mapper;

        public PersonServiceReadReplicaTests()
        {
            _personRepo = new Mock<IPersonRepo>();
            _consumerRepo = new Mock<IConsumerRepo>();
            _consumerService = new Mock<IConsumerService>();
            _logger = new Mock<ILogger<PersonService>>();
            _mapper = new Mock<IMapper>();
        }

        [Fact]
        public async Task GetPersonData_WithoutReadReplica_UsesRepository()
        {
            // Arrange
            var mockModel = new PersonModel { PersonId = 1, FirstName = "John", LastName = "Doe", DeleteNbr = 0 };
            var mockDto = new PersonDto { PersonId = 1, FirstName = "John", LastName = "Doe" };

            _personRepo
                .Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<PersonModel, bool>>>(), false))
                .ReturnsAsync(mockModel);
            _mapper.Setup(m => m.Map<PersonDto>(mockModel)).Returns(mockDto);

            var service = new PersonService(_logger.Object, _mapper.Object, _personRepo.Object, _consumerRepo.Object, _consumerService.Object, null);

            // Act
            var result = await service.GetPersonData(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.PersonId);
            _personRepo.Verify(r => r.FindOneAsync(It.IsAny<Expression<Func<PersonModel, bool>>>(), false), Times.Once);
        }

        [Fact]
        public async Task GetPersonData_WithoutReadReplica_ReturnsEmptyDto_WhenNotFound()
        {
            // Arrange
            _personRepo
                .Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<PersonModel, bool>>>(), false))
                .ReturnsAsync((PersonModel?)null);

            var service = new PersonService(_logger.Object, _mapper.Object, _personRepo.Object, _consumerRepo.Object, _consumerService.Object, null);

            // Act
            var result = await service.GetPersonData(999);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.PersonId);
        }

        [Fact]
        public async Task GetPersonData_WithoutReadReplica_ReturnsEmptyDto_OnException()
        {
            // Arrange
            _personRepo
                .Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<PersonModel, bool>>>(), false))
                .ThrowsAsync(new Exception("Database error"));

            var service = new PersonService(_logger.Object, _mapper.Object, _personRepo.Object, _consumerRepo.Object, _consumerService.Object, null);

            // Act
            var result = await service.GetPersonData(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.PersonId);
        }

        [Fact]
        public async Task GetPersonData_WithoutReadReplica_ReturnsEmptyDto_WhenPersonIdIsZero()
        {
            // Arrange
            var mockModel = new PersonModel { PersonId = 0, DeleteNbr = 0 };

            _personRepo
                .Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<PersonModel, bool>>>(), false))
                .ReturnsAsync(mockModel);

            var service = new PersonService(_logger.Object, _mapper.Object, _personRepo.Object, _consumerRepo.Object, _consumerService.Object, null);

            // Act
            var result = await service.GetPersonData(0);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.PersonId);
        }
    }
}

