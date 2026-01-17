using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SunnyRewards.Helios.Tenant.Api.Controllers;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;
using SunnyRewards.Helios.Tenant.Core.Domain.Models;
using SunnyRewards.Helios.Tenant.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Tenant.Infrastructure.Services;
using SunnyRewards.Helios.Tenant.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Tenant.UnitTest.Fixture.MockDto;
using SunnyRewards.Helios.Tenant.UnitTest.Fixture.MockModel;
using SunnyRewards.Helios.Tenant.UnitTest.Fixture.MockRepo;
using System.Linq.Expressions;
using Xunit;

namespace SunnyRewards.Helios.Tenant.UnitTest.Controller
{
    public class CustomerControllerUnitTest
    {
        private readonly Mock<ILogger<CustomerController>> _CustomerControllerLogger;
        public readonly ICustomerService _customerService;
        public readonly Mock<ILogger<CustomerService>> _CustomerServicelogger;
        public readonly Mock<ICustomerRepo> _customerRepo;
        public readonly IMapper _mapper;
        private readonly CustomerController _CustomerController;

        public readonly Mock<ITenantRepo> _tenantRepo;
        public readonly Mock<ISponsorRepo> _sponsorRepo;
        private readonly Mock<ICustomerService> _customerServiceMock;
        private readonly CustomerController _controllerMockService;

        public CustomerControllerUnitTest()
        {

            _CustomerControllerLogger = new Mock<ILogger<CustomerController>>();
            _CustomerServicelogger = new Mock<ILogger<CustomerService>>();
            _customerRepo = new CustomerMockRepo();
            _tenantRepo = new TenantMockRepo();
            _sponsorRepo = new SponsorMockRepo();
            _customerServiceMock = new Mock<ICustomerService>();
            _mapper = new Mapper(new MapperConfiguration(
                          configure =>
                          {
                              configure.AddMaps(typeof(Infrastructure.Mappings.MappingProfile.CustomerMapping).Assembly.FullName);
                          }));
            _customerService = new CustomerService(_CustomerServicelogger.Object, _customerRepo.Object, _mapper , _tenantRepo.Object , _sponsorRepo.Object);
            _CustomerController = new CustomerController(_CustomerControllerLogger.Object, _customerService);
            _controllerMockService = new CustomerController(_CustomerControllerLogger.Object, _customerServiceMock.Object);
        }

        [Fact]
        public async void Should_Return_OkResponse_GetTenantCustomerDetails_Controller()
        {
            var customerRequestMockDto = new CustomerRequestMockDto();
            var response = await _CustomerController.GetTenantCustomerDetails(customerRequestMockDto);
            var result = response.Result as OkObjectResult;
            Assert.True(result?.Value != null);
            Assert.True(result.StatusCode == 200);
        }

        [Fact]
        public async void Should_Return_Exception_GetTenantCustomerDetails_Controller()
        {
            var customerRequestMockDto = new CustomerRequestMockDto();    
            var customerService = new Mock<ICustomerService>();
            _customerRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<CustomerModel, bool>>>(),false))
                         .ThrowsAsync(new Exception("Simulated exception"));
           await Assert.ThrowsAsync<Exception>(async () => await _CustomerController.GetTenantCustomerDetails(customerRequestMockDto));          
        }

        [Fact]
        public async void Should_Return_OkResponse_GetTenantCustomerDetails_Service()
        {
            var customerRequestMockDto = new CustomerRequestMockDto();
            var response = await _customerService.GetTenantCustomerDetails(customerRequestMockDto);
            Assert.True(response.customer != null);
        }

        [Fact]
        public async void Should_Return_NullCheck_GetTenantCustomerDetails_Service()
        {
            var customerRequestMockDto = new CustomerRequestMockDto();
            _customerRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<CustomerModel, bool>>>(), false))
                       .ReturnsAsync((CustomerMockModel)null);
            var response = await _customerService.GetTenantCustomerDetails(customerRequestMockDto);
            Assert.True(response.customer != null);
            Assert.True(response.ErrorCode != 400);
        }
        [Fact]
        public async void Should_Return_Exception_GetTenantCustomerDetails_Service()
        {
            var customerRequestMockDto = new CustomerRequestMockDto();
            var customerService = new Mock<ICustomerService>();
            _customerRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<CustomerModel, bool>>>(), false))
                         .ThrowsAsync(new Exception("Simulated exception"));
            await Assert.ThrowsAsync<Exception>(async () => await _customerService.GetTenantCustomerDetails(customerRequestMockDto));
        }

        [Fact]
        public async Task GetSponsorCustomerByTenant_ShouldReturnNotFound_WhenErrorCodeIs404()
        {
            // Arrange
            string tenantCode = "some-tenant-code";
            var response = new CustomerResponseDto { ErrorCode = 404 };
            _customerServiceMock
                .Setup(x => x.GetSponsorCustomerByTenant(tenantCode))
                .ReturnsAsync(response);

            // Act
            var result = await _controllerMockService.GetSponsorCustomerByTenant(new GetCustomerByTenantRequestDto() { TenantCode = tenantCode });

            // Assert
            var actionResult = Assert.IsType<ActionResult<CustomerResponseDto>>(result);
            var notFoundResult = Assert.IsType<ObjectResult>(actionResult.Result);
            Assert.Equal(response, notFoundResult.Value);
        }

        [Fact]
        public async Task GetSponsorCustomerByTenant_ShouldReturnInternalServerError_WhenErrorCodeIs500()
        {
            // Arrange
            string tenantCode = "some-tenant-code";
            var response = new CustomerResponseDto { ErrorCode = 500 };
            _customerServiceMock
                .Setup(x => x.GetSponsorCustomerByTenant(tenantCode))
                .ReturnsAsync(response);

            // Act
            var result = await _controllerMockService.GetSponsorCustomerByTenant(new GetCustomerByTenantRequestDto() { TenantCode = tenantCode });

            // Assert
            var actionResult = Assert.IsType<ActionResult<CustomerResponseDto>>(result);
            var statusCodeResult = Assert.IsType<ObjectResult>(actionResult.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
            Assert.Equal(response, statusCodeResult.Value);
        }

        [Fact]
        public async Task GetSponsorCustomerByTenant_ShouldReturnOk_WhenErrorCodeIsNot404Or500()
        {
            // Arrange
            string tenantCode = "some-tenant-code";
            var response = new CustomerResponseDto { ErrorCode = null };
            _customerServiceMock
                .Setup(x => x.GetSponsorCustomerByTenant(tenantCode))
                .ReturnsAsync(response);

            // Act
            var result = await _controllerMockService.GetSponsorCustomerByTenant(new GetCustomerByTenantRequestDto() { TenantCode = tenantCode });

            // Assert
            var actionResult = Assert.IsType<ActionResult<CustomerResponseDto>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            Assert.Equal(response, okResult.Value);
        }

        [Fact]
        public async Task GetSponsorCustomerByTenant_ShouldLogError_WhenExceptionIsThrown()
        {
            // Arrange
            string tenantCode = "some-tenant-code";
            var exception = new Exception("Something went wrong");
            _customerServiceMock
                .Setup(x => x.GetSponsorCustomerByTenant(tenantCode))
                .ThrowsAsync(exception);


            // Act
            var result = await _controllerMockService.GetSponsorCustomerByTenant(new GetCustomerByTenantRequestDto() { TenantCode = tenantCode });

            //Assert
            var actionResult = Assert.IsType<ActionResult<CustomerResponseDto>>(result);
            var objectResult = Assert.IsType<ObjectResult>(actionResult.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        [Fact]
        public async Task GetSponsorCustomerByTenant_ReturnsNotFound_WhenTenantDoesNotExist()
        {
            // Arrange
            var tenantCode = "invalidTenant";
            _tenantRepo
                .Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<TenantModel, bool>>>(), false));

            // Act
            var result = await _customerService.GetSponsorCustomerByTenant(tenantCode);

            // Assert
            Assert.Equal(StatusCodes.Status404NotFound, result.ErrorCode);
        }

        [Fact]
        public async Task GetSponsorCustomerByTenant_ReturnsCustomer_WhenDataIsFound()
        {
            // Arrange
            var tenantCode = "validTenant";
            var tenant = new TenantModel { TenantCode = tenantCode, SponsorId = 1 };
            var sponsor = new SponsorModel { SponsorId = 1, CustomerId = 1 };
            var customer = new CustomerModel { CustomerId = 1 };
            var customerDto = _mapper.Map<CustomerDto>(customer);

            _tenantRepo
                .Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<TenantModel, bool>>>(), false))
                .ReturnsAsync(tenant);
            _sponsorRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<SponsorModel, bool>>>(), false))
                .ReturnsAsync(sponsor);
            _customerRepo
                .Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<CustomerModel, bool>>>(), false))
                .ReturnsAsync(customer);

            // Act
            var result = await _customerService.GetSponsorCustomerByTenant(tenantCode);

            // Assert
            Assert.NotNull(result.customer);
        }

        [Fact]
        public async Task GetSponsorCustomerByTenant_ReturnsNotFound_WhenCustomerIsNotFound()
        {
            // Arrange
            var tenantCode = "validTenant";
            var tenant = new TenantModel { TenantCode = tenantCode, SponsorId = 1 };
            var sponsor = new SponsorModel { SponsorId = 1, CustomerId = 1 };

            _tenantRepo
                .Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<TenantModel, bool>>>(), false))
                .ReturnsAsync(tenant);
            _sponsorRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<SponsorModel, bool>>>(), false))
                .ReturnsAsync(sponsor);
            _customerRepo
                .Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<CustomerModel, bool>>>(), false));

            // Act
            var result = await _customerService.GetSponsorCustomerByTenant(tenantCode);

            // Assert
            Assert.Equal(StatusCodes.Status404NotFound, result.ErrorCode);
            Assert.Equal("Customer Not Found", result.ErrorMessage);
        }

        [Fact]
        public async Task GetAllCutomers_Should_Return_all_Customers()
        {
            // Arrange
            var customerModels = new List<CustomerModel>
            {
                new CustomerModel { CustomerCode = "cus-abcdefghijk", DeleteNbr = 0 },
                new CustomerModel { CustomerCode = "cus-abcdefghijk", DeleteNbr = 0 }
            };

            var customerDtos = new List<CustomerDto>
            {
                new CustomerDto { CustomerCode = "cus-abcdefghijk", CustomerId = 1},
                new CustomerDto { CustomerCode = "cus-abcdefghijk", CustomerId = 2}
            };


            var mockMapper = new Mock<IMapper>();

            _customerRepo.Setup(x => x.FindAllAsync()).ReturnsAsync(customerModels);

            mockMapper.Setup(mapper => mapper.Map<List<CustomerDto>>(customerModels)).Returns(customerDtos);

            // Act
            var result = await _CustomerController.GetAllCustomers();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<CustomersReponseDto>(okResult.Value);
            Assert.NotNull(response.Customers);
            Assert.Equal(2, response.Customers.Count);
            var actionResult = Assert.IsType<ActionResult<CustomersReponseDto>>(result);
            var objectResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            Assert.Equal(StatusCodes.Status200OK, objectResult.StatusCode);

        }

        [Fact]
        public async Task GetAllCutomers_Should_Return_NotFound()
        {
            // Arrange
            var customerModels = new List<CustomerModel>();

            var customerDtos = new List<CustomerDto>();
            
            var mockMapper = new Mock<IMapper>();

            _customerRepo.Setup(x => x.FindAllAsync()).ReturnsAsync(customerModels);

            mockMapper.Setup(mapper => mapper.Map<List<CustomerDto>>(customerModels)).Returns(customerDtos);

            // Act
            var result = await _CustomerController.GetAllCustomers();

            // Assert
            var actionResult = Assert.IsType<ActionResult<CustomersReponseDto>>(result);
            var objectResult = Assert.IsType<ObjectResult>(actionResult.Result);
            Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);

        }
        [Fact]
        public async Task GetAllCutomers_Should_Throws_Exception()
        {
            // Arrange
            _customerRepo.Setup(x => x.FindAllAsync()).ThrowsAsync(new Exception("simulated exception"));

            // Act
            var result = await _CustomerController.GetAllCustomers();

            // Assert
            var actionResult = Assert.IsType<ActionResult<CustomersReponseDto>>(result);
            var objectResult = Assert.IsType<ObjectResult>(actionResult.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);

        }

        [Fact]
        public async Task GetAllSponsors_Should_Return_all_Sponsors()
        {
            // Arrange
            var sponsorModels = new List<SponsorModel>
            {
                new SponsorModel { SponsorCode = "spo-abcdefghijk", DeleteNbr = 0 },
                new SponsorModel { SponsorCode = "spo-abcdefghijkgh", DeleteNbr = 0 }
            };

            var sponsorDtos = new List<SponsorDto>
            {
                new SponsorDto { SponsorCode = "spo-abcdefghijk", CustomerId = 1},
                new SponsorDto { SponsorCode = "spo-abcdefghijkgh", CustomerId = 2}
            };


            var mockMapper = new Mock<IMapper>();

            _sponsorRepo.Setup(x => x.FindAllAsync()).ReturnsAsync(sponsorModels);

            mockMapper.Setup(mapper => mapper.Map<List<SponsorDto>>(sponsorModels)).Returns(sponsorDtos);

            // Act
            var result = await _CustomerController.GetAllSponsors();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<SponsorsResponseDto>(okResult.Value);
            Assert.NotNull(response.Sponsors);
            Assert.Equal(2, response.Sponsors.Count);
            var actionResult = Assert.IsType<ActionResult<SponsorsResponseDto>>(result);
            var objectResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            Assert.Equal(StatusCodes.Status200OK, objectResult.StatusCode);

        }

        [Fact]
        public async Task GetAllSponsors_Should_Return_NotFound()
        {
            // Arrange
            var sponsorModels = new List<SponsorModel>();

            var sponsorDtos = new List<SponsorDto>();

            var mockMapper = new Mock<IMapper>();

            _sponsorRepo.Setup(x => x.FindAllAsync()).ReturnsAsync(sponsorModels);

            mockMapper.Setup(mapper => mapper.Map<List<SponsorDto>>(sponsorModels)).Returns(sponsorDtos);

            // Act
            var result = await _CustomerController.GetAllSponsors();

            // Assert
            var actionResult = Assert.IsType<ActionResult<SponsorsResponseDto>>(result);
            var objectResult = Assert.IsType<ObjectResult>(actionResult.Result);
            Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);

        }
        [Fact]
        public async Task GetAllSponsors_Should_Throws_Exception()
        {
            // Arrange
            _sponsorRepo.Setup(x => x.FindAllAsync()).ThrowsAsync(new Exception("simulated exception"));

            // Act
            var result = await _CustomerController.GetAllSponsors();

            // Assert
            var actionResult = Assert.IsType<ActionResult<SponsorsResponseDto>>(result);
            var objectResult = Assert.IsType<ObjectResult>(actionResult.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);

        }

        [Fact]
        public async Task CreateCustomer_ShouldReturnOk_WhenCustomerCreatedSuccessfully()
        {
            // Arrange
            var request = new CreateCustomerDto
            {
                CustomerCode = "customerCode",
                CustomerName = "Test",
                CustomerDescription = "some Description"

            };
            _customerRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<CustomerModel, bool>>>(), false)).ReturnsAsync((CustomerModel)null);

            // Act
            var result = await _CustomerController.CreateCustomer(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Fact]
        public async Task CreateCustomer_ShouldReturn409_WhenCustomer_already_exist()
        {
            // Arrange
            var request = new CreateCustomerDto
            {
                CustomerCode = "customerCode",
                CustomerName = "Test",
                CustomerDescription = "some Description"
                
            };

            // Act
            var result = await _CustomerController.CreateCustomer(request);

            // Assert
            var okResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status409Conflict, okResult.StatusCode);
        }

        [Fact]
        public async Task CreateCustomer_Should_Throws_Exception()
        {
            // Arrange
            var request = new CreateCustomerDto
            {
                CustomerCode = "customerCode",
                CustomerName = "Test",
                CustomerDescription = "some Description"

            };
            _customerRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<CustomerModel, bool>>>(), false)).ThrowsAsync(new Exception("simulated exception"));

            // Act
            var result = await _CustomerController.CreateCustomer(request);

            // Assert
            var okResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, okResult.StatusCode);
        }

        [Fact]
        public async Task CreateSponsor_ShouldReturnOk_When_sponsor_CreatedSuccessfully()
        {
            // Arrange
            var request = new CreateSponsorDto
            {
                CustomerId = 123,
                SponsorCode = "sponsorCode",
                SponsorName = "Test",
                SponsorDescription = "some Description"
            };
            _customerRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<CustomerModel, bool>>>(), false)).ReturnsAsync(new CustomerModel { CustomerId = 123});
            _sponsorRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<SponsorModel, bool>>>(), false)).ReturnsAsync((SponsorModel)null);

            // Act
            var result = await _CustomerController.CreateSponsor(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Fact]
        public async Task CreateSponsor_ShouldReturn_404_When_Customer_NotFound()
        {
            // Arrange
            var request = new CreateSponsorDto
            {
                SponsorCode = "sponsorCode",
                SponsorName = "Test",
                SponsorDescription = "some Description"
            };
            _customerRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<CustomerModel, bool>>>(), false)).ReturnsAsync((CustomerModel)null);

            // Act
            var result = await _CustomerController.CreateSponsor(request);

            // Assert
            var okResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status404NotFound, okResult.StatusCode);
        }

        [Fact]
        public async Task CreateSponsor_ShouldReturn_409_When_Sponsor_already_exist()
        {
            // Arrange
            var request = new CreateSponsorDto
            {
                SponsorCode = "sponsorCode",
                SponsorName = "Test",
                SponsorDescription = "some Description"
            };

            // Act
            var result = await _CustomerController.CreateSponsor(request);

            // Assert
            var okResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status409Conflict, okResult.StatusCode);
        }
        [Fact]
        public async Task CreateSponsor_Should_Throws_Exception()
        {
            // Arrange
            var request = new CreateSponsorDto
            {
                SponsorCode = "sponsorCode",
                SponsorName = "Test",
                SponsorDescription = "some Description"
            };
            _customerRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<CustomerModel, bool>>>(), false)).ThrowsAsync(new Exception("simulated exception"));

            // Act
            var result = await _CustomerController.CreateSponsor(request);

            // Assert
            var okResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, okResult.StatusCode);
        }

        [Fact]
        public async Task GetTenantSponsorCustomer_ShouldReturnOk_WhenValidTenantCode()
        {
            // Arrange
            var tenantCode = "VALID_TENANT";
            var responseDto = new TenantSponsorCustomerResponseDto();
            _tenantRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TenantModel, bool>>>(), false))
                       .ReturnsAsync(new TenantMockModel());

            // Act
            var result = await _CustomerController.GetTenantSponsorCustomer(tenantCode);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Fact]
        public async Task GetTenantSponsorCustomer_ShouldReturnNotFound_WhenInvalidTenantCode()
        {
            // Arrange
            var tenantCode = "INVALID_TENANT";
            var responseDto = new TenantSponsorCustomerResponseDto { ErrorCode = 404, ErrorMessage = "Invalid Tenant" };

            _tenantRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TenantModel, bool>>>(), false))
                       .ReturnsAsync((TenantModel)null);

            // Act
            var result = await _CustomerController.GetTenantSponsorCustomer(tenantCode);

            // Assert
            var notFoundResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task GetTenantSponsorCustomer_ShouldReturnNotFound_WhenSponsorNotFound()
        {
            // Arrange
            var tenantCode = "NO_SPONSOR";
            var responseDto = new TenantSponsorCustomerResponseDto { ErrorCode = 404, ErrorMessage = "Invalid Sponsor" };

            _sponsorRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<SponsorModel, bool>>>(), false))
                       .ReturnsAsync((SponsorModel)null);

            // Act
            var result = await _CustomerController.GetTenantSponsorCustomer(tenantCode);

            // Assert
            var notFoundResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task GetTenantSponsorCustomer_ShouldReturnNotFound_WhenCustomerNotFound()
        {
            // Arrange
            var tenantCode = "NO_CUSTOMER";
            var responseDto = new TenantSponsorCustomerResponseDto { ErrorCode = 404, ErrorMessage = "Invalid Customer" };

            _customerRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<CustomerModel, bool>>>(), false))
                       .ReturnsAsync((CustomerModel)null);

            // Act
            var result = await _CustomerController.GetTenantSponsorCustomer(tenantCode);

            // Assert
            var notFoundResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task GetTenantSponsorCustomer_ShouldReturnInternalServerError_OnException()
        {
            // Arrange
            var tenantCode = "EXCEPTION_CASE";
            _customerRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<CustomerModel, bool>>>(), false))
                    .ThrowsAsync(new Exception("Internal Error"));

            // Act
            var result = await _CustomerController.GetTenantSponsorCustomer(tenantCode);

            // Assert
            var errorResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, errorResult.StatusCode);
        }

        [Fact]
        public async Task GetCustomerSponsorTenants_ShouldReturnInternalServerError_OnException()
        {
            // Arrange
            var requestDto = new CustomerSponsorTenantsRequestDto
            {
                CustomerSponsorTenants = [new CustomerSponsorTenantRequestDto { CustomerCode = "cus-8d9e6f00eec8436a8251d55ff74b1642" }]
            };

            var responseDto = new CustomerSponsorTenantsResponseDto()
            {
                ErrorCode = StatusCodes.Status500InternalServerError,
                ErrorMessage = "Internal Server Error"
            };

            _customerRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<CustomerModel, bool>>>(), false))
                    .ThrowsAsync(new Exception("Internal Server Error"));

            // Act
            var result = await _CustomerController.GetCustomerSponsorTenants(requestDto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result.Result);

            Assert.Equal(responseDto.ErrorCode, objectResult.StatusCode);
            var response = _mapper.Map<CustomerSponsorTenantsResponseDto>(objectResult.Value);
            Assert.Equal(responseDto.ErrorCode, response.ErrorCode);
            Assert.Equal(responseDto.ErrorMessage, response.ErrorMessage);
            Assert.Null(response?.CustomerSponsorTenants);
        }

        [Fact]
        public async Task GetCustomerSponsorTenants_ShouldReturnBadRequest_WhenCustomerSponsorTenants_IsEmpty()
        {
            // Arrange
            var requestDto = new CustomerSponsorTenantsRequestDto();
            var responseDto = new TenantSponsorCustomerResponseDto 
            { 
                ErrorCode = StatusCodes.Status400BadRequest, 
                ErrorMessage = "Bad Request.CustomerSponsorTenant Array is required." 
            };

            // Act
            var result = await _CustomerController.GetCustomerSponsorTenants(requestDto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result.Result);

            Assert.Equal(responseDto.ErrorCode, objectResult.StatusCode);
            var response = _mapper.Map<CustomerSponsorTenantsResponseDto>(objectResult.Value);
            Assert.Equal(responseDto.ErrorCode, response.ErrorCode);
            Assert.Equal(responseDto.ErrorMessage, response.ErrorMessage);
            Assert.Null(response?.CustomerSponsorTenants);
        }

        [Fact]
        public async Task GetCustomerSponsorTenants_ShouldReturnOk_WithCustomer_WhenValidCustomerCode()
        {
            // Arrange
            var requestDto = new CustomerSponsorTenantsRequestDto
            {
                CustomerSponsorTenants = [new CustomerSponsorTenantRequestDto { CustomerCode = "cus-8d9e6f00eec8436a8251d55ff74b1642" }]
            };

            _customerRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<CustomerModel, bool>>>(), false))
                       .ReturnsAsync(new CustomerMockModel());

            // Act
            var result = await _CustomerController.GetCustomerSponsorTenants(requestDto);

            var okObjectResult = Assert.IsType<OkObjectResult>(result.Result);

            // Assert
            Assert.Equal(StatusCodes.Status200OK, okObjectResult.StatusCode);
            var response = _mapper.Map<CustomerSponsorTenantsResponseDto>(okObjectResult.Value);
            Assert.Null(response.ErrorCode);
            Assert.Null(response.ErrorMessage);
            Assert.Equal(1, response?.CustomerSponsorTenants?.Count);
        }

        [Fact]
        public async Task GetCustomerSponsorTenants_ShouldReturnOk_WithCustomerSponsor_WhenValidSponsorCode()
        {
            // Arrange
            var requestDto = new CustomerSponsorTenantsRequestDto
            {
                CustomerSponsorTenants = [new CustomerSponsorTenantRequestDto { SponsorCode = "ten-8d9e6f00eec8436a8251d55ff74b1642" }]
            };
            var responseDto = new TenantSponsorCustomerResponseDto();
            _sponsorRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<SponsorModel, bool>>>(), false))
                       .ReturnsAsync(new SponsorMockModel());
            _customerRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<CustomerModel, bool>>>(), false))
                       .ReturnsAsync(new CustomerMockModel());

            // Act
            var result = await _CustomerController.GetCustomerSponsorTenants(requestDto);

            var okObjectResult = Assert.IsType<OkObjectResult>(result.Result);

            // Assert
            Assert.Equal(StatusCodes.Status200OK, okObjectResult.StatusCode);
            var response = _mapper.Map<CustomerSponsorTenantsResponseDto>(okObjectResult.Value);
            Assert.Null(response.ErrorCode);
            Assert.Null(response.ErrorMessage);
            Assert.Equal(1, response?.CustomerSponsorTenants?.Count);
        }

        [Fact]
        public async Task GetCustomerSponsorTenants_ShouldReturnOk_WithCustomerSponsorTenant_WhenValidTenantCode()
        {
            // Arrange
            var requestDto = new CustomerSponsorTenantsRequestDto
            {
                CustomerSponsorTenants = [new CustomerSponsorTenantRequestDto { TenantCode = "ten-8d9e6f00eec8436a8251d55ff74b1642" }]
            };
            var responseDto = new TenantSponsorCustomerResponseDto();
            _tenantRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TenantModel, bool>>>(), false))
                       .ReturnsAsync(new TenantMockModel());
            _sponsorRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<SponsorModel, bool>>>(), false))
                       .ReturnsAsync(new SponsorMockModel());
            _customerRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<CustomerModel, bool>>>(), false))
                       .ReturnsAsync(new CustomerMockModel());

            // Act
            var result = await _CustomerController.GetCustomerSponsorTenants(requestDto);

            var okObjectResult = Assert.IsType<OkObjectResult>(result.Result);

            // Assert
            Assert.Equal(StatusCodes.Status200OK, okObjectResult.StatusCode);
            var response = _mapper.Map<CustomerSponsorTenantsResponseDto>(okObjectResult.Value);
            Assert.Null(response.ErrorCode);
            Assert.Null(response.ErrorMessage);
            Assert.Equal(1, response?.CustomerSponsorTenants?.Count);
        }
    }
}
