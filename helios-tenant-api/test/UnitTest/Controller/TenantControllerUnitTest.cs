using AutoMapper;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Helpers;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using SunnyRewards.Helios.Tenant.Api.Controllers;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos.Json;
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
    public class TenantControllerUnitTest
    {
        private readonly Mock<ILogger<TenantController>> _controllerLogger;
        private readonly Mock<ILogger<TenantService>> _serviceLogger;
        private readonly ITenantService _tenantService;
        private readonly TenantController _tenantController;
        private readonly IMapper _mapper;
        private readonly Mock<ITenantRepo> _tenantRepo;
        private readonly Mock<IVault> _vault;
        public readonly Mock<ICustomerRepo> _customerRepo;
        private readonly Mock<ISponsorRepo> _sponsorRepo;
        public TenantControllerUnitTest()
        {
            _controllerLogger = new Mock<ILogger<TenantController>>();
            _serviceLogger = new Mock<ILogger<TenantService>>();
            _tenantRepo = new TenantMockRepo();
            _customerRepo = new CustomerMockRepo();
            _sponsorRepo = new SponsorMockRepo();
            _vault = new Mock<IVault>();
            _mapper = new Mapper(new MapperConfiguration(
                          configure =>
                         {
                             configure.AddMaps(typeof(Infrastructure.Mappings.MappingProfile.TenantMapping).Assembly.FullName);
                         }));
            _tenantService = new TenantService(_serviceLogger.Object, _mapper, _tenantRepo.Object, _customerRepo.Object, _vault.Object, _sponsorRepo.Object);
            _tenantController = new TenantController(_controllerLogger.Object, _tenantService);
        }

        [Fact]
        public async void Should_GetPartnerCode()
        {
            var tenantByPartnerCodeRequestDto = new GetTenantByPartnerCodeRequestMockDto();
            var response = await _tenantController.GetByPartnerCode(tenantByPartnerCodeRequestDto);
            var result = response.Result as OkObjectResult;
            Assert.True(result?.Value != null);
            Assert.True(result.StatusCode == 200);
        }


        [Fact]
        public async void Should_Not_Get_PartnerCode()
        {
            var tenantByPartnerCodeRequestDto = new GetTenantByPartnerCodeRequestMockDto();
            var tenantRepo = new Mock<ITenantRepo>();
            var tenantService = new TenantService(_serviceLogger.Object, _mapper, tenantRepo.Object, _customerRepo.Object, _vault.Object, _sponsorRepo.Object);
            var tenantController = new TenantController(_controllerLogger.Object, tenantService);
            var response = await tenantController.GetByPartnerCode(tenantByPartnerCodeRequestDto);
            var result = response.Result as NotFoundResult;
            Assert.True(result?.StatusCode == 404);
        }


        [Fact]
        public async void Should_Return_Exception_Catch_PartnerCode()
        {
            var controllerLogger = new Mock<ILogger<TenantController>>();
            var tenantService = new Mock<ITenantService>();
            var tenantController = new TenantController(controllerLogger.Object, tenantService.Object);
            var tenantByPartnerCodeRequestDto = new GetTenantByPartnerCodeRequestMockDto();
            tenantService.Setup(x => x.GetTenantByPartnerCode(It.IsAny<GetTenantByPartnerCodeRequestMockDto>()))
                           .ThrowsAsync(new InvalidOperationException("PartnerCode exception"));
            var result = await tenantController.GetByPartnerCode(tenantByPartnerCodeRequestDto);
        }

        [Fact]
        public async Task Should_Return_Null_WhenNoPartnerCodeFound()
        {
            var logger = new Mock<ILogger<TenantService>>();
            var tenantRepo = new Mock<ITenantRepo>();
            var mapper = new Mock<IMapper>();
            var tenantService = new TenantService(logger.Object, mapper.Object, tenantRepo.Object, _customerRepo.Object, _vault.Object, _sponsorRepo.Object);
            var requestDto = new GetTenantByPartnerCodeRequestDto()
            {
                PartnerCode = null
            };
            tenantRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TenantModel, bool>>>(), false))
                .ReturnsAsync((TenantMockModel)null);
            var response = await tenantService.GetTenantByPartnerCode(requestDto);
            Assert.True(response.Tenant == null);
        }

        [Fact]
        public async Task GetTenantByPartnerCode_Should_Handle_Exception()
        {
            var logger = new Mock<ILogger<TenantService>>();
            var tenantRepo = new Mock<ITenantRepo>();
            var mapper = new Mock<IMapper>();
            var tenantService = new TenantService(logger.Object, mapper.Object, tenantRepo.Object, _customerRepo.Object, _vault.Object, _sponsorRepo.Object);
            var requestDto = new GetTenantByPartnerCodeRequestDto();

            tenantRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TenantModel, bool>>>(), false))
                .ThrowsAsync(new InvalidOperationException("Repository exception"));

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await tenantService.GetTenantByPartnerCode(requestDto));

            Assert.Equal("Repository exception", exception.Message);
        }

        [Fact]
        public async void Should_GetTenantCode()
        {
            var getTenantCodeRequestDto = new GetTenantCodeRequestMockDto();
            var response = await _tenantController.GetByTenantCode(getTenantCodeRequestDto);
            var result = response.Result as ObjectResult;
            Assert.True(result?.Value != null);
            Assert.True(result.StatusCode == 200);
        }


        [Fact]
        public async void Should_Not_Get_TenantCode()
        {
            var getTenantCodeRequestDto = new GetTenantCodeRequestMockDto();
            var tenantRepo = new Mock<ITenantRepo>();
            var tenantService = new TenantService(_serviceLogger.Object, _mapper, tenantRepo.Object, _customerRepo.Object, _vault.Object, _sponsorRepo.Object);
            var tenantController = new TenantController(_controllerLogger.Object, tenantService);
            var response = await tenantController.GetByTenantCode(getTenantCodeRequestDto);
            var result = response.Result as NotFoundResult;
            Assert.True(result?.StatusCode == 404);
        }



        [Fact]
        public async void Should_Return_Null_WhenNoTenantCodeFound()
        {
            var logger = new Mock<ILogger<TenantService>>();
            var tenantRepo = new Mock<ITenantRepo>();
            var mapper = new Mock<IMapper>();
            var tenantService = new TenantService(logger.Object, mapper.Object, tenantRepo.Object, _customerRepo.Object, _vault.Object, _sponsorRepo.Object);
            var getTenantCodeRequestDto = new GetTenantCodeRequestMockDto()
            {
                TenantCode = null
            };
            tenantRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TenantModel, bool>>>(), false))
              .ReturnsAsync((TenantMockModel)null);

            var response = await tenantService.GetByTenantCode(getTenantCodeRequestDto);
            Assert.True(response.TenantCode == null);
        }


        [Fact]
        public async void Should_Return_Exceptions_Catch_TenantCode()
        {
            var controllerLogger = new Mock<ILogger<TenantController>>();
            var tenantService = new Mock<ITenantService>();
            var tenantController = new TenantController(controllerLogger.Object, tenantService.Object);
            var getTenantCodeRequestDto = new GetTenantCodeRequestMockDto();
            tenantService.Setup(x => x.GetByTenantCode(It.IsAny<GetTenantCodeRequestMockDto>()))
                .ThrowsAsync(new Exception("TenantCode Exception"));
            var result = await tenantController.GetByTenantCode(getTenantCodeRequestDto);
        }
        [Fact]
        public async Task GetByTenantCode_Should_Handle_Exception()
        {
            var logger = new Mock<ILogger<TenantService>>();
            var tenantRepo = new Mock<ITenantRepo>();
            var mapper = new Mock<IMapper>();
            var tenantService = new TenantService(logger.Object, mapper.Object, tenantRepo.Object, _customerRepo.Object, _vault.Object, _sponsorRepo.Object);
            var requestDto = new GetTenantCodeRequestDto();

            tenantRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TenantModel, bool>>>(), false))
                .ThrowsAsync(new InvalidOperationException("Repository exception"));

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await tenantService.GetByTenantCode(requestDto));

            Assert.Equal("Repository exception", exception.Message);
        }

        [Fact]
        public async Task Should_Post_ValidateApiKey()
        {
            var apiKey = "valid-api-key";
            _tenantRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TenantModel, bool>>>(), false))
                       .ReturnsAsync(new TenantMockModel { ApiKey = apiKey });
            var result = await _tenantController.ValidateApiKey(apiKey);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task Should_Post_validateApiKey_Exception()
        {
            var apiKey = "your_api_key";
            var tenantService = new Mock<ITenantService>();
            var logger = new Mock<ILogger<TenantController>>();
            tenantService.Setup(x => x.ValidateApiKey(apiKey)).ThrowsAsync(new Exception("Mock exception"));
            var tenantController = new TenantController(logger.Object, tenantService.Object);
            var result = await tenantController.ValidateApiKey(apiKey);
            Assert.False(result.Value);
        }

        [Fact]
        public async Task Should_Post_ValidApiKey_Service()
        {
            var apiKey = "jxGl0FBPhRbe0A0BW4X5tA7vUoV/qVbjleQAu3CrRUIvmTRGqxfQF4+fg/tHBpyba9aWJFyrSQFmWEuv8Ph44u6QjWPj+1MfkayVYpSNT4NRyoL6CPZyGo1brnV1sQvR";
            var encryptionKey = "yVOgU6MJruN1oNTuBxYX4rahJn5NPSs4AJ7e5w9+rTw=";
            _vault.Setup(x => x.GetSecret("DF_API_ENC_KEY")).ReturnsAsync(encryptionKey);
            var encryptionHelper = new EncryptionHelper();
            var decryptedKey = encryptionHelper.Decrypt(apiKey, Convert.FromBase64String(encryptionKey));
            _tenantRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TenantModel, bool>>>(), false))
                       .ReturnsAsync(new TenantMockModel { ApiKey = "19afb4b6-dvtu-5869-ndvr-025d2a617b78" });
            var result = await _tenantService.ValidateApiKey(apiKey);
            Assert.True(result);
        }
        [Fact]
        public async Task Should_Post_ValidApiKey_NullCheck_Service()
        {
            var apiKey = "213wretry";
            _tenantRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TenantModel, bool>>>(), false))
                       .ReturnsAsync(new TenantMockModel());
            var result = await _tenantService.ValidateApiKey(apiKey);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task Should_Post_ValidApiKey_Exception_Service()
        {
            var apiKey = "jxGl0FBPhRbe0A0BW4X5tA7vUoV/qVbjleQAu3CrRUIvmTRGqxfQF4+fg/tHBpyba9aWJFyrSQFmWEuv8Ph44u6QjWPj+1MfkayVYpSNT4NRyoL6CPZyGo1brnV1sQvR";
            var encryptionKey = "yVOgU6MJruN1oNTuBxYX4rahJn5NPSs4AJ7e5w9+rTw=";
            _vault.Setup(x => x.GetSecret("DF_API_ENC_KEY")).ReturnsAsync(encryptionKey);
            var encryptionHelper = new EncryptionHelper();
            var decryptedKey = encryptionHelper.Decrypt(apiKey, Convert.FromBase64String(encryptionKey));
            _tenantRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TenantModel, bool>>>(), false))
                .ThrowsAsync(new Exception("Mock exception"));
            var tenantService = new TenantService(_serviceLogger.Object, _mapper, _tenantRepo.Object, _customerRepo.Object, _vault.Object, _sponsorRepo.Object);
            await Assert.ThrowsAsync<Exception>(async () => await tenantService.ValidateApiKey(apiKey));
        }

        [Fact]
        public async Task Should_return_false_when_secret_notfound()
        {
            var apiKey = "jxGl0FBPhRbe0A0BW4X5tA7vUoV/qVbjleQAu3CrRUIvmTRGqxfQF4+fg/tHBpyba9aWJFyrSQFmWEuv8Ph44u6QjWPj+1MfkayVYpSNT4NRyoL6CPZyGo1brnV1sQvR";
            new TenantMockModel { ApiKey = "19afb4b6-dvtu-5869-ndvr-025d2a617b78" };
            _tenantRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TenantModel, bool>>>(), false))
                .ReturnsAsync(new TenantMockModel { ApiKey = apiKey });
            var tenantService = new TenantService(_serviceLogger.Object, _mapper, _tenantRepo.Object, _customerRepo.Object, _vault.Object, _sponsorRepo.Object);
            var result = await tenantService.ValidateApiKey(apiKey);

            Assert.False(result);
        }

        [Fact]
        public async Task Should_return_false_when_apiKey_notmatched()
        {
            var apiKey = "jxGl0FBPhRbe0A0BW4X5tA7vUoV/qVbjleQAu3CrRUIvmTRGqxfQF4+fg/tHBpyba9aWJFyrSQFmWEuv8Ph44u6QjWPj+1MfkayVYpSNT4NRyoL6CPZyGo1brnV1sQvR";
            new TenantMockModel { ApiKey = "19afb4b6-dvtu-5869-ndvr-025d2a617b78" };
            var secret = "yVOgU6MJruN1oNTuBxYX4rahJn5NPSs4AJ7e5w9+rTw=";
            _vault.Setup(x => x.GetSecret("DF_API_ENC_KEY")).ReturnsAsync(secret);

            var encryptionHelper = new EncryptionHelper();
            var decryptedKey = encryptionHelper.Decrypt(apiKey, Convert.FromBase64String(secret));


            _tenantRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TenantModel, bool>>>(), false))
                .ReturnsAsync(new TenantMockModel { ApiKey = apiKey });
            var tenantService = new TenantService(_serviceLogger.Object, _mapper, _tenantRepo.Object, _customerRepo.Object, _vault.Object, _sponsorRepo.Object);
            var result = await tenantService.ValidateApiKey(apiKey);

            Assert.False(result);
        }

        [Fact]
        public async Task Validate_Api_Key_Should_return_true()
        {
            var apiKey = "jxGl0FBPhRbe0A0BW4X5tA7vUoV/qVbjleQAu3CrRUIvmTRGqxfQF4+fg/tHBpyba9aWJFyrSQFmWEuv8Ph44u6QjWPj+1MfkayVYpSNT4NRyoL6CPZyGo1brnV1sQvR";
            var secret = "yVOgU6MJruN1oNTuBxYX4rahJn5NPSs4AJ7e5w9+rTw=";
            _vault.Setup(x => x.GetSecret("DF_API_ENC_KEY")).ReturnsAsync(secret);

            var encryptionHelper = new EncryptionHelper();
            var decryptedKey = encryptionHelper.Decrypt(apiKey, Convert.FromBase64String(secret));


            _tenantRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TenantModel, bool>>>(), false))
                .ReturnsAsync(new TenantMockModel { ApiKey = "19afb4b6-dvtu-5869-ndvr-025d2a617b78" });
            var tenantService = new TenantService(_serviceLogger.Object, _mapper, _tenantRepo.Object, _customerRepo.Object, _vault.Object, _sponsorRepo.Object);
            var result = await tenantService.ValidateApiKey(apiKey);

            Assert.True(result);
        }



        #region Get Tenant By EncKeyId Unit Tests

        [Fact]
        public async void GetTenantByEncKeyId_Should_Return_Bad_Result()
        {
            var getTenantByEncKeyIdRequestDto = new GetTenantByEncKeyIdRequestDto()
            {
                EncKeyId = string.Empty
            };
            var response = await _tenantController.GetTenantByEncKeyId(getTenantByEncKeyIdRequestDto);
            var result = response.Result as ObjectResult;
            Assert.True(result?.Value != null);
            Assert.True(result.StatusCode == 400);
        }

        [Fact]
        public async void GetTenantByEncKeyId_Should_Return_Ok_Result()
        {
            var getTenantByEncKeyIdRequestDto = new GetTenantByEncKeyIdRequestDto()
            {
                EncKeyId = Guid.NewGuid().ToString("N")
            };
            var response = await _tenantController.GetTenantByEncKeyId(getTenantByEncKeyIdRequestDto);
            var result = response.Result as OkObjectResult;
            Assert.True(result?.Value != null);
            Assert.True(result.StatusCode == 200);
        }


        [Fact]
        public async void GetTenantByEncKeyId_Should_Not_Found_Result()
        {
            var getTenantByEncKeyIdRequestDto = new GetTenantByEncKeyIdRequestDto()
            {
                EncKeyId = Guid.NewGuid().ToString("N")
            };
            _tenantRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TenantModel, bool>>>(), false));
            var response = await _tenantController.GetTenantByEncKeyId(getTenantByEncKeyIdRequestDto);
            var result = response.Result as ObjectResult;
            Assert.True(result?.Value != null);
            Assert.True(result.StatusCode == 404);
        }


        [Fact]
        public async void GetTenantByEncKeyId_Should_Return_InternalSererError_When_Exception_Occurred()
        {
            var getTenantByEncKeyIdRequestDto = new GetTenantByEncKeyIdRequestDto()
            {
                EncKeyId = Guid.NewGuid().ToString("N")
            };
            _tenantRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TenantModel, bool>>>(), false))
                .ThrowsAsync(new InvalidOperationException("PartnerCode exception"));
            var response = await _tenantController.GetTenantByEncKeyId(getTenantByEncKeyIdRequestDto);
            var result = response.Result as ObjectResult;
            Assert.True(result?.Value != null);
            Assert.True(result.StatusCode == 500);
        }

        #endregion

        [Fact]
        public async Task CreateTenant_ShouldReturnOk_WhenTenantCreatedSuccessfully()
        {
            // Arrange
            var request = new CreateTenantRequestDto
            {
                CustomerCode = "Customer123",
                SponsorCode = "Sponsor123",
                Tenant = new PostTenantDto { TenantCode = "Tenant123", TenantName = "sample-Tenant" }
            };


            // Act
            var result = await _tenantController.CreateTenant(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Fact]
        public async Task CreateTenant_ShouldReturnNotFoundResult_WhenCustomerNotExist()
        {
            // Arrange
            var request = new CreateTenantRequestDto
            {
                CustomerCode = "Customer123",
                SponsorCode = "Sponsor123",
                Tenant = new PostTenantDto { TenantCode = "Tenant123" }
            };
            _customerRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<CustomerModel, bool>>>(), false));

            // Act
            var result = await _tenantController.CreateTenant(request);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);
        }

        [Fact]
        public async Task CreateTenant_ShouldReturnNotFoundResult_WhenSponsorNotExist()
        {
            // Arrange
            var request = new CreateTenantRequestDto
            {
                CustomerCode = "Customer123",
                SponsorCode = "Sponsor123",
                Tenant = new PostTenantDto { TenantCode = "Tenant123" }
            };
            _sponsorRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<SponsorModel, bool>>>(), false));

            // Act
            var result = await _tenantController.CreateTenant(request);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);
        }

        [Fact]
        public async Task CreateTenant_ShouldReturnInternalServerError_WhenExceptionOccurs()
        {
            // Arrange
            var request = new CreateTenantRequestDto
            {
                CustomerCode = "Customer123",
                SponsorCode = "Sponsor123",
                Tenant = new PostTenantDto { TenantCode = "Tenant123" }
            };

            _customerRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<CustomerModel, bool>>>(), false)).
                ThrowsAsync(new Exception("Testing"));

            // Act
            var result = await _tenantController.CreateTenant(request);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        [Fact]
        public async Task CreateTenant_Should_Generate_Random_PartnerCode_When_Duplicate_Exists()
        {
            // Arrange
            string tenantCode = "TEN-12345";
            var createTenantRequest = new CreateTenantRequestDto
            {
                CustomerCode = "Customer123",
                SponsorCode = "Sponsor123",
                Tenant = new PostTenantDto 
                {
                    TenantName = "TenantName",
                    TenantCode = tenantCode,
                    PartnerCode = "PARTNER-ABC",
                    PlanYear = 2025
                }
            };

            var existingTenant = new TenantModel
            {
                TenantCode = tenantCode,
                PartnerCode = "PARTNER-OLD",
                DeleteNbr = 0
            };

            var duplicateTenant = new TenantModel
            {
                TenantCode = "TEN-67890",
                PartnerCode = "PARTNER-ABC",
                DeleteNbr = 0
            };

            _tenantRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<TenantModel, bool>>>(), false))
                .ReturnsAsync(existingTenant);

            _tenantRepo.Setup(repo => repo.FindOneAsync(It.Is<Expression<Func<TenantModel, bool>>>(x =>
                    x.Compile().Invoke(duplicateTenant)), false))
                .ReturnsAsync(duplicateTenant);

            _tenantRepo.Setup(repo => repo.CreateAsync(It.IsAny<TenantModel>()))
                .ReturnsAsync(duplicateTenant);

            // Act
            var result = await _tenantController.CreateTenant(createTenantRequest);

            // Assert
            Assert.NotNull(result);
            var objectResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, objectResult.StatusCode);
        }

        [Fact]
        public async Task GetAllTenants_Should_Return_all_Tenants()
        {
            // Arrange
            var tenantModels = new List<TenantModel>
            {
                new TenantModel { TenantCode = "tenantcode1", DeleteNbr = 0 },
                new TenantModel { TenantCode = "tenantcode2", DeleteNbr = 0 }
            };

            var tenantDtos = new List<TenantDto>
            {
                new TenantDto { TenantCode = "tenantcode1", DeleteNbr = 0 },
                new TenantDto { TenantCode = "tenantcode2", DeleteNbr = 0 }
            };

            
            var mockMapper = new Mock<IMapper>();

            _tenantRepo.Setup(x => x.FindAllAsync()).ReturnsAsync(tenantModels);

            mockMapper.Setup(mapper => mapper.Map<List<TenantDto>>(tenantModels)).Returns(tenantDtos);

            // Act
            var result = await _tenantController.GetAllTenants();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<TenantsResponseDto>(okResult.Value);

            Assert.NotNull(response.Tenants);
            Assert.Equal(2, response.Tenants.Count);
            var actionResult = Assert.IsType<ActionResult<TenantsResponseDto>>(result);
            var objectResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            Assert.Equal(StatusCodes.Status200OK, objectResult.StatusCode);

        }

        [Fact]
        public async Task GetAllTenants_Should_Return_NotFound()
        {
            // Arrange
            var tenantModels = new List<TenantModel>();

            var tenantDtos = new List<TenantDto>();
            
            var mockMapper = new Mock<IMapper>();

            _tenantRepo.Setup(x => x.FindAllAsync()).ReturnsAsync(tenantModels);

            mockMapper.Setup(mapper => mapper.Map<List<TenantDto>>(tenantModels)).Returns(tenantDtos);

            // Act
            var result = await _tenantController.GetAllTenants();

            // Assert
            var actionResult = Assert.IsType<ActionResult<TenantsResponseDto>>(result);

            var objectResult = Assert.IsType<ObjectResult>(actionResult.Result);

            Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);
        }

        [Fact]
        public async Task GetAllTenants_Should_Throws_Exception()
        {
            // Arrange

            _tenantRepo.Setup(x => x.FindAllAsync()).Throws(new Exception());

            // Act
            var result = await _tenantController.GetAllTenants();

            // Assert
            var actionResult = Assert.IsType<ActionResult<TenantsResponseDto>>(result);

            var objectResult = Assert.IsType<ObjectResult>(actionResult.Result);

            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }
        [Fact]
        public async Task UpdateTenant_ShouldReturnOk_WhenUpdateIsSuccessful()
        {
            // Arrange
            string tenantCode = "TestTenant";
            var updateRequest = new UpdateTenantDto { TenantCode = "TestTenant", PlanYear = 2024 };
            var tenant = new TenantModel
            {
                TenantCode = "TestTenant",
                PlanYear = 2024,
                PeriodStartTs = new DateTime(2024, 1, 1),
                PeriodEndTs = new DateTime(2024, 12, 31, 23, 59, 59),
                DeleteNbr = 0,
                UpdateTs = DateTime.UtcNow
            };

            _tenantRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<TenantModel, bool>>>(), false))
                .ReturnsAsync(new TenantModel { TenantCode = "TestTenant", PlanYear = 2023 });
            _tenantRepo.Setup(repo => repo.UpdateAsync(It.IsAny<TenantModel>()))
                      .ReturnsAsync(tenant);

            // Act
            var result = await _tenantController.UpdateTenant(tenantCode, updateRequest);

            // Assert
            Assert.NotNull(result);
            var actionResult = Assert.IsType<ActionResult<UpdateTenantResponseDto>>(result);
            var objectResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            Assert.Equal(StatusCodes.Status200OK, objectResult.StatusCode);
        }
        [Fact]
        public async Task UpdateTenant_ShouldReturnNotFound_When_Tenant_NotFound()
        {
            // Arrange
            string tenantCode = "TestTenant";
            var updateRequest = new UpdateTenantDto { TenantCode = "TestTenant", PlanYear = 2024 };

            _tenantRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<TenantModel, bool>>>(), false));

            // Act
            var result = await _tenantController.UpdateTenant(tenantCode, updateRequest);

            // Assert
            Assert.NotNull(result);
            var actionResult = Assert.IsType<ActionResult<UpdateTenantResponseDto>>(result);
            var objectResult = Assert.IsType<ObjectResult>(actionResult.Result);
            Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);
        }
        [Fact]
        public async Task UpdateTenant_Should_throws_Exception()
        {
            // Arrange
            string tenantCode = "TestTenant";
            var updateRequest = new UpdateTenantDto { TenantCode = "TestTenant", PlanYear = 2024 };
            _tenantRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<TenantModel, bool>>>(), false))
                .ThrowsAsync(new Exception("simulated exception"));

            // Act
            var result = await _tenantController.UpdateTenant(tenantCode, updateRequest);

            // Assert
            Assert.NotNull(result);
            var actionResult = Assert.IsType<ActionResult<UpdateTenantResponseDto>>(result);
            var objectResult = Assert.IsType<ObjectResult>(actionResult.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }
        [Fact]
        public async Task UpdateTenant_ShouldReturnBadRequest_When_TenantCode_MisMatchPathAndBody()
        {
            // Arrange
            string tenantCode = "TenantCode";
            var updateRequest = new UpdateTenantDto { TenantCode = "TestTenant", PlanYear = 2024 };

            // Act
            var result = await _tenantController.UpdateTenant(tenantCode, updateRequest);

            // Assert
            Assert.NotNull(result);
            var actionResult = Assert.IsType<ActionResult<UpdateTenantResponseDto>>(result);
            var objectResult = Assert.IsType<ObjectResult>(actionResult.Result);
            Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
        }

        [Fact]
        public async Task UpdateTenant_Should_Generate_Random_PartnerCode_When_Duplicate_Exists()
        {
            // Arrange
            string tenantCode = "TEN-12345";
            var updateTenantRequest = new UpdateTenantDto
            {
                TenantCode = tenantCode,
                PartnerCode = "PARTNER-ABC",
                PlanYear = 2025
            };

            var existingTenant = new TenantModel
            {
                TenantCode = tenantCode,
                PartnerCode = "PARTNER-OLD",
                DeleteNbr = 0
            };

            var duplicateTenant = new TenantModel
            {
                TenantCode = "TEN-67890",
                PartnerCode = "PARTNER-ABC",
                DeleteNbr = 0
            };

            _tenantRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<TenantModel, bool>>>(), false))
                .ReturnsAsync(existingTenant);

            _tenantRepo.Setup(repo => repo.FindOneAsync(It.Is<Expression<Func<TenantModel, bool>>>(x =>
                    x.Compile().Invoke(duplicateTenant)), false))
                .ReturnsAsync(duplicateTenant);

            _tenantRepo.Setup(repo => repo.UpdateAsync(It.IsAny<TenantModel>()))
                .ReturnsAsync(duplicateTenant);

            // Act
            var result = await _tenantController.UpdateTenant(tenantCode, updateTenantRequest);

            // Assert
            Assert.NotNull(result);
            var actionResult = Assert.IsType<ActionResult<UpdateTenantResponseDto>>(result);
            var objectResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            Assert.Equal(StatusCodes.Status200OK, objectResult.StatusCode);
        }

        [Fact]
        public void TenantAttributeDto_ShouldSerializeAndDeserializeCorrectly()
        {
            // Arrange
            var original = new TenantAttributeDto
            {
                PickAPurseOnboardingEnabled = true,
                AutosweepSweepstakesReward = false,
                ConsumerWallet = new ConsumerWallet
                {
                    OwnerMaximum = 1000.50,
                    WalletMaximum = 5000,
                    ContributorMaximum = 2500.75,
                    IndividualWallet = true
                },
                MembershipWallet = new MembershipWallet
                {
                    EarnMaximum = 1500.25
                }
            };

            // Act
            var json = JsonConvert.SerializeObject(original);
            var deserialized = JsonConvert.DeserializeObject<TenantAttributeDto>(json);

            // Assert
            Assert.NotNull(deserialized);
            Assert.True(deserialized.PickAPurseOnboardingEnabled);
            Assert.False(deserialized.AutosweepSweepstakesReward);

            Assert.NotNull(deserialized.ConsumerWallet);
            Assert.Equal(1000.50, deserialized.ConsumerWallet.OwnerMaximum);
            Assert.Equal(5000, deserialized.ConsumerWallet.WalletMaximum);
            Assert.Equal(2500.75, deserialized.ConsumerWallet.ContributorMaximum);
            Assert.True(deserialized.ConsumerWallet.IndividualWallet);

            Assert.NotNull(deserialized.MembershipWallet);
            Assert.Equal(1500.25, deserialized.MembershipWallet.EarnMaximum);
        }

        [Fact]
        public void TenantAttributeDto_ShouldHandleMissingConsumerWallet()
        {
            // Arrange
            var json = @"{
                ""pickAPurseOnboardingEnabled"": true,
                ""autosweepSweepstakesReward"": true,
                ""membershipWallet"": {
                    ""earnMaximum"": 2000
                }
            }";

            // Act
            var deserialized = JsonConvert.DeserializeObject<TenantAttributeDto>(json);

            // Assert
            Assert.NotNull(deserialized);
            Assert.True(deserialized.PickAPurseOnboardingEnabled);
            Assert.True(deserialized.AutosweepSweepstakesReward);
            Assert.Null(deserialized.ConsumerWallet);
            Assert.NotNull(deserialized.MembershipWallet);
            Assert.Equal(2000, deserialized.MembershipWallet.EarnMaximum);
        }

        [Fact]
        public void ConsumerWallet_Default_IndividualWallet_ShouldBeFalse()
        {
            // Arrange
            var wallet = new ConsumerWallet();

            // Assert
            Assert.False(wallet.IndividualWallet);
        }

    }
}

