using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NSubstitute;
using Sunny.Benefits.Bff.Api.Controllers;
using Sunny.Benefits.Bff.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;
using Xunit;

namespace Sunny.Benefits.Bff.UnitTest.Controllers
{
    public class WalletCategoryControllerTests
    {
        private readonly Mock<IWalletCategoryService> _serviceMock;
        private readonly WalletCategoryController _controller;

        public WalletCategoryControllerTests()
        {
            _serviceMock = new Mock<IWalletCategoryService>();
            var loggerMock = new Mock<ILogger<WalletCategoryController>>();
            _controller = new WalletCategoryController(loggerMock.Object, _serviceMock.Object);
        }

        [Fact]
        public async Task GetByTenant_ReturnsOk_WhenDataExists()
        {
            // Arrange
            var tenantCode = "T1";
            var expected = new List<WalletCategoryResponseDto>
            {
                new WalletCategoryResponseDto { Id = 1},
                new WalletCategoryResponseDto { Id = 2}
            };

            _serviceMock
                .Setup(s => s.GetByTenant(tenantCode))
                .Returns(Task.FromResult<IEnumerable<WalletCategoryResponseDto>>(expected));

            // Act
            var result = await _controller.GetByTenant(tenantCode);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsAssignableFrom<IEnumerable<WalletCategoryResponseDto>>(okResult.Value);
            Assert.Equal(2, value.Count());
        }

        [Fact]
        public async Task GetByTenant_ReturnsNotFound_WhenEmpty()
        {
            // Arrange
            var tenantCode = "T1";
            _serviceMock
                .Setup(s => s.GetByTenant(tenantCode))
                .Returns(Task.FromResult<IEnumerable<WalletCategoryResponseDto>>(Enumerable.Empty<WalletCategoryResponseDto>()));

            // Act
            var result = await _controller.GetByTenant(tenantCode);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetByTenant_ReturnsServerError_OnException()
        {
            // Arrange
            var tenantCode = "T1";
            _serviceMock
                .Setup(s => s.GetByTenant(tenantCode))
                .ThrowsAsync(new Exception("fail"));

            // Act
            var result = await _controller.GetByTenant(tenantCode);

            // Assert
            var objectResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Fact]
        public async Task GetById_ReturnsOk_WhenFound()
        {
            // Arrange
            int id = 1;
            var expected = new WalletCategoryResponseDto { Id = id};
            _serviceMock.Setup(s => s.GetById(id))
                .Returns(Task.FromResult(expected));

            // Act
            var result = await _controller.GetById(id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<WalletCategoryResponseDto>(okResult.Value);
            Assert.Equal(id, value.Id);
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenNull()
        {
            // Arrange
            long id = 1;
            _serviceMock.Setup(s => s.GetById(id))
                .Returns(Task.FromResult<WalletCategoryResponseDto>(null));

            // Act
            var result = await _controller.GetById(id);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetById_ReturnsServerError_OnException()
        {
            // Arrange
            long id = 1;
            _serviceMock.Setup(s => s.GetById(id))
                .ThrowsAsync(new Exception("fail"));

            // Act
            var result = await _controller.GetById(id);

            // Assert
            var objectResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Fact]
        public async Task GetByTenantAndWallet_ReturnsOk_WhenFound()
        {
            // Arrange
            var tenantCode = "T1";
            long walletTypeId = 2;
            var expected = new List<WalletCategoryResponseDto>
            {
                new WalletCategoryResponseDto { Id = 10 }
            };

            _serviceMock.Setup(s => s.GetByTenantAndWallet(tenantCode, walletTypeId))
                .Returns(Task.FromResult<IEnumerable<WalletCategoryResponseDto>>(expected));

            // Act
            var result = await _controller.GetByTenantAndWallet(tenantCode, walletTypeId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsAssignableFrom<IEnumerable<WalletCategoryResponseDto>>(okResult.Value);
            Assert.Single(value);
            Assert.Equal(expected[0].Id, value.First().Id);
        }

        [Fact]
        public async Task GetByTenantAndWallet_ReturnsNotFound_WhenNull()
        {
            // Arrange
            var tenantCode = "T1";
            long walletTypeId = 2;
            _serviceMock.Setup(s => s.GetByTenantAndWallet(tenantCode, walletTypeId))
                .Returns(Task.FromResult<IEnumerable<WalletCategoryResponseDto>>(null));

            // Act
            var result = await _controller.GetByTenantAndWallet(tenantCode, walletTypeId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }


        [Fact]
        public async Task GetByTenantAndWallet_ReturnsServerError_OnException()
        {
            // Arrange
            var tenantCode = "T1";
            long walletTypeId = 2;
            _serviceMock.Setup(s => s.GetByTenantAndWallet(tenantCode, walletTypeId))
                .ThrowsAsync(new Exception("fail"));

            // Act
            var result = await _controller.GetByTenantAndWallet(tenantCode, walletTypeId);

            // Assert
            var objectResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(500, objectResult.StatusCode);
        }
    }
}
