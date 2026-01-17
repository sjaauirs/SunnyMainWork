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
    public class PersonAddressControllerUnitTests
    {
        private readonly IPersonAddressService _personAddressService;
        private readonly PersonAddressController _personAddressController;

        private readonly Mock<ILogger<PersonAddressController>> _controllerLogger;
        private readonly Mock<ILogger<PersonAddressService>> _serviceLogger;
        private readonly Mock<IPersonAddressRepo> _personAddressRepo;
        private readonly Mock<IMapper> _mapper;
        private readonly Mock<IAddressTypeService> _addressTypeService;
        private readonly Mock<IPersonService> _personService;

        public PersonAddressControllerUnitTests()
        {
            _controllerLogger = new Mock<ILogger<PersonAddressController>>();
            _serviceLogger = new Mock<ILogger<PersonAddressService>>();
            _personAddressRepo = new Mock<IPersonAddressRepo>();
            _mapper = new Mock<IMapper>();
            _addressTypeService = new Mock<IAddressTypeService>();
            _personService = new Mock<IPersonService>();

            _personAddressService = new PersonAddressService( _personAddressRepo.Object, _serviceLogger.Object, _mapper.Object, _addressTypeService.Object, _personService.Object);

            _personAddressController = new PersonAddressController(_personAddressService, _controllerLogger.Object);
        }

        [Fact]
        public async Task GetAllPersonAddresses_ReturnsOk_WithData()
        {
            // Arrange
            long personId = 123;

            var mockModels = new List<PersonAddressModel>
            {
                new PersonAddressModel
                {
                    PersonAddressId = 1,
                    AddressTypeId = 101,
                    PersonId = personId,
                    AddressLabel = "test_label_1",
                    Line1 = "test_line1_1",
                    Line2 = "test_line2_1",
                    City = "test_city_1",
                    State = "test_state_1",
                    PostalCode = "12345",
                    Region = "test_region_1",
                    CountryCode = "US",
                    Country = "United States",
                    Source = "test_source_1",
                    IsPrimary = true,
                    CreateTs = DateTime.UtcNow,
                    UpdateTs = DateTime.UtcNow,
                    CreateUser = "tester1",
                    UpdateUser = "tester2",
                    DeleteNbr = 0
                }
            };

            var mockDtos = new List<PersonAddressDto>
            {
                new PersonAddressDto
                {
                    PersonAddressId = 1,
                    AddressTypeId = 101,
                    PersonId = personId,
                    AddressLabel = "test_label_1",
                    Line1 = "test_line1_1",
                    Line2 = "test_line2_1",
                    City = "test_city_1",
                    State = "test_state_1",
                    PostalCode = "12345",
                    Region = "test_region_1",
                    CountryCode = "US",
                    Country = "United States",
                    Source = "test_source_1",
                    IsPrimary = true,
                    CreateTs = mockModels[0].CreateTs,
                    UpdateTs = mockModels[0].UpdateTs,
                    CreateUser = "tester1",
                    UpdateUser = "tester2",
                    DeleteNbr = 0
                }
            };

            _personAddressRepo
                .Setup(r => r.FindOrderedAsync(
                    It.IsAny<Expression<Func<PersonAddressModel, bool>>>(),
                    It.IsAny<Expression<Func<PersonAddressModel, DateTime>>>(),
                    true, false))
                .ReturnsAsync(mockModels);

            _mapper
                .Setup(m => m.Map<IList<PersonAddressDto>>(mockModels))
                .Returns(mockDtos);

            // Act
            var result = await _personAddressController.GetAllPersonAddresses(personId);

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, objectResult.StatusCode);

            var response = Assert.IsType<GetAllPersonAddressesResponseDto>(objectResult.Value);
            var dto = Assert.Single(response.PersonAddressesList);

            Assert.Equal(1, dto.PersonAddressId);
            Assert.Equal(101, dto.AddressTypeId);
            Assert.Equal(personId, dto.PersonId);
            Assert.Equal("test_label_1", dto.AddressLabel);
            Assert.Equal("test_line1_1", dto.Line1);
            Assert.Equal("test_line2_1", dto.Line2);
            Assert.Equal("test_city_1", dto.City);
            Assert.Equal("test_state_1", dto.State);
            Assert.Equal("12345", dto.PostalCode);
            Assert.Equal("test_region_1", dto.Region);
            Assert.Equal("US", dto.CountryCode);
            Assert.Equal("United States", dto.Country);
            Assert.Equal("test_source_1", dto.Source);
            Assert.True(dto.IsPrimary);
        }

        [Fact]
        public async Task GetAllPersonAddresses_ReturnsNotFound_WhenNoAddressesExist()
        {
            // Arrange
            _personAddressRepo
                .Setup(r => r.FindOrderedAsync(
                    It.IsAny<Expression<Func<PersonAddressModel, bool>>>(),
                    It.IsAny<Expression<Func<PersonAddressModel, DateTime>>>(),
                    true, false))
                .ReturnsAsync(new List<PersonAddressModel>());

            // Act
            var result = await _personAddressController.GetAllPersonAddresses(5679);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            var response = Assert.IsType<GetAllPersonAddressesResponseDto>(objectResult.Value);
            Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
        }

        [Fact]
        public async Task GetAllPersonAddresses_ReturnsServerError_OnException()
        {
            // Arrange
            _personAddressRepo
                .Setup(r => r.FindOrderedAsync(
                    It.IsAny<Expression<Func<PersonAddressModel, bool>>>(),
                    It.IsAny<Expression<Func<PersonAddressModel, DateTime>>>(),
                    true, false))
                .ThrowsAsync(new Exception("Simulated failure"));

            // Act
            var result = await _personAddressController.GetAllPersonAddresses(7890);

            // Assert
            var objectResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        [Fact]
        public async Task GetPersonAddress_ReturnsOk_WithData()
        {
            // Arrange
            long personId = 123;
            long? addressTypeId = 101;
            bool? isPrimary = true;

            var mockModels = new List<PersonAddressModel>
            {
                new PersonAddressModel
                {
                    PersonAddressId = 1,
                    AddressTypeId = addressTypeId.Value,
                    PersonId = personId,
                    AddressLabel = "test_label_1",
                    Line1 = "test_line1_1",
                    Line2 = "test_line2_1",
                    City = "test_city_1",
                    State = "test_state_1",
                    PostalCode = "12345",
                    Region = "test_region_1",
                    CountryCode = "US",
                    Country = "United States",
                    Source = "test_source_1",
                    IsPrimary = true,
                    CreateTs = DateTime.UtcNow,
                    UpdateTs = DateTime.UtcNow,
                    CreateUser = "tester1",
                    UpdateUser = "tester2",
                    DeleteNbr = 0
                }
            };

            var mockDtos = new List<PersonAddressDto>
            {
                new PersonAddressDto
                {
                    PersonAddressId = 1,
                    AddressTypeId = addressTypeId.Value,
                    PersonId = personId,
                    AddressLabel = "test_label_1",
                    Line1 = "test_line1_1",
                    Line2 = "test_line2_1",
                    City = "test_city_1",
                    State = "test_state_1",
                    PostalCode = "12345",
                    Region = "test_region_1",
                    CountryCode = "US",
                    Country = "United States",
                    Source = "test_source_1",
                    IsPrimary = true,
                    CreateTs = mockModels[0].CreateTs,
                    UpdateTs = mockModels[0].UpdateTs,
                    CreateUser = "tester1",
                    UpdateUser = "tester2",
                    DeleteNbr = 0
                }
            };

            _personAddressRepo
                .Setup(r => r.FindOrderedAsync(
                    It.IsAny<Expression<Func<PersonAddressModel, bool>>>(),
                    It.IsAny<Expression<Func<PersonAddressModel, DateTime>>>(),
                    true, false))
                .ReturnsAsync(mockModels);

            _mapper
                .Setup(m => m.Map<IList<PersonAddressDto>>(mockModels))
                .Returns(mockDtos);

            // Act
            var result = await _personAddressController.GetPersonAddress(personId, addressTypeId, isPrimary);

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, objectResult.StatusCode);

            var response = Assert.IsType<GetAllPersonAddressesResponseDto>(objectResult.Value);
            var dto = Assert.Single(response.PersonAddressesList);

            Assert.Equal(1, dto.PersonAddressId);
            Assert.Equal(addressTypeId.Value, dto.AddressTypeId);
            Assert.Equal(personId, dto.PersonId);
            Assert.Equal("test_label_1", dto.AddressLabel);
            Assert.Equal("test_line1_1", dto.Line1);
            Assert.Equal("test_line2_1", dto.Line2);
            Assert.Equal("test_city_1", dto.City);
            Assert.Equal("test_state_1", dto.State);
            Assert.Equal("12345", dto.PostalCode);
            Assert.Equal("test_region_1", dto.Region);
            Assert.Equal("US", dto.CountryCode);
            Assert.Equal("United States", dto.Country);
            Assert.Equal("test_source_1", dto.Source);
            Assert.True(dto.IsPrimary);
        }

        [Fact]
        public async Task GetPersonAddress_ReturnsNotFound_WhenNoAddressesMatch()
        {
            // Arrange
            _personAddressRepo
                .Setup(r => r.FindOrderedAsync(
                    It.IsAny<Expression<Func<PersonAddressModel, bool>>>(),
                    It.IsAny<Expression<Func<PersonAddressModel, DateTime>>>(),
                    true, false))
                .ReturnsAsync(new List<PersonAddressModel>());

            // Act
            var result = await _personAddressController.GetPersonAddress(789, 999, true);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            var response = Assert.IsType<GetAllPersonAddressesResponseDto>(objectResult.Value);
            Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
            Assert.Equal("No addresses found for the given criteria.", response.ErrorMessage);
        }

        [Fact]
        public async Task GetPersonAddress_ReturnsServerError_OnException()
        {
            // Arrange
            _personAddressRepo
                .Setup(r => r.FindOrderedAsync(
                    It.IsAny<Expression<Func<PersonAddressModel, bool>>>(),
                    It.IsAny<Expression<Func<PersonAddressModel, DateTime>>>(),
                    true, false))
                .ThrowsAsync(new Exception("Simulated failure"));

            // Act
            var result = await _personAddressController.GetPersonAddress(999, null, null);

            // Assert
            var objectResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        [Fact]
        public async Task CreatePersonAddress_ReturnsOk_WhenCreatedSuccessfully()
        {
            // Arrange
            var request = new CreatePersonAddressRequestDto
            {
                AddressTypeId = 1,
                PersonId = 123,
                AddressLabel = "Home",
                Line1 = "123 Main St",
                Line2 = "Apt 4",
                City = "TestCity",
                State = "TestState",
                PostalCode = "12345",
                Region = "TestRegion",
                CountryCode = "US",
                Country = "USA",
                Source = "test_source",
                CreateUser = "test_user"
            };

            var model = new PersonAddressModel
            {
                PersonAddressId = 1,
                AddressTypeId = 1,
                PersonId = 123,
                AddressLabel = "Home",
                Line1 = "123 Main St",
                Line2 = "Apt 4",
                City = "TestCity",
                State = "TestState",
                PostalCode = "12345",
                Region = "TestRegion",
                CountryCode = "US",
                Country = "USA",
                Source = "test_source",
                IsPrimary = false,
                CreateUser = "test_user",
                UpdateUser = "test_user",
                CreateTs = DateTime.UtcNow,
                UpdateTs = DateTime.UtcNow,
                DeleteNbr = 0
            };

            _personAddressRepo
                .Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<PersonAddressModel, bool>>>(), false))
                .ReturnsAsync((PersonAddressModel?)null);

            _addressTypeService
                .Setup(s => s.GetAddressTypeById(1))
                .ReturnsAsync(new GetAddressTypeResponseDto());

            _personService
                .Setup(s => s.GetPersonData(123))
                .ReturnsAsync(new SunnyRewards.Helios.User.Core.Domain.Dtos.PersonDto { PersonId = 123 });

            _mapper.Setup(m => m.Map<PersonAddressModel>(request))
                .Returns(model);

            _mapper.Setup(m => m.Map<PersonAddressDto>(model))
                .Returns(new PersonAddressDto
                {
                    PersonAddressId = 1,
                    AddressTypeId = 1,
                    PersonId = 123,
                    AddressLabel = "Home",
                    Line1 = "123 Main St",
                    Line2 = "Apt 4",
                    City = "TestCity",
                    State = "TestState",
                    PostalCode = "12345",
                    Region = "TestRegion",
                    CountryCode = "US",
                    Country = "USA",
                    Source = "test_source",
                    IsPrimary = false,
                    CreateUser = "test_user",
                    UpdateUser = "test_user",
                    CreateTs = model.CreateTs,
                    UpdateTs = model.UpdateTs,
                    DeleteNbr = 0
                });

            // Act
            var result = await _personAddressController.CreatePersonAddress(request);

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, objectResult.StatusCode);

            var response = Assert.IsType<PersonAddressResponseDto>(objectResult.Value);
            Assert.Equal(1, response.PersonAddress.PersonAddressId);
            Assert.Equal("Home", response.PersonAddress.AddressLabel);
            Assert.Equal("123 Main St", response.PersonAddress.Line1);
        }

        [Fact]
        public async Task CreatePersonAddress_ReturnsConflict_WhenAddressAlreadyExists()
        {
            // Arrange
            var request = new CreatePersonAddressRequestDto
            {
                AddressTypeId = 1,
                PersonId = 123,
                Line1 = "123 Main St",
                State = "TestState",
                CountryCode = "US",
                PostalCode = "12345",
                City = "TestCity",
                Region = "TestRegion",
                Country = "USA",
                Source = "test_source",
                CreateUser = "test_user"
            };

            _personAddressRepo
                .Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<PersonAddressModel, bool>>>(), false))
                .ReturnsAsync(new PersonAddressModel());

            // Act
            var result = await _personAddressController.CreatePersonAddress(request);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            var response = Assert.IsType<PersonAddressResponseDto>(objectResult.Value);
            Assert.Equal(StatusCodes.Status409Conflict, response.ErrorCode);
        }

        [Fact]
        public async Task CreatePersonAddress_ReturnsBadRequest_WhenAddressTypeNotFound()
        {
            // Arrange
            var request = new CreatePersonAddressRequestDto
            {
                AddressTypeId = 999,
                PersonId = 123,
                Line1 = "123 Main St",
                State = "TestState",
                CountryCode = "US",
                PostalCode = "12345",
                City = "TestCity",
                Region = "TestRegion",
                Country = "USA",
                Source = "test_source",
                CreateUser = "test_user"
            };

            _personAddressRepo
                .Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<PersonAddressModel, bool>>>(), false))
                .ReturnsAsync((PersonAddressModel?)null);

            _addressTypeService
                .Setup(s => s.GetAddressTypeById(999))
                .ReturnsAsync(new GetAddressTypeResponseDto
                {
                    ErrorCode = StatusCodes.Status404NotFound
                });

            // Act
            var result = await _personAddressController.CreatePersonAddress(request);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            var response = Assert.IsType<PersonAddressResponseDto>(objectResult.Value);
            Assert.Equal(StatusCodes.Status400BadRequest, response.ErrorCode);
        }

        [Fact]
        public async Task CreatePersonAddress_ReturnsBadRequest_WhenPersonDoesNotExist()
        {
            // Arrange
            var request = new CreatePersonAddressRequestDto
            {
                AddressTypeId = 1,
                PersonId = 123,
                Line1 = "123 Main St",
                State = "TestState",
                CountryCode = "US",
                PostalCode = "12345",
                City = "TestCity",
                Region = "TestRegion",
                Country = "USA",
                Source = "test_source",
                CreateUser = "test_user"
            };

            _personAddressRepo
                .Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<PersonAddressModel, bool>>>(), false))
                .ReturnsAsync((PersonAddressModel?)null);

            _addressTypeService
                .Setup(s => s.GetAddressTypeById(1))
                .ReturnsAsync(new GetAddressTypeResponseDto());

            _personService
                .Setup(s => s.GetPersonData(123))
                .ReturnsAsync(new SunnyRewards.Helios.User.Core.Domain.Dtos.PersonDto { PersonId = 0 });

            // Act
            var result = await _personAddressController.CreatePersonAddress(request);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            var response = Assert.IsType<PersonAddressResponseDto>(objectResult.Value);
            Assert.Equal(StatusCodes.Status400BadRequest, response.ErrorCode);
        }

        [Fact]
        public async Task CreatePersonAddress_ReturnsServerError_OnException()
        {
            // Arrange
            var request = new CreatePersonAddressRequestDto
            {
                AddressTypeId = 1,
                PersonId = 123,
                Line1 = "123 Main St",
                State = "TestState",
                CountryCode = "US",
                PostalCode = "12345",
                City = "TestCity",
                Region = "TestRegion",
                Country = "USA",
                Source = "test_source",
                CreateUser = "test_user"
            };

            _personAddressRepo
                .Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<PersonAddressModel, bool>>>(), false))
                .ThrowsAsync(new Exception("Simulated exception"));

            // Act
            var result = await _personAddressController.CreatePersonAddress(request);

            // Assert
            var objectResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        [Fact]
        public async Task UpdatePersonAddress_ReturnsOk_WhenUpdatedSuccessfully()
        {
            // Arrange
            var updateRequest = new UpdatePersonAddressRequestDto
            {
                PersonAddressId = 1,
                AddressTypeId = 100,
                AddressLabel = "Updated Label",
                Line1 = "Updated Line1",
                Line2 = "Updated Line2",
                City = "Updated City",
                State = "Updated State",
                PostalCode = "98765",
                Region = "Updated Region",
                CountryCode = "IN",
                Country = "India",
                Source = "updated_source",
                UpdateUser = "updater"
            };

            var existingModel = new PersonAddressModel
            {
                PersonAddressId = 1,
                AddressTypeId = 100,
                Line1 = "Old Line1",
                State = "Old State",
                CountryCode = "IN",
                PostalCode = "98765",
                DeleteNbr = 0
            };

            _personAddressRepo
                .Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<PersonAddressModel, bool>>>(), false))
                .ReturnsAsync(existingModel);

            _addressTypeService
                .Setup(s => s.GetAddressTypeById(updateRequest.AddressTypeId))
                .ReturnsAsync(new GetAddressTypeResponseDto());

            _mapper
                .Setup(m => m.Map<PersonAddressDto>(It.IsAny<PersonAddressModel>()))
                .Returns(new PersonAddressDto
                {
                    PersonAddressId = 1,
                    AddressTypeId = 100,
                    AddressLabel = "Updated Label",
                    Line1 = "Updated Line1",
                    Line2 = "Updated Line2",
                    City = "Updated City",
                    State = "Updated State",
                    PostalCode = "98765",
                    Region = "Updated Region",
                    CountryCode = "IN",
                    Country = "India",
                    Source = "updated_source",
                    IsPrimary = false,
                    CreateUser = "test_user",
                    UpdateUser = "updater",
                    CreateTs = DateTime.UtcNow.AddDays(-1),
                    UpdateTs = DateTime.UtcNow,
                    DeleteNbr = 0,
                    PersonId = 123
                });

            // Act
            var result = await _personAddressController.UpdatePersonAddress(updateRequest);

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, objectResult.StatusCode);

            var response = Assert.IsType<PersonAddressResponseDto>(objectResult.Value);
            Assert.Equal(1, response.PersonAddress.PersonAddressId);
            Assert.Equal("Updated Label", response.PersonAddress.AddressLabel);
            Assert.Equal("Updated Line1", response.PersonAddress.Line1);
            Assert.Equal("India", response.PersonAddress.Country);
        }

        [Fact]
        public async Task UpdatePersonAddress_ReturnsNotFound_WhenAddressDoesNotExist()
        {
            // Arrange
            var updateRequest = new UpdatePersonAddressRequestDto
            {
                PersonAddressId = 999,
                AddressTypeId = 100,
                UpdateUser = "updater"
            };

            _personAddressRepo
                .Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<PersonAddressModel, bool>>>(), false))
                .ReturnsAsync((PersonAddressModel?)null);

            // Act
            var result = await _personAddressController.UpdatePersonAddress(updateRequest);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            var response = Assert.IsType<PersonAddressResponseDto>(objectResult.Value);
            Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
        }

        [Fact]
        public async Task UpdatePersonAddress_ReturnsBadRequest_WhenAddressTypeIsInvalid()
        {
            // Arrange
            var updateRequest = new UpdatePersonAddressRequestDto
            {
                PersonAddressId = 1,
                AddressTypeId = 555,
                UpdateUser = "updater"
            };

            _personAddressRepo
                .Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<PersonAddressModel, bool>>>(), false))
                .ReturnsAsync(new PersonAddressModel());

            _addressTypeService
                .Setup(s => s.GetAddressTypeById(updateRequest.AddressTypeId))
                .ReturnsAsync(new GetAddressTypeResponseDto
                {
                    ErrorCode = StatusCodes.Status404NotFound
                });

            // Act
            var result = await _personAddressController.UpdatePersonAddress(updateRequest);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            var response = Assert.IsType<PersonAddressResponseDto>(objectResult.Value);
            Assert.Equal(StatusCodes.Status400BadRequest, response.ErrorCode);
        }

        [Fact]
        public async Task UpdatePersonAddress_ReturnsServerError_OnException()
        {
            // Arrange
            var updateRequest = new UpdatePersonAddressRequestDto
            {
                PersonAddressId = 1,
                AddressTypeId = 100,
                UpdateUser = "updater"
            };

            _personAddressRepo
                .Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<PersonAddressModel, bool>>>(), false))
                .ThrowsAsync(new Exception("Simulated failure"));

            // Act
            var result = await _personAddressController.UpdatePersonAddress(updateRequest);

            // Assert
            var objectResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        [Fact]
        public async Task DeletePersonAddress_ReturnsOk_WhenDeletedSuccessfully()
        {
            // Arrange
            var request = new DeletePersonAddressRequestDto
            {
                PersonAddressId = 1,
                UpdateUser = "deleter"
            };

            var mockModel = new PersonAddressModel
            {
                PersonAddressId = 1,
                AddressTypeId = 200,
                PersonId = 123,
                AddressLabel = "Office",
                Line1 = "123 St",
                City = "TestCity",
                State = "TestState",
                PostalCode = "12345",
                Region = "TestRegion",
                CountryCode = "US",
                Country = "USA",
                Source = "manual",
                IsPrimary = false,
                DeleteNbr = 0,
                CreateUser = "creator",
                UpdateUser = "updater",
                CreateTs = DateTime.UtcNow.AddDays(-10),
                UpdateTs = DateTime.UtcNow
            };

            var mockDto = new PersonAddressDto
            {
                PersonAddressId = 1,
                AddressTypeId = 200,
                PersonId = 123,
                AddressLabel = "Office",
                Line1 = "123 St",
                City = "TestCity",
                State = "TestState",
                PostalCode = "12345",
                Region = "TestRegion",
                CountryCode = "US",
                Country = "USA",
                Source = "manual",
                IsPrimary = false,
                DeleteNbr = 1,
                CreateUser = "creator",
                UpdateUser = "deleter",
                CreateTs = mockModel.CreateTs,
                UpdateTs = mockModel.UpdateTs
            };

            _personAddressRepo
                .Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<PersonAddressModel, bool>>>(), false))
                .ReturnsAsync(mockModel);

            _mapper
                .Setup(m => m.Map<PersonAddressDto>(mockModel))
                .Returns(mockDto);

            // Act
            var result = await _personAddressController.DeletePersonAddress(request);

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, objectResult.StatusCode);

            var response = Assert.IsType<PersonAddressResponseDto>(objectResult.Value);
            Assert.Equal(1, response.PersonAddress.PersonAddressId);
            Assert.Equal(1, response.PersonAddress.DeleteNbr);
            Assert.Equal("deleter", response.PersonAddress.UpdateUser);
        }

        [Fact]
        public async Task DeletePersonAddress_ReturnsNotFound_WhenAddressDoesNotExist()
        {
            // Arrange
            var request = new DeletePersonAddressRequestDto
            {
                PersonAddressId = 999,
                UpdateUser = "tester"
            };

            _personAddressRepo
                .Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<PersonAddressModel, bool>>>(), false))
                .ReturnsAsync((PersonAddressModel?)null);

            // Act
            var result = await _personAddressController.DeletePersonAddress(request);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            var response = Assert.IsType<PersonAddressResponseDto>(objectResult.Value);
            Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
        }

        [Fact]
        public async Task DeletePersonAddress_ReturnsServerError_OnException()
        {
            // Arrange
            var request = new DeletePersonAddressRequestDto
            {
                PersonAddressId = 5,
                UpdateUser = "tester"
            };

            _personAddressRepo
                .Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<PersonAddressModel, bool>>>(), false))
                .ThrowsAsync(new Exception("Simulated DB crash"));

            // Act
            var result = await _personAddressController.DeletePersonAddress(request);

            // Assert
            var objectResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        [Fact]
        public async Task SetPrimaryAddress_ReturnsOk_WhenSuccessfullySet()
        {
            // Arrange
            var request = new UpdatePrimaryPersonAddressRequestDto
            {
                PersonAddressId = 1001,
                UpdateUser = "admin_user"
            };

            var oldPrimary = new PersonAddressModel
            {
                PersonAddressId = 1,
                PersonId = 123,
                IsPrimary = true,
                DeleteNbr = 0,
                UpdateUser = "prev_user",
                UpdateTs = DateTime.UtcNow.AddDays(-1)
            };

            var newPrimary = new PersonAddressModel
            {
                PersonAddressId = 1001,
                PersonId = 123,
                IsPrimary = false,
                DeleteNbr = 0,
                UpdateUser = "old_user",
                UpdateTs = DateTime.UtcNow.AddDays(-2)
            };

            var expectedDto = new PersonAddressDto
            {
                PersonAddressId = 1001,
                PersonId = 123,
                IsPrimary = true,
                UpdateUser = "admin_user",
                UpdateTs = DateTime.UtcNow
            };

            _personAddressRepo
                .Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<PersonAddressModel, bool>>>(), false))
                .Returns<Expression<Func<PersonAddressModel, bool>>, bool>((predicate, tracking) =>
                {
                    if (predicate.Compile()(newPrimary)) return Task.FromResult(newPrimary);
                    if (predicate.Compile()(oldPrimary)) return Task.FromResult(oldPrimary);
                    return Task.FromResult<PersonAddressModel?>(null);
                });


            _mapper
                .Setup(m => m.Map<PersonAddressDto>(It.Is<PersonAddressModel>(a => a.PersonAddressId == 1001)))
                .Returns(expectedDto);

            // Act
            var result = await _personAddressController.SetPrimaryAddress(request);

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, objectResult.StatusCode);

            var response = Assert.IsType<PersonAddressResponseDto>(objectResult.Value);
            Assert.True(response.PersonAddress.IsPrimary);
            Assert.Equal("admin_user", response.PersonAddress.UpdateUser);
        }

        [Fact]
        public async Task SetPrimaryAddress_ReturnsNotFound_WhenAddressNotFound()
        {
            // Arrange
            var request = new UpdatePrimaryPersonAddressRequestDto
            {
                PersonAddressId = 9999,
                UpdateUser = "tester"
            };

            _personAddressRepo
                .Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<PersonAddressModel, bool>>>(), false))
                .ReturnsAsync((PersonAddressModel?)null);

            // Act
            var result = await _personAddressController.SetPrimaryAddress(request);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            var response = Assert.IsType<PersonAddressResponseDto>(objectResult.Value);
            Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
        }

        [Fact]
        public async Task SetPrimaryAddress_ReturnsServerError_OnException()
        {
            // Arrange
            var request = new UpdatePrimaryPersonAddressRequestDto
            {
                PersonAddressId = 5,
                UpdateUser = "tester"
            };

            _personAddressRepo
                .Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<PersonAddressModel, bool>>>(), false))
                .ThrowsAsync(new Exception("Simulated crash"));

            // Act
            var result = await _personAddressController.SetPrimaryAddress(request);

            // Assert
            var objectResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

    }
}

