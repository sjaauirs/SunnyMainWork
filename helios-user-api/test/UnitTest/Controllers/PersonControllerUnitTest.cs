using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using SunnyRewards.Helios.Common.Core.Repositories;
using SunnyRewards.Helios.User.Api.Controllers;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.enums;
using SunnyRewards.Helios.User.Core.Domain.Models;
using SunnyRewards.Helios.User.Infrastructure.Mappings;
using SunnyRewards.Helios.User.Infrastructure.Repositories;
using SunnyRewards.Helios.User.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.User.Infrastructure.Services;
using SunnyRewards.Helios.User.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.User.UnitTest.Fixtures.MockRepositories;
using System;
using System.Linq.Expressions;
using Xunit;

namespace SunnyRewards.Helios.User.UnitTest.Controllers
{
    public class PersonControllerUnitTest
    {
        private readonly Mock<ILogger<PersonController>> _personControllerLogger;
        private readonly Mock<ILogger<PersonService>> _personServiceLogger;
        private readonly IMapper _mapper;
        private readonly Mock<IConsumerRepo> _consumerRepo;
        private readonly Mock<IPersonRepo> _personRepo;
        private readonly IPersonService _personService;
        private readonly Mock<IConsumerService> _consumerService;
        private readonly PersonController _personController;
        public PersonControllerUnitTest()
        {
            _personControllerLogger = new Mock<ILogger<PersonController>>();
            _personServiceLogger = new Mock<ILogger<PersonService>>();
            _consumerService= new Mock<IConsumerService>();
            _mapper = new Mapper(new MapperConfiguration(
                configure =>
                {
                    configure.AddMaps(typeof(Infrastructure.Mappings.MappingProfile.PersonMapping).Assembly.FullName);
                }));
            _personRepo = new PersonMockRepo();
            _consumerRepo = new Mock<IConsumerRepo>();
            _personService = new PersonService(_personServiceLogger.Object, _mapper, _personRepo.Object , _consumerRepo.Object, _consumerService.Object);
            _personController = new PersonController(_personControllerLogger.Object, _personService);
        }

        [Fact]
        public async Task Should_Get_Person()
        {
            var personMap = new PersonMap();
            var personRoleMap = new PersonRoleMap();
            var person = await _personController.GetPerson(1);
            var result = person.Result as OkObjectResult;
            Assert.True(result?.Value != null);
        }

        [Fact]
        public async Task Should_Not_Get_Person_If_Record_NotFound()
        {
            var repo = new Mock<IPersonRepo>();
            repo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<PersonModel, bool>>>(), false)).ReturnsAsync(new PersonModel());
            var personService = new PersonService(_personServiceLogger.Object, _mapper, repo.Object, _consumerRepo.Object, _consumerService.Object);
            var personController = new PersonController(_personControllerLogger.Object, personService);
            var person = await personController.GetPerson(1);
            var result = person.Result as BadRequestObjectResult;
            Assert.True(result?.Value == null);
        }

        [Fact]
        public async Task Should_Catch_GetPerson_Controller_Level_Exception()
        {
            var mockService = new Mock<IPersonService>();
            mockService.Setup(s => s.GetPersonData(It.IsAny<long>())).ThrowsAsync(new Exception());
            var controller = new PersonController(_personControllerLogger.Object, mockService.Object);
            var result = await controller.GetPerson(1);
            Assert.True(result?.Value?.PersonId <= 0);
        }

        [Fact]
        public async Task Should_Catch_GetPersonData_Service_Level_Exception()
        {
            _personRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<PersonModel, bool>>>(), false)).ThrowsAsync(new Exception("intended exception"));
            var result = await _personService.GetPersonData(1);
            Assert.True(result.PersonId <= 0);
        }

        [Fact]
        public void PersonRoleRepo_Constructor_ShouldInstantiate_WhenDependenciesAreProvided()
        {
            var mockLogger = new Mock<ILogger<BaseRepo<PersonRoleModel>>>();
            var mockSession = new Mock<NHibernate.ISession>();
            var repo = new Infrastructure.Repositories.PersonRoleRepo(mockLogger.Object, mockSession.Object);
            Assert.NotNull(repo);
        }

        [Fact]
        public async Task GetPersonAndConsumerDetails_ReturnsUnprocessableEntity_WhenConsumerIsnotNull()
        {
            // Arrange
            var consumerRequestDto = new GetConsumerRequestDto { ConsumerCode = "testConsumerCode" };
            var expectedResponse = new GetPersonAndConsumerResponseDto { Consumer = new ConsumerDto(), Person = new PersonDto() };
            var mockService = new Mock<IPersonService>();

            mockService.Setup(service => service.GetOverAllConsumerDetails(consumerRequestDto))
                              .ReturnsAsync(expectedResponse);

            // Act
            var result = await _personController.GetPersonAndConsumerDetails(consumerRequestDto);

            // Assert
            var objectResult = result.Result as ObjectResult;
            Assert.NotNull(objectResult);
        }
        [Fact]
        public async Task GetOverAllConsumerDetails_ReturnsValidResponse()
        {
            // Arrange
            var consumerRequestDto = new GetConsumerRequestDto { ConsumerCode = "testConsumerCode" };
            var expectedConsumerRecord = new GetConsumerResponseDto
            {
                Consumer = new ConsumerDto { PersonId = 123 }
            };
            var expectedPersonDto = new PersonDto { PersonId = 123 };

            _consumerService.Setup(service => service.GetConsumerData(consumerRequestDto))
                                .ReturnsAsync(expectedConsumerRecord);
            _personRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<PersonModel, bool>>>(), false))
                           .ReturnsAsync(new PersonModel { PersonId = 123 });
            Mock<IMapper> mapper=new Mock<IMapper>();
            mapper.Setup(mapper => mapper.Map<PersonDto>(It.IsAny<PersonModel>()))
                       .Returns(expectedPersonDto);

            // Act
            var result = await _personService.GetOverAllConsumerDetails(consumerRequestDto);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Consumer);
            
        }


        [Fact]
        public async Task GetOverAllConsumerDetails_Should_Return_Error_Result_When_Service_Throws_Exception()
        {

            // Arrange
            var consumerRequestDto = new GetConsumerRequestDto { ConsumerCode = "testConsumerCode" };
            var expectedResponse = new GetPersonAndConsumerResponseDto { Consumer = new ConsumerDto(), Person = new PersonDto() };
            var expectedPersonDto = new PersonDto { PersonId = 123 };

            _consumerService.Setup(service => service.GetConsumerData(consumerRequestDto))
                                .ThrowsAsync(new Exception("Testing"));
            _personRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<PersonModel, bool>>>(), false))
                           .ThrowsAsync(new Exception("Testing"));
            Mock<IMapper> mapper = new Mock<IMapper>();
            mapper.Setup(mapper => mapper.Map<PersonDto>(It.IsAny<PersonModel>()))
                       .Returns(expectedPersonDto);
            //var mockService = new Mock<IPersonService>();

            //mockService.Setup(service => service.GetOverAllConsumerDetails(consumerRequestDto))
            //                  .ThrowsAsync(new Exception("Testing"));

            // Act
            var result = await _personController.GetPersonAndConsumerDetails(consumerRequestDto);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ObjectResult>(result.Result);
            var objectResult = result.Result as ObjectResult;
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult?.StatusCode);
        }

        [Fact]
        public async Task UpdatePersonData_ReturnsOk_WhenUpdatedSuccessfully()
        {
            // Arrange
            var updateRequest = new UpdatePersonRequestDto
            {
                ConsumerCode = "ABC123",
                PhoneNumber = "1234567890",
                UpdateUser = "updater"
            };

            var consumer = new ConsumerModel { ConsumerCode = "ABC123", PersonId = 10, DeleteNbr = 0 };
            var person = new PersonModel { PersonId = 10, PhoneNumber = "0000000000", DeleteNbr = 0 };

            _consumerRepo.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false))
                .ReturnsAsync(consumer);
            _personRepo.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<PersonModel, bool>>>(), false))
                .ReturnsAsync(person);
            Mock<IMapper> mapper = new Mock<IMapper>();
            mapper.Setup(mapper => mapper.Map<PersonDto>(It.IsAny<PersonModel>()))
                       .Returns(new PersonDto { PersonId = 10, PhoneNumber = "1234567890" });

            // Act
            var result = await _personController.UpdatePersonData(updateRequest);

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status200OK, objectResult.StatusCode);

            var response = Assert.IsType<PersonResponseDto>(objectResult.Value);
            Assert.Equal("1234567890", response.Person.PhoneNumber);
        }

        [Fact]
        public async Task UpdatePersonData_ReturnsNotFound_WhenConsumerDoesNotExist()
        {
            // Arrange
            var updateRequest = new UpdatePersonRequestDto
            {
                ConsumerCode = "NOTFOUND",
                UpdateUser = "updater"
            };

            _consumerRepo.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false))
                .ReturnsAsync((ConsumerModel?)null);

            // Act
            var result = await _personController.UpdatePersonData(updateRequest);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            var response = Assert.IsType<PersonResponseDto>(objectResult.Value);
            Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
        }

        [Fact]
        public async Task UpdatePersonData_ReturnsNotFound_WhenPersonDoesNotExist()
        {
            // Arrange
            var updateRequest = new UpdatePersonRequestDto
            {
                ConsumerCode = "ABC123",
                UpdateUser = "updater"
            };

            var consumer = new ConsumerModel { ConsumerCode = "ABC123", PersonId = 10, DeleteNbr = 0 };

            _consumerRepo.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false))
                .ReturnsAsync(consumer);
            _personRepo.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<PersonModel, bool>>>(), false))
                .ReturnsAsync((PersonModel?)null);

            // Act
            var result = await _personController.UpdatePersonData(updateRequest);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            var response = Assert.IsType<PersonResponseDto>(objectResult.Value);
            Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
        }

        [Fact]
        public async Task UpdatePersonData_ReturnsServerError_OnException()
        {
            // Arrange
            var updateRequest = new UpdatePersonRequestDto
            {
                ConsumerCode = "ABC123",
                UpdateUser = "updater"
            };

            _consumerRepo.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false))
                .ThrowsAsync(new Exception("Simulated failure"));

            // Act
            var result = await _personController.UpdatePersonData(updateRequest);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            var response = Assert.IsType<PersonResponseDto>(objectResult.Value);
            Assert.Equal(StatusCodes.Status500InternalServerError, response.ErrorCode);
        }

    }
}