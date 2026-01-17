using System.Linq.Expressions;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Models;
using SunnyRewards.Helios.User.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.User.Infrastructure.Services;
using Xunit;

namespace SunnyRewards.Helios.User.UnitTest.ReadReplica
{
    public class PhoneTypeServiceReadReplicaTests
    {
        private readonly Mock<IPhoneTypeRepo> _phoneTypeRepo;
        private readonly Mock<ILogger<PhoneTypeService>> _logger;
        private readonly Mock<IMapper> _mapper;

        public PhoneTypeServiceReadReplicaTests()
        {
            _phoneTypeRepo = new Mock<IPhoneTypeRepo>();
            _logger = new Mock<ILogger<PhoneTypeService>>();
            _mapper = new Mock<IMapper>();
        }

        [Fact]
        public async Task GetAllPhoneTypes_WithoutReadReplica_UsesRepository()
        {
            // Arrange
            var mockModels = new List<PhoneTypeModel>
            {
                new PhoneTypeModel { PhoneTypeId = 1, PhoneTypeName = "Mobile", DeleteNbr = 0 },
                new PhoneTypeModel { PhoneTypeId = 2, PhoneTypeName = "Home", DeleteNbr = 0 }
            };
            var mockDtos = new List<PhoneTypeDto>
            {
                new PhoneTypeDto { PhoneTypeId = 1, PhoneTypeName = "Mobile" },
                new PhoneTypeDto { PhoneTypeId = 2, PhoneTypeName = "Home" }
            };

            _phoneTypeRepo
                .Setup(r => r.FindAsync(It.IsAny<Expression<Func<PhoneTypeModel, bool>>>(), false))
                .ReturnsAsync(mockModels);
            _mapper.Setup(m => m.Map<IList<PhoneTypeDto>>(mockModels)).Returns(mockDtos);

            var service = new PhoneTypeService(_phoneTypeRepo.Object, _logger.Object, _mapper.Object, null);

            // Act
            var result = await service.GetAllPhoneTypes();

            // Assert
            Assert.NotNull(result.PhoneTypesList);
            Assert.Equal(2, result.PhoneTypesList.Count);
            _phoneTypeRepo.Verify(r => r.FindAsync(It.IsAny<Expression<Func<PhoneTypeModel, bool>>>(), false), Times.Once);
        }

        [Fact]
        public async Task GetAllPhoneTypes_WithoutReadReplica_ReturnsEmptyOrNullList_WhenNoneExist()
        {
            // Arrange
            _phoneTypeRepo
                .Setup(r => r.FindAsync(It.IsAny<Expression<Func<PhoneTypeModel, bool>>>(), false))
                .ReturnsAsync(new List<PhoneTypeModel>());
            _mapper.Setup(m => m.Map<IList<PhoneTypeDto>>(It.IsAny<List<PhoneTypeModel>>()))
                .Returns(new List<PhoneTypeDto>());

            var service = new PhoneTypeService(_phoneTypeRepo.Object, _logger.Object, _mapper.Object, null);

            // Act
            var result = await service.GetAllPhoneTypes();

            // Assert
            Assert.True(result.PhoneTypesList == null || result.PhoneTypesList.Count == 0);
        }

        [Fact]
        public async Task GetPhoneTypeById_WithoutReadReplica_UsesRepository()
        {
            // Arrange
            var mockModel = new PhoneTypeModel { PhoneTypeId = 1, PhoneTypeName = "Mobile", DeleteNbr = 0 };
            var mockDto = new PhoneTypeDto { PhoneTypeId = 1, PhoneTypeName = "Mobile" };

            _phoneTypeRepo
                .Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<PhoneTypeModel, bool>>>(), false))
                .ReturnsAsync(mockModel);
            _mapper.Setup(m => m.Map<PhoneTypeDto>(mockModel)).Returns(mockDto);

            var service = new PhoneTypeService(_phoneTypeRepo.Object, _logger.Object, _mapper.Object, null);

            // Act
            var result = await service.GetPhoneTypeById(1);

            // Assert
            Assert.NotNull(result.PhoneType);
            Assert.Equal("Mobile", result.PhoneType.PhoneTypeName);
            _phoneTypeRepo.Verify(r => r.FindOneAsync(It.IsAny<Expression<Func<PhoneTypeModel, bool>>>(), false), Times.Once);
        }

        [Fact]
        public async Task GetPhoneTypeById_WithoutReadReplica_ReturnsNull_WhenNotFound()
        {
            // Arrange
            _phoneTypeRepo
                .Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<PhoneTypeModel, bool>>>(), false))
                .ReturnsAsync((PhoneTypeModel?)null);

            var service = new PhoneTypeService(_phoneTypeRepo.Object, _logger.Object, _mapper.Object, null);

            // Act
            var result = await service.GetPhoneTypeById(999);

            // Assert
            Assert.Null(result.PhoneType);
        }

        [Fact]
        public async Task GetAllPhoneTypes_WithoutReadReplica_HandlesException()
        {
            // Arrange
            _phoneTypeRepo
                .Setup(r => r.FindAsync(It.IsAny<Expression<Func<PhoneTypeModel, bool>>>(), false))
                .ThrowsAsync(new Exception("Database error"));

            var service = new PhoneTypeService(_phoneTypeRepo.Object, _logger.Object, _mapper.Object, null);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => service.GetAllPhoneTypes());
        }
    }
}
