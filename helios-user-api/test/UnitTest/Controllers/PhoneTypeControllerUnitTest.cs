using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
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
using Xunit;

namespace SunnyRewards.Helios.User.UnitTest.Controllers
{
    public class PhoneTypeControllerUnitTests
    {
        private readonly PhoneTypeController _phoneTypeController;
        private readonly Mock<ILogger<PhoneTypeController>> _controllerLogger;
        private readonly Mock<ILogger<PhoneTypeService>> _serviceLogger;
        private readonly Mock<IPhoneTypeRepo> _phoneTypeRepo;
        private readonly Mock<IMapper> _mapper;
        private readonly PhoneTypeService _phoneTypeService;

        public PhoneTypeControllerUnitTests()
        {
            _controllerLogger = new Mock<ILogger<PhoneTypeController>>();
            _serviceLogger = new Mock<ILogger<PhoneTypeService>>();
            _phoneTypeRepo = new Mock<IPhoneTypeRepo>();
            _mapper = new Mock<IMapper>();
            _phoneTypeService = new PhoneTypeService(_phoneTypeRepo.Object, _serviceLogger.Object, _mapper.Object);
            _phoneTypeController = new PhoneTypeController(_phoneTypeService, _controllerLogger.Object);
        }

        [Fact]
        public async Task GetAllPhoneTypes_ReturnsOk_WithData()
        {
            // Arrange
            var mockModels = new List<PhoneTypeModel>
            {
                new PhoneTypeModel
                {
                    PhoneTypeId = 1001,
                    PhoneTypeCode = "test_code_1",
                    PhoneTypeName = "test_name_1",
                    Description = "test_desc_1",
                    CreateUser = "test_user",
                    UpdateUser = "test_user",
                    CreateTs = DateTime.UtcNow,
                    UpdateTs = DateTime.UtcNow,
                    DeleteNbr = 0
                }
            };

            var mockDtos = new List<PhoneTypeDto>
            {
                new PhoneTypeDto
                {
                    PhoneTypeId = 1001,
                    PhoneTypeCode = "test_code_1",
                    PhoneTypeName = "test_name_1",
                    Description = "test_desc_1"
                }
            };

            _phoneTypeRepo
                .Setup(r => r.FindAsync(It.IsAny<Expression<Func<PhoneTypeModel, bool>>>(), false))
                .ReturnsAsync(mockModels);

            _mapper
                .Setup(m => m.Map<IList<PhoneTypeDto>>(mockModels))
                .Returns(mockDtos);

            // Act
            var result = await _phoneTypeController.GetAllPhoneTypes();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<GetAllPhoneTypesResponseDto>(okResult.Value);
            Assert.Single(response.PhoneTypesList);
            Assert.Equal("test_code_1", response.PhoneTypesList[0].PhoneTypeCode);
        }

        [Fact]
        public async Task GetAllPhoneTypes_ReturnsNotFound_WhenNoData()
        {
            // Arrange
            _phoneTypeRepo
                .Setup(r => r.FindAsync(It.IsAny<Expression<Func<PhoneTypeModel, bool>>>(), false))
                .ReturnsAsync(new List<PhoneTypeModel>());

            // Act
            var result = await _phoneTypeController.GetAllPhoneTypes();

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            var response = Assert.IsType<GetAllPhoneTypesResponseDto>(objectResult.Value);
            Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
        }

        [Fact]
        public async Task GetAllPhoneTypes_ReturnsServerError_OnException()
        {
            // Arrange
            _phoneTypeRepo
                .Setup(r => r.FindAsync(It.IsAny<Expression<Func<PhoneTypeModel, bool>>>(), false))
                .ThrowsAsync(new Exception("DB error"));

            // Act
            var result = await _phoneTypeController.GetAllPhoneTypes();

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            var response = Assert.IsType<GetAllPhoneTypesResponseDto>(objectResult.Value);
            Assert.Equal(StatusCodes.Status500InternalServerError, response.ErrorCode);
        }

        [Fact]
        public async Task GetPhoneTypeById_ReturnsOk_WhenDataExists()
        {
            // Arrange
            var mockModel = new PhoneTypeModel
            {
                PhoneTypeId = 1001,
                PhoneTypeCode = "test_code_1",
                PhoneTypeName = "test_name_1",
                Description = "test_desc_1",
                CreateUser = "test_user",
                UpdateUser = "test_user",
                CreateTs = DateTime.UtcNow,
                UpdateTs = DateTime.UtcNow,
                DeleteNbr = 0
            };

            var mockDto = new PhoneTypeDto
            {
                PhoneTypeId = 1001,
                PhoneTypeCode = "test_code_1",
                PhoneTypeName = "test_name_1",
                Description = "test_desc_1"
            };

            _phoneTypeRepo
                .Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<PhoneTypeModel, bool>>>(), false))
                .ReturnsAsync(mockModel);

            _mapper
                .Setup(m => m.Map<PhoneTypeDto>(mockModel))
                .Returns(mockDto);

            // Act
            var result = await _phoneTypeController.GetPhoneTypeById(1001);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<GetPhoneTypeResponseDto>(okResult.Value);
            Assert.Equal(1001, response.PhoneType.PhoneTypeId);
            Assert.Equal("test_code_1", response.PhoneType.PhoneTypeCode);
        }

        [Fact]
        public async Task GetPhoneTypeById_ReturnsNotFound_WhenNotExists()
        {
            // Arrange
            _phoneTypeRepo
                .Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<PhoneTypeModel, bool>>>(), false))
                .ReturnsAsync((PhoneTypeModel?)null);

            // Act
            var result = await _phoneTypeController.GetPhoneTypeById(999);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            var response = Assert.IsType<GetPhoneTypeResponseDto>(objectResult.Value);
            Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
            Assert.Null(response.PhoneType);
        }

        [Fact]
        public async Task GetPhoneTypeById_ReturnsServerError_OnException()
        {
            // Arrange
            _phoneTypeRepo
                .Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<PhoneTypeModel, bool>>>(), false))
                .ThrowsAsync(new Exception("DB error"));

            // Act
            var result = await _phoneTypeController.GetPhoneTypeById(1001);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            var response = Assert.IsType<GetPhoneTypeResponseDto>(objectResult.Value);
            Assert.Equal(StatusCodes.Status500InternalServerError, response.ErrorCode);
        }
    }
}
