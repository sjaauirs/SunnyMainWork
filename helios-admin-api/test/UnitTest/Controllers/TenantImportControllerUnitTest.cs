using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SunnyRewards.Helios.Admin.Api.Controllers;
using SunnyRewards.Helios.Admin.Core.Domain.Constants;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos.Enums;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SunnyRewards.Helios.Admin.UnitTest.Controllers
{
    public class TenantImportControllerUnitTest
    {
        private readonly Mock<ILogger<TenantImportController>> _loggerMock;
        private readonly Mock<ITenantImportService> _tenantImportServiceMock;
        private readonly TenantImportController _controller;

        public TenantImportControllerUnitTest()
        {
            _loggerMock = new Mock<ILogger<TenantImportController>>();
            _tenantImportServiceMock = new Mock<ITenantImportService>();

            _controller = new TenantImportController(_loggerMock.Object, _tenantImportServiceMock.Object);
        }

        [Fact]
        public async void UploadFile_ShouldReturn400_WhenImportOptionsAreInvalid()
        {
            // Arrange
            var requestDto = new TenantImportRequestDto
            {
                ImportOptions = new List<string> { "INVALID_OPTION" },
                tenantCode = "Tenant123"
            };

            // Act
            var result = await _controller.UploadFile(requestDto) as BadRequestObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
            Assert.Equal("Invalid import options provided.", result.Value);
        }

        [Fact]
        public async void UploadFile_ShouldReturn400_WhenTenantCodeIsMissing()
        {
            // Arrange
            var requestDto = new TenantImportRequestDto
            {
                ImportOptions = new List<string> { "ALL" },
                tenantCode = null
            };

            // Act
            var result = await _controller.UploadFile(requestDto) as BadRequestObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
            Assert.Equal("Invalid tenant Code.", result.Value);
        }

        [Fact]
        public async void UploadFile_ShouldReturn400_WhenFisImportMissingSponsorAndCustomerCode()
        {
            // Arrange
            var requestDto = new TenantImportRequestDto
            {
                ImportOptions = new List<string> { nameof(ImportOption.FIS) },
                tenantCode = "Tenant123",
                SponsorCode = null,
                CustomerCode = null
            };

            // Act
            var result = await _controller.UploadFile(requestDto) as BadRequestObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
            Assert.Equal("SponsorCode and ConsumerCode Cannot be null for fis import type.", result.Value);
        }

        [Fact]
        public async void UploadFile_ShouldReturn404_WhenServiceReturnsNotFound()
        {
            // Arrange
            var requestDto = new TenantImportRequestDto
            {
                ImportOptions = new List<string> { "ALL" },
                tenantCode = "Tenant123"
            };

            _tenantImportServiceMock
                .Setup(service => service.TenantImport(requestDto))
                .ReturnsAsync(new BaseResponseDto { ErrorCode = 404 });

            // Act
            var result = await _controller.UploadFile(requestDto) as NotFoundObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status404NotFound, result.StatusCode);
        }

        [Fact]
        public async void UploadFile_ShouldReturn500_WhenServiceReturnsInternalServerError()
        {
            // Arrange
            var requestDto = new TenantImportRequestDto
            {
                ImportOptions = new List<string> { "ALL" },
                tenantCode = "Tenant123"
            };

            _tenantImportServiceMock
                .Setup(service => service.TenantImport(requestDto))
                .ReturnsAsync(new BaseResponseDto { ErrorCode = 500 });

            // Act
            var result = await _controller.UploadFile(requestDto) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, result.StatusCode);
        }

        [Fact]
        public async void UploadFile_ShouldReturn200_WhenImportIsSuccessful()
        {
            // Arrange
            var requestDto = new TenantImportRequestDto
            {
                ImportOptions = new List<string> { "ALL" },
                tenantCode = "Tenant123"
            };

            _tenantImportServiceMock
                .Setup(service => service.TenantImport(requestDto))
                .ReturnsAsync(new BaseResponseDto { ErrorCode = null });

            // Act
            var result = await _controller.UploadFile(requestDto) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }

        [Fact]
        public async void UploadFile_ShouldReturn500_WhenExceptionOccurs()
        {
            // Arrange
            var requestDto = new TenantImportRequestDto
            {
                ImportOptions = new List<string> { "ALL" },
                tenantCode = "Tenant123"
            };

            _tenantImportServiceMock
                .Setup(service => service.TenantImport(requestDto))
                .ThrowsAsync(new Exception("Unexpected error"));

            // Act
            var result = await _controller.UploadFile(requestDto) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, result.StatusCode);
            Assert.Contains("Internal server error", result.Value.ToString());
        }
    }
}
