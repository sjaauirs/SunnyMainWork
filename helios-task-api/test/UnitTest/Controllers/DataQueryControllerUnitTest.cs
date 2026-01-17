using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json.Linq;
using NHibernate;
using NHibernate.Transform;
using SunnyRewards.Helios.Task.Api.Controller;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Infrastructure.Helpers;
using SunnyRewards.Helios.Task.Infrastructure.Helpers.Interface;
using SunnyRewards.Helios.Task.Infrastructure.Services;
using SunnyRewards.Helios.Task.Infrastructure.Services.Helpers;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace SunnyRewards.Helios.Task.UnitTest.Controllers
{
    public class DataQueryControllerUnitTest
    {
        private readonly Mock<ISessionFactory> _sessionFactoryMock;
        private readonly Mock<ISession> _sessionMock;
        private readonly QueryGeneratorService _dynamicQueryService;
        private readonly IRowToDtoMapper _rowToDtoMapper;
        private readonly Mock<ILogger<DataQueryService>> _serviceLoggerMock;
        private readonly Mock<ILogger<DataQueryController>> _controllerLoggerMock;
        private readonly IMappingProfileProvider _mappingProfileProvider;
        private readonly DataQueryService _service;
        private readonly DataQueryController _controller;

        public DataQueryControllerUnitTest()
        {
            // NHibernate session mock
            _sessionFactoryMock = new Mock<ISessionFactory>();
            _sessionMock = new Mock<ISession>();
            _sessionFactoryMock.Setup(f => f.OpenSession()).Returns(_sessionMock.Object);

            // Loggers
            _serviceLoggerMock = new Mock<ILogger<DataQueryService>>();
            _controllerLoggerMock = new Mock<ILogger<DataQueryController>>();

            // Mapping
            _mappingProfileProvider = new MappingProfileProvider(new Mock<ILogger<MappingProfileProvider>>().Object);
            _rowToDtoMapper = new RowToDtoMapper(_mappingProfileProvider);

            // Real QueryGeneratorService
            _dynamicQueryService = new QueryGeneratorService(new Mock<ILogger<QueryGeneratorService>>().Object);

            // DataQueryService
            _service = new DataQueryService(
                _sessionFactoryMock.Object,
                _serviceLoggerMock.Object,
                _rowToDtoMapper,
                _dynamicQueryService);

            // Controller
            _controller = new DataQueryController(_controllerLoggerMock.Object, _service);
        }

        [Fact]
        public async System.Threading.Tasks.Task GetConsumerTask_ShouldReturnOk_WhenNoError()
        {
            // Arrange
            var request = new DataQueryRequestDto
            {
                TenantCode = "TEN123",
                ConsumerCode = "CNS456",
                LanguageCode = "en-US",
                SearchAttributes = new List<SearchAttributeDto>
                {
                    new SearchAttributeDto
                    {
                        Column = "task_id",
                        DataType = "int",
                        Operator = "in",
                        Value = new JArray(1, 2, 2),
                        Criteria = "AND"
                    }
                }
            };

            // Mock ISQLQuery for NHibernate
            var sqlQueryMock = new Mock<ISQLQuery>();
            sqlQueryMock.Setup(q => q.SetResultTransformer(It.IsAny<IResultTransformer>()))
                        .Returns(sqlQueryMock.Object);

            // Fake async query data
            var fakeData = new List<IDictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    { "task_reward_code", "TR123" },
                    { "consumer_task_id", 1 }
                }
            };

            sqlQueryMock.Setup(q => q.ListAsync<IDictionary<string, object>>(It.IsAny<CancellationToken>()))
                        .ReturnsAsync(fakeData);

            // Make session return the mocked query
            _sessionMock.Setup(s => s.CreateSQLQuery(It.IsAny<string>()))
                        .Returns(sqlQueryMock.Object);
            _sessionFactoryMock.Setup(f => f.OpenSession()).Returns(_sessionMock.Object);


            // Act
            var result = await _controller.GetConsumerTask(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var value = Assert.IsType<DataQueryResponseDto>(okResult.Value);
            Assert.NotNull(value);
            Assert.NotNull(value.TaskRewardDetail);
        }

        [Fact]
        public async System.Threading.Tasks.Task GetConsumerTask_ShouldReturn500_WhenExceptionThrown()
        {
            // Arrange
            var request = new DataQueryRequestDto
            {
                TenantCode = "TEN123",
                ConsumerCode = "CNS456",
                LanguageCode = "en-US",
                SearchAttributes = new List<SearchAttributeDto>
                {
                    new SearchAttributeDto
                    {
                        Column = "task_id",
                        DataType = "int",
                        Operator = "in",
                        Value = new JArray(1, 2, 2),
                        Criteria = "AND"
                    }
                }
            };

            // Throw exception when session creates query
            _sessionMock.Setup(s => s.CreateSQLQuery(It.IsAny<string>()))
                        .Throws(new System.Exception("Simulated exception"));

            // Act
            var result = await _controller.GetConsumerTask(request);

            // Assert
            var response = Assert.IsType<DataQueryResponseDto>(result.Value);
            Assert.Equal(500, response.ErrorCode);
            Assert.Equal("Simulated exception", response.ErrorMessage);
        }
        [Fact]
        public async System.Threading.Tasks.Task GetConsumerTask_ShouldReturnOk_WhenNoError_like()
        {
            // Arrange
            var request = new DataQueryRequestDto
            {
                TenantCode = "TEN123",
                ConsumerCode = "CNS456",
                LanguageCode = "en-US",
                SearchAttributes = new List<SearchAttributeDto>
                {
                    new SearchAttributeDto
                    {
                        Column = "task_name",
                        DataType = "string",
                        Operator = "CONTAINS",
                        Value = "Voice",
                        Criteria = "AND"
                    }, new SearchAttributeDto
                    {
                        Column = "task_name",
                        DataType = "string",
                        Operator = "EQUALS",
                        Value = "Voice",
                        Criteria = "or"
                    }, new SearchAttributeDto
                    {
                        Column = "task_name",
                        DataType = "string",
                        Operator = ">=",
                        Value = "Voice",
                        Criteria = "or"
                    }
                }
            };

            // Mock SQL query to avoid hitting real DB
            var sqlQueryMock = new Mock<ISQLQuery>();
            sqlQueryMock.Setup(q => q.SetResultTransformer(It.IsAny<IResultTransformer>()))
                        .Returns(sqlQueryMock.Object);

            var fakeData = new List<IDictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    { "task_reward_code", "TR123" },
                    { "consumer_task_id", 1 }
                }
            };

            sqlQueryMock.Setup(q => q.List<IDictionary<string, object>>())
                        .Returns(fakeData);

            _sessionMock.Setup(s => s.CreateSQLQuery(It.IsAny<string>()))
                        .Returns(sqlQueryMock.Object);

            // Act
            var result = await _controller.GetConsumerTask(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var value = Assert.IsType<DataQueryResponseDto>(okResult.Value);
            Assert.NotNull(value);
        }
        [Fact]
        public async System.Threading.Tasks.Task GetConsumerTask_ShouldReturnOk_WhenNoError_between()
        {
            // Arrange
            var request = new DataQueryRequestDto
            {
                TenantCode = "TEN123",
                ConsumerCode = "CNS456",
                LanguageCode = "en-US",
                SearchAttributes = new List<SearchAttributeDto>
                {
                    new SearchAttributeDto
                    {
                        Column = "task_id",
                        DataType = "int",
                        Operator = "INRANGE",
                        Value = new JArray(1, 2),
                        Criteria = "AND"
                    }
                }
            };

            // Mock SQL query to avoid hitting real DB
            var sqlQueryMock = new Mock<ISQLQuery>();
            sqlQueryMock.Setup(q => q.SetResultTransformer(It.IsAny<IResultTransformer>()))
                        .Returns(sqlQueryMock.Object);

            var fakeData = new List<IDictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    { "task_reward_code", "TR123" },
                    { "consumer_task_id", 1 }
                }
            };

            sqlQueryMock.Setup(q => q.List<IDictionary<string, object>>())
                        .Returns(fakeData);

            _sessionMock.Setup(s => s.CreateSQLQuery(It.IsAny<string>()))
                        .Returns(sqlQueryMock.Object);

            // Act
            var result = await _controller.GetConsumerTask(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var value = Assert.IsType<DataQueryResponseDto>(okResult.Value);
            Assert.NotNull(value);
        }
        public static IQueryGeneratorService GetMockedQueryGeneratorService()
        {
            var mockService = new Mock<IQueryGeneratorService>();

            // Correct type: List<IDictionary<string, object>>
            var fakeData = new List<Dictionary<string, object>>
    {
        new Dictionary<string, object>
        {
            { "task_reward_code", "TR123" },
            { "consumer_task_id", 1 }
        }
    };

            var responseDto = new DataQueryResponseDto(); // no error

            // Setup ReturnsAsync with correct tuple type
            mockService
                .Setup(x => x.ExecuteDynamicQueryAsync(
                    It.IsAny<string>(),
                    It.IsAny<List<SearchAttributeDto>>(),
                    It.IsAny<IDictionary<string, object>>(),
                    It.IsAny<ISession>()))
                .ReturnsAsync((fakeData, responseDto)); // now types match

            return mockService.Object;
        }


        [Fact]
        public async System.Threading.Tasks.Task GetConsumerTask_ShouldReturnOk_WithMockedService()
        {
            // Arrange
            var request = new DataQueryRequestDto
            {
                TenantCode = "TEN123",
                ConsumerCode = "CNS456",
                LanguageCode = "en-US",
                SearchAttributes = new List<SearchAttributeDto>
        {
            new SearchAttributeDto
            {
                Column = "task_id",
                DataType = "int",
                Operator = "in",
                Value = new JArray(1, 2, 2),
                Criteria = "AND"
            }
        }
            };

            // Mock session
            var sessionFactoryMock = new Mock<ISessionFactory>();
            var sessionMock = new Mock<ISession>();
            sessionFactoryMock.Setup(f => f.OpenSession()).Returns(sessionMock.Object);

            // Real mapper
            var mappingProfileProvider = new MappingProfileProvider(new Mock<ILogger<MappingProfileProvider>>().Object);
            var rowToDtoMapper = new RowToDtoMapper(mappingProfileProvider);

            // Use mocked QueryGeneratorService
            var dynamicQueryServiceMocked = GetMockedQueryGeneratorService();

            // Service and controller
            var serviceLoggerMock = new Mock<ILogger<DataQueryService>>();
            var service = new DataQueryService(
                sessionFactoryMock.Object,
                serviceLoggerMock.Object,
                rowToDtoMapper,
                dynamicQueryServiceMocked
            );

            var controllerLoggerMock = new Mock<ILogger<DataQueryController>>();
            var controller = new DataQueryController(controllerLoggerMock.Object, service);

            // Act
            var actionResult = await controller.GetConsumerTask(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var value = Assert.IsType<DataQueryResponseDto>(okResult.Value);

            Assert.NotNull(value);
            Assert.NotNull(value.TaskRewardDetail);
            Assert.Single(value.TaskRewardDetail);
            Assert.True(value.TaskRewardDetail[0].ContainsKey("TR123"));
        }


    }
}
