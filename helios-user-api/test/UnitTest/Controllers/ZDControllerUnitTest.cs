using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using SunnyRewards.Helios.User.Api.Controllers;
using SunnyRewards.Helios.User.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.User.Infrastructure.Services;
using SunnyRewards.Helios.User.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.User.UnitTest.Fixtures.MockDto;
using SunnyRewards.Helios.User.UnitTest.Fixtures.MockRepositories;
using Xunit;

namespace SunnyRewards.Helios.User.UnitTest.Controllers
{
    public class ZDControllerUnitTest
    {
        private readonly Mock<ILogger<ZDController>> _zdControllerLogger;
        private readonly Mock<ILogger<ZDService>> _zdServiceLogger;
        private readonly Mock<IVault> _vault;
        private readonly Mock<IConsumerRepo> _consumerRepo;
        private readonly Mock<IPersonRepo> _personRepo;
        private readonly ZDController _zDController;
        private readonly IZDService _zdService;

        public ZDControllerUnitTest()
        {
            _zdControllerLogger = new Mock<ILogger<ZDController>>();
            _zdServiceLogger = new Mock<ILogger<ZDService>>();
            _consumerRepo = new ConsumerMockRepo();
            _vault = new Mock<IVault>();
            _personRepo = new PersonMockRepo();
            _zdService = new ZDService(_zdServiceLogger.Object, _vault.Object, _consumerRepo.Object, _personRepo.Object);
            _zDController = new ZDController(_zdControllerLogger.Object, _zdService);
        }
        [Fact]
        public async Task Should_create_zd_token()
        {
            var zdTokenRequestDto = new ZdTokenRequestMockDto();
            _vault.Setup(vault => vault.GetSecret("ZENDESK_KEY")).ReturnsAsync("vAtYFu98qiccEdXghc_ZWoBhxrvirbX5zGwh3mXpVKs4p10wxTlrB_slCNh57prct08_FavoFOJZk5wwfFVw");
            _vault.Setup(vault => vault.GetSecret("ZENDESK_KEYID")).ReturnsAsync("app_652d6fc85d84f8ae375fc45");
            var zdTokenResponseMockDto = await _zDController.CreateZdToken(zdTokenRequestDto);
            var result = zdTokenResponseMockDto.Result as OkObjectResult;
            Assert.True(result?.StatusCode == 200);
        }

        [Fact]
        public async Task CreateZdToken_Catch_Exception_Controller()
        {
            var zdService = new Mock<IZDService>();
            zdService.Setup(s => s.CreateZdToken(It.IsAny<ZdTokenRequestMockDto>())).ThrowsAsync(new Exception("intended exception"));
            var controller = new ZDController(_zdControllerLogger.Object, zdService.Object);
            var zdTokenRequestDto = new ZdTokenRequestMockDto();
            var result = await controller.CreateZdToken(zdTokenRequestDto);
            Assert.True(result?.Value?.ErrorMessage == "intended exception");
        }
        [Fact]
        public async Task Should_create_zd_token_InternalError_Controller()
        {
            var zdTokenRequestDto = new ZdTokenRequestMockDto();
            _vault.Setup(vault => vault.GetSecret("ZENDESK_KEY")).ReturnsAsync("");
            _vault.Setup(vault => vault.GetSecret("ZENDESK_KEYID")).ReturnsAsync("");
            var response = await _zDController.CreateZdToken(zdTokenRequestDto);
            var result = response.Result as ObjectResult;
            Assert.NotNull(result);
            Assert.Equal(500, result.StatusCode);
        }

        [Fact]
        public async Task CreateZdToken_ValidConsumerCode_ReturnsValidToken()
        {
            var zdTokenRequestDto = new ZdTokenRequestMockDto();
            _vault.Setup(vault => vault.GetSecret("ZENDESK_KEY")).ReturnsAsync("vAtYFu98qiccEdXghc_ZWoBhxrvirbX5zGwh3mXpVKs4p10wxTlrB_slCNh57prct08_FavoFOJZk5wwfFVw");
            _vault.Setup(vault => vault.GetSecret("ZENDESK_KEYID")).ReturnsAsync("app_652d6fc85d84f8ae375fc45");
            var result = await _zdService.CreateZdToken(zdTokenRequestDto);
            Assert.True(result.Jwt != null);
        }

        [Fact]
        public async Task CreateZdToken_Catch_Exception_Service()
        {
            var zdTokenRequestDto = new ZdTokenRequestMockDto();
            _vault.Setup(vault => vault.GetSecret("ZENDESK_KEY")).ThrowsAsync(new Exception("Simulated exception"));
            var result = await _zdService.CreateZdToken(zdTokenRequestDto);
            Assert.NotNull(result.ErrorMessage);
        }
    }
}

