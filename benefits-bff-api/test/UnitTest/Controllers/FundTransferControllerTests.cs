using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Sunny.Benefits.Bff.Api.Controllers;
using Sunny.Benefits.Bff.Core.Domain.Constants;
using Sunny.Benefits.Bff.Core.Domain.Dtos;
using Sunny.Benefits.Bff.Infrastructure.HttpClients.Interfaces;
using Sunny.Benefits.Bff.Infrastructure.Repositories.Interfaces;
using Sunny.Benefits.Bff.Infrastructure.Services;
using Sunny.Benefits.Bff.Infrastructure.Services.Interfaces;
using Sunny.Benefits.Bff.UnitTest.Fixtures.MockDtos;
using SunnyBenefits.Fis.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;
using Xunit;

namespace Sunny.Benefits.Bff.UnitTest.Controllers
{
    public class FundTransferControllerTests
    {
        private readonly Mock<ILogger<FundTransferController>> _fundTransferControllerLogger;
        private readonly Mock<ILogger<FundTransferService>> _fundTransferServiceLogger;
        private readonly Mock<IWalletClient> _walletClient;
        private readonly Mock<IUserClient> _userClient;
        private readonly Mock<IConfiguration> _configuration;
        private readonly Mock<IFisClient> _fisClient;
        private readonly FundTransferController _controller;
        private readonly IFundTransferService _fundTransferService;
        private readonly Mock<ITenantAccountService> _tenantAccountService;

        public FundTransferControllerTests()
        {
            _fundTransferControllerLogger = new Mock<ILogger<FundTransferController>>();
            _fundTransferServiceLogger = new Mock<ILogger<FundTransferService>>();
            _configuration = new Mock<IConfiguration>();
            _walletClient = new Mock<IWalletClient>();
            _userClient = new Mock<IUserClient>();
            _fisClient = new Mock<IFisClient>();
            _tenantAccountService = new Mock<ITenantAccountService>();
            _fundTransferService = new FundTransferService(_fundTransferServiceLogger.Object, _userClient.Object,
                _walletClient.Object, _fisClient.Object, _configuration.Object, _tenantAccountService.Object);
            _controller = new FundTransferController(_fundTransferControllerLogger.Object, _fundTransferService);
        }

        [Fact]
        public async Task FundTransfer_Success_ReturnsOk()
        {
            // Arrange
            var request = new FundTransferRequestMockDto();
            _userClient.Setup(client => client.Post<GetConsumerResponseDto>(UserConstants.GetConsumerAPIUrl, It.IsAny<GetConsumerRequestDto>()))
              .ReturnsAsync(new GetConsumerResponseDto()
              {
                  Consumer = new ConsumerDto() { ConsumerCode = request.ConsumerCode, TenantCode = request.TenantCode, ConsumerId = 101 }
              });
            _walletClient.Setup(client => client.Post<ConsumerWalletResponseDto>(WalletConstants.GetAllConsumerWalletsAPIUrl, It.IsAny<GetConsumerWalletRequestDto>()))
             .ReturnsAsync(new ConsumerWalletResponseDto()
             {
                 ConsumerWalletDetails = new List<ConsumerWalletDetailDto>
                {
                    new ConsumerWalletDetailDto() {  WalletType = new WalletTypeDto() { WalletTypeCode = request.SourceWalletType } },
                    new ConsumerWalletDetailDto() {  WalletType = new WalletTypeDto() { WalletTypeCode = request.TargetWalletType } }
                }
             });
            _configuration.Setup(c => c.GetSection("Health_Actions_Redemption_Wallet_Type_Code").Value).Returns("456");
            _walletClient.Setup(x => x.Post<PostRedeemStartResponseDto>(WalletConstants.WalletRedeemStartAPIUrl, It.IsAny<PostRedeemStartRequestDto>()))
               .ReturnsAsync(new PostRedeemStartResponseDto());
            _fisClient.Setup(x => x.Post<LoadValueResponseDto>(WalletConstants.FISValueLoadAPIUrl, It.IsAny<LoadValueRequestDto>()))
                .ReturnsAsync(new LoadValueResponseDto());
            _walletClient.Setup(x => x.Post<PostRedeemCompleteResponseDto>(WalletConstants.WalletRedeemCompleteAPIUrl, It.IsAny<PostRedeemCompleteRequestDto>()))
                .ReturnsAsync(new PostRedeemCompleteResponseDto());
            _tenantAccountService.Setup(x => x.GetTenantAccount(It.IsAny<ExportTenantAccountRequestDto>())).ReturnsAsync(new ExportTenantAccountResponseDto()
            {
                TenantAccount = new GetTenantAccountDto()
                {
                    TenantConfigJson = "{\"purseConfig\":{\"purses\":[{\"purseLabel\":\"Food and OTC\",\"walletType\":\"wat-4e6c6142e62943eb9611560a821b1293\",\"purseNumber\":\"12501\",\"periodConfig\":{\"fundDate\":0,\"applyDateConfig\":{\"applyDate\":1,\"applyDateType\":\"NEXT_MONTH\"}},\"purseWalletType\":\"wat-35b3ac62d74b4119a5f630c9b6446035\",\"masterWalletType\":\"wat-4e6c6142e62943eb9611560a821b1293\",\"pickAPurseStatus\":\"AVAILABLE_TO_PICK\",\"masterRedemptionWalletType\":\"wat-26abe9a0ff9e4142bdb83a7754000b13\",\"redemptionTarget\":true}]},\"fisProgramDetail\":{\"clientId\":\"1308511\",\"companyId\":\"1204185\",\"packageId\":\"726373\",\"subprogramId\":\"880896\"}}"
                }
            });
            // Act
            var result = await _controller.FundTransfer(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task FundTransfer_Return_Error_Result_WhenRedemptionTargetSetToFalse()
        {
            // Arrange
            var request = new FundTransferRequestMockDto();
            _userClient.Setup(client => client.Post<GetConsumerResponseDto>(UserConstants.GetConsumerAPIUrl, It.IsAny<GetConsumerRequestDto>()))
              .ReturnsAsync(new GetConsumerResponseDto()
              {
                  Consumer = new ConsumerDto() { ConsumerCode = request.ConsumerCode, TenantCode = request.TenantCode, ConsumerId = 101 }
              });
            _walletClient.Setup(client => client.Post<ConsumerWalletResponseDto>(WalletConstants.GetAllConsumerWalletsAPIUrl, It.IsAny<GetConsumerWalletRequestDto>()))
             .ReturnsAsync(new ConsumerWalletResponseDto()
             {
                 ConsumerWalletDetails = new List<ConsumerWalletDetailDto>
                {
                    new ConsumerWalletDetailDto() {  WalletType = new WalletTypeDto() { WalletTypeCode = request.SourceWalletType } },
                    new ConsumerWalletDetailDto() {  WalletType = new WalletTypeDto() { WalletTypeCode = request.TargetWalletType } }
                }
             });
            _configuration.Setup(c => c.GetSection("Health_Actions_Redemption_Wallet_Type_Code").Value).Returns("456");
            _walletClient.Setup(x => x.Post<PostRedeemStartResponseDto>(WalletConstants.WalletRedeemStartAPIUrl, It.IsAny<PostRedeemStartRequestDto>()))
               .ReturnsAsync(new PostRedeemStartResponseDto());
            _fisClient.Setup(x => x.Post<LoadValueResponseDto>(WalletConstants.FISValueLoadAPIUrl, It.IsAny<LoadValueRequestDto>()))
                .ReturnsAsync(new LoadValueResponseDto());
            _walletClient.Setup(x => x.Post<PostRedeemCompleteResponseDto>(WalletConstants.WalletRedeemCompleteAPIUrl, It.IsAny<PostRedeemCompleteRequestDto>()))
                .ReturnsAsync(new PostRedeemCompleteResponseDto());
            _tenantAccountService.Setup(x => x.GetTenantAccount(It.IsAny<ExportTenantAccountRequestDto>())).ReturnsAsync(new ExportTenantAccountResponseDto()
            {
                TenantAccount = new GetTenantAccountDto()
                {
                    TenantConfigJson = "{\"purseConfig\":{\"purses\":[{\"purseLabel\":\"Food and OTC\",\"walletType\":\"wat-4e6c6142e62943eb9611560a821b1293\",\"purseNumber\":\"12501\",\"periodConfig\":{\"fundDate\":0,\"applyDateConfig\":{\"applyDate\":1,\"applyDateType\":\"NEXT_MONTH\"}},\"purseWalletType\":\"wat-35b3ac62d74b4119a5f630c9b6446035\",\"masterWalletType\":\"wat-4e6c6142e62943eb9611560a821b1293\",\"pickAPurseStatus\":\"AVAILABLE_TO_PICK\",\"masterRedemptionWalletType\":\"wat-26abe9a0ff9e4142bdb83a7754000b13\",\"redemptionTarget\":false}]},\"fisProgramDetail\":{\"clientId\":\"1308511\",\"companyId\":\"1204185\",\"packageId\":\"726373\",\"subprogramId\":\"880896\"}}"
                }
            });
            // Act
            var result = await _controller.FundTransfer(request);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(400, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task FundTransfer_Error_ReturnErrorResult_WhenAmountIsZero()
        {
            // Arrange
            var request = new FundTransferRequestMockDto();
            request.Amount = 0;

            // Act
            var result = await _controller.FundTransfer(request);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(400, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task FundTransfer_Error_ReturnErrorResult_WhenExceptionOccurred()
        {
            // Arrange
            var request = new FundTransferRequestMockDto();
            Mock<IFundTransferService> mockFundTransferService = new Mock<IFundTransferService> ();
            mockFundTransferService.Setup(x => x.TransferFundsAsync(It.IsAny<FundTransferRequestDto>())).ThrowsAsync(new Exception("Testing"));

            var controller = new FundTransferController(_fundTransferControllerLogger.Object, mockFundTransferService.Object);

            // Act
            var result = await controller.FundTransfer(request);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task FundTransfer_Error_ReturnErrorResult_WhenGetConsumerAPIThrowsException()
        {
            // Arrange
            var request = new FundTransferRequestMockDto();
            _userClient.Setup(client => client.Post<GetConsumerResponseDto>(UserConstants.GetConsumerAPIUrl, It.IsAny<GetConsumerRequestDto>()))
              .ThrowsAsync(new Exception("Testing"));

            // Act
            var result = await _controller.FundTransfer(request);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task FundTransfer_Error_ReturnErrorResult_WhenRedeemCompleteAPIThrowsException()
        {
            // Arrange
            var request = new FundTransferRequestMockDto();
            _userClient.Setup(client => client.Post<GetConsumerResponseDto>(UserConstants.GetConsumerAPIUrl, It.IsAny<GetConsumerRequestDto>()))
              .ReturnsAsync(new GetConsumerResponseDto()
              {
                  Consumer = new ConsumerDto() { ConsumerCode = request.ConsumerCode, TenantCode = request.TenantCode, ConsumerId = 101 }
              });
            _walletClient.Setup(client => client.Post<ConsumerWalletResponseDto>(WalletConstants.GetAllConsumerWalletsAPIUrl, It.IsAny<GetConsumerWalletRequestDto>()))
             .ReturnsAsync(new ConsumerWalletResponseDto()
             {
                 ConsumerWalletDetails = new List<ConsumerWalletDetailDto>
                {
                    new ConsumerWalletDetailDto() {  WalletType = new WalletTypeDto() { WalletTypeCode = request.SourceWalletType } },
                    new ConsumerWalletDetailDto() {  WalletType = new WalletTypeDto() { WalletTypeCode = request.TargetWalletType } }
                }
             });
            _configuration.Setup(c => c.GetSection("Health_Actions_Redemption_Wallet_Type_Code").Value).Returns("456");
            _walletClient.Setup(x => x.Post<PostRedeemStartResponseDto>(WalletConstants.WalletRedeemStartAPIUrl, It.IsAny<PostRedeemStartRequestDto>()))
               .ReturnsAsync(new PostRedeemStartResponseDto());
            _fisClient.Setup(x => x.Post<LoadValueResponseDto>(WalletConstants.FISValueLoadAPIUrl, It.IsAny<LoadValueRequestDto>()))
                .ReturnsAsync(new LoadValueResponseDto());
            _walletClient.Setup(x => x.Post<PostRedeemCompleteResponseDto>(WalletConstants.WalletRedeemCompleteAPIUrl, It.IsAny<PostRedeemCompleteRequestDto>()))
                .ThrowsAsync(new Exception("Testing"));

            // Act
            var result = await _controller.FundTransfer(request);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task FundTransfer_Error_ReturnErrorResult_WhenValueLoadReturnsError()
        {
            // Arrange
            var request = new FundTransferRequestMockDto();
            _userClient.Setup(client => client.Post<GetConsumerResponseDto>(UserConstants.GetConsumerAPIUrl, It.IsAny<GetConsumerRequestDto>()))
              .ReturnsAsync(new GetConsumerResponseDto()
              {
                  Consumer = new ConsumerDto() { ConsumerCode = request.ConsumerCode, TenantCode = request.TenantCode, ConsumerId = 101 }
              });
            _walletClient.Setup(client => client.Post<ConsumerWalletResponseDto>(WalletConstants.GetAllConsumerWalletsAPIUrl, It.IsAny<GetConsumerWalletRequestDto>()))
             .ReturnsAsync(new ConsumerWalletResponseDto()
             {
                 ConsumerWalletDetails = new List<ConsumerWalletDetailDto>
                {
                    new ConsumerWalletDetailDto() {  WalletType = new WalletTypeDto() { WalletTypeCode = request.SourceWalletType } },
                    new ConsumerWalletDetailDto() {  WalletType = new WalletTypeDto() { WalletTypeCode = request.TargetWalletType } }
                }
             });
            _configuration.Setup(c => c.GetSection("Health_Actions_Redemption_Wallet_Type_Code").Value).Returns("456");
            _walletClient.Setup(x => x.Post<PostRedeemStartResponseDto>(WalletConstants.WalletRedeemStartAPIUrl, It.IsAny<PostRedeemStartRequestDto>()))
               .ReturnsAsync(new PostRedeemStartResponseDto());
            _walletClient.Setup(x => x.Post<PostRedeemFailResponseDto>(WalletConstants.WalletRedeemFailAPIUrl, It.IsAny<PostRedeemFailRequestDto>()))
               .ReturnsAsync(new PostRedeemFailResponseDto());
            _fisClient.Setup(x => x.Post<LoadValueResponseDto>(WalletConstants.FISValueLoadAPIUrl, It.IsAny<LoadValueRequestDto>()))
                .ReturnsAsync(new LoadValueResponseDto() { ErrorCode = 500 });

            // Act
            var result = await _controller.FundTransfer(request);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task FundTransfer_Error_ReturnErrorResult_WhenRedeemFailAPIReturnsErrorResponse()
        {
            // Arrange
            var request = new FundTransferRequestMockDto();
            _userClient.Setup(client => client.Post<GetConsumerResponseDto>(UserConstants.GetConsumerAPIUrl, It.IsAny<GetConsumerRequestDto>()))
              .ReturnsAsync(new GetConsumerResponseDto()
              {
                  Consumer = new ConsumerDto() { ConsumerCode = request.ConsumerCode, TenantCode = request.TenantCode, ConsumerId = 101 }
              });
            _walletClient.Setup(client => client.Post<ConsumerWalletResponseDto>(WalletConstants.GetAllConsumerWalletsAPIUrl, It.IsAny<GetConsumerWalletRequestDto>()))
             .ReturnsAsync(new ConsumerWalletResponseDto()
             {
                 ConsumerWalletDetails = new List<ConsumerWalletDetailDto>
                {
                    new ConsumerWalletDetailDto() {  WalletType = new WalletTypeDto() { WalletTypeCode = request.SourceWalletType } },
                    new ConsumerWalletDetailDto() {  WalletType = new WalletTypeDto() { WalletTypeCode = request.TargetWalletType } }
                }
             });
            _configuration.Setup(c => c.GetSection("Health_Actions_Redemption_Wallet_Type_Code").Value).Returns("456");
            _walletClient.Setup(x => x.Post<PostRedeemStartResponseDto>(WalletConstants.WalletRedeemStartAPIUrl, It.IsAny<PostRedeemStartRequestDto>()))
               .ReturnsAsync(new PostRedeemStartResponseDto());
            _walletClient.Setup(x => x.Post<PostRedeemFailResponseDto>(WalletConstants.WalletRedeemFailAPIUrl, It.IsAny<PostRedeemFailRequestDto>()))
               .ReturnsAsync(new PostRedeemFailResponseDto() { ErrorCode = 500 });
            _fisClient.Setup(x => x.Post<LoadValueResponseDto>(WalletConstants.FISValueLoadAPIUrl, It.IsAny<LoadValueRequestDto>()));

            // Act
            var result = await _controller.FundTransfer(request);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task FundTransfer_Error_ReturnErrorResult_WhenRedeemStartFail()
        {
            // Arrange
            var request = new FundTransferRequestMockDto();
            _userClient.Setup(client => client.Post<GetConsumerResponseDto>(UserConstants.GetConsumerAPIUrl, It.IsAny<GetConsumerRequestDto>()))
              .ReturnsAsync(new GetConsumerResponseDto()
              {
                  Consumer = new ConsumerDto() { ConsumerCode = request.ConsumerCode, TenantCode = request.TenantCode, ConsumerId = 101 }
              });
            _walletClient.Setup(client => client.Post<ConsumerWalletResponseDto>(WalletConstants.GetAllConsumerWalletsAPIUrl, It.IsAny<GetConsumerWalletRequestDto>()))
             .ReturnsAsync(new ConsumerWalletResponseDto()
             {
                 ConsumerWalletDetails = new List<ConsumerWalletDetailDto>
                {
                    new ConsumerWalletDetailDto() {  WalletType = new WalletTypeDto() { WalletTypeCode = request.SourceWalletType } },
                    new ConsumerWalletDetailDto() {  WalletType = new WalletTypeDto() { WalletTypeCode = request.TargetWalletType } }
                }
             });
            _configuration.Setup(c => c.GetSection("Health_Actions_Redemption_Wallet_Type_Code").Value).Returns("456");
            _walletClient.Setup(x => x.Post<PostRedeemStartResponseDto>(WalletConstants.WalletRedeemStartAPIUrl, It.IsAny<PostRedeemStartRequestDto>()))
               .ReturnsAsync(new PostRedeemStartResponseDto() { ErrorCode = 500 });

            // Act
            var result = await _controller.FundTransfer(request);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task FundTransfer_Error_ReturnErrorResult_WhenSourceWalletsNotFound()
        {
            // Arrange
            var request = new FundTransferRequestMockDto();
            _userClient.Setup(client => client.Post<GetConsumerResponseDto>(UserConstants.GetConsumerAPIUrl, It.IsAny<GetConsumerRequestDto>()))
              .ReturnsAsync(new GetConsumerResponseDto()
              {
                  Consumer = new ConsumerDto() { ConsumerCode = request.ConsumerCode, TenantCode = request.TenantCode, ConsumerId = 101 }
              });
            _walletClient.Setup(client => client.Post<ConsumerWalletResponseDto>(WalletConstants.GetAllConsumerWalletsAPIUrl, It.IsAny<GetConsumerWalletRequestDto>()))
            .ReturnsAsync(new ConsumerWalletResponseDto()
            {
                ConsumerWalletDetails = new List<ConsumerWalletDetailDto>
               {
                    new ConsumerWalletDetailDto() {  WalletType = new WalletTypeDto() { WalletTypeCode = request.TargetWalletType } }
               }
            });

            // Act
            var result = await _controller.FundTransfer(request);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(400, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task FundTransfer_Error_ReturnErrorResult_WhenTenantNotMatched()
        {
            // Arrange
            var request = new FundTransferRequestMockDto();
            _userClient.Setup(client => client.Post<GetConsumerResponseDto>(UserConstants.GetConsumerAPIUrl, It.IsAny<GetConsumerRequestDto>()))
              .ReturnsAsync(new GetConsumerResponseDto()
              {
                  Consumer = new ConsumerDto() { ConsumerCode = request.ConsumerCode, TenantCode = "Tenant1", ConsumerId = 101 }
              });

            // Act
            var result = await _controller.FundTransfer(request);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(400, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task FundTransfer_Error_ReturnErrorResult_WhenConsumerWalletsNotFound()
        {
            // Arrange
            var request = new FundTransferRequestMockDto();
            _userClient.Setup(client => client.Post<GetConsumerResponseDto>(UserConstants.GetConsumerAPIUrl, It.IsAny<GetConsumerRequestDto>()))
              .ReturnsAsync(new GetConsumerResponseDto()
              {
                  Consumer = new ConsumerDto() { ConsumerCode = request.ConsumerCode, TenantCode = request.TenantCode, ConsumerId = 101 }
              });
            _walletClient.Setup(client => client.Post<ConsumerWalletResponseDto>(WalletConstants.GetAllConsumerWalletsAPIUrl, It.IsAny<GetConsumerWalletRequestDto>()))
              .ReturnsAsync(new ConsumerWalletResponseDto()
              {
                  ErrorCode = 400
              });

            // Act
            var result = await _controller.FundTransfer(request);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(400, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task FundTransfer_Error_ReturnErrorResult_WhenConsumerNotFound()
        {
            // Arrange
            var request = new FundTransferRequestMockDto();

            // Act
            var result = await _controller.FundTransfer(request);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(404, statusCodeResult.StatusCode);
        }

        //[Fact]
        //public async Task FundTransfer_Error_ReturnsErrorCode()
        //{
        //    // Arrange
        //    var request = new FundTransferRequestDto { /* initialize properties */ };
        //    var response = new FundTransferResponseDto { ErrorCode = 400 /* or any error code */ };
        //    _mockFundTransferService.Setup(service => service.TransferFundsAsync(request)).ReturnsAsync(response);

        //    // Act
        //    var result = await _controller.FundTransfer(request);

        //    // Assert
        //    var statusCodeResult = Assert.IsType<ObjectResult>(result);
        //    Assert.Equal(400, statusCodeResult.StatusCode);
        //    Assert.Equal(response, statusCodeResult.Value);
        //}

        //[Fact]
        //public async Task FundTransfer_Exception_ReturnsInternalServerError()
        //{
        //    // Arrange
        //    var request = new FundTransferRequestDto { /* initialize properties */ };
        //    _mockFundTransferService.Setup(service => service.TransferFundsAsync(request)).ThrowsAsync(new Exception("Test exception"));

        //    // Act
        //    var result = await _controller.FundTransfer(request);

        //    // Assert
        //    var statusCodeResult = Assert.IsType<ObjectResult>(result);
        //    Assert.Equal(500, statusCodeResult.StatusCode);
        //}
    }
}
