using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Sunny.Benefits.Cms.Core.Domain.Dtos;
using SunnyBenefits.Fis.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Api.Controllers;
using SunnyRewards.Helios.Admin.Infrastructure.Helpers.Interface;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Admin.UnitTest.Fixtures.MockDtos;
using SunnyRewards.Helios.Admin.UnitTest.Helpers.HttpClientsMock;
using SunnyRewards.Helios.Cohort.Core.Domain.Dtos;
using SunnyRewards.Helios.Sweepstakes.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;
using Xunit;

namespace SunnyRewards.Helios.Admin.UnitTest.Controllers
{
    public class TenantExportControllerTest
    {
        private readonly Mock<ILogger<TenantExportController>> _controllerLogger;
        private readonly Mock<ILogger<TenantExportService>> _tenantExportServiceLogger;
        private readonly Mock<ISecretHelper> _secretHelper;
        private readonly Mock<IS3Service> _s3Service;
        private readonly Mock<IConfiguration> _configuration;
        private readonly Mock<IUserClient> _userClient;
        private readonly Mock<ICohortClient> _cohortClient;
        private readonly Mock<ICmsClient> _cmsClient;
        private readonly Mock<IWalletClient> _walletClient;
        private readonly Mock<ITaskClient> _taskClient;
        private readonly Mock<ITenantClient> _tenantClient;
        private readonly Mock<IFisClient> _fisClient;
        private readonly Mock<ISweepstakesClient> _sweepstakesClient;
        private readonly Mock<IWalletClient> _walletClientMock;
        private readonly ITenantExportService _tenantExportService;
        private readonly TenantExportController _tenantExportController;
        private readonly Mock<IAdminService> _adminService;
        private readonly Mock<ITaskRewardTypeService> _taskRewardTypeService;
        private readonly Mock<ITaskCategoryService> _taskCategoryService;
        private readonly Mock<ITaskTypeService> _taskTypeService;
        private readonly Mock<IComponentService> _componentService;
        private readonly Mock<IWalletTypeService> _walletTypeService;

        public TenantExportControllerTest()
        {
            _configuration = new Mock<IConfiguration>();
            _controllerLogger = new Mock<ILogger<TenantExportController>>();
            _tenantExportServiceLogger = new Mock<ILogger<TenantExportService>>();
            _s3Service = new Mock<IS3Service>();
            _secretHelper = new Mock<ISecretHelper>();
            _userClient = new UserClientMock();
            _cohortClient = new CohortMockClient();
            _walletClient = new WalletClientMock();
            _fisClient = new FisClientMock();
            _taskClient = new TaskClientMock();
            _cmsClient = new CmsMockClient();
            _sweepstakesClient = new SweepstakesClientMock();
            _tenantClient = new TenantClientMock();
            _adminService = new Mock<IAdminService>();
            _walletClientMock = new Mock<IWalletClient>();
            _taskRewardTypeService = new Mock<ITaskRewardTypeService>();
            _taskCategoryService = new Mock<ITaskCategoryService>();
            _taskTypeService = new Mock<ITaskTypeService>();
            _componentService = new Mock<IComponentService>();
            _walletTypeService = new Mock<IWalletTypeService>();
            _tenantExportService = new TenantExportService(_tenantExportServiceLogger.Object, _tenantClient.Object,
                _s3Service.Object, _secretHelper.Object, _cohortClient.Object, _taskClient.Object, _cmsClient.Object,
                _fisClient.Object, _sweepstakesClient.Object,_adminService.Object, _walletClient.Object, _taskRewardTypeService.Object,
                _taskCategoryService.Object, _taskTypeService.Object,_componentService.Object,_walletTypeService.Object
                );
            _tenantExportController = new TenantExportController(_controllerLogger.Object, _tenantExportService);
        }
        [Fact]
        public async System.Threading.Tasks.Task ExportTenant_ShouldReturnFile_WhenNoError()
        {
            // Arrange
            var requestDto = new ExportTenantRequestMockDto();
            _s3Service.Setup(x => x.ZipFolderAndUpload(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
            _s3Service.Setup(x => x.DownloadZipFile(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new MemoryStream());

            // Act
            var result = await _tenantExportController.ExportTenant(requestDto);

            // Assert
            var fileResult = Assert.IsType<FileStreamResult>(result);
            Assert.NotNull(fileResult.FileStream);
            Assert.NotNull(fileResult.FileDownloadName);
        }

        [Fact]
        public async System.Threading.Tasks.Task ExportTenant_ShouldReturnError_WhenZipUploadFails()
        {
            // Arrange
            var requestDto = new ExportTenantRequestMockDto();

            // Act
            var result = await _tenantExportController.ExportTenant(requestDto);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
        }

        [Fact]
        public async System.Threading.Tasks.Task ExportTenant_ShouldReturnError_WhenExportOptionsAreEmpty()
        {
            // Arrange
            var requestDto = new ExportTenantRequestMockDto()
            {
                ExportOptions = []
            };

            // Act
            var result = await _tenantExportController.ExportTenant(requestDto);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status400BadRequest, statusCodeResult.StatusCode);
        }

        [Fact]
        public async System.Threading.Tasks.Task ExportTenant_ShouldReturnError_WhenTenantIsNull()
        {
            // Arrange
            var requestDto = new ExportTenantRequestMockDto();
            _tenantClient.Setup(c => c.Post<TenantDto>(It.IsAny<string>(), It.IsAny<GetTenantCodeRequestDto>()));

            // Act
            var result = await _tenantExportController.ExportTenant(requestDto);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status404NotFound, statusCodeResult.StatusCode);
        }

        [Fact]
        public async System.Threading.Tasks.Task ExportTenant_ShouldReturnError_When_Cohort_API_Throws_Exception()
        {
            // Arrange
            var requestDto = new ExportTenantRequestMockDto();
            _cohortClient.Setup(c => c.Post<ExportCohortResponseDto>(It.IsAny<string>(), It.IsAny<ExportCohortRequestDto>()))
                .ThrowsAsync(new Exception("Testing"));

            // Act
            var result = await _tenantExportController.ExportTenant(requestDto);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
        }

        [Fact]
        public async System.Threading.Tasks.Task ExportTenant_ShouldReturnError_When_Task_API_Throws_Exception()
        {
            // Arrange
            var requestDto = new ExportTenantRequestMockDto();
            _taskClient.Setup(c => c.Post<ExportTaskResponseDto>(It.IsAny<string>(), It.IsAny<ExportTaskRequestDto>()))
                .ThrowsAsync(new Exception("Testing"));

            // Act
            var result = await _tenantExportController.ExportTenant(requestDto);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
        }

        [Fact]
        public async System.Threading.Tasks.Task ExportTenant_ShouldReturnError_When_CMS_API_Throws_Exception()
        {
            // Arrange
            var requestDto = new ExportTenantRequestMockDto();
            _cmsClient.Setup(c => c.Post<ExportCohortResponseDto>(It.IsAny<string>(), It.IsAny<ExportCmsRequestDto>()))
                .ThrowsAsync(new Exception("Testing"));

            // Act
            var result = await _tenantExportController.ExportTenant(requestDto);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
        }

        [Fact]
        public async System.Threading.Tasks.Task ExportTenant_ShouldReturnError_When_FIS_API_Throws_Exception()
        {
            // Arrange
            var requestDto = new ExportTenantRequestMockDto();
            _fisClient.Setup(c => c.Post<TenantAccountDto>(It.IsAny<string>(), It.IsAny<TenantAccountCreateRequestDto>()))
                .ThrowsAsync(new Exception("Testing"));

            // Act
            var result = await _tenantExportController.ExportTenant(requestDto);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
        }

        [Fact]
        public async System.Threading.Tasks.Task ExportTenant_ShouldReturnError_When_Sweepstakes_API_Throws_Exception()
        {
            // Arrange
            var requestDto = new ExportTenantRequestMockDto();
            _sweepstakesClient.Setup(c => c.Post<ExportSweepstakesResponseDto>(It.IsAny<string>(), It.IsAny<ExportSweepstakesRequestDto>()))
                .ThrowsAsync(new Exception("Testing"));

            // Act
            var result = await _tenantExportController.ExportTenant(requestDto);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
        }


        [Fact]
        public async System.Threading.Tasks.Task ExportTenant_ShouldReturnError_When_wallet_API_Throws_Exception()
        {
            // Arrange
            var requestDto = new ExportTenantRequestMockDto();
            _walletClient.Setup(c => c.Post<ExportSweepstakesResponseDto>(It.IsAny<string>(), It.IsAny<ExportSweepstakesRequestDto>()))
                .ThrowsAsync(new Exception("Testing"));

            // Act
            var result = await _tenantExportController.ExportTenant(requestDto);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
        }

    }
}
