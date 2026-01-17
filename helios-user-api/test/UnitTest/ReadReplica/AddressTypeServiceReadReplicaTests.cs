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
    public class AddressTypeServiceReadReplicaTests
    {
        private readonly Mock<IAddressTypeRepo> _addressTypeRepo;
        private readonly Mock<ILogger<AddressTypeService>> _logger;
        private readonly Mock<IMapper> _mapper;

        public AddressTypeServiceReadReplicaTests()
        {
            _addressTypeRepo = new Mock<IAddressTypeRepo>();
            _logger = new Mock<ILogger<AddressTypeService>>();
            _mapper = new Mock<IMapper>();
        }

        [Fact]
        public async Task GetAllAddressTypes_WithoutReadReplica_UsesRepository()
        {
            // Arrange
            var mockModels = new List<AddressTypeModel>
            {
                new AddressTypeModel { AddressTypeId = 1, AddressTypeName = "Home", DeleteNbr = 0 },
                new AddressTypeModel { AddressTypeId = 2, AddressTypeName = "Work", DeleteNbr = 0 }
            };
            var mockDtos = new List<AddressTypeDto>
            {
                new AddressTypeDto { AddressTypeId = 1, AddressTypeName = "Home" },
                new AddressTypeDto { AddressTypeId = 2, AddressTypeName = "Work" }
            };

            _addressTypeRepo
                .Setup(r => r.FindAsync(It.IsAny<Expression<Func<AddressTypeModel, bool>>>(), false))
                .ReturnsAsync(mockModels);
            _mapper.Setup(m => m.Map<IList<AddressTypeDto>>(mockModels)).Returns(mockDtos);

            var service = new AddressTypeService(_addressTypeRepo.Object, _logger.Object, _mapper.Object, null);

            // Act
            var result = await service.GetAllAddressTypes();

            // Assert
            Assert.NotNull(result.AddressTypesList);
            Assert.Equal(2, result.AddressTypesList.Count);
            _addressTypeRepo.Verify(r => r.FindAsync(It.IsAny<Expression<Func<AddressTypeModel, bool>>>(), false), Times.Once);
        }

        [Fact]
        public async Task GetAllAddressTypes_WithoutReadReplica_ReturnsEmptyOrNullList_WhenNoneExist()
        {
            // Arrange
            _addressTypeRepo
                .Setup(r => r.FindAsync(It.IsAny<Expression<Func<AddressTypeModel, bool>>>(), false))
                .ReturnsAsync(new List<AddressTypeModel>());
            _mapper.Setup(m => m.Map<IList<AddressTypeDto>>(It.IsAny<List<AddressTypeModel>>()))
                .Returns(new List<AddressTypeDto>());

            var service = new AddressTypeService(_addressTypeRepo.Object, _logger.Object, _mapper.Object, null);

            // Act
            var result = await service.GetAllAddressTypes();

            // Assert
            Assert.True(result.AddressTypesList == null || result.AddressTypesList.Count == 0);
        }

        [Fact]
        public async Task GetAddressTypeById_WithoutReadReplica_UsesRepository()
        {
            // Arrange
            var mockModel = new AddressTypeModel { AddressTypeId = 1, AddressTypeName = "Home", DeleteNbr = 0 };
            var mockDto = new AddressTypeDto { AddressTypeId = 1, AddressTypeName = "Home" };

            _addressTypeRepo
                .Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<AddressTypeModel, bool>>>(), false))
                .ReturnsAsync(mockModel);
            _mapper.Setup(m => m.Map<AddressTypeDto>(mockModel)).Returns(mockDto);

            var service = new AddressTypeService(_addressTypeRepo.Object, _logger.Object, _mapper.Object, null);

            // Act
            var result = await service.GetAddressTypeById(1);

            // Assert
            Assert.NotNull(result.AddressType);
            Assert.Equal("Home", result.AddressType.AddressTypeName);
            _addressTypeRepo.Verify(r => r.FindOneAsync(It.IsAny<Expression<Func<AddressTypeModel, bool>>>(), false), Times.Once);
        }

        [Fact]
        public async Task GetAddressTypeById_WithoutReadReplica_ReturnsNull_WhenNotFound()
        {
            // Arrange
            _addressTypeRepo
                .Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<AddressTypeModel, bool>>>(), false))
                .ReturnsAsync((AddressTypeModel?)null);

            var service = new AddressTypeService(_addressTypeRepo.Object, _logger.Object, _mapper.Object, null);

            // Act
            var result = await service.GetAddressTypeById(999);

            // Assert
            Assert.Null(result.AddressType);
        }
    }
}
