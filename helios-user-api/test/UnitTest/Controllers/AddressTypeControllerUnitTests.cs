using System.Linq.Expressions;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SunnyRewards.Helios.User.Api.Controllers;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Models;
using SunnyRewards.Helios.User.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.User.Infrastructure.Services;
using SunnyRewards.Helios.User.Infrastructure.Services.Interfaces;
using Xunit;

namespace SunnyRewards.Helios.User.UnitTest.Controllers
{
    public class AddressTypeControllerUnitTests
    {
        private readonly IAddressTypeService _addressTypeService;
        private readonly AddressTypeController _addressTypeController;
        private readonly Mock<ILogger<AddressTypeController>> _controllerLogger;
        private readonly Mock<ILogger<AddressTypeService>> _serviceLogger;
        private readonly Mock<IMapper> _mapper;
        private readonly Mock<IAddressTypeRepo> _addressTypeRepo;

        public AddressTypeControllerUnitTests()
        {
            _controllerLogger = new Mock<ILogger<AddressTypeController>>();
            _serviceLogger = new Mock<ILogger<AddressTypeService>>();
            _mapper = new Mock<IMapper>();
            _addressTypeRepo = new Mock<IAddressTypeRepo>();
            _addressTypeService = new AddressTypeService(_addressTypeRepo.Object, _serviceLogger.Object, _mapper.Object);
            _addressTypeController = new AddressTypeController(_addressTypeService, _controllerLogger.Object);
        }

        [Fact]
        public async Task GetAllAddressTypes_ReturnsOk_WithData()
        {
            // Arrange
            var mockModels = new List<AddressTypeModel>
            {
                new AddressTypeModel
                {
                    AddressTypeId = 1001,
                    AddressTypeCode = "test_code_1",
                    AddressTypeName = "test_name_1",
                    Description = "test_desc_1",
                    CreateTs = DateTime.UtcNow,
                    UpdateTs = DateTime.UtcNow,
                    CreateUser = "test_create_user_1",
                    UpdateUser = "test_update_user_1",
                    DeleteNbr = 0
                }
            };

            var mockDtos = new List<AddressTypeDto>
            {
                new AddressTypeDto
                {
                    AddressTypeId = 1001,
                    AddressTypeCode = "test_code_1",
                    AddressTypeName = "test_name_1",
                    Description = "test_desc_1"
                }
            };

            _addressTypeRepo
                .Setup(r => r.FindAsync(It.IsAny<Expression<Func<AddressTypeModel, bool>>>(), false))
                .ReturnsAsync(mockModels);


            _mapper.Setup(m => m.Map<IList<AddressTypeDto>>(mockModels))
                .Returns(mockDtos);

            // Act
            var result = await _addressTypeController.GetAllAddressTypes();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<GetAllAddressTypesResponseDto>(okResult.Value);
            Assert.Single(response.AddressTypesList);
            Assert.Equal("test_code_1", response.AddressTypesList[0].AddressTypeCode);
        }

        [Fact]
        public async Task GetAllAddressTypes_ReturnsNotFound_WhenNoData()
        {
            // Arrange
            _addressTypeRepo
                .Setup(r => r.FindAsync(It.IsAny<Expression<Func<AddressTypeModel, bool>>>(), false))
                .ReturnsAsync(new List<AddressTypeModel>());

            // Act
            var result = await _addressTypeController.GetAllAddressTypes();

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            var response = Assert.IsType<GetAllAddressTypesResponseDto>(objectResult.Value);
            Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
        }

        [Fact]
        public async Task GetAllAddressTypes_ReturnsServerError_OnException()
        {
            // Arrange
            _addressTypeRepo
                .Setup(r => r.FindAsync(It.IsAny<Expression<Func<AddressTypeModel, bool>>>(), false))
                .ThrowsAsync(new Exception("DB failure"));

            // Act
            var result = await _addressTypeController.GetAllAddressTypes();

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            var response = Assert.IsType<GetAllAddressTypesResponseDto>(objectResult.Value);
            Assert.Equal(StatusCodes.Status500InternalServerError, response.ErrorCode);
        }

        [Fact]
        public async Task GetAddressTypeById_ReturnsOk_WhenDataExists()
        {
            // Arrange
            var mockModel = new AddressTypeModel
            {
                AddressTypeId = 1001,
                AddressTypeCode = "test_code_1",
                AddressTypeName = "test_name_1",
                Description = "test_desc_1",
                CreateTs = DateTime.UtcNow,
                UpdateTs = DateTime.UtcNow,
                CreateUser = "test_user",
                UpdateUser = "test_user",
                DeleteNbr = 0
            };

            var mockDto = new AddressTypeDto
            {
                AddressTypeId = 1001,
                AddressTypeCode = "test_code_1",
                AddressTypeName = "test_name_1",
                Description = "test_desc_1"
            };

            _addressTypeRepo
                .Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<AddressTypeModel, bool>>>(), false))
                .ReturnsAsync(mockModel);

            _mapper
                .Setup(m => m.Map<AddressTypeDto>(mockModel))
                .Returns(mockDto);

            // Act
            var result = await _addressTypeController.GetAddressTypeById(1001);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<GetAddressTypeResponseDto>(okResult.Value);
            Assert.Equal(1001, response.AddressType.AddressTypeId);
            Assert.Equal("test_code_1", response.AddressType.AddressTypeCode);
        }

        [Fact]
        public async Task GetAddressTypeById_ReturnsNotFound_WhenNotExists()
        {
            // Arrange
            _addressTypeRepo
                .Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<AddressTypeModel, bool>>>(), false))
                .ReturnsAsync((AddressTypeModel?)null);

            // Act
            var result = await _addressTypeController.GetAddressTypeById(999);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            var response = Assert.IsType<GetAddressTypeResponseDto>(objectResult.Value);
            Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
            Assert.Null(response.AddressType);
        }

        [Fact]
        public async Task GetAddressTypeById_ReturnsServerError_OnException()
        {
            // Arrange
            _addressTypeRepo
                .Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<AddressTypeModel, bool>>>(), false))
                .ThrowsAsync(new Exception("DB failure"));

            // Act
            var result = await _addressTypeController.GetAddressTypeById(1001);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            var response = Assert.IsType<GetAddressTypeResponseDto>(objectResult.Value);
            Assert.Equal(StatusCodes.Status500InternalServerError, response.ErrorCode);
        }
    }
}
