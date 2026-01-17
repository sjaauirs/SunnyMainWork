using Microsoft.Extensions.Logging;
using Moq;
using SunnyRewards.Helios.Admin.Api.Controllers;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services;
using SunnyRewards.Helios.Admin.UnitTest.Helpers.HttpClientsMock;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SunnyRewards.Helios.Cohort.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using System.Dynamic;
using Xunit;

namespace SunnyRewards.Helios.Admin.UnitTest.Services
{
    public class CohortConsumerServiceTest
    {
        private readonly Mock<ILogger<CohortConsumerService>> _cohortServiceLogger;
        private readonly Mock<ICohortClient> _cohortClient;
        private readonly CohortConsumerService _cohortService;

        public CohortConsumerServiceTest()
        {
            _cohortServiceLogger = new Mock<ILogger<CohortConsumerService>>();
            _cohortClient = new CohortMockClient();
            _cohortService = new CohortConsumerService(_cohortServiceLogger.Object, _cohortClient.Object);
        }
        [Fact]
        public void AddConsumerCohort_ValidRequest_ReturnsSuccessResponse()
        {
            // Arrange
            dynamic request = new ExpandoObject();
            request.ConsumerCode = "Consumer123";
            request.TenantCode = "TenantABC";
            request.CohortName = "TestCohort";
            var expectedResponse = new BaseResponseDto { ErrorCode = null };
            _cohortClient.Setup(client => client.Post<BaseResponseDto>("add-consumer", It.IsAny<CohortConsumerRequestDto>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = _cohortService.AddConsumerCohort(request);

            // Assert
            Assert.Equal(expectedResponse, result);
        }

        [Fact]
        public void AddConsumerCohort_CohortClientError_ReturnsErrorResponse()
        {
            // Arrange
            dynamic request = new ExpandoObject();
            request.ConsumerCode = "Consumer123"; // Missing CohortName
            request.TenantCode = "TenantABC";
            var expectedResponse = new BaseResponseDto { };
            _cohortClient.Setup(client => client.Post<BaseResponseDto>("add-consumer", It.IsAny<CohortConsumerRequestDto>()))
                .ReturnsAsync(expectedResponse);

            // Act
            BaseResponseDto result = _cohortService.AddConsumerCohort(request);

            // Assert
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
        }

        [Fact]
        public void AddConsumerCohort_ExceptionThrown_ReturnsInternalServerError()
        {
            // Arrange
            dynamic request = new ExpandoObject();
            request.ConsumerCode = "Consumer123";
            request.TenantCode = "TenantABC";
            request.CohortName = "TestCohort";

            _cohortClient.Setup(client => client.Post<BaseResponseDto>("add-consumer", It.IsAny<CohortConsumerRequestDto>()))
                .Throws(new Exception("Test exception"));

            // Act
            BaseResponseDto result = _cohortService.AddConsumerCohort(request);

            // Assert
            Assert.Equal(StatusCodes.Status500InternalServerError, result.ErrorCode);
        }

        [Fact]
        public void RemoveConsumerCohort_ValidRequest_ReturnsSuccessResponse()
        {
            // Arrange
            dynamic request = new ExpandoObject();
            request.ConsumerCode = "Consumer123";
            request.TenantCode = "TenantABC";
            request.CohortName = "TestCohort";
            var expectedResponse = new BaseResponseDto { ErrorCode = null };
            _cohortClient.Setup(client => client.Post<BaseResponseDto>("remove-consumer", It.IsAny<CohortConsumerRequestDto>()))
                .ReturnsAsync(expectedResponse);

            // Act
            BaseResponseDto result = _cohortService.RemoveConsumerCohort(request);

            // Assert
            Assert.Equal(expectedResponse, result);
        }

        [Fact]
        public void RemoveConsumerCohort_CohortClientError_ReturnsErrorResponse()
        {
            // Arrange
            dynamic request = new ExpandoObject();
            request.ConsumerCode = "Consumer123";
            request.TenantCode = "TenantABC";
            request.CohortName = "TestCohort";
            var expectedResponse = new BaseResponseDto { ErrorCode = StatusCodes.Status400BadRequest };
            _cohortClient.Setup(client => client.Post<BaseResponseDto>("remove-consumer", It.IsAny<CohortConsumerRequestDto>()))
                .ReturnsAsync(expectedResponse);

            // Act
            BaseResponseDto result = _cohortService.RemoveConsumerCohort(request);

            // Assert
            Assert.Equal(expectedResponse, result);
        }

        [Fact]
        public void RemoveConsumerCohort_ExceptionThrown_ReturnsInternalServerError()
        {
            // Arrange
            dynamic request = new ExpandoObject();
            request.ConsumerCode = "Consumer123";
            request.TenantCode = "TenantABC";
            request.CohortName = "TestCohort";
            _cohortClient.Setup(client => client.Post<BaseResponseDto>("remove-consumer", It.IsAny<CohortConsumerRequestDto>()))
                .Throws(new Exception("Test exception"));

            // Act
            var result = _cohortService.RemoveConsumerCohort(request);

            // Assert
            Assert.Equal(StatusCodes.Status500InternalServerError, result.ErrorCode);
        }
    }
}
