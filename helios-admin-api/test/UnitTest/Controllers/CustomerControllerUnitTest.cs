using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SunnyRewards.Helios.Admin.Api.Controllers;
using SunnyRewards.Helios.Admin.Core.Domain.Constants;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;
using Xunit;
using TaskAlias = System.Threading.Tasks.Task;

namespace SunnyRewards.Helios.Admin.UnitTest.Controllers
{
    public class CustomerControllerUnitTest
    {
        private readonly Mock<ILogger<CustomerController>> _controllerLogger;
        private readonly Mock<ILogger<CustomerService>> _customerServiceLogger;
        private readonly Mock<ITenantClient> _tenantClient;
        private readonly ICustomerService _customerService;
        private readonly CustomerController _customerController;

        public CustomerControllerUnitTest()
        {
            _controllerLogger = new Mock<ILogger<CustomerController>>();
            _customerServiceLogger = new Mock<ILogger<CustomerService>>();
            _tenantClient = new Mock<ITenantClient>();
            _customerService = new CustomerService(_customerServiceLogger.Object, _tenantClient.Object);
            _customerController = new CustomerController(_controllerLogger.Object, _customerService);
        }
        [Fact]
        public async TaskAlias GetCustomers_Should_Return_Success_When_Customers_Are_Fetched_Successfully()
        {
            // Arrange
            IDictionary<string, long> parameters = new Dictionary<string, long>();
            _tenantClient.Setup(x => x.Get<CustomersReponseDto>(Constant.Customers, parameters)).ReturnsAsync(new CustomersReponseDto
            {
                Customers = new List<CustomerDto>
                {
                    new CustomerDto(),
                    new CustomerDto()
                }
            });

            // Act
            var result = await _customerController.GetCustomers();

            // Assert
            Assert.NotNull(result);
            var actionResult = Assert.IsType<ActionResult<CustomersReponseDto>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }
        [Fact]
        public async TaskAlias GetCustomers_Should_Return_NotFound_When_Customers_Not_Found()
        {
            // Arrange
            IDictionary<string, long> parameters = new Dictionary<string, long>();
            _tenantClient.Setup(x => x.Get<CustomersReponseDto>(Constant.Customers, parameters)).ReturnsAsync(new CustomersReponseDto
            {
                ErrorCode = StatusCodes.Status404NotFound,
                ErrorMessage = "No customers found."
            });

            // Act
            var result = await _customerController.GetCustomers();

            // Assert
            Assert.NotNull(result);
            var actionResult = Assert.IsType<ActionResult<CustomersReponseDto>>(result);
            var notFoundResult = Assert.IsType<ObjectResult>(actionResult.Result);
            Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
        }
        [Fact]
        public async TaskAlias GetCustomers_Should_Return_InternalServerError_When_Exception_Thrown()
        {
            // Arrange

            IDictionary<string, long> parameters = new Dictionary<string, long>();
            _tenantClient.Setup(x => x.Get<CustomersReponseDto>(Constant.Customers, parameters)).ThrowsAsync(new Exception("Simulated Exception"));

            // Act
            var result = await _customerController.GetCustomers();

            // Assert
            Assert.NotNull(result);
            var actionResult = Assert.IsType<ActionResult<CustomersReponseDto>>(result);
            var errorResult = Assert.IsType<ObjectResult>(actionResult.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, errorResult.StatusCode);
        }
        [Fact]
        public async TaskAlias CreateCustomer_Should_Return_Success_When_Customer_Is_Created_Successfully()
        {
            // Arrange
            var mockRequest = new CreateCustomerDto { CustomerCode = "CUST123" };

            _tenantClient.Setup(x => x.Post<BaseResponseDto>(Constant.Customer, mockRequest)).ReturnsAsync(new BaseResponseDto());


            // Act
            var result = await _customerController.CreateCustomer(mockRequest);

            // Assert
            Assert.NotNull(result);
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }
        [Fact]
        public async TaskAlias CreateCustomer_Should_Return_Error_When_Customer_Already_Exist()
        {
            // Arrange
            var mockRequest = new CreateCustomerDto { CustomerCode = "CUST123" };
            var mockResponse = new BaseResponseDto
            {
                ErrorCode = StatusCodes.Status409Conflict,
            };
            _tenantClient.Setup(x => x.Post<BaseResponseDto>(Constant.Customer, mockRequest)).ReturnsAsync(mockResponse);

            // Act
            var result = await _customerController.CreateCustomer(mockRequest);

            // Assert
            Assert.NotNull(result);
            var errorResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status409Conflict, errorResult.StatusCode);
        }
        [Fact]
        public async TaskAlias CreateCustomer_Should_Return_InternalServerError_When_Exception_Is_Thrown()
        {
            // Arrange
            var mockRequest = new CreateCustomerDto { CustomerCode = "CUST123" };

            _tenantClient.Setup(x => x.Post<BaseResponseDto>(Constant.Customer, mockRequest)).ThrowsAsync(new Exception("simulated Exception"));

            // Act
            var result = await _customerController.CreateCustomer(mockRequest);

            // Assert
            Assert.NotNull(result);
            var errorResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, errorResult.StatusCode);

        }
        
    }
}
