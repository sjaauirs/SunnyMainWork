using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SunnyRewards.Helios.Tenant.Api.Controllers;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;
using SunnyRewards.Helios.Tenant.Core.Domain.Models;
using SunnyRewards.Helios.Tenant.Infrastructure.Mappings.MappingProfile;
using SunnyRewards.Helios.Tenant.Infrastructure.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SunnyRewards.Helios.Tenant.UnitTest.Controller
{
    public class WalletCategoryControllerTests
    {
        private readonly IMapper _mapper;
        private readonly Mock<IWalletCategoryService> _serviceMock;
        private readonly Mock<ILogger<WalletCategoryController>> _loggerMock;

        public WalletCategoryControllerTests()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<WalletCategoryMapping>());
            _mapper = config.CreateMapper();
            _serviceMock = new Mock<IWalletCategoryService>();
            _loggerMock = new Mock<ILogger<WalletCategoryController>>();
        }

        [Fact]
        public async Task GetByTenantCode_ReturnsOkWithMappedDtos()
        {
            var tenantCode = "T1";
            var models = new List<WalletCategoryModel>
            {
                new WalletCategoryModel { Id = 1, TenantCode = tenantCode, WalletTypeCode = "W1", CategoryFk = 2 }
            };

            _serviceMock.Setup(s => s.GetByTenantCodeAsync(tenantCode)).ReturnsAsync(models);
            var controller = new WalletCategoryController(_serviceMock.Object, _mapper, _loggerMock.Object);

            var actionResult = await controller.GetByTenantCode(tenantCode) as OkObjectResult;

            Assert.NotNull(actionResult);
            var dtos = actionResult.Value as IEnumerable<WalletCategoryResponseDto>;
            Assert.Single(dtos!);
            Assert.Equal(models.First().Id, dtos!.First().Id);
        }

        [Fact]
        public async Task GetByTenantCode_ReturnsNotFound_WhenNoResults()
        {
            var tenantCode = "T1";
            _serviceMock.Setup(s => s.GetByTenantCodeAsync(tenantCode)).ReturnsAsync(new List<WalletCategoryModel>());
            var controller = new WalletCategoryController(_serviceMock.Object, _mapper, _loggerMock.Object);

            var actionResult = await controller.GetByTenantCode(tenantCode);

            Assert.IsType<NotFoundResult>(actionResult);
        }

        [Fact]
        public async Task GetByTenantCode_Returns500_WhenExceptionThrown()
        {
            var tenantCode = "T1";
            _serviceMock.Setup(s => s.GetByTenantCodeAsync(tenantCode))
                        .ThrowsAsync(new System.Exception("DB error"));
            var controller = new WalletCategoryController(_serviceMock.Object, _mapper, _loggerMock.Object);

            var actionResult = await controller.GetByTenantCode(tenantCode) as ObjectResult;

            Assert.NotNull(actionResult);
            Assert.Equal(500, actionResult!.StatusCode);
            Assert.Equal("An error occurred while processing your request.", actionResult.Value);
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenMissing()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(123))
                        .ReturnsAsync((WalletCategoryModel?)null);
            var controller = new WalletCategoryController(_serviceMock.Object, _mapper, _loggerMock.Object);

            var actionResult = await controller.GetById(123);

            Assert.IsType<NotFoundResult>(actionResult);
        }

        [Fact]
        public async Task GetById_ReturnsOk_WhenFound()
        {
            var model = new WalletCategoryModel
            {
                Id = 1,
                TenantCode = "T1",
                WalletTypeCode = "W1",
                CategoryFk = 2
            };
            _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(model);
            var controller = new WalletCategoryController(_serviceMock.Object, _mapper, _loggerMock.Object);

            var actionResult = await controller.GetById(1) as OkObjectResult;

            Assert.NotNull(actionResult);
            var dto = actionResult!.Value as WalletCategoryResponseDto;
            Assert.NotNull(dto);
            Assert.Equal(model.Id, dto!.Id);
        }

        [Fact]
        public async Task GetById_Returns500_WhenExceptionThrown()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(1))
                        .ThrowsAsync(new System.Exception("DB error"));
            var controller = new WalletCategoryController(_serviceMock.Object, _mapper, _loggerMock.Object);

            var actionResult = await controller.GetById(1) as ObjectResult;

            Assert.NotNull(actionResult);
            Assert.Equal(500, actionResult!.StatusCode);
            Assert.Equal("An error occurred while processing your request.", actionResult.Value);
        }

        // 🔥 NEW TESTS FOR GetByTenantAndWallet 🔥

        [Fact]
        public async Task GetByTenantAndWallet_ReturnsOkWithDtos()
        {
            var tenantCode = "T1";
            var walletTypeId = 61;
            var models = new List<WalletCategoryModel>
            {
                new WalletCategoryModel { Id = 1, TenantCode = tenantCode, WalletTypeId = walletTypeId, WalletTypeCode = "W1",CategoryFk = 2 }
            };

            // The controller method GetByTenantAndWalletTypeId fetches all categories for tenant
            // and filters by walletTypeId. We mock GetByTenantCodeAsync to return matching models.
            _serviceMock.Setup(s => s.GetByTenantCodeAsync(tenantCode)).ReturnsAsync(models);

            var controller = new WalletCategoryController(_serviceMock.Object, _mapper, _loggerMock.Object);

            var actionResult = await controller.GetByTenantAndWalletTypeId(tenantCode, walletTypeId) as OkObjectResult;

            Assert.NotNull(actionResult);
            var dtos = actionResult!.Value as IEnumerable<WalletCategoryResponseDto>;
            Assert.Single(dtos!);
            Assert.Equal(models.First().Id, dtos!.First().Id);
        }

        [Fact]
        public async Task GetByTenantAndWallet_ReturnsNotFound_WhenNoResults()
        {
            var tenantCode = "T1";
            var walletTypeId = 61;

            _serviceMock.Setup(s => s.GetByTenantCodeAsync(tenantCode))
                        .ReturnsAsync(new List<WalletCategoryModel>());

            var controller = new WalletCategoryController(_serviceMock.Object, _mapper, _loggerMock.Object);

            var actionResult = await controller.GetByTenantAndWalletTypeId(tenantCode, walletTypeId);

            Assert.IsType<NotFoundResult>(actionResult);
        }

        [Fact]
        public async Task GetByTenantAndWallet_Returns500_WhenExceptionThrown()
        {
            var tenantCode = "T1";
            var walletTypeId = 61;

            _serviceMock.Setup(s => s.GetByTenantCodeAsync(tenantCode))
                        .ThrowsAsync(new System.Exception("DB error"));

            var controller = new WalletCategoryController(_serviceMock.Object, _mapper, _loggerMock.Object);

            var actionResult = await controller.GetByTenantAndWalletTypeId(tenantCode, walletTypeId) as ObjectResult;

            Assert.NotNull(actionResult);
            Assert.Equal(500, actionResult!.StatusCode);
            Assert.Equal("An error occurred while processing your request.", actionResult.Value);
        }
    }
}
