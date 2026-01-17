using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using AutoMapper;
using SunnyRewards.Helios.User.Api.Controllers;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Models;
using SunnyRewards.Helios.User.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.User.Infrastructure.Services;
using SunnyRewards.Helios.User.Infrastructure.Repositories;
using System.Linq.Expressions;
using NSubstitute;

namespace SunnyRewards.Helios.User.UnitTest.Controllers
{
    public class ConsumerHistoryControllerIntegrationTests
    {
        private readonly Mock<IConsumerHistoryRepo> _consumerHistoryRepoMock;
        private readonly Mock<ILogger<ConsumerHistoryService>> _serviceLoggerMock;
        private readonly Mock<ILogger<ConsumerHistoryController>> _controllerLoggerMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly ConsumerHistoryService _consumerHistoryService;
        private readonly ConsumerHistoryController _controller;

        public ConsumerHistoryControllerIntegrationTests()
        {
            _consumerHistoryRepoMock = new Mock<IConsumerHistoryRepo>();
            _serviceLoggerMock = new Mock<ILogger<ConsumerHistoryService>>();
            _controllerLoggerMock = new Mock<ILogger<ConsumerHistoryController>>();
            _mapperMock = new Mock<IMapper>();

            _consumerHistoryService = new ConsumerHistoryService(
                _serviceLoggerMock.Object,
                _mapperMock.Object,
                _consumerHistoryRepoMock.Object
            );

            _controller = new ConsumerHistoryController(_controllerLoggerMock.Object, _consumerHistoryService);
        }

        [Fact]
        public async Task InsertConsumerHistory_ReturnsOk_WhenValidConsumersProvided()
        {
            // Arrange
            var consumers = new List<ConsumerDto>
            {
                new ConsumerDto { ConsumerCode = "C123", TenantCode = "T1", CreateUser = "test_user" }
            };

            var model = new ConsumerHistoryModel
            {
                ConsumerCode = "C123",
                TenantCode = "T1",
                CreateUser = "test_user"
            };

            var model2 = new ConsumerHistoryModel
            {
                ConsumerHistoryId = 1,
                ConsumerCode = "C123",
                TenantCode = "T1",
                CreateUser = "test_user"
            };

            _mapperMock.Setup(m => m.Map<ConsumerHistoryModel>(It.IsAny<ConsumerDto>()))
                       .Returns(model);

            _consumerHistoryRepoMock
                .Setup(r => r.CreateAsync(It.IsAny<ConsumerHistoryModel>()))
                .ReturnsAsync(model2);

            // Act
            var result = await _controller.InsertConsumerHistory(consumers);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<BaseResponseDto>(okResult.Value);
            Assert.Null(response.ErrorCode);
            Assert.Null(response.ErrorMessage);
        }

        [Fact]
        public async Task InsertConsumerHistory_ReturnsOk_AndMapsAndSavesCorrectly()
        {
            // Arrange
            var consumers = new List<ConsumerDto>
    {
        new ConsumerDto
        {
            ConsumerCode = "C123",
            TenantCode = "T1",
            CreateUser = "test_user",
            UpdateUser = null // force fallback logic to test CreateUser logic
        }
    };

            var model = new ConsumerHistoryModel
            {
                ConsumerCode = "C123",
                TenantCode = "T1",
                CreateUser = "test_user"
            };
            _mapperMock.Setup(m => m.Map<ConsumerHistoryModel>(It.IsAny<ConsumerDto>()))
                .Returns(model);

            _consumerHistoryRepoMock
               .Setup(r => r.CreateAsync(It.IsAny<ConsumerHistoryModel>()))
               .ReturnsAsync(model);

            // Act
            var result = await _controller.InsertConsumerHistory(consumers);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<BaseResponseDto>(okResult.Value);
            Assert.Null(response.ErrorCode);
            Assert.Null(response.ErrorMessage);

            Assert.NotNull(model);
            Assert.Equal("C123", model.ConsumerCode);
            Assert.Equal("T1", model.TenantCode);
            Assert.Equal("test_user", model.CreateUser);
        }


        [Fact]
        public async Task InsertConsumerHistory_ReturnsError_WhenConsumersNull()
        {
            // Act
            var result = await _controller.InsertConsumerHistory(null);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<BaseResponseDto>(okResult.Value);
            Assert.Null(response.ErrorCode);
            Assert.Equal("No consumers to insert into history.", response.ErrorMessage);
        }

        [Fact]
        public async Task InsertConsumerHistory_Returns500_WhenExceptionThrown()
        {
            // Arrange
            var consumers = new List<ConsumerDto>
            {
                new ConsumerDto { ConsumerCode = "C123", TenantCode = "T1" , CreateTs = DateTime.Now, CreateUser = "abc" }
            };
            var model = new ConsumerHistoryModel
            {
                ConsumerCode = "C123",
                TenantCode = "T1",
                CreateUser = "test_user",
                CreateTs = DateTime.UtcNow
            };

            _mapperMock.Setup(m => m.Map<ConsumerHistoryModel>(It.IsAny<ConsumerDto>()))
                       .Returns(model);

            _consumerHistoryRepoMock
                .Setup(r => r.CreateAsync(It.IsAny<ConsumerHistoryModel>()))
                       .ThrowsAsync(new Exception("Mapping failed"));

            // Act
            var result = await _controller.InsertConsumerHistory(consumers);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            var response = Assert.IsType<BaseResponseDto>(objectResult.Value);
            Assert.Equal(500, response.ErrorCode);
            Assert.Equal("Mapping failed", response.ErrorMessage);
        }

        [Fact]
        public async Task InsertConsumerHistory_ReturnsError_WhenEmptyListProvided()
        {
            // Act
            var result = await _controller.InsertConsumerHistory(new List<ConsumerDto>());

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<BaseResponseDto>(okResult.Value);
            Assert.Null(response.ErrorCode);
            Assert.Equal("No consumers to insert into history.", response.ErrorMessage);
        }


        [Fact]
        public async Task InsertConsumerHistory_HandlesMultipleConsumersSuccessfully()
        {
            // Arrange
            var consumers = new List<ConsumerDto>
    {
        new ConsumerDto { ConsumerCode = "C001", TenantCode = "T1", CreateUser = "U1" },
        new ConsumerDto { ConsumerCode = "C002", TenantCode = "T2", CreateUser = "U2" }
    };

            _mapperMock.Setup(m => m.Map<ConsumerHistoryModel>(It.IsAny<ConsumerDto>()))
                .Returns((ConsumerDto dto) => new ConsumerHistoryModel
                {
                    ConsumerCode = dto.ConsumerCode,
                    TenantCode = dto.TenantCode,
                    CreateUser = dto.CreateUser,
                    CreateTs = DateTime.UtcNow
                });
            var model = new ConsumerHistoryModel
            {
                ConsumerCode = "C123",
                TenantCode = "T1",
                CreateUser = "test_user",
                CreateTs = DateTime.UtcNow,
                AgreementFileName = "df",
                AgreementStatus = "sda",
                AnonymousCode = "sdas"  , Auth0UserName = "sdsa", ConsumerAttribute = "s" , ConsumerHistoryId = 1 , ConsumerId = 1 , DeleteNbr =0 , Eligible = false,
                EligibleEndTs = DateTime.UtcNow , EligibleStartTs = DateTime.UtcNow , EnrollmentStatus = "s" , EnrollmentStatusSource = "d" , Id = 1 , IsSSOUser = true , MemberId = "d",
                MemberNbr ="nbr" , MemberNbrPrefix = "su" , MemberType = "SU" , OnBoardingState = "a" , Person = null, PersonId = 1 , PlanId = "1", PlanType = "c" , RegionCode = "I" , Registered = false , RegistrationTs = DateTime.UtcNow, 
                SubgroupId = "2" , SubsciberMemberNbrPrefix = "s", SubscriberMemberNbr = "" , UpdateTs = DateTime.UtcNow , UpdateUser = "sa"
            

            };
            _consumerHistoryRepoMock
               .Setup(r => r.CreateAsync(It.IsAny<ConsumerHistoryModel>()))
               .ReturnsAsync(model);
            // Act
            var result = await _controller.InsertConsumerHistory(consumers);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<BaseResponseDto>(okResult.Value);
            Assert.Null(response.ErrorCode);
            Assert.Null(response.ErrorMessage);

            // Verify both inserts were called
            _consumerHistoryRepoMock.Verify(r => r.CreateAsync(It.IsAny<ConsumerHistoryModel>()), Times.Exactly(2));
        }

    }
}
