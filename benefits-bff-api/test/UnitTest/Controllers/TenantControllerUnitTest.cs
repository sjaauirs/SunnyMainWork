using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Sunny.Benefits.Bff.Api.Controllers;
using Sunny.Benefits.Bff.Infrastructure.HttpClients.Interfaces;
using Sunny.Benefits.Bff.Infrastructure.Repositories.Interfaces;
using Sunny.Benefits.Bff.Infrastructure.Services.Interfaces;
using Sunny.Benefits.Bff.Infrastructure.Services;
using Sunny.Benefits.Bff.UnitTest.HttpClients;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;
using Sunny.Benefits.Bff.UnitTest.Fixtures.MockDtos;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Sunny.Benefits.Bff.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Models;
using Microsoft.AspNetCore.Http;

namespace Sunny.Benefits.Bff.UnitTest.Controllers
{
    public class TenantControllerUnitTest
    {

        private readonly Mock<ILogger<TenantController>> _tenantLogger;
        private readonly Mock<ILogger<TenantService>> _tenantServiceLogger;
        private readonly Mock<IUserClient> _userClient;
        private readonly Mock<ITenantClient> _tenantClient;
        private readonly Mock<ITenantService> _tenantServiceMock;
        private readonly ITenantService _tenantService;
        private readonly TenantController _tenantController;

        public TenantControllerUnitTest()
        {
            _tenantLogger = new Mock<ILogger<TenantController>>();
            _tenantServiceLogger = new Mock<ILogger<TenantService>>();
            _userClient = new UserClientMock();
            _tenantClient = new TenantClientMock();
            _tenantServiceMock = new Mock<ITenantService>();
            _tenantService = new TenantService(_tenantServiceLogger.Object, _tenantClient.Object, _userClient.Object);

            _tenantController = new TenantController(_tenantLogger.Object, _tenantServiceMock.Object);
        }
        //#1
        [Fact]
        public async System.Threading.Tasks.Task Should_GetTenantByConsumerCode()
        {
           string ConsumerCode = "cmr-c457c5257c59451d8a93ea941a9f2e0a";

            var expectedResponse = new GetTenantResponseMockDto();
            _tenantServiceMock.Setup(x => x.GetTenantByConsumerCode(ConsumerCode)).ReturnsAsync(expectedResponse);

           
            var response = await _tenantController.GetTenantByConsumerCode(ConsumerCode);
            var result = response.Result as OkObjectResult;
            Assert.True(result?.Value != null);
            Assert.True(result.StatusCode == 200);
        }

        [Fact]
        public async System.Threading.Tasks.Task GetTenantByConsumerCode_Should_Return_NotFoundResponse()
        {
            string consumerCode = "cmr-c457c5257c59451d8a93ea941a9f2e0a";
            var mockService = new Mock<ITenantService>();
            var user = new Mock<IUserClient>();
            var tenant = new Mock<ITenantClient>();
            user.Setup(client => client.Post<GetConsumerResponseDto>("consumer/get-consumer", It.IsAny<BaseRequestDto>()))
                      .ReturnsAsync(new GetConsumerResponseMockDto());
            tenant.Setup(c => c.Post<TenantDto>("tenant/get-by-tenant-code", It.IsAny<GetTenantCodeRequestDto>()))
            .ReturnsAsync(new TenantMockDto());
            var controller = new TenantController(_tenantLogger.Object, mockService.Object);
            var Response = await controller.GetTenantByConsumerCode(consumerCode);
            var badRequestResult = Response.Result as NotFoundResult;
            Assert.True(badRequestResult?.StatusCode == 404);
        }

        [Fact]
        public async System.Threading.Tasks.Task GetTenantByConsumerCode_Should_Return_Exception_Catch_In_Controller()
        {
            string consumerCode = "cmr-";
            _tenantServiceMock.Setup(s => s.GetTenantByConsumerCode(consumerCode)).ThrowsAsync(new Exception("Simulated exception"));
            var result = await _tenantController.GetTenantByConsumerCode(consumerCode);
            var badRequestResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.True(badRequestResult.StatusCode == StatusCodes.Status500InternalServerError);
        }


        [Fact]
        public async System.Threading.Tasks.Task GetTenantByConsumerCode_Should_Return_Exception_Catch_In_Service()
        {
            string consumerCode = "cmr-c457c5257c59451d8a93ea941a9f2e0a";
            var consumerRequest = new BaseRequestDto { consumerCode = consumerCode };
            
            _userClient.Setup(client => client.Post<GetConsumerResponseDto>(It.IsAny<string>(), consumerRequest))
                           .ThrowsAsync(new Exception("Simulated exception"));
            var response = await _tenantController.GetTenantByConsumerCode(consumerCode);
            var RequestResult = Assert.IsType<NotFoundResult>(response.Result);
            Assert.True(RequestResult.StatusCode == StatusCodes.Status404NotFound);
        }
        [Fact]
        public async System.Threading.Tasks.Task GetTenantByConsumerCode_WhenConsumerIsNull_ReturnsEmptyResponseDto()
        {

            _userClient.Setup(client => client.Post<GetConsumerResponseDto>("consumer/get-consumer", It.IsAny<BaseRequestDto>()))
                      .ReturnsAsync(new GetConsumerResponseMockDto() { Consumer = null });
            _tenantClient.Setup(c => c.Post<TenantDto>("tenant/get-by-tenant-code", It.IsAny<GetTenantCodeRequestDto>()))
              .ReturnsAsync(new TenantMockDto() { TenantCode = null });

            var response = await _tenantService.GetTenantByConsumerCode(null);
            Assert.True(response?.Tenant?.TenantCode == null);
        }




    }
}
