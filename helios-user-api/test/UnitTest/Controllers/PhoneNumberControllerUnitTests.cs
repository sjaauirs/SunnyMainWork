using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using SunnyRewards.Helios.User.Api.Controllers;
using SunnyRewards.Helios.User.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.User.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.User.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Models;
using System.Linq.Expressions;
using Xunit;

namespace SunnyRewards.Helios.User.UnitTest.Controllers
{
    public class PhoneNumberControllerUnitTests
    {
        private readonly IPhoneNumberService _phoneNumberService;
        private readonly PhoneNumberController _phoneNumberController;

        private readonly Mock<ILogger<PhoneNumberController>> _controllerLogger;
        private readonly Mock<ILogger<PhoneNumberService>> _serviceLogger;
        private readonly Mock<IPhoneNumberRepo> _phoneNumberRepo;
        private readonly Mock<IMapper> _mapper;
        private readonly Mock<IPhoneTypeService> _phoneTypeService;
        private readonly Mock<IPersonService> _personService;

        public PhoneNumberControllerUnitTests()
        {
            _controllerLogger = new Mock<ILogger<PhoneNumberController>>();
            _serviceLogger = new Mock<ILogger<PhoneNumberService>>();
            _phoneNumberRepo = new Mock<IPhoneNumberRepo>();
            _mapper = new Mock<IMapper>();
            _phoneTypeService = new Mock<IPhoneTypeService>();
            _personService = new Mock<IPersonService>();

            _phoneNumberService = new PhoneNumberService(_phoneNumberRepo.Object, _serviceLogger.Object, _mapper.Object, _phoneTypeService.Object, _personService.Object);

            _phoneNumberController = new PhoneNumberController(_phoneNumberService, _controllerLogger.Object);
        }

        [Fact]
        public async Task GetAllPhoneNumbers_ReturnsOk_WithData()
        {
            // Arrange
            long personId = 123;

            var mockModels = new List<PhoneNumberModel>
            {
                new PhoneNumberModel
                {
                    PhoneNumberId = 1,
                    PersonId = personId,
                    PhoneTypeId = 101,
                    PhoneNumberCode = "+1",
                    PhoneNumber = "1234567890",
                    IsPrimary = true,
                    IsVerified = true,
                    VerifiedDate = DateTime.UtcNow,
                    Source = "test_source",
                    CreateTs = DateTime.UtcNow,
                    UpdateTs = DateTime.UtcNow,
                    DeleteNbr = 0
                }
            };

            var mockDtos = new List<PhoneNumberDto>
            {
                new PhoneNumberDto
                {
                    PhoneNumberId = 1,
                    PersonId = personId,
                    PhoneTypeId = 101,
                    PhoneNumberCode = "+1",
                    PhoneNumber = "1234567890",
                    IsPrimary = true,
                    IsVerified = true,
                    VerifiedDate = mockModels[0].VerifiedDate,
                    Source = "test_source"
                }
            };

            _phoneNumberRepo
                .Setup(r => r.FindOrderedAsync(
                    It.IsAny<Expression<Func<PhoneNumberModel, bool>>>(),
                    It.IsAny<Expression<Func<PhoneNumberModel, DateTime>>>(),
                    true, false))
                .ReturnsAsync(mockModels);

            _mapper
                .Setup(m => m.Map<IList<PhoneNumberDto>>(mockModels))
                .Returns(mockDtos);

            // Act
            var result = await _phoneNumberController.GetAllPhoneNumbers(personId);

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, objectResult.StatusCode);

            var response = Assert.IsType<GetAllPhoneNumbersResponseDto>(objectResult.Value);
            var dto = Assert.Single(response.PhoneNumbersList);

            Assert.Equal(1, dto.PhoneNumberId);
            Assert.Equal(101, dto.PhoneTypeId);
            Assert.Equal(personId, dto.PersonId);
            Assert.Equal("+1", dto.PhoneNumberCode);
            Assert.Equal("1234567890", dto.PhoneNumber);
            Assert.Equal("test_source", dto.Source);
            Assert.True(dto.IsPrimary);
            Assert.True(dto.IsVerified);
        }

        [Fact]
        public async Task GetAllPhoneNumbers_ReturnsNotFound_WhenNoNumbersExist()
        {
            // Arrange
            _phoneNumberRepo
                .Setup(r => r.FindOrderedAsync(
                    It.IsAny<Expression<Func<PhoneNumberModel, bool>>>(),
                    It.IsAny<Expression<Func<PhoneNumberModel, DateTime>>>(),
                    true, false))
                .ReturnsAsync(new List<PhoneNumberModel>());

            // Act
            var result = await _phoneNumberController.GetAllPhoneNumbers(5679);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            var response = Assert.IsType<GetAllPhoneNumbersResponseDto>(objectResult.Value);
            Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
        }

        [Fact]
        public async Task GetAllPhoneNumbers_ReturnsServerError_OnException()
        {
            // Arrange
            _phoneNumberRepo
                .Setup(r => r.FindOrderedAsync(
                    It.IsAny<Expression<Func<PhoneNumberModel, bool>>>(),
                    It.IsAny<Expression<Func<PhoneNumberModel, DateTime>>>(),
                    true, false))
                .ThrowsAsync(new Exception("Simulated failure"));

            // Act
            var result = await _phoneNumberController.GetAllPhoneNumbers(7890);

            // Assert
            var objectResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        [Fact]
        public async Task CreatePhoneNumber_ReturnsOk_WhenCreatedSuccessfully()
        {
            // Arrange
            var request = new CreatePhoneNumberRequestDto
            {
                PersonId = 123,
                PhoneTypeId = 1,
                PhoneNumber = "1234567890",
                ConsumerCode = "consumer",
                TenantCode = "tenant",
                IsVerified = false,
                Source = "test_source",
                CreateUser = "test_user"
            };

            var model = new PhoneNumberModel
            {
                PhoneNumberId = 1,
                PersonId = 123,
                PhoneTypeId = 1,
                PhoneNumber = "1234567890",
                PhoneNumberCode = "pnc-abc",
                IsPrimary = true,
                IsVerified = false,
                Source = "test_source",
                CreateUser = "test_user",
                CreateTs = DateTime.UtcNow
            };

            _phoneNumberRepo
                .Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<PhoneNumberModel, bool>>>(), false))
                .ReturnsAsync((PhoneNumberModel?)null);

            _phoneTypeService
                .Setup(s => s.GetPhoneTypeById(1))
                .ReturnsAsync(new GetPhoneTypeResponseDto());

            _personService
                .Setup(s => s.GetPersonData(123))
                .ReturnsAsync(new SunnyRewards.Helios.User.Core.Domain.Dtos.PersonDto { PersonId = 123 });

            _mapper.Setup(m => m.Map<PhoneNumberModel>(request))
                .Returns(model);

            _mapper.Setup(m => m.Map<PhoneNumberDto>(model))
                .Returns(new PhoneNumberDto
                {
                    PhoneNumberId = 1,
                    PersonId = 123,
                    PhoneTypeId = 1,
                    PhoneNumber = "1234567890",
                    PhoneNumberCode = "pnc-abc",
                    IsPrimary = true,
                    IsVerified = false,
                    Source = "test_source"
                });

            // Act
            var result = await _phoneNumberController.CreatePhoneNumber(request);

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, objectResult.StatusCode);

            var response = Assert.IsType<PhoneNumberResponseDto>(objectResult.Value);
            Assert.Equal(1, response.PhoneNumber.PhoneNumberId);
            Assert.Equal("1234567890", response.PhoneNumber.PhoneNumber);
            Assert.True(response.PhoneNumber.IsPrimary);
        }

        [Fact]
        public async Task CreatePhoneNumber_ReturnsConflict_WhenPhoneNumberExists()
        {
            // Arrange
            var request = new CreatePhoneNumberRequestDto
            {
                PersonId = 123,
                PhoneTypeId = 1,
                PhoneNumber = "1234567890",
                ConsumerCode = "consumer",
                TenantCode = "tenant",
                IsVerified = false,
                Source = "test_source",
                CreateUser = "test_user"
            };

            _phoneNumberRepo
                .Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<PhoneNumberModel, bool>>>(), false))
                .ReturnsAsync(new PhoneNumberModel());

            // Act
            var result = await _phoneNumberController.CreatePhoneNumber(request);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            var response = Assert.IsType<PhoneNumberResponseDto>(objectResult.Value);
            Assert.Equal(StatusCodes.Status409Conflict, response.ErrorCode);
        }

        [Fact]
        public async Task CreatePhoneNumber_ReturnsBadRequest_WhenPhoneTypeNotFound()
        {
            // Arrange
            var request = new CreatePhoneNumberRequestDto
            {
                PersonId = 123,
                PhoneTypeId = 99,
                PhoneNumber = "1234567890",
                ConsumerCode = "consumer",
                TenantCode = "tenant",
                IsVerified = false,
                Source = "test_source",
                CreateUser = "test_user"
            };

            _phoneNumberRepo
                .Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<PhoneNumberModel, bool>>>(), false))
                .ReturnsAsync((PhoneNumberModel?)null);

            _phoneTypeService
                .Setup(s => s.GetPhoneTypeById(99))
                .ReturnsAsync(new GetPhoneTypeResponseDto
                {
                    ErrorCode = StatusCodes.Status404NotFound
                });

            // Act
            var result = await _phoneNumberController.CreatePhoneNumber(request);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            var response = Assert.IsType<PhoneNumberResponseDto>(objectResult.Value);
            Assert.Equal(StatusCodes.Status400BadRequest, response.ErrorCode);
            Assert.Equal("Phone type with id 99 does not exist.", response.ErrorMessage);
        }

        [Fact]
        public async Task CreatePhoneNumber_ReturnsBadRequest_WhenPersonNotFound()
        {
            // Arrange
            var request = new CreatePhoneNumberRequestDto
            {
                PersonId = 999,
                PhoneTypeId = 1,
                PhoneNumber = "1234567890",
                ConsumerCode = "consumer",
                TenantCode = "tenant",
                IsVerified = false,
                Source = "test_source",
                CreateUser = "test_user"
            };

            _phoneNumberRepo
                .Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<PhoneNumberModel, bool>>>(), false))
                .ReturnsAsync((PhoneNumberModel?)null);

            _phoneTypeService
                .Setup(s => s.GetPhoneTypeById(1))
                .ReturnsAsync(new GetPhoneTypeResponseDto());

            _personService
                .Setup(s => s.GetPersonData(999))
                .ReturnsAsync(new SunnyRewards.Helios.User.Core.Domain.Dtos.PersonDto { PersonId = 0 });

            // Act
            var result = await _phoneNumberController.CreatePhoneNumber(request);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            var response = Assert.IsType<PhoneNumberResponseDto>(objectResult.Value);
            Assert.Equal(StatusCodes.Status400BadRequest, response.ErrorCode);
            Assert.Equal("Person with id 999 does not exist.", response.ErrorMessage);
        }

        [Fact]
        public async Task CreatePhoneNumber_ReturnsServerError_OnException()
        {
            // Arrange
            var request = new CreatePhoneNumberRequestDto
            {
                PersonId = 123,
                PhoneTypeId = 1,
                PhoneNumber = "1234567890",
                ConsumerCode = "consumer",
                TenantCode = "tenant",
                IsVerified = false,
                Source = "test_source",
                CreateUser = "test_user"
            };

            _phoneNumberRepo
                .Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<PhoneNumberModel, bool>>>(), false))
                .ThrowsAsync(new Exception("Simulated exception"));

            // Act
            var result = await _phoneNumberController.CreatePhoneNumber(request);

            // Assert
            var objectResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        [Fact]
        public async Task UpdatePhoneNumber_ReturnsOk_WhenUpdatedSuccessfully()
        {
            // Arrange
            var updateRequest = new UpdatePhoneNumberRequestDto
            {
                PhoneNumberId = 1,
                PhoneTypeId = 101,
                PhoneNumber = "9998887777",
                Source = "update_source",
                IsVerified = true,
                VerifiedDate = DateTime.UtcNow,
                ConsumerCode = "consumer1",
                TenantCode = "tenant1",
                UpdateUser = "admin"
            };

            var existingModel = new PhoneNumberModel
            {
                PhoneNumberId = 1,
                PersonId = 123,
                PhoneTypeId = 100,
                PhoneNumber = "1234567890",
                IsPrimary = false,
                Source = "original_source",
                IsVerified = false,
                VerifiedDate = null,
                UpdateUser = "user1",
                UpdateTs = DateTime.UtcNow.AddDays(-2),
                DeleteNbr = 0
            };

            _phoneNumberRepo
                .Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<PhoneNumberModel, bool>>>(), false))
                .ReturnsAsync(existingModel);

            _phoneTypeService
                .Setup(s => s.GetPhoneTypeById(updateRequest.PhoneTypeId))
                .ReturnsAsync(new GetPhoneTypeResponseDto());

            _mapper
                .Setup(m => m.Map<PhoneNumberDto>(It.IsAny<PhoneNumberModel>()))
                .Returns(new PhoneNumberDto
                {
                    PhoneNumberId = 1,
                    PhoneTypeId = updateRequest.PhoneTypeId,
                    PhoneNumber = updateRequest.PhoneNumber,
                    Source = updateRequest.Source,
                    IsVerified = updateRequest.IsVerified,
                    VerifiedDate = updateRequest.VerifiedDate,
                    UpdateUser = updateRequest.UpdateUser
                });

            // Act
            var result = await _phoneNumberController.UpdatePhoneNumber(updateRequest);

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, objectResult.StatusCode);

            var response = Assert.IsType<PhoneNumberResponseDto>(objectResult.Value);
            Assert.Equal(1, response.PhoneNumber.PhoneNumberId);
            Assert.Equal("9998887777", response.PhoneNumber.PhoneNumber);
            Assert.Equal("update_source", response.PhoneNumber.Source);
            Assert.True(response.PhoneNumber.IsVerified);
        }

        [Fact]
        public async Task UpdatePhoneNumber_ReturnsNotFound_WhenPhoneNumberDoesNotExist()
        {
            // Arrange
            var updateRequest = new UpdatePhoneNumberRequestDto
            {
                PhoneNumberId = 999,
                PhoneTypeId = 100,
                ConsumerCode = "c",
                TenantCode = "t",
                UpdateUser = "user"
            };

            _phoneNumberRepo
                .Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<PhoneNumberModel, bool>>>(), false))
                .ReturnsAsync((PhoneNumberModel?)null);

            // Act
            var result = await _phoneNumberController.UpdatePhoneNumber(updateRequest);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            var response = Assert.IsType<PhoneNumberResponseDto>(objectResult.Value);
            Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
            Assert.Equal("Phone number with id 999 does not exist.", response.ErrorMessage);
        }

        [Fact]
        public async Task UpdatePhoneNumber_ReturnsBadRequest_WhenPhoneTypeIsInvalid()
        {
            // Arrange
            var updateRequest = new UpdatePhoneNumberRequestDto
            {
                PhoneNumberId = 1,
                PhoneTypeId = 555,
                ConsumerCode = "c",
                TenantCode = "t",
                UpdateUser = "user"
            };

            _phoneNumberRepo
                .Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<PhoneNumberModel, bool>>>(), false))
                .ReturnsAsync(new PhoneNumberModel());

            _phoneTypeService
                .Setup(s => s.GetPhoneTypeById(555))
                .ReturnsAsync(new GetPhoneTypeResponseDto
                {
                    ErrorCode = StatusCodes.Status404NotFound
                });

            // Act
            var result = await _phoneNumberController.UpdatePhoneNumber(updateRequest);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            var response = Assert.IsType<PhoneNumberResponseDto>(objectResult.Value);
            Assert.Equal(StatusCodes.Status400BadRequest, response.ErrorCode);
            Assert.Equal("Phone type with id 555 does not exist.", response.ErrorMessage);
        }

        [Fact]
        public async Task UpdatePhoneNumber_ReturnsServerError_OnException()
        {
            // Arrange
            var updateRequest = new UpdatePhoneNumberRequestDto
            {
                PhoneNumberId = 1,
                PhoneTypeId = 100,
                ConsumerCode = "c",
                TenantCode = "t",
                UpdateUser = "user"
            };

            _phoneNumberRepo
                .Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<PhoneNumberModel, bool>>>(), false))
                .ThrowsAsync(new Exception("Simulated error"));

            // Act
            var result = await _phoneNumberController.UpdatePhoneNumber(updateRequest);

            // Assert
            var objectResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        [Fact]
        public async Task DeletePhoneNumber_ReturnsOk_WhenDeletedSuccessfully()
        {
            // Arrange
            var request = new DeletePhoneNumberRequestDto
            {
                PhoneNumberId = 1,
                UpdateUser = "deleter"
            };

            var mockModel = new PhoneNumberModel
            {
                PhoneNumberId = 1,
                PersonId = 123,
                PhoneTypeId = 200,
                PhoneNumber = "1234567890",
                PhoneNumberCode = "pnc-abc",
                Source = "manual",
                IsPrimary = false,
                IsVerified = false,
                DeleteNbr = 0,
                CreateUser = "creator",
                UpdateUser = "updater",
                CreateTs = DateTime.UtcNow.AddDays(-10),
                UpdateTs = DateTime.UtcNow
            };

            var mockDto = new PhoneNumberDto
            {
                PhoneNumberId = 1,
                PersonId = 123,
                PhoneTypeId = 200,
                PhoneNumber = "1234567890",
                PhoneNumberCode = "pnc-abc",
                Source = "manual",
                IsPrimary = false,
                IsVerified = false,
                DeleteNbr = 1,
                CreateUser = "creator",
                UpdateUser = "deleter",
                CreateTs = mockModel.CreateTs,
                UpdateTs = mockModel.UpdateTs
            };

            _phoneNumberRepo
                .Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<PhoneNumberModel, bool>>>(), false))
                .ReturnsAsync(mockModel);

            _mapper
                .Setup(m => m.Map<PhoneNumberDto>(mockModel))
                .Returns(mockDto);

            // Act
            var result = await _phoneNumberController.DeletePhoneNumber(request);

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, objectResult.StatusCode);

            var response = Assert.IsType<PhoneNumberResponseDto>(objectResult.Value);
            Assert.Equal(1, response.PhoneNumber.PhoneNumberId);
            Assert.Equal(1, response.PhoneNumber.DeleteNbr);
            Assert.Equal("deleter", response.PhoneNumber.UpdateUser);
        }

        [Fact]
        public async Task DeletePhoneNumber_ReturnsNotFound_WhenPhoneNumberDoesNotExist()
        {
            // Arrange
            var request = new DeletePhoneNumberRequestDto
            {
                PhoneNumberId = 999,
                UpdateUser = "tester"
            };

            _phoneNumberRepo
                .Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<PhoneNumberModel, bool>>>(), false))
                .ReturnsAsync((PhoneNumberModel?)null);

            // Act
            var result = await _phoneNumberController.DeletePhoneNumber(request);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            var response = Assert.IsType<PhoneNumberResponseDto>(objectResult.Value);
            Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
            Assert.Equal("Phone number with id 999 does not exist.", response.ErrorMessage);
        }

        [Fact]
        public async Task DeletePhoneNumber_ReturnsServerError_OnException()
        {
            // Arrange
            var request = new DeletePhoneNumberRequestDto
            {
                PhoneNumberId = 5,
                UpdateUser = "tester"
            };

            _phoneNumberRepo
                .Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<PhoneNumberModel, bool>>>(), false))
                .ThrowsAsync(new Exception("Simulated DB crash"));

            // Act
            var result = await _phoneNumberController.DeletePhoneNumber(request);

            // Assert
            var objectResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        [Fact]
        public async Task SetPrimaryPhoneNumber_ReturnsOk_WhenSuccessfullySet()
        {
            // Arrange
            var request = new UpdatePrimaryPhoneNumberRequestDto
            {
                PhoneNumberId = 1001,
                UpdateUser = "admin_user"
            };

            var oldPrimary = new PhoneNumberModel
            {
                PhoneNumberId = 1,
                PersonId = 123,
                IsPrimary = true,
                DeleteNbr = 0,
                UpdateUser = "prev_user",
                UpdateTs = DateTime.UtcNow.AddDays(-1)
            };

            var newPrimary = new PhoneNumberModel
            {
                PhoneNumberId = 1001,
                PersonId = 123,
                IsPrimary = false,
                DeleteNbr = 0,
                UpdateUser = "old_user",
                UpdateTs = DateTime.UtcNow.AddDays(-2)
            };

            var expectedDto = new PhoneNumberDto
            {
                PhoneNumberId = 1001,
                PersonId = 123,
                IsPrimary = true,
                UpdateUser = "admin_user",
                UpdateTs = DateTime.UtcNow
            };

            _phoneNumberRepo
                .Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<PhoneNumberModel, bool>>>(), false))
                .Returns<Expression<Func<PhoneNumberModel, bool>>, bool>((predicate, tracking) =>
                {
                    if (predicate.Compile()(newPrimary)) return Task.FromResult(newPrimary);
                    if (predicate.Compile()(oldPrimary)) return Task.FromResult(oldPrimary);
                    return Task.FromResult<PhoneNumberModel?>(null);
                });

            _mapper
                .Setup(m => m.Map<PhoneNumberDto>(It.Is<PhoneNumberModel>(p => p.PhoneNumberId == 1001)))
                .Returns(expectedDto);

            // Act
            var result = await _phoneNumberController.SetPrimaryPhoneNumber(request);

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, objectResult.StatusCode);

            var response = Assert.IsType<PhoneNumberResponseDto>(objectResult.Value);
            Assert.True(response.PhoneNumber.IsPrimary);
            Assert.Equal("admin_user", response.PhoneNumber.UpdateUser);
        }

        [Fact]
        public async Task SetPrimaryPhoneNumber_ReturnsNotFound_WhenPhoneNumberNotFound()
        {
            // Arrange
            var request = new UpdatePrimaryPhoneNumberRequestDto
            {
                PhoneNumberId = 9999,
                UpdateUser = "tester"
            };

            _phoneNumberRepo
                .Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<PhoneNumberModel, bool>>>(), false))
                .ReturnsAsync((PhoneNumberModel?)null);

            // Act
            var result = await _phoneNumberController.SetPrimaryPhoneNumber(request);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            var response = Assert.IsType<PhoneNumberResponseDto>(objectResult.Value);
            Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
            Assert.Equal("Phone number with id 9999 does not exist.", response.ErrorMessage);
        }

        [Fact]
        public async Task SetPrimaryPhoneNumber_ReturnsServerError_OnException()
        {
            // Arrange
            var request = new UpdatePrimaryPhoneNumberRequestDto
            {
                PhoneNumberId = 5,
                UpdateUser = "tester"
            };

            _phoneNumberRepo
                .Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<PhoneNumberModel, bool>>>(), false))
                .ThrowsAsync(new Exception("Simulated crash"));

            // Act
            var result = await _phoneNumberController.SetPrimaryPhoneNumber(request);

            // Assert
            var objectResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        [Fact]
        public async Task GetPhoneNumber_ReturnsOk_WithData()
        {
            // Arrange
            long personId = 123;
            long? phoneTypeId = 101;
            bool? isPrimary = true;

            var mockModels = new List<PhoneNumberModel>
    {
        new PhoneNumberModel
        {
            PhoneNumberId = 1,
            PhoneTypeId = phoneTypeId.Value,
            PersonId = personId,
            PhoneNumber = "1234567890",
            PhoneNumberCode = "pnc-001",
            IsPrimary = true,
            IsVerified = false,
            Source = "test_source_1",
            CreateUser = "tester1",
            UpdateUser = "tester2",
            CreateTs = DateTime.UtcNow,
            UpdateTs = DateTime.UtcNow,
            DeleteNbr = 0
        }
    };

            var mockDtos = new List<PhoneNumberDto>
    {
        new PhoneNumberDto
        {
            PhoneNumberId = 1,
            PhoneTypeId = phoneTypeId.Value,
            PersonId = personId,
            PhoneNumber = "1234567890",
            PhoneNumberCode = "pnc-001",
            IsPrimary = true,
            IsVerified = false,
            Source = "test_source_1",
            CreateUser = "tester1",
            UpdateUser = "tester2",
            CreateTs = mockModels[0].CreateTs,
            UpdateTs = mockModels[0].UpdateTs,
            DeleteNbr = 0
        }
    };

            _phoneNumberRepo
                .Setup(r => r.FindOrderedAsync(
                    It.IsAny<Expression<Func<PhoneNumberModel, bool>>>(),
                    It.IsAny<Expression<Func<PhoneNumberModel, DateTime>>>(),
                    true, false))
                .ReturnsAsync(mockModels);

            _mapper
                .Setup(m => m.Map<IList<PhoneNumberDto>>(mockModels))
                .Returns(mockDtos);

            // Act
            var result = await _phoneNumberController.GetPhoneNumber(personId, phoneTypeId, isPrimary);

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, objectResult.StatusCode);

            var response = Assert.IsType<GetAllPhoneNumbersResponseDto>(objectResult.Value);
            var dto = Assert.Single(response.PhoneNumbersList);

            Assert.Equal(1, dto.PhoneNumberId);
            Assert.Equal(phoneTypeId.Value, dto.PhoneTypeId);
            Assert.Equal(personId, dto.PersonId);
            Assert.Equal("1234567890", dto.PhoneNumber);
            Assert.Equal("pnc-001", dto.PhoneNumberCode);
            Assert.True(dto.IsPrimary);
            Assert.False(dto.IsVerified);
            Assert.Equal("test_source_1", dto.Source);
        }

        [Fact]
        public async Task GetPhoneNumber_ReturnsNotFound_WhenNoNumbersMatch()
        {
            // Arrange
            _phoneNumberRepo
                .Setup(r => r.FindOrderedAsync(
                    It.IsAny<Expression<Func<PhoneNumberModel, bool>>>(),
                    It.IsAny<Expression<Func<PhoneNumberModel, DateTime>>>(),
                    true, false))
                .ReturnsAsync(new List<PhoneNumberModel>());

            // Act
            var result = await _phoneNumberController.GetPhoneNumber(789, 999, true);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            var response = Assert.IsType<GetAllPhoneNumbersResponseDto>(objectResult.Value);
            Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
            Assert.Equal("No phone numbers found for the given criteria.", response.ErrorMessage);
        }

        [Fact]
        public async Task GetPhoneNumber_ReturnsServerError_OnException()
        {
            // Arrange
            _phoneNumberRepo
                .Setup(r => r.FindOrderedAsync(
                    It.IsAny<Expression<Func<PhoneNumberModel, bool>>>(),
                    It.IsAny<Expression<Func<PhoneNumberModel, DateTime>>>(),
                    true, false))
                .ThrowsAsync(new Exception("Simulated failure"));

            // Act
            var result = await _phoneNumberController.GetPhoneNumber(999, null, null);

            // Assert
            var objectResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }


    }
}
