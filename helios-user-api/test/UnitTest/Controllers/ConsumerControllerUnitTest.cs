using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NHibernate;
using SunnyRewards.Helios.Common.Core.Repositories;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Api.Controllers;
using SunnyRewards.Helios.User.Core.Domain.Constant;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.enums;
using SunnyRewards.Helios.User.Core.Domain.Models;
using SunnyRewards.Helios.User.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.User.Infrastructure.Mappings;
using SunnyRewards.Helios.User.Infrastructure.Repositories;
using SunnyRewards.Helios.User.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.User.Infrastructure.Services;
using SunnyRewards.Helios.User.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.User.UnitTest.Fixtures.MockDto;
using SunnyRewards.Helios.User.UnitTest.Fixtures.MockHttpClient;
using SunnyRewards.Helios.User.UnitTest.Fixtures.MockModels;
using SunnyRewards.Helios.User.UnitTest.Fixtures.MockRepositories;
using SunnyRewards.Helios.User.UnitTest.Helpers;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using System.Linq.Expressions;
using Xunit;

namespace SunnyRewards.Helios.User.UnitTest.Controllers
{
    public class ConsumerControllerUnitTest
    {
        private readonly Mock<ILogger<ConsumerService>> _consumerServiceLogger;
        private readonly Mock<ILogger<ConsumerController>> _consumerControllerLogger;
        private readonly IMapper _mapper;
        private readonly Mock<IConsumerRepo> _consumerRepo;
        private readonly Mock<IRoleRepo> _roleRepo;
        private readonly Mock<IPersonRepo> _personRepo;
        private readonly Mock<IPersonRoleRepo> _personRoleRepo;
        private readonly Mock<NHibernate.ISession> _session;
        private readonly Mock<ITenantClient> _tenantClient;
        private readonly Mock<IAddressTypeService> _addressTypeService;
        private readonly Mock<IUploadAgreementPDFService> _uploadpdfService;
        private readonly IConsumerService _consumerService;
        private readonly ConsumerController _consumerController;
        private readonly Mock<IPersonAddressRepo> _personAddressRepo;
        private readonly Mock<IMemberImportFileDataRepo> _memberImportFileDataRepo;
        private readonly Mock<IPhoneNumberRepo> _phoneNumberRepo;
        private readonly Mock<IPhoneTypeService> _phoneTypeService;
        private readonly Mock<IConsumerETLRepo> _consumerETLRepo;
        private readonly Mock<IEventService> _eventService;
        private readonly Mock<IHeliosEventPublisher<AgreementsVerifiedEventDto>> _heliosEventPublisher;
        
        //private readonly Mock<ISession> _session;
        public ConsumerControllerUnitTest()
        {
            _consumerControllerLogger = new Mock<ILogger<ConsumerController>>();
            _consumerServiceLogger = new Mock<ILogger<ConsumerService>>();
            _mapper = new Mapper(new MapperConfiguration(
                configure =>
                {
                    configure.AddMaps(typeof(Infrastructure.Mappings.MappingProfile.ConsumerMapping).Assembly.FullName);
                }));
            _consumerRepo = new ConsumerMockRepo();
            _roleRepo = new RoleMockRepo();
            _personRepo = new PersonMockRepo();
            _personRoleRepo = new PersonRoleMockRepo();
            _session = new Mock<NHibernate.ISession>();
            _tenantClient = new TenantClientMock();
            _uploadpdfService = new Mock<IUploadAgreementPDFService>();
            _addressTypeService = new Mock<IAddressTypeService>();
            _personAddressRepo = new PersonAddressMockRepo();
            _memberImportFileDataRepo = new Mock<IMemberImportFileDataRepo>();
            _eventService = new Mock<IEventService>();
            _phoneNumberRepo = new Mock<IPhoneNumberRepo>();
            _phoneTypeService = new Mock<IPhoneTypeService>();
            _consumerETLRepo = new Mock<IConsumerETLRepo>();
            _heliosEventPublisher = new Mock<IHeliosEventPublisher<AgreementsVerifiedEventDto>>();

            _consumerService = new ConsumerService(_consumerServiceLogger.Object, _mapper, _consumerRepo.Object, _personRepo.Object, _roleRepo.Object,
                _session.Object, _personRoleRepo.Object, _tenantClient.Object, _addressTypeService.Object, _uploadpdfService.Object, _personAddressRepo.Object, _phoneNumberRepo.Object, _memberImportFileDataRepo.Object , _eventService.Object, _consumerETLRepo.Object, _heliosEventPublisher.Object);
            _consumerController = new ConsumerController(_consumerControllerLogger.Object, _consumerService);
        }

        [Fact]
        public async Task Should_Get_Consumer()
        {
            var consumerRequestMockDto = new GetConsumerRequestMockDto();
            var consumerMap = new ConsumerMap();
            var consumerResponseMockDto = await _consumerController.GetConsumer(consumerRequestMockDto);
            var result = consumerResponseMockDto.Result as OkObjectResult;
            Assert.True(result?.StatusCode == 200);
        }

        [Fact]
        public async Task Should_Not_Get_Consumer_For_InValid_ConsumerCode()
        {
            GetConsumerRequestMockDto consumerRequestMockDto = new GetConsumerRequestMockDto();
            _consumerRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false)).ReturnsAsync(new ConsumerModel());
            var response = await _consumerController.GetConsumer(consumerRequestMockDto);
            var result = response.Result as NotFoundObjectResult;
            Assert.True(result?.StatusCode == 404);
        }

        [Fact]
        public async Task Should_Catch_GetConsumer_Controller_Level_Exception()
        {
            var mockService = new Mock<IConsumerService>();
            mockService.Setup(s => s.GetConsumerData(It.IsAny<GetConsumerRequestDto>())).ThrowsAsync(new Exception("intended exception"));
            var controller = new ConsumerController(_consumerControllerLogger.Object, mockService.Object);
            var dto = new GetConsumerRequestMockDto();
            var result = await controller.GetConsumer(dto);
            Assert.True(result?.Value?.ErrorMessage == "intended exception");
        }

        [Fact]
        public async Task Should_Catch_GetConsumerData_Service_Level_Exception()
        {
            _consumerRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false)).ThrowsAsync(new Exception("intended exception"));
            var consumerService = new ConsumerService(_consumerServiceLogger.Object, _mapper, _consumerRepo.Object, _personRepo.Object, _roleRepo.Object, _session.Object, _personRoleRepo.Object, _tenantClient.Object, _addressTypeService.Object, _uploadpdfService.Object, _personAddressRepo.Object, _phoneNumberRepo.Object, _memberImportFileDataRepo.Object, _eventService.Object, _consumerETLRepo.Object, _heliosEventPublisher.Object);
            var dto = new GetConsumerRequestMockDto();
            var result = await consumerService.GetConsumerData(dto);
            Assert.True(result.ErrorMessage == "intended exception");
        }

        [Fact]
        public async Task Should_Get_Consumer_By_MemNbr()
        {
            var consumerRequestMockDto = new GetConsumerByMemNbrRequestMockDto();
            var consumerResponseMockDto = await _consumerController.GetConsumerByMemId(consumerRequestMockDto);
            var result = consumerResponseMockDto.Result as OkObjectResult;
            Assert.True(result?.StatusCode == 200);
        }

        [Fact]
        public async Task Should_Not_Get_Consumer_By_MemNbr()
        {
            var consumerRequestMockDto = new GetConsumerByMemNbrRequestMockDto();
            _consumerRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false)).ReturnsAsync(new ConsumerModel() { });
            var response = await _consumerController.GetConsumerByMemId(consumerRequestMockDto);
            var result = response.Result as NotFoundObjectResult;
            Assert.Null(result);
        }

        [Fact]
        public async Task Should_Catch_GetConsumerByMemNbr_Controller_Level_Exception()
        {
            var mockService = new Mock<IConsumerService>();
            mockService.Setup(s => s.GetConsumerByMemId(It.IsAny<GetConsumerByMemIdRequestDto>()))
                .ThrowsAsync(new Exception("intended exception"));
            var controller = new ConsumerController(_consumerControllerLogger.Object, mockService.Object);
            var consumerRequestMockDto = new GetConsumerByMemNbrRequestMockDto();
            var result = await controller.GetConsumerByMemId(consumerRequestMockDto);
            Assert.True(result?.Value?.ErrorMessage == "intended exception");
        }

        [Fact]
        public async Task Should_Catch_GetConsumerByMemNbr_Service_Level_Exception()
        {
            _consumerRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false))
                .ThrowsAsync(new Exception("intended exception"));
            var consumerService = new ConsumerService(_consumerServiceLogger.Object, _mapper,
                _consumerRepo.Object, _personRepo.Object, _roleRepo.Object, _session.Object, _personRoleRepo.Object, _tenantClient.Object, _addressTypeService.Object, _uploadpdfService.Object, _personAddressRepo.Object, _phoneNumberRepo.Object, _memberImportFileDataRepo.Object, _eventService.Object, _consumerETLRepo.Object, _heliosEventPublisher.Object);
            var consumerRequestMockDto = new GetConsumerByMemNbrRequestMockDto();
            var result = await consumerService.GetConsumerByMemId(consumerRequestMockDto);
            Assert.True(result.ErrorMessage == "intended exception");
        }

        [Fact]
        public async Task Should_Returns_oKResult_For_PostConsumerData()
        {
            var consumerDataRequestDto = new ConsumerDataMockDto();
            var response = await _consumerController.CreateConsumers(new List<ConsumerDataDto> { consumerDataRequestDto });
            var result = response.Result as OkObjectResult;
            Assert.True(result?.StatusCode == 200);
        }

        [Fact]
        public async Task Catch_Exception_For_PostConsumerData()
        {
            var consumerService = new Mock<IConsumerService>();
            var consumerLogger = new Mock<ILogger<ConsumerController>>();
            var controller = new ConsumerController(consumerLogger.Object, consumerService.Object);
            var consumerDataRequestDto = new List<ConsumerDataDto>();

            consumerService.Setup(service => service.CreateConsumers(consumerDataRequestDto))
                .ThrowsAsync(new Exception("Simulated error"));
            var result = await controller.CreateConsumers(consumerDataRequestDto);
            Assert.Empty(result.Value);
        }

        [Fact]
        public async Task Should_Return_oKResult_CreateConsumerData()
        {
            var transactionMock = new Mock<ITransaction>();
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);
            var consumerService = new Mock<IConsumerService>();
            consumerService.Setup(service => service.CreateConsumers(It.IsAny<IList<ConsumerDataDto>>()))
                         .ReturnsAsync(new List<ConsumerDataResponseDto>());
            var consumerDataRequestDto = new ConsumerDataMockDto()
            {
                Person = new PersonDto(),

                Consumer = new ConsumerDto()
                {
                    TenantCode = "TenantCode123"
                }

            };
            var res = await _consumerController.CreateConsumers(new List<ConsumerDataDto> { consumerDataRequestDto });
            var result = res.Result as OkObjectResult;
            Assert.True(result?.Value != null);
            Assert.True(result?.StatusCode == 200);
        }
       
        [Fact]
        public async Task Should_Return_oKResult_consumerAttributeNull_CreateConsumerData()
        {
            var transactionMock = new Mock<ITransaction>();
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);
            var consumerService = new Mock<IConsumerService>();
            consumerService.Setup(service => service.CreateConsumers(It.IsAny<IList<ConsumerDataDto>>()))
                         .ReturnsAsync(new List<ConsumerDataResponseDto>());
            var consumerDataRequestDto = new ConsumerDataMockDto()
            {
                Person = new PersonDto(),

                Consumer = new ConsumerDto {
                    ConsumerId = 3,
                    PersonId = 120,
                    TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4",
                    ConsumerCode = "cmr-bjuebdf-492f-46i4-bh55-5a2b0134cbc",
                    Registered = true,
                    Eligible = true,
                    MemberNbr = "78b1bhf61-2e75-4029-8e81-3jjy47654a8",
                    RegistrationTs = DateTime.Now,
                    EligibleStartTs = DateTime.Now,
                    EligibleEndTs = DateTime.Now,
                    CreateTs = DateTime.Now,
                    UpdateTs = DateTime.Now,
                    CreateUser = "sunny",
                    UpdateUser = "sunny rewards",
                    DeleteNbr = 1,
                    SubscriberMemberNbr = "78b1bhf61-2e75-4029-8e81-3jjy47654a8",
                    RegionCode = "US",
                    PlanId = "plan",
                    PlanType = "plan",
                    SubgroupId = "subgroup",
                    SubsciberMemberNbrPrefix = "subsciber",
                    MemberNbrPrefix = "member",
                    SubscriberOnly = true,
                }

            };
            // Arrange
            var expectedResponse = new TenantSponsorCustomerResponseDto
            {
                Customer = new CustomerDto { CustomerCode = "CUST123" },
                Sponsor = new SponsorDto { SponsorCode = "SPON456" }
            };
            _tenantClient
                .Setup(x => x.Get<TenantSponsorCustomerResponseDto>(
                    It.Is<string>(s => s.Contains(Constant.GetTenantSponsorCustomer)),
                    It.IsAny<Dictionary<string, long>>()
                ))
                .ReturnsAsync(expectedResponse);
            _consumerRepo.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false))
                           .ReturnsAsync((ConsumerModel)null);
            var roleSubscriber = new RoleModel { RoleId = 1, RoleName = "subscriber" };
            _roleRepo.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<RoleModel, bool>>>(), false))
                     .ReturnsAsync(roleSubscriber);

            _personRepo.Setup(p => p.FindOneAsync(It.IsAny<Expression<Func<PersonModel, bool>>>(), false))
                       .ReturnsAsync((PersonModel)null);

            _personRepo.Setup(p => p.CreateAsync(It.IsAny<PersonModel>()))
                       .ReturnsAsync(new PersonModel { PersonId = 123, Email = "test@example.com" });

            _personRoleRepo.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<PersonRoleModel, bool>>>(), false))
                           .ReturnsAsync((PersonRoleModel)null);

            _personRoleRepo.Setup(r => r.CreateAsync(It.IsAny<PersonRoleModel>()))
                           .ReturnsAsync(new PersonRoleModel { RoleId = 1, PersonId = 123 });

            _addressTypeService.Setup(r => r.GetAddressTypeById(It.IsAny<long>())).ReturnsAsync(new GetAddressTypeResponseDto());

            _personRepo.Setup(p => p.CreateAsync(It.IsAny<PersonModel>()))
                       .ReturnsAsync(new PersonModel { PersonId = 123, Email = "test@example.com" });

            _personRepo.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<PersonModel, bool>>>(), false))
                       .ReturnsAsync(new PersonModel { PersonId = 123, Email = "test@example.com" });

            _personRoleRepo.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<PersonRoleModel, bool>>>(), false))
                           .ReturnsAsync(new PersonRoleModel { RoleId = 1, PersonId = 123 });
            var res = await _consumerController.CreateConsumers(new List<ConsumerDataDto> { consumerDataRequestDto });
            var result = res.Result as OkObjectResult;
            Assert.True(result?.Value != null);
            Assert.True(result?.StatusCode == 200);
        }

        [Fact]
        public async Task Should_Return_oKResult_When_PersonRole_Is_Null_CreateConsumerData()
        {
            var transactionMock = new Mock<ITransaction>();
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);
            var consumerService = new Mock<IConsumerService>();
            consumerService.Setup(service => service.CreateConsumers(It.IsAny<IList<ConsumerDataDto>>()))
                         .ReturnsAsync(new List<ConsumerDataResponseDto>());
            var consumerDataRequestDto = new ConsumerDataMockDto()
            {
                Person = new PersonDto(),

                Consumer = new ConsumerDto()

            };
            // Arrange
            var expectedResponse = new TenantSponsorCustomerResponseDto
            {
                Customer = new CustomerDto { CustomerCode = "CUST123" },
                Sponsor = new SponsorDto { SponsorCode = "SPON456" }
            };
            _tenantClient
                .Setup(x => x.Get<TenantSponsorCustomerResponseDto>(
                    It.Is<string>(s => s.Contains(Constant.GetTenantSponsorCustomer)),
                    It.IsAny<Dictionary<string, long>>()
                ))
                .ReturnsAsync(expectedResponse);
            

            var res = await _consumerController.CreateConsumers(new List<ConsumerDataDto> { consumerDataRequestDto });
            var result = res.Result as OkObjectResult;
            Assert.True(result?.Value != null);
            Assert.True(result?.StatusCode == 200);
        }

        [Fact]
        public async Task Should_Return_OkResult_When_PersonRole_Is_Null_And_TenantSponsorCustomerResponse_Exists()
        {
            // Arrange
            var transactionMock = new Mock<ITransaction>();
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);
    
            var consumerService = new Mock<IConsumerService>();
            consumerService.Setup(service => service.CreateConsumers(It.IsAny<IList<ConsumerDataDto>>() ))
                           .ReturnsAsync(new List<ConsumerDataResponseDto>());
    
            var consumerDataRequestDto = new ConsumerDataDto
            {
                Person = new PersonDto { Email = "test@example.com", PersonId = 123 },
                Consumer = new ConsumerDto { TenantCode = "TEN001" }
            };

            var expectedResponse = new TenantSponsorCustomerResponseDto
            {
                Customer = new CustomerDto { CustomerCode = "CUST123" },
                Sponsor = new SponsorDto { SponsorCode = "SPON456" }
            };
    
            _tenantClient
                .Setup(x => x.Get<TenantSponsorCustomerResponseDto>(
                    It.Is<string>(s => s.Contains(Constant.GetTenantSponsorCustomer)),
                    It.IsAny<Dictionary<string, long>>()
                ))
                .ReturnsAsync(expectedResponse);
    
            var roleSubscriber = new RoleModel { RoleId = 1, RoleName = "subscriber" };
            _roleRepo.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<RoleModel, bool>>>(), false))
                     .ReturnsAsync(roleSubscriber);

            _personRepo.Setup(p => p.FindOneAsync(It.IsAny<Expression<Func<PersonModel, bool>>>(), false))
                       .ReturnsAsync((PersonModel)null);
    
            _personRepo.Setup(p => p.CreateAsync(It.IsAny<PersonModel>()))
                       .ReturnsAsync(new PersonModel { PersonId = 123, Email = "test@example.com" });
    
            _personRoleRepo.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<PersonRoleModel, bool>>>(), false))
                           .ReturnsAsync((PersonRoleModel)null);
    
            _personRoleRepo.Setup(r => r.CreateAsync(It.IsAny<PersonRoleModel>()))
                           .ReturnsAsync(new PersonRoleModel { RoleId = 1, PersonId = 123 });

            _consumerRepo.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false))
                           .ReturnsAsync((ConsumerModel)null);

            _personRepo.Setup(p => p.CreateAsync(It.IsAny<PersonModel>()))
                       .ReturnsAsync(new PersonModel { PersonId = 123, Email = "test@example.com" });

            _personRepo.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<PersonModel, bool>>>(), false))
                       .ReturnsAsync(new PersonModel { PersonId = 123, Email = "test@example.com" });

            _personRoleRepo.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<PersonRoleModel, bool>>>(), false))
                           .ReturnsAsync(new PersonRoleModel { RoleId = 1, PersonId = 123 });

            var res = await _consumerController.CreateConsumers(new List<ConsumerDataDto> { consumerDataRequestDto });
            var result = res.Result as OkObjectResult;
    
            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
            var data = _mapper.Map<List<ConsumerDataResponseDto>>(result.Value);
            Assert.NotNull(data);
            Assert.Equal(StatusCodes.Status409Conflict, data[0].ErrorCode);
        }

        [Theory]
        [InlineData("{\r\n  \"apps\": [\r\n    \"REWARDS\",\r\n    \"BENEFITS\"\r\n  ],\r\n  \"benefitsOptions\": {\r\n    \"cardIssueFlowType\": [\r\n      \"TASK_COMPLETION_CHECK\"\r\n    ],\r\n    \"disableOnboardingFlow\": false,\r\n    \"autoCompleteTaskOnLogin\": true,\r\n    \"taskCompletionCheckCode\": [\r\n      \"trw-1f76cf0007594d16b1afe5df45178730\"\r\n    ],\r\n    \"manualCardRequestRequired\": true\r\n  }\r\n}")]
        [InlineData("{\r\n  \"apps\": [\r\n    \"REWARDS\",\r\n    \"BENEFITS\"\r\n  ],\r\n  \"benefitsOptions\": {\r\n    \"cardIssueFlowType\": [\r\n      \"TASK_COMPLETION_CHECK\"\r\n    ],\r\n    \"disableOnboardingFlow\": false,\r\n    \"autoCompleteTaskOnLogin\": true,\r\n    \"taskCompletionCheckCode\": [\r\n      \"trw-1f76cf0007594d16b1afe5df45178730\"\r\n    ],\r\n    \"manualCardRequestRequired\": true,\r\n    \"reactivateDeletedConsumer\": true\r\n  }\r\n}")]
        public async Task Should_Return_OkResult_And_Update_Consumer_When_Consumer_Is_Soft_Deleted_And_Tenant_Has_ReactiveDeletedConsumer(string tenantOption)
        {
            // Arrange
            var transactionMock = new Mock<ITransaction>();
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);

            var consumerService = new Mock<IConsumerService>();
            consumerService.Setup(service => service.CreateConsumers(It.IsAny<IList<ConsumerDataDto>>()))
                           .ReturnsAsync(new List<ConsumerDataResponseDto>());

            var consumerDataRequestDto = new ConsumerDataDto
            {
                Person = new PersonDto { Email = "test@example.com", PersonId = 123 },
                Consumer = new ConsumerDto { TenantCode = "TEN001" }
            };

            var expectedResponse = new TenantSponsorCustomerResponseDto
            {
                Customer = new CustomerDto { CustomerCode = "CUST123" },
                Sponsor = new SponsorDto { SponsorCode = "SPON456" }
            };

            _tenantClient
                .Setup(x => x.Get<TenantSponsorCustomerResponseDto>(
                    It.Is<string>(s => s.Contains(Constant.GetTenantSponsorCustomer)),
                    It.IsAny<Dictionary<string, long>>()
                ))
                .ReturnsAsync(expectedResponse);

            var expectedTenantResponse = new TenantDto
            {
                TenantOption = tenantOption
            };

            _tenantClient
                .Setup(x => x.Post<TenantDto>(
                    It.Is<string>(s => s.Contains(Constant.GetTenantByTenantCode)),
                    It.IsAny<GetTenantCodeRequestDto>()
                ))
                .ReturnsAsync(expectedTenantResponse);

            var roleSubscriber = new RoleModel { RoleId = 1, RoleName = "subscriber" };
            _roleRepo.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<RoleModel, bool>>>(), false))
                     .ReturnsAsync(roleSubscriber);

            _personRepo.Setup(p => p.FindOneAsync(It.IsAny<Expression<Func<PersonModel, bool>>>(), false))
                       .ReturnsAsync((PersonModel)null);
    
            _personRepo.Setup(p => p.CreateAsync(It.IsAny<PersonModel>()))
                       .ReturnsAsync(new PersonModel { PersonId = 123, Email = "test@example.com" });
    
            _personRoleRepo.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<PersonRoleModel, bool>>>(), false))
                           .ReturnsAsync((PersonRoleModel)null);
    
            _personRoleRepo.Setup(r => r.CreateAsync(It.IsAny<PersonRoleModel>()))
                           .ReturnsAsync(new PersonRoleModel { RoleId = 1, PersonId = 123 });

            _consumerRepo.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false))
                           .ReturnsAsync((ConsumerModel)null);

            _personRepo.Setup(p => p.CreateAsync(It.IsAny<PersonModel>()))
                       .ReturnsAsync(new PersonModel { PersonId = 123, Email = "test@example.com" });

            _personRepo.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<PersonModel, bool>>>(), false))
                       .ReturnsAsync(new PersonModel { PersonId = 123, Email = "test@example.com" });

            _personRoleRepo.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<PersonRoleModel, bool>>>(), false))
                           .ReturnsAsync(new PersonRoleModel { RoleId = 1, PersonId = 123 });

            var res = await _consumerController.CreateConsumers(new List<ConsumerDataDto> { consumerDataRequestDto });
            var result = res.Result as OkObjectResult;
    
            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
            var data = _mapper.Map<List<ConsumerDataResponseDto>>(result.Value);
            Assert.NotNull(data);
            Assert.Equal(StatusCodes.Status409Conflict, data[0].ErrorCode);
        }

        [Fact]
        public async Task Should_Return_Not_OkResult_When_PersonRole_Is_Null_And_TenantSponsorCustomerResponse_Not_Exists()
        {
            // Arrange
            var transactionMock = new Mock<ITransaction>();
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);

            var consumerService = new Mock<IConsumerService>();
            consumerService.Setup(service => service.CreateConsumers(It.IsAny<IList<ConsumerDataDto>>()))
                           .ReturnsAsync(new List<ConsumerDataResponseDto>());

            var consumerDataRequestDto = new ConsumerDataDto
            {
                Person = new PersonDto { Email = "test@example.com", PersonId = 123 },
                Consumer = new ConsumerDto { TenantCode = "TEN001" }
            };

            var expectedResponse = new TenantSponsorCustomerResponseDto
            {
                Customer = new CustomerDto { CustomerCode = "CUST123" },
                Sponsor = new SponsorDto { SponsorCode = "SPON456" }
            };

            
            var roleSubscriber = new RoleModel { RoleId = 1, RoleName = "subscriber" };
            _roleRepo.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<RoleModel, bool>>>(), false))
                     .ReturnsAsync(roleSubscriber);

            _personRepo.Setup(p => p.FindOneAsync(It.IsAny<Expression<Func<PersonModel, bool>>>(), false))
                       .ReturnsAsync((PersonModel)null);

            _personRepo.Setup(p => p.CreateAsync(It.IsAny<PersonModel>()))
                       .ReturnsAsync(new PersonModel { PersonId = 123, Email = "test@example.com" });

            _personRoleRepo.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<PersonRoleModel, bool>>>(), false))
                           .ReturnsAsync((PersonRoleModel)null);

            _personRoleRepo.Setup(r => r.CreateAsync(It.IsAny<PersonRoleModel>()))
                           .ReturnsAsync(new PersonRoleModel { RoleId = 1, PersonId = 123 });

            _consumerRepo.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false))
                           .ReturnsAsync((ConsumerModel)null);

            _personRepo.Setup(p => p.CreateAsync(It.IsAny<PersonModel>()))
                       .ReturnsAsync(new PersonModel { PersonId = 123, Email = "test@example.com" });

            _tenantClient
                .Setup(x => x.Get<TenantSponsorCustomerResponseDto>(
                    It.Is<string>(s => s.Contains(Constant.GetTenantSponsorCustomer)),
                    It.IsAny<Dictionary<string, long>>()
                ))
                .ReturnsAsync((TenantSponsorCustomerResponseDto)null);
            
            _personRepo.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<PersonModel, bool>>>(), false))
                       .ReturnsAsync(new PersonModel { PersonId = 123, Email = "test@example.com" });

            var res = await _consumerController.CreateConsumers(new List<ConsumerDataDto> { consumerDataRequestDto });
            var result = res.Result as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
            var data = _mapper.Map<List<ConsumerDataResponseDto>>(result.Value);
            Assert.NotNull(data);
            Assert.Equal(StatusCodes.Status409Conflict, data[0].ErrorCode);
        }

        [Fact]
        public async Task Should_Return_Conflict_OkResult_When_PersonRole_Is_Not_Created_And_TenantSponsorCustomerResponse_Not_Exists()
        {
            // Arrange
            var transactionMock = new Mock<ITransaction>();
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);

            var consumerService = new Mock<IConsumerService>();
            consumerService.Setup(service => service.CreateConsumers(It.IsAny<IList<ConsumerDataDto>>()))
                           .ReturnsAsync(new List<ConsumerDataResponseDto>());

            var consumerDataRequestDto = new ConsumerDataDto
            {
                Person = new PersonDto { Email = "test@example.com", PersonId = 123 },
                Consumer = new ConsumerDto { TenantCode = "TEN001" }
            };

            var expectedResponse = new TenantSponsorCustomerResponseDto
            {
                Customer = new CustomerDto { CustomerCode = "CUST123" },
                Sponsor = new SponsorDto { SponsorCode = "SPON456" }
            };

            _tenantClient
                .Setup(x => x.Get<TenantSponsorCustomerResponseDto>(
                    It.Is<string>(s => s.Contains(Constant.GetTenantSponsorCustomer)),
                    It.IsAny<Dictionary<string, long>>()
                ))
                .ReturnsAsync(expectedResponse);

            var roleSubscriber = new RoleModel { RoleId = 0, RoleName = "subscriber" };
            _roleRepo.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<RoleModel, bool>>>(), false))
                     .ReturnsAsync(roleSubscriber);

            _personRepo.Setup(p => p.FindOneAsync(It.IsAny<Expression<Func<PersonModel, bool>>>(), false))
                       .ReturnsAsync((PersonModel)null);

            _personRepo.Setup(p => p.CreateAsync(It.IsAny<PersonModel>()))
                       .ReturnsAsync(new PersonModel { PersonId = 123, Email = "test@example.com" });

            _personRoleRepo.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<PersonRoleModel, bool>>>(), false))
                           .ReturnsAsync((PersonRoleModel)null);

            _personRoleRepo.Setup(r => r.CreateAsync(It.IsAny<PersonRoleModel>()))
                           .ReturnsAsync((PersonRoleModel)null);

            _consumerRepo.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false))
                           .ReturnsAsync((ConsumerModel)null);

            _personRepo.Setup(p => p.CreateAsync(It.IsAny<PersonModel>()))
                       .ReturnsAsync(new PersonModel { PersonId = 123, Email = "test@example.com" });

            _personRepo.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<PersonModel, bool>>>(), false))
                       .ReturnsAsync(new PersonModel { PersonId = 123, Email = "test@example.com" });

            var res = await _consumerController.CreateConsumers(new List<ConsumerDataDto> { consumerDataRequestDto });
            var result = res.Result as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
            var data = _mapper.Map<List<ConsumerDataResponseDto>>(result.Value);
            Assert.NotNull(data);
            Assert.Equal(StatusCodes.Status409Conflict, data[0].ErrorCode);
        }

        [Fact]
        public async Task CreateConsumerData_ShouldCreateConsumerDataSuccessfully()
        {
            var consumerDataRequestDto = new ConsumerDataMockDto();
            _session.Setup(session => session.BeginTransaction())
            .Returns(new Mock<ITransaction>().Object);
            var result = await _consumerService.CreateConsumers(new List<ConsumerDataDto> { consumerDataRequestDto });
            Assert.NotNull(result);
        }

        [Fact]
        public async Task CreateConsumerData_ShouldCallValidateAddressType_WhenAddressProvided()
        {
            // Arrange
            var consumerDataRequestDto = new ConsumerDataMockDto();
            consumerDataRequestDto.PersonAddresses = new List<PersonAddressDto>
            {
                new PersonAddressDto
                {
                    AddressTypeId = 1,
                    Line1 = "123 Test St",
                    City = "TestCity",
                    State = "TS",
                    PostalCode = "12345",
                    Country = "US"
                }
            };

            var transactionMock = new Mock<ITransaction>();

            _session.Setup(s => s.BeginTransaction())
                    .Returns(transactionMock.Object);

            _personRepo.Setup(p => p.FindOneAsync(It.IsAny<Expression<Func<PersonModel, bool>>>(), false))
                       .ReturnsAsync(new PersonModel
                       {
                           PersonId = 111
                       });

            _consumerRepo.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false))
                         .ReturnsAsync((ConsumerModel)null);

            _roleRepo.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<RoleModel, bool>>>(), false))
                     .ReturnsAsync(new RoleModel { RoleId = 1 });

            _personRoleRepo.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<PersonRoleModel, bool>>>(), false))
                           .ReturnsAsync((PersonRoleModel)null);

            _tenantClient.Setup(c => c.Get<TenantSponsorCustomerResponseDto>(It.IsAny<string>(), It.IsAny<Dictionary<string, long>>()))
                         .ReturnsAsync(new TenantSponsorCustomerResponseDto());

            _addressTypeService.Setup(x => x.GetAddressTypeById(It.IsAny<long>()))
                               .ReturnsAsync(new GetAddressTypeResponseDto { ErrorCode = null });

            // Act
            var result = await _consumerService.CreateConsumers(new List<ConsumerDataDto> { consumerDataRequestDto });

            // Assert
            _addressTypeService.Verify(x => x.GetAddressTypeById(It.IsAny<long>()), Times.Once);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task CreateConsumerData_ShouldCallValidateAddressType_WhenAddressTypeInvalid()
        {
            // Arrange
            var consumerDataRequestDto = new ConsumerDataMockDto();
            consumerDataRequestDto.PersonAddresses = new List<PersonAddressDto>
            {
                new PersonAddressDto
                {
                    AddressTypeId = 1,
                    Line1 = "123 Test St",
                    City = "TestCity",
                    State = "TS",
                    PostalCode = "12345",
                    Country = "US"
                }
            };

            var transactionMock = new Mock<ITransaction>();

            _session.Setup(s => s.BeginTransaction())
                    .Returns(transactionMock.Object);

            _personRepo.Setup(p => p.FindOneAsync(It.IsAny<Expression<Func<PersonModel, bool>>>(), false))
                       .ReturnsAsync(new PersonModel
                       {
                           PersonId = 111
                       });

            _consumerRepo.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false))
                         .ReturnsAsync((ConsumerModel)null);

            _roleRepo.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<RoleModel, bool>>>(), false))
                     .ReturnsAsync(new RoleModel { RoleId = 1 });

            _personRoleRepo.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<PersonRoleModel, bool>>>(), false))
                           .ReturnsAsync((PersonRoleModel)null);

            _tenantClient.Setup(c => c.Get<TenantSponsorCustomerResponseDto>(It.IsAny<string>(), It.IsAny<Dictionary<string, long>>()))
                         .ReturnsAsync(new TenantSponsorCustomerResponseDto());

            _addressTypeService.Setup(x => x.GetAddressTypeById(It.IsAny<long>()))
                               .ReturnsAsync(new GetAddressTypeResponseDto { ErrorCode = StatusCodes.Status404NotFound });

            // Act
            var result = await _consumerService.CreateConsumers(new List<ConsumerDataDto> { consumerDataRequestDto });

            // Assert
            _addressTypeService.Verify(x => x.GetAddressTypeById(It.IsAny<long>()), Times.Once);
            Assert.NotNull(result);
        }


        [Fact]
        public async Task Should_Not_Create_Consumer_When_Subscriber_Member_Nbr_Does_Not_Exist()
        {
            var consumerDataRequestDto = new ConsumerDataMockDto();
            _session.Setup(session => session.BeginTransaction())
            .Returns(new Mock<ITransaction>().Object);
            _consumerRepo.Setup(s => s.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false));
            var result = await _consumerService.CreateConsumers(new List<ConsumerDataDto> { consumerDataRequestDto });
            Assert.NotNull(result);
        }

        [Fact]
        public async Task CreateConsumerData_Null_Check()
        {
            var consumerDataRequestDto = new List<ConsumerDataDto>();
            var consumerService = new ConsumerService(_consumerServiceLogger.Object, _mapper, _consumerRepo.Object, _personRepo.Object,
                _roleRepo.Object, _session.Object, _personRoleRepo.Object, _tenantClient.Object, _addressTypeService.Object, _uploadpdfService.Object, _personAddressRepo.Object, _phoneNumberRepo.Object, _memberImportFileDataRepo.Object, _eventService.Object, _consumerETLRepo.Object, _heliosEventPublisher.Object);
            await Assert.ThrowsAsync<InvalidOperationException>(() => consumerService.CreateConsumers(null));
            var result = await Assert.ThrowsAsync<InvalidOperationException>(() => consumerService.
            CreateConsumers(new List<ConsumerDataDto>()));
        }

        [Fact]
        public async Task Catch_Exception_For_CreateConsumerData()
        {
            var controller = new ConsumerController(_consumerControllerLogger.Object, _consumerService);
            var ConsumerDataMockDto = new List<ConsumerDataMockDto>();
            var consumerService = new Mock<IConsumerService>();
            var consumerDataRequestDto = new ConsumerDataMockDto();
            consumerService.Setup(service => service.CreateConsumers(It.IsAny<IList<ConsumerDataDto>>()))
                       .ThrowsAsync(new Exception("Simulated exception message"));
            await _consumerController.CreateConsumers(new List<ConsumerDataDto> { consumerDataRequestDto });
        }

        [Fact]
        public async Task Should_Return_NullPerson_CreateConsumerData()
        {
            var consumerDataRequestDto = new ConsumerDataMockDto
            {
                Person = new PersonDto()
                {
                    PersonCode = null,
                    Email = "test@test.com"
                },
                Consumer = new ConsumerDto()
            };
            _personRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<PersonModel, bool>>>(), false));
            _consumerRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false));
            var data = await _consumerService.CreateConsumers(new List<ConsumerDataDto> { consumerDataRequestDto });

            Assert.NotNull(data);
            Assert.Equal(data[0].ErrorCode, StatusCodes.Status409Conflict);

        }

        [Fact]
        public async Task Should_Return_InvalidPersonId_CreateConsumerData()
        {
            var consumerDataRequestDto = new ConsumerDataMockDto
            {
                Person = new PersonDto
                {
                    PersonId = 0,
                    Email = "test@gmail.com"
                },
                Consumer = new ConsumerDto()
            };
            _personRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<PersonModel, bool>>>(), false));
            _consumerRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false));
            var data = await _consumerService.CreateConsumers((new List<ConsumerDataDto> { consumerDataRequestDto }));
            Assert.NotNull(data);
            Assert.Equal(data[0].ErrorCode, StatusCodes.Status409Conflict);

        }

        [Fact]
        public async Task Should_Return_InvalidRoleId_CreateConsumerData()
        {
            var response = _roleRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<RoleModel, bool>>>(), false))
                 .ReturnsAsync(new RoleMockModel { RoleId = 0 });
            var consumerDataRequestDto = new ConsumerDataMockDto
            {
                Person = new PersonDto
                {
                    PersonId = 0,
                    Email = "test@gmail.com"
                },
                Consumer = new ConsumerDto()
            };
            _personRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<PersonModel, bool>>>(), false));
            _consumerRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false));
            var data = await _consumerService.CreateConsumers((new List<ConsumerDataDto> { consumerDataRequestDto }));
            Assert.NotNull(data);
            Assert.Equal(data[0].ErrorCode, StatusCodes.Status409Conflict);
        }

        [Fact]
        public async Task Should_Return_Invalid_When_PersonExist_Consumer_Not_exist_CreateConsumerData()
        {
            var response = _roleRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<RoleModel, bool>>>(), false))
                 .ReturnsAsync(new RoleMockModel { RoleId = 0 });
            var consumerDataRequestDto = new ConsumerDataMockDto
            {
                Person = new PersonDto
                {
                    PersonId = 0,
                    Email = "test@gmail.com"
                },
                Consumer = new ConsumerDto()
            };
            var personMockData = new PersonMockModel();
            personMockData.Email = "test@gmail.com";
            _personRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<PersonModel, bool>>>(), false)).ReturnsAsync(personMockData);
            var consumerMockModel = new ConsumerMockModel();
            consumerMockModel.PersonId = personMockData.PersonId;
            _consumerRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false)).ReturnsAsync(consumerMockModel);
            var data = await _consumerService.CreateConsumers((new List<ConsumerDataDto> { consumerDataRequestDto }));
            Assert.NotNull(data);
            Assert.Equal(data[0].ErrorCode, StatusCodes.Status409Conflict);
        }


        [Fact]
        public async Task ConsumerAttributes_Controller_ReturnsData()
        {
            var consumerAttributesRequestMockDto = new ConsumerAttributesRequestMockDto();
            var transactionMock = new Mock<ITransaction>();
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);
            var mockConsumerService = new Mock<IConsumerService>();
            var response = await _consumerController.ConsumerAttributes(consumerAttributesRequestMockDto);
            var result = response.Result as OkObjectResult;
            Assert.True(result?.StatusCode == 200);

        }
      
        [Fact]
        public async Task ConsumerAttributes_Controller_ReturnsDataForArray()
        {
            var consumerAttributesRequestMockDto = new ConsumerAttributesRequestMockDto();
            var transactionMock = new Mock<ITransaction>();
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);
            var mockConsumerService = new Mock<IConsumerService>();
            _consumerRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false)).ReturnsAsync(new ConsumerModel {
                ConsumerCode = "cmr-bjuebdf-492f-46i4-bh55-5a2b0134cbc",
                TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4",
                ConsumerAttribute = "{\r\n  \"is_ssbci\": true,\r\n  \"SurveyTaskRewardCodes\": [\r\n    {\r\n      \"trw-11e8eeb6b0ec4cfa8aa6f82abdd4e4b9\": 0\r\n    }\r\n  ]\r\n}",
            });

            var response = await _consumerController.ConsumerAttributes(consumerAttributesRequestMockDto);
            var result = response.Result as OkObjectResult;
            Assert.True(result?.StatusCode == 200);

        }

        [Fact]
        public async Task ConsumerAttributes_Controller_Try_Catch()
        {

            var transactionMock = new Mock<ITransaction>();
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);
            var mockService = new Mock<IConsumerService>();
            mockService.Setup(s => s.ConsumerAttributes(It.IsAny<ConsumerAttributesRequestDto>()))
                .ThrowsAsync(new Exception("intended exception"));
            var controller = new ConsumerController(_consumerControllerLogger.Object, mockService.Object);
            var consumerAttributesRequestMockDto = new ConsumerAttributesRequestMockDto();
            var result = await controller.ConsumerAttributes(consumerAttributesRequestMockDto);
            Assert.True(result?.Value?.ErrorCode == null);
        }

        [Fact]
        public async Task ConsumerAttributes_Service_Try_Catch()
        {
            var transactionMock = new Mock<ITransaction>();
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);
            var mockService = new Mock<IConsumerService>();
            _consumerRepo.Setup(s => s.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false))
               .ThrowsAsync(new Exception("intended exception"));
            var consumerAttributesRequestMockDto = new ConsumerAttributesRequestMockDto();
            var result = await _consumerService.ConsumerAttributes(consumerAttributesRequestMockDto);
            var res = result;
            Assert.True(res?.ErrorMessage == "intended exception");
        }

        [Fact]
        public async Task ConsumerAttributes_Success()
        {
            var transactionMock = new Mock<ITransaction>();
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);
            var consumerAttributesRequestMockDto = new ConsumerAttributesRequestMockDto();
            var result = await _consumerService.ConsumerAttributes(consumerAttributesRequestMockDto);
            Assert.True(result?.ErrorCode == null);
        }

        [Fact]
        public async Task ConsumerAttributes_Successful_Process_Test()
        {
            _consumerRepo.Setup(s => s.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false))
                .ReturnsAsync(new ConsumerModel());

            var transactionMock = new Mock<ITransaction>();
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);

            var consumerAttributesRequestMockDto = new ConsumerAttributesRequestMockDto()
            {
                TenantCode = null,
                ConsumerAttributes = new ConsumerAttributeDetailDto[]
                {
            new ConsumerAttributeDetailDto()
            {
                ConsumerCode  = null,
                AttributeName = null,
                AttributeValue = null,
                GroupName = null,
            }
                }
            };
            var result = await _consumerService.ConsumerAttributes(consumerAttributesRequestMockDto);
            Assert.True(result?.ErrorCode == null);

        }

        [Fact]
        public async Task ConsumerAttributes_Successful_Boolean_Process_Test()
        {
            _consumerRepo.Setup(s => s.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false))
                .ReturnsAsync(new ConsumerModel());

            var transactionMock = new Mock<ITransaction>();
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);

            var consumerAttributesRequestMockDto = new ConsumerAttributesRequestMockDto()
            {
                TenantCode = "T1",
                ConsumerAttributes = new ConsumerAttributeDetailDto[]
                {
                    new ConsumerAttributeDetailDto()
                    {
                        ConsumerCode  = "C1",
                        AttributeName = "is_Ssbci",
                        AttributeValue = "true",
                        GroupName = null,
                    }
                }
            };
            var result = await _consumerService.ConsumerAttributes(consumerAttributesRequestMockDto);
            Assert.True(result?.ErrorCode == null);

        }

        [Fact]
        public async Task ConsumerAttributes_Should_Group_Attributes_For_Same_Consumer()
        {
            var consumer = new ConsumerModel
            {
                ConsumerCode = "C1",
                ConsumerAttribute = "{}"
            };

            _consumerRepo.Setup(r => r.FindOneAsync(
                    It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false))
                .ReturnsAsync(consumer);

            var transactionMock = new Mock<ITransaction>();
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);

            var req = new ConsumerAttributesRequestDto
            {
                TenantCode = "T1",
                ConsumerAttributes = new[]
                {
                    new ConsumerAttributeDetailDto { ConsumerCode="C1", AttributeName="A1", AttributeValue="1", GroupName="" },
                    new ConsumerAttributeDetailDto { ConsumerCode="C1", AttributeName="A2", AttributeValue="true", GroupName="" },
                    new ConsumerAttributeDetailDto { ConsumerCode="C1", AttributeName="A2", AttributeValue="test", GroupName="" }
                }
            };

            var result = await _consumerService.ConsumerAttributes(req);

            Assert.Single(result.Consumers);
        }



        [Fact]
        public async Task ConsumerAttributes_Catch_Block_Test()
        {

            _consumerRepo.Setup(s => s.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false))
                .ThrowsAsync(new Exception("intended exception"));

            var transactionMock = new Mock<ITransaction>();
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);

            var consumerAttributesRequestMockDto = new ConsumerAttributesRequestMockDto();

            var result = await _consumerService.ConsumerAttributes(consumerAttributesRequestMockDto);
            Assert.True(result?.ErrorCode == null);
        }

        [Fact]
        public async Task GetConsumerByEmail_Controller_Try_Catch()
        {
            string email = "jaskaran@gmail.com";
            var mockService = new Mock<IConsumerService>();
            var controller = new ConsumerController(_consumerControllerLogger.Object, mockService.Object);
            var consumer = await controller.GetConsumerByEmail(email);
            var result = consumer.Result as BadRequestObjectResult;
            Assert.True(result?.Value == null);
        }

        [Fact]
        public async Task GetConsumerByEmail_Service()
        {
            var email = "test@example.com";

            _personRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<PersonModel, bool>>>(), false))
                .ReturnsAsync(new List<PersonModel>());
            var result = await _consumerService.GetConsumerByEmail(email);
            Assert.True(result?.ErrorCode == 404);

        }
        [Fact]
        public async Task Should_Catch_GetConsumerByEmail_Service()
        {
            var email = "test@example.com";
            var mockService = new Mock<IConsumerService>();
            mockService.Setup(s => s.GetConsumerByEmail(email))
                .ThrowsAsync(new Exception("intended exception"));
            var mockPersonRepo = new Mock<IPersonRepo>();
            mockPersonRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<PersonModel, bool>>>(), false))
             .ThrowsAsync(new Exception("Simulated exception"));
            var result = await _consumerService.GetConsumerByEmail(email);
            Assert.True(result?.ErrorCode == null);

        }

        [Fact]
        public async Task GetConsumerByEmail_Service_Person_Consumer_Data()
        {
            var email = "test@example.com";
            var mockPersonRepo = new Mock<IPersonRepo>();
            _personRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<PersonModel, bool>>>(), false)).ReturnsAsync(PersonMockDto.personData());
            _consumerRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false)).ReturnsAsync(ConsumerMockDto.consumerModel());
            var result = await _consumerService.GetConsumerByEmail(email);
            Assert.True(result?.ErrorCode == null);

        }

        [Fact]
        public async Task Should_Be_Return_Consumer_GetConsumerByEmail_Service()
        {
            var email = "test@example.com";

            _consumerRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false))
                .ReturnsAsync(new List<ConsumerModel>());
            var result = await _consumerService.GetConsumerByEmail(email);
            Assert.True(result?.ErrorCode == 404);

        }

        [Fact]
        public async Task GetConsumerByEmail_Returns_Ok_Response()
        {
            // Arrange
            string email = "test@example.com";
            var responseDto = new GetConsumerByEmailResponseDto { ErrorCode = 400 };
            _personRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<PersonModel, bool>>>(), false)).ReturnsAsync(PersonMockDto.personData());
            _consumerRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false)).ReturnsAsync(ConsumerMockDto.consumerModel());
            var response = await _consumerController.GetConsumerByEmail(email);
            var result = response.Result as OkObjectResult;
            Assert.True(result?.StatusCode == 200);

        }

        [Fact]
        public async Task GetConsumerByEmail_Returns_NotFound()
        {
            var personModel = new List<PersonModel>();
            var consumerModel = new List<ConsumerModel>();
            var email = "test@example.com";
            _personRepo.Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<PersonModel, bool>>>(), false))
            .ReturnsAsync(new List<PersonModel>(personModel));

            _consumerRepo.Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false))
                .ReturnsAsync(new List<ConsumerModel>(consumerModel));
            var result = await _consumerController.GetConsumerByEmail(email);
            Assert.True(result.Value == null);

        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task UpdateRegisterFlag_Controller_Response()
        {

            var consumerDto = new ConsumerDto();
            var mockLogger = new Mock<ILogger<ConsumerController>>();
            var transactionMock = new Mock<ITransaction>();
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);
            var mockService = new Mock<IConsumerService>();
            mockService.Setup(service => service.updateRegisterFlag(It.IsAny<ConsumerDto>()))
                       .ReturnsAsync(new ConsumerModel { /* Initialize with expected result properties */ });
            var result = await _consumerController.updateRegisterFlag(consumerDto);
            Assert.True(result.Value == null);
        }

        [Fact]
        public async Task UpdateRegisterFlag_ExceptionHandling()
        {
            // Arrange
            var consumerDto = new ConsumerDto { ConsumerId = 1, /* other properties */ };
            var mockedSession = new Mock<NHibernate.ISession>();
            _consumerRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false))
                .ReturnsAsync(ConsumerMockDto.consumerModel());
            mockedSession.Setup(x => x.BeginTransaction()).Returns(Mock.Of<ITransaction>());
            var response = await _consumerController.updateRegisterFlag(consumerDto);
            var result = response.Result as OkObjectResult;
            Assert.True(result == null);
        }
        [Fact]
        public async Task Should_Not_Create_Consumer_When_Email_Already_Exist()
        {
            // Arrange
            var transactionMock = new Mock<ITransaction>();
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);
            _consumerRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false));
            _personRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<PersonModel, bool>>>(), false))
                .ReturnsAsync(new PersonModel());
            var consumerDataRequestDto = new ConsumerDataMockDto()
            {
                Person = new PersonDto(),
                Consumer = new ConsumerDto()
            };

            // Act
            var response = await _consumerController.CreateConsumers(new List<ConsumerDataDto> { consumerDataRequestDto });

            // Assert
            var objectresult = response.Result as ObjectResult;
            var consumerResponse = objectresult?.Value as List<ConsumerDataResponseDto>;
            Assert.NotNull(response);
            Assert.Equal(StatusCodes.Status409Conflict, consumerResponse[0].ErrorCode);
            Assert.Equal(StatusCodes.Status200OK, objectresult?.StatusCode);
        }
        [Fact]
        public async Task PostConsumer_Should_Update_Consumer_When_Mem_number_Already_Exist()
        {
            // Arrange
            var transactionMock = new Mock<ITransaction>();
            var consumerModel = new ConsumerMockModel();
            var personModel = new PersonMockModel();
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);
            _personRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<PersonModel, bool>>>(), false)).ReturnsAsync(personModel);
            _consumerRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false))
                .ReturnsAsync(consumerModel);
            var consumerDataRequestDto = new ConsumerDataMockDto()
            {
                Person = new PersonDto() { Email = "test@test.com" },
                Consumer = new ConsumerDto()
            };

            // Act
            var response = await _consumerController.CreateConsumers(new List<ConsumerDataDto> { consumerDataRequestDto });

            // Assert
            var objectresult = response.Result as ObjectResult;
            var consumerResponse = objectresult?.Value as List<ConsumerDataResponseDto>;
            Assert.NotNull(response);
            Assert.NotNull(consumerResponse[0].ErrorCode);
            Assert.Equal(StatusCodes.Status200OK, objectresult?.StatusCode);
        }
        [Fact]
        public async Task PostConsumer_Should_Not_Create_Consumer_When_Exception_occurs()
        {
            // Arrange
            var consumerDataRequestDto = new ConsumerDataMockDto()
            {
                Person = new PersonDto() { Email = "testing@test.com" },
                Consumer = new ConsumerDto()
            };

            var transactionMock = new Mock<ITransaction>();
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);
            _personRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<PersonModel, bool>>>(), false));
            _consumerRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false))
                .Throws(new Exception("Some thing went wrong"));

            // Act
            var response = await _consumerController.CreateConsumers(new List<ConsumerDataDto> { consumerDataRequestDto });

            // Assert
            var objectresult = response.Result as ObjectResult;
            var consumerResponse = objectresult?.Value as List<ConsumerDataResponseDto>;
            Assert.Equal(consumerResponse[0].ErrorCode, StatusCodes.Status500InternalServerError);
            Assert.NotNull(response);
            Assert.Equal(StatusCodes.Status200OK, objectresult?.StatusCode);
        }
        [Fact]
        public async Task Consumer_And_Person_Should_Update_When_Member_Already_Exist()
        {
            //Arrange
            var consumerDataRequestDto = new ConsumerDataMockDto();
            var consumerModel = new ConsumerMockModel();
            var personModel = new PersonMockModel();
            var transactionMock = new Mock<ITransaction>();
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);
            _personRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<PersonModel, bool>>>(), false)).ReturnsAsync(personModel);
            _consumerRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false)).ReturnsAsync(consumerModel);

            //Act
            var response = await _consumerController.CreateConsumers(new List<ConsumerDataDto> { consumerDataRequestDto, consumerDataRequestDto });

            //Assert
            var objectresult = response.Result as ObjectResult;
            var consumerResponse = objectresult.Value as List<ConsumerDataResponseDto>;
            Assert.NotNull(response);
            Assert.NotNull(consumerResponse[0].ErrorCode);
            Assert.NotNull(consumerResponse[0].Person);
            Assert.NotNull(consumerResponse[0].Consumer);
            Assert.Equal(StatusCodes.Status200OK, objectresult?.StatusCode);
        }

        [Fact]
        public async Task Consumer_And_Person_Should_Not_Update_When_CreateConsumerModel_Throws_Exception()
        {
            //Arrange
            var consumerDataRequestDto = new ConsumerDataMockDto();
            var consumerModel = new ConsumerMockModel();
            var personModel = new PersonMockModel();
            var transactionMock = new Mock<ITransaction>();
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);
            _personRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<PersonModel, bool>>>(), false)).ReturnsAsync(personModel);
            _consumerRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false)).Throws(new InvalidOperationException("Invalid operation"));

            //Act
            var response = await _consumerController.CreateConsumers(new List<ConsumerDataDto> { consumerDataRequestDto, consumerDataRequestDto });

            //Assert
            var objectresult = response.Result as ObjectResult;
            var consumerResponse = objectresult.Value as List<ConsumerDataResponseDto>;
            Assert.NotNull(response);
            Assert.Equal(StatusCodes.Status500InternalServerError, consumerResponse[0].ErrorCode);
            Assert.Equal(StatusCodes.Status200OK, objectresult?.StatusCode);
        }
        [Fact]
        public async Task Consumer_And_Person_Should_Not_Update_When_CreatePersonModel_Throws_Exception()
        {
            //Arrange
            var consumerDataRequestDto = new ConsumerDataMockDto();
            var consumerModel = new ConsumerMockModel();
            var personModel = new PersonMockModel();
            var transactionMock = new Mock<ITransaction>();
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);
            _personRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<PersonModel, bool>>>(), false)).Throws(new InvalidOperationException("Invalid operation"));
            _consumerRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false));

            //Act
            var response = await _consumerController.CreateConsumers(new List<ConsumerDataDto> { consumerDataRequestDto, consumerDataRequestDto });

            //Assert
            var objectresult = response.Result as ObjectResult;
            var consumerResponse = objectresult.Value as List<ConsumerDataResponseDto>;
            Assert.NotNull(response);
            Assert.Equal(StatusCodes.Status500InternalServerError, consumerResponse[0].ErrorCode);
            Assert.Equal(StatusCodes.Status200OK, objectresult?.StatusCode);
        }

        [Theory]
        [InlineData("{\r\n  \"apps\": [\r\n    \"REWARDS\",\r\n    \"BENEFITS\"\r\n  ],\r\n  \"benefitsOptions\": {\r\n    \"cardIssueFlowType\": [\r\n      \"TASK_COMPLETION_CHECK\"\r\n    ],\r\n    \"disableOnboardingFlow\": false,\r\n    \"autoCompleteTaskOnLogin\": true,\r\n    \"taskCompletionCheckCode\": [\r\n      \"trw-1f76cf0007594d16b1afe5df45178730\"\r\n    ],\r\n    \"manualCardRequestRequired\": true\r\n  }\r\n}")]
        [InlineData("{\r\n  \"apps\": [\r\n    \"REWARDS\",\r\n    \"BENEFITS\"\r\n  ],\r\n  \"benefitsOptions\": {\r\n    \"cardIssueFlowType\": [\r\n      \"TASK_COMPLETION_CHECK\"\r\n    ],\r\n    \"disableOnboardingFlow\": false,\r\n    \"autoCompleteTaskOnLogin\": true,\r\n    \"taskCompletionCheckCode\": [\r\n      \"trw-1f76cf0007594d16b1afe5df45178730\"\r\n    ],\r\n    \"manualCardRequestRequired\": true,\r\n    \"reactivateDeletedConsumer\": true\r\n  }\r\n}")]
        public async Task UpdateConsumers_Should_Return_Ok_For_Success_Result_ReactiveDeletedConsumer(string tenantOption)
        {
            // Arrange

            var expectedResponse = new TenantSponsorCustomerResponseDto
            {
                Customer = new CustomerDto { CustomerCode = "CUST123" },
                Sponsor = new SponsorDto { SponsorCode = "SPON456" }
            };

            _tenantClient
                .Setup(x => x.Get<TenantSponsorCustomerResponseDto>(
                    It.Is<string>(s => s.Contains(Constant.GetTenantSponsorCustomer)),
                    It.IsAny<Dictionary<string, long>>()
                ))
                .ReturnsAsync(expectedResponse);

            var expectedTenantResponse = new TenantDto
            {
                TenantOption = tenantOption
            };

            _tenantClient
                .Setup(x => x.Post<TenantDto>(
                    It.Is<string>(s => s.Contains(Constant.GetTenantByTenantCode)),
                    It.IsAny<GetTenantCodeRequestDto>()
                ))
                .ReturnsAsync(expectedTenantResponse);

            var transactionMock = new Mock<ITransaction>();
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);
            var consumerService = new Mock<IConsumerService>();
            consumerService.Setup(service => service.CreateConsumers(It.IsAny<IList<ConsumerDataDto>>()))
                         .ReturnsAsync(new List<ConsumerDataResponseDto>());
            var consumerDataRequestDto = new ConsumerDataMockDto()
            {
                Person = new PersonDto(),

                Consumer = new ConsumerDto(),

                PersonAddresses = new List<PersonAddressDto>
                {
                    new PersonAddressDto
                    {
                        AddressTypeId = 1,
                        IsPrimary = true
                    }
                }

            };
            var res = await _consumerController.UpdateConsumers(new List<ConsumerDataDto> { consumerDataRequestDto });
            var result = res.Result as OkObjectResult;
            Assert.True(result?.Value != null);
            Assert.True(result?.StatusCode == 200);
        }

        [Fact]
        public async Task UpdateConsumers_Should_Return_Ok_For_Success_Result()
        {
            var transactionMock = new Mock<ITransaction>();
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);
            var consumerService = new Mock<IConsumerService>();
            consumerService.Setup(service => service.CreateConsumers(It.IsAny<IList<ConsumerDataDto>>()))
                         .ReturnsAsync(new List<ConsumerDataResponseDto>());
            var consumerDataRequestDto = new ConsumerDataMockDto()
            {
                Person = new PersonDto(),

                Consumer = new ConsumerDto(),

                PersonAddresses = new List<PersonAddressDto>
                {
                    new PersonAddressDto
                    {
                        AddressTypeId = 1,
                        IsPrimary = true
                    }
                }

            };
            var res = await _consumerController.UpdateConsumers(new List<ConsumerDataDto> { consumerDataRequestDto });
            var result = res.Result as OkObjectResult;
            Assert.True(result?.Value != null);
            Assert.True(result?.StatusCode == 200);
        }
        [Fact]
        public async Task UpdateConsumers_Should_Not_update_Consumer_When_MemberNbr_Is_Not_Exist()
        {
            // Arrange
            var transactionMock = new Mock<ITransaction>();
            var consumerModel = new ConsumerMockModel();
            consumerModel.ConsumerId = 0;
            var personModel = new PersonMockModel();
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);
            _personRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<PersonModel, bool>>>(), false)).ReturnsAsync(personModel);
            _consumerRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false))
                .ReturnsAsync(consumerModel);
            var consumerDataRequestDto = new ConsumerDataMockDto()
            {
                Person = new PersonDto() { Email = "test@test.com" },
                Consumer = new ConsumerDto()
            };

            // Act
            var response = await _consumerController.UpdateConsumers(new List<ConsumerDataDto> { consumerDataRequestDto });

            // Assert
            var objectresult = response.Result as ObjectResult;
            var consumerResponse = objectresult?.Value as List<ConsumerDataResponseDto>;
            Assert.NotNull(response);
            Assert.Equal(StatusCodes.Status404NotFound, consumerResponse[0].ErrorCode);
            Assert.Equal(StatusCodes.Status200OK, objectresult?.StatusCode);
        }
        [Fact]
        public async Task UpdateConsumers_Should_Not_update_Consumer_When_Exception_Occurs()
        {
            // Arrange
            var consumerDataRequestDto = new ConsumerDataMockDto()
            {
                Person = new PersonDto() { Email = "testing@test.com" },
                Consumer = new ConsumerDto()
            };

            var transactionMock = new Mock<ITransaction>();
            _session.Setup(s => s.BeginTransaction()).Throws(new Exception("Some thing went wrong"));
            _personRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<PersonModel, bool>>>(), false));
            _consumerRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false)).Throws(new Exception("Some thing went wrong"));

            // Act
            var response = await _consumerController.UpdateConsumers(new List<ConsumerDataDto> { consumerDataRequestDto });

            // Assert
            var objectresult = response.Result as ObjectResult;
            var consumerResponse = objectresult?.Value as List<ConsumerDataResponseDto>;
            Assert.Null(response.Result);
            Assert.Null(objectresult);
            Assert.Null(consumerResponse);

        }
        [Fact]
        public async Task UpdateConsumers_Should_Return_Error_When_Email_Is_Invalid()
        {
            // Arrange
            var consumerDataRequestDto = new ConsumerDataMockDto()
            {
                Person = new PersonDto() { Email = "invalidemail" },  // Invalid email format
                Consumer = new ConsumerDto() { IsSsoAuthenticated = true } // SSO user
            };

            // Mock transaction and repository interactions (if needed, but this test is focused on validation)
            var transactionMock = new Mock<ITransaction>();
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);  // No exceptions needed here

            // Act
            var response = await _consumerController.UpdateConsumers(new List<ConsumerDataDto> { consumerDataRequestDto });

            // Assert
            var objectResult = response.Result as ObjectResult;
            var consumerResponse = objectResult.Value as List<ConsumerDataResponseDto>;

            Assert.NotNull(objectResult);
            Assert.NotNull(consumerResponse);
            Assert.Equal(StatusCodes.Status200OK, objectResult.StatusCode);
            Assert.Equal(StatusCodes.Status400BadRequest, consumerResponse?[0].ErrorCode); // Check that the status is 400 (Bad Request)
            Assert.Contains("Invalid email format.", consumerResponse?[0].ErrorMessage); // Verify the "Invalid email format."
        }

        [Fact]
        public async Task UpdateConsumers_Should_Update_Consumer_Successfully()
        {
            // Arrange
            var consumerDataRequestDto = new ConsumerDataMockDto()
            {
                Person = new PersonDto() { Email = "testing@test.com" },  // Valid email
                Consumer = new ConsumerDto() { IsSsoAuthenticated = true } // SSO user
            };

            _session.Setup(s => s.BeginTransaction()).Returns(Mock.Of<ITransaction>());  // No exceptions thrown
            _personRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<PersonModel, bool>>>(), false)).ReturnsAsync((PersonModel)null); // Simulate no existing email
            _consumerRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false)).ReturnsAsync(Mock.Of<ConsumerModel>());

            // Act
            var response = await _consumerController.UpdateConsumers(new List<ConsumerDataDto> { consumerDataRequestDto });

            // Assert
            var objectresult = response.Result as ObjectResult;
            var consumerResponse = objectresult?.Value as List<ConsumerDataResponseDto>;

            Assert.Equal(StatusCodes.Status200OK, objectresult.StatusCode);  // Ensure the status code is 200 (OK)
            Assert.NotNull(consumerResponse);  // Ensure the consumer response is not null
        }
        [Fact]
        public async Task UpdateConsumers_Should_Return_Error_When_Email_Already_Exists()
        {
            // Arrange
            var consumerDataRequestDto = new ConsumerDataDto()
            {
                Person = new PersonDto() { PersonId = 1, Email = "existing1@test.com" },  // Email already exists
                Consumer = new ConsumerDto() { IsSsoAuthenticated = true } // SSO user
            };

            _session.Setup(s => s.BeginTransaction()).Returns(Mock.Of<ITransaction>());

            // Simulate email already exists in the database
            _personRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<PersonModel, bool>>>(), false))
                       .ReturnsAsync(new PersonModel() { PersonId = 2, Email = "existing@test.com" });  // Different PersonId, same email (email conflict)

            // Act
            var response = await _consumerController.UpdateConsumers(new List<ConsumerDataDto> { consumerDataRequestDto });

            // Assert
            var objectResult = response.Result as ObjectResult;
            var consumerResponse = objectResult?.Value as List<ConsumerDataResponseDto>;

            Assert.NotNull(objectResult);
            Assert.NotNull(consumerResponse);

            // Ensure that the status code is 200 (the API returned correctly)
            Assert.Equal(StatusCodes.Status200OK, objectResult.StatusCode);

            // Check if the consumer response contains the 404 error code and email conflict message
            Assert.Equal(StatusCodes.Status404NotFound, consumerResponse?[0].ErrorCode);  // Error code is 404 (Not Found)
            Assert.Contains("That email is taken. Try another.", consumerResponse?[0].ErrorMessage);  // Error message matches
        }

        [Fact]
        public async Task CancelConsumers_Should_Return_Ok_For_Success_Result()
        {
            var transactionMock = new Mock<ITransaction>();
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);
            var consumerService = new Mock<IConsumerService>();
            consumerService.Setup(service => service.CreateConsumers(It.IsAny<IList<ConsumerDataDto>>()))
                         .ReturnsAsync(new List<ConsumerDataResponseDto>());
            var consumerDataRequestDto = new ConsumerDataMockDto()
            {
                Person = new PersonDto(),

                Consumer = new ConsumerDto(),

                PersonAddresses = new List<PersonAddressDto>
                {
                    new PersonAddressDto
                    {
                        AddressTypeId = 1
                    }
                }

            };
            var res = await _consumerController.CancelConsumers(new List<ConsumerDataDto> { consumerDataRequestDto });
            var result = res.Result as OkObjectResult;
            Assert.True(result?.Value != null);
            Assert.True(result?.StatusCode == 200);
        }
        [Fact]
        public async Task CancelConsumers_Should_Not_update_Consumer_When_MemberNbr_Is_Not_Exist()
        {
            // Arrange
            var transactionMock = new Mock<ITransaction>();
            var consumerModel = new ConsumerMockModel();
            consumerModel.ConsumerId = 0;
            var personModel = new PersonMockModel();
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);
            _personRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<PersonModel, bool>>>(), false)).ReturnsAsync(personModel);
            _consumerRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false))
                .ReturnsAsync(consumerModel);
            var consumerDataRequestDto = new ConsumerDataMockDto()
            {
                Person = new PersonDto() { Email = "test@test.com" },
                Consumer = new ConsumerDto()
            };

            // Act
            var response = await _consumerController.CancelConsumers(new List<ConsumerDataDto> { consumerDataRequestDto });

            // Assert
            var objectresult = response.Result as ObjectResult;
            var consumerResponse = objectresult?.Value as List<ConsumerDataResponseDto>;
            Assert.NotNull(response);
            Assert.Equal(StatusCodes.Status404NotFound, consumerResponse[0].ErrorCode);
            Assert.Equal(StatusCodes.Status200OK, objectresult?.StatusCode);
        }
        [Fact]
        public async Task DeleteConsumers_Should_Return_Ok_For_Success_Result()
        {
            var transactionMock = new Mock<ITransaction>();
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);
            var consumerService = new Mock<IConsumerService>();
            consumerService.Setup(service => service.CreateConsumers(It.IsAny<IList<ConsumerDataDto>>()))
                         .ReturnsAsync(new List<ConsumerDataResponseDto>());
            var consumerDataRequestDto = new ConsumerDataMockDto()
            {
                Person = new PersonDto(),

                Consumer = new ConsumerDto()
                {
                    SubscriberOnly = true,
                    EligibleEndTs = DateTime.UtcNow
                }

            };
            var res = await _consumerController.DeleteConsumers(new List<ConsumerDataDto> { consumerDataRequestDto });
            var result = res.Result as OkObjectResult;
            Assert.True(result?.Value != null);
            Assert.True(result?.StatusCode == 200);
        }

        [Fact]
        public async Task DeleteConsumers_Should_Return_Ok_For_When_Eligible_EndTS_Less_Than_Eligible_Start_TS()
        {
            var transactionMock = new Mock<ITransaction>();
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);
            var consumerService = new Mock<IConsumerService>();
            consumerService.Setup(service => service.CreateConsumers(It.IsAny<IList<ConsumerDataDto>>()))
                         .ReturnsAsync(new List<ConsumerDataResponseDto>());
            var consumerDataRequestDto = new ConsumerDataMockDto()
            {
                Person = new PersonDto(),

                Consumer = new ConsumerDto()
                {
                    SubscriberOnly = true,
                    EligibleStartTs = DateTime.UtcNow.AddDays(-10),
                    EligibleEndTs = DateTime.UtcNow.AddDays(-12)
                }

            };
            var res = await _consumerController.DeleteConsumers(new List<ConsumerDataDto> { consumerDataRequestDto });
            var result = res.Result as OkObjectResult;
            Assert.True(result?.Value != null);
            Assert.True(result?.StatusCode == 200);
        }

        [Fact]
        public async Task DeleteConsumers_Should_Return_Not_Found_When_Get_Empty_Consumers()
        {
            var transactionMock = new Mock<ITransaction>();
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);
            var consumerService = new Mock<IConsumerService>();
            consumerService.Setup(service => service.CreateConsumers(It.IsAny<IList<ConsumerDataDto>>()))
                         .ReturnsAsync(new List<ConsumerDataResponseDto>());

            var res = await _consumerController.DeleteConsumers(new List<ConsumerDataDto>());
            var result = res.Result as ObjectResult;
            Assert.True(result?.Value != null);
            Assert.True(result?.StatusCode == 404);
        }

        [Fact]
        public async Task DeleteConsumers_Should_Return_Internal_Server_Error_When_Service_Throws_Exception()
        {
            var transactionMock = new Mock<ITransaction>();
            _session.Setup(s => s.BeginTransaction()).Throws(new Exception("Testing"));
            var consumerService = new Mock<IConsumerService>();
            consumerService.Setup(service => service.CreateConsumers(It.IsAny<IList<ConsumerDataDto>>()))
                         .ReturnsAsync(new List<ConsumerDataResponseDto>());
            var consumerDataRequestDto = new ConsumerDataMockDto()
            {
                Person = new PersonDto(),

                Consumer = new ConsumerDto()
                {
                    SubscriberOnly = true,
                    EligibleEndTs = DateTime.UtcNow
                }

            };
            var res = await _consumerController.DeleteConsumers(new List<ConsumerDataDto> { consumerDataRequestDto });
            var result = res.Result as ObjectResult;
            Assert.True(result?.Value != null);
            Assert.True(result?.StatusCode == 500);
        }

        [Fact]
        public async Task CancelConsumers_Should_Not_update_Consumer_When_Exception_Occurs()
        {
            // Arrange
            var consumerDataRequestDto = new ConsumerDataMockDto()
            {
                Person = new PersonDto() { Email = "testing@test.com" },
                Consumer = new ConsumerDto()
            };

            var transactionMock = new Mock<ITransaction>();
            _session.Setup(s => s.BeginTransaction()).Throws(new Exception("Some thing went wrong"));
            _personRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<PersonModel, bool>>>(), false));
            _consumerRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false)).Throws(new Exception("Some thing went wrong"));

            // Act
            var response = await _consumerController.CancelConsumers(new List<ConsumerDataDto> { consumerDataRequestDto });

            // Assert
            var objectresult = response.Result as ObjectResult;
            var consumerResponse = objectresult?.Value as List<ConsumerDataResponseDto>;
            Assert.Null(response.Result);
            Assert.Null(objectresult);
            Assert.Null(consumerResponse);
        }
        [Fact]
        public async Task Consumer_And_Person_Should_Not_Update_When_Update_Consumers_Throws_Exception()
        {
            //Arrange
            var consumerDataRequestDto = new ConsumerDataMockDto();
            var personModel = new PersonMockModel();
            var transactionMock = new Mock<ITransaction>();
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);
            _personRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<PersonModel, bool>>>(), false)).ReturnsAsync(personModel);
            _consumerRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false)).Throws(new InvalidOperationException("Invalid operation"));

            //Act
            var response = await _consumerController.UpdateConsumers(new List<ConsumerDataDto> { consumerDataRequestDto, consumerDataRequestDto });

            //Assert
            var objectresult = response.Result as ObjectResult;
            var consumerResponse = objectresult.Value as List<ConsumerDataResponseDto>;
            Assert.NotNull(response);
            Assert.Equal(StatusCodes.Status500InternalServerError, consumerResponse[0].ErrorCode);
            Assert.Equal(StatusCodes.Status200OK, objectresult?.StatusCode);
        }
        [Fact]
        public async Task Consumer_And_Person_Should_Not_Update_When_Delete_Consumers_Throws_Exception()
        {
            //Arrange
            var consumerDataRequestDto = new ConsumerDataMockDto();
            var consumerModel = new ConsumerMockModel();
            var transactionMock = new Mock<ITransaction>();
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);
            _personRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<PersonModel, bool>>>(), false)).Throws(new InvalidOperationException("Invalid operation"));
            _consumerRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false)).ReturnsAsync(consumerModel);

            //Act
            var response = await _consumerController.CancelConsumers(new List<ConsumerDataDto> { consumerDataRequestDto, consumerDataRequestDto });

            //Assert
            var objectresult = response.Result as ObjectResult;
            var consumerResponse = objectresult.Value as List<ConsumerDataResponseDto>;
            Assert.NotNull(response);
            Assert.Equal(StatusCodes.Status500InternalServerError, consumerResponse[0].ErrorCode);
            Assert.Equal(StatusCodes.Status200OK, objectresult?.StatusCode);
        }

        [Fact]
        public async Task GetConsumerDetailsByTenantCode_ReturnsOkResult_WhenDataExists()
        {
            // Arrange
            var tenantCode = "TestTenantCode";
            var requestDto = new GetConsumerByTenantRequestDto { TenantCode = tenantCode, PageNumber = 1, PageSize = 1 };

            // Act
            var result = await _consumerController.GetConsumerDetailsByTenantCode(requestDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var resultValue = Assert.IsType<ConsumersAndPersonsListResponseDto>(okResult.Value);
            Assert.NotNull(resultValue);
            Assert.NotEmpty(resultValue.ConsumerAndPersons);
        }

        [Fact]
        public async Task GetConsumerDetailsByTenantCode_ReturnsNotFound_WhenNoRecordsFound()
        {
            // Arrange
            var tenantCode = "TestTenantCode";
            var requestDto = new GetConsumerByTenantRequestDto { TenantCode = tenantCode, PageNumber = 1, PageSize = 1 };
            _personRepo.Setup(x => x.GetConsumerPersons(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()));

            // Act
            var result = await _consumerController.GetConsumerDetailsByTenantCode(requestDto);

            // Assert
            var notFoundResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
        }


        [Fact]
        public async Task GetConsumerDetailsByTenantCode_ReturnsInternalServerError_WhenExceptionThrown()
        {
            // Arrange
            var tenantCode = "TestTenantCode";
            var requestDto = new GetConsumerByTenantRequestDto { TenantCode = tenantCode, PageNumber = 1, PageSize = 1 };

            //mock
            _personRepo.Setup(x => x.GetConsumerPersons(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            .ThrowsAsync(new Exception("intended exception"));

            // Act
            var result = await _consumerController.GetConsumerDetailsByTenantCode(requestDto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        [Fact]
        public async Task Update_Consumer_Should_Return_Ok_Response()
        {
            // Arrange 
            var requestDto = GetConsumerRequestDto();
            var consumerId = 124567;
            _consumerRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false)).ReturnsAsync(new ConsumerMockModel());

            // Act 
            var response = await _consumerController.UpdateConsumerAsync(consumerId, requestDto);

            // Assert
            Assert.NotNull(response);
            var okObjectResponse = Assert.IsType<OkObjectResult>(response);
            Assert.Equal(StatusCodes.Status200OK, okObjectResponse.StatusCode);
        }
        [Fact]
        public async Task Update_Consumer_Should_Return_Not_Found_Response_When_Consumer_Is_Null()
        {
            // Arrange 
            var requestDto = GetConsumerRequestDto();
            var consumerId = 124567;
            _consumerRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false));

            // Act 
            var response = await _consumerController.UpdateConsumerAsync(consumerId, requestDto);

            // Assert
            Assert.NotNull(response);
            var objectResponse = Assert.IsType<ObjectResult>(response);
            Assert.Equal(StatusCodes.Status404NotFound, objectResponse.StatusCode);
        }
        [Fact]
        public async Task Update_Consumer_Should_Return_Internal_Server_Error_Response_When_Exception_Occurs()
        {
            // Arrange 
            var requestDto = GetConsumerRequestDto();
            var consumerId = 124567;
            _consumerRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false)).Throws(new Exception("Testing"));

            // Act 
            var response = await _consumerController.UpdateConsumerAsync(consumerId, requestDto);

            // Assert
            Assert.NotNull(response);
            var objectResponse = Assert.IsType<ObjectResult>(response);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResponse.StatusCode);
        }
        private ConsumerRequestDto GetConsumerRequestDto()
        {
            return new ConsumerRequestDto()
            {
                ConsumerCode = "cmr-f67b1adbed33411dbe797eb300a83b0c",
                TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4",
                ConsumerAttribute = "{ \"benefitsCardOptions\": { \"cardCreateOptions\": { \"deliveryMethod\": 11 } }, \"profileSettings\": { \"healthMetricsEnabled\": true } }"
            };
        }


        [Fact]
        public async Task UpdateOnboardingState_ShouldReturnConsumerResponse_WhenConsumerFoundAndStateUpdated()
        {
            // Arrange
            var updateDto = new UpdateOnboardingStateDto
            {
                ConsumerCode = "12345",
                TenantCode = "tenant1",
                OnboardingState = OnboardingState.VERIFIED,
                //AgreementUrl = "https://test/cms/html/tenant1/t_and_c.html"
                HtmlFileName = new Dictionary<string, string> { { "key", "test" } }
            };

            var consumerModel = new ConsumerModel
            {
                ConsumerCode = "12345",
                TenantCode = "tenant1",
                OnBoardingState = OnboardingState.NOT_STARTED.ToString(),
                DeleteNbr = 0
            };

            var updatedConsumerModel = new ConsumerModel
            {
                ConsumerCode = "12345",
                TenantCode = "tenant1",
                OnBoardingState = OnboardingState.VERIFIED.ToString(),
                DeleteNbr = 0
            };

            _consumerRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false))
                .ReturnsAsync(consumerModel);

            _consumerRepo
                .Setup(repo => repo.UpdateAsync(It.IsAny<ConsumerModel>()))
                .ReturnsAsync(consumerModel);

            // Act
            var result = await _consumerController.OnboardingState(updateDto);

            // Assert
            Assert.NotNull(result);
            var objectResponse = Assert.IsType<OkObjectResult>(result.Result);
            var resultValue = Assert.IsType<ConsumerResponseDto>(objectResponse.Value);
            Assert.NotNull(objectResponse.Value);
            Assert.Equal(updateDto.OnboardingState.ToString(), resultValue.Consumer.OnBoardingState);
            Assert.Equal(EnrollmentStatus.ENROLLED.ToString(), resultValue.Consumer.EnrollmentStatus);

            _consumerRepo.Verify(repo => repo.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(),false), Times.Once);
            _consumerRepo.Verify(repo => repo.UpdateAsync(It.IsAny<ConsumerModel>()), Times.Once);
        }
        [Fact]
        public async Task UpdateOnboardingState_ShouldReturnConsumerResponse_WhenConsumerFoundAndfileuploadedUpdated()
        {
            // Arrange
            var updateDto = new UpdateOnboardingStateDto
            {
                ConsumerCode = "12345",
                TenantCode = "tenant1",
                OnboardingState = OnboardingState.VERIFIED,
                HtmlFileName = new Dictionary<string, string> { { "key", "test" } }
            };

            var consumerModel = new ConsumerModel
            {
                ConsumerCode = "12345",
                TenantCode = "tenant1",
                OnBoardingState = OnboardingState.NOT_STARTED.ToString(),
                DeleteNbr = 0
            };

            var updatedConsumerModel = new ConsumerModel
            {
                ConsumerCode = "12345",
                TenantCode = "tenant1",
                OnBoardingState = OnboardingState.VERIFIED.ToString(),
                DeleteNbr = 0
            };

            _consumerRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false))
                .ReturnsAsync(consumerModel);
            _uploadpdfService.Setup(service => service.UploadAgreementPDf(updateDto, It.IsAny<string>(), It.IsAny<string>()))
                            .ReturnsAsync( new Dictionary<string, string> { { "key", "test.pdf" } }
);

            _consumerRepo
                .Setup(repo => repo.UpdateAsync(It.IsAny<ConsumerModel>()))
                .ReturnsAsync(consumerModel);

            // Act
            var result = await _consumerController.OnboardingState(updateDto);

            // Assert
            Assert.NotNull(result);
            var objectResponse = Assert.IsType<OkObjectResult>(result.Result);
            var resultValue = Assert.IsType<ConsumerResponseDto>(objectResponse.Value);
            Assert.NotNull(objectResponse.Value);
            Assert.Equal(updateDto.OnboardingState.ToString(), resultValue.Consumer.OnBoardingState);
            Assert.Equal(EnrollmentStatus.ENROLLED.ToString(), resultValue.Consumer.EnrollmentStatus);

            _consumerRepo.Verify(repo => repo.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(),false), Times.Once);
            _consumerRepo.Verify(repo => repo.UpdateAsync(It.IsAny<ConsumerModel>()), Times.Once);
        }
        [Fact]
        public async Task UpdateOnboardingState_ShouldReturnConsumerResponse_WhenConsumerFoundAndInvalidfile()
        {
            // Arrange
            var updateDto = new UpdateOnboardingStateDto
            {
                ConsumerCode = "12345",
                TenantCode = "tenant1",
                OnboardingState = OnboardingState.VERIFIED,
                
            };

            var consumerModel = new ConsumerModel
            {
                ConsumerCode = "12345",
                TenantCode = "tenant1",
                OnBoardingState = OnboardingState.NOT_STARTED.ToString(),
                DeleteNbr = 0
            };

            var updatedConsumerModel = new ConsumerModel
            {
                ConsumerCode = "12345",
                TenantCode = "tenant1",
                OnBoardingState = OnboardingState.VERIFIED.ToString(),
                DeleteNbr = 0
            };

            _consumerRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false))
                .ReturnsAsync(consumerModel);
           

            _consumerRepo
                .Setup(repo => repo.UpdateAsync(It.IsAny<ConsumerModel>()))
                .ReturnsAsync(consumerModel);

            // Act
            var result = await _consumerController.OnboardingState(updateDto);

            // Assert
            Assert.NotNull(result);
            var objectResponse = Assert.IsType<OkObjectResult>(result.Result);
            var resultValue = Assert.IsType<ConsumerResponseDto>(objectResponse.Value);
            Assert.NotNull(objectResponse.Value);
            Assert.Equal(updateDto.OnboardingState.ToString(), resultValue.Consumer.OnBoardingState);
            Assert.Equal(EnrollmentStatus.ENROLLED.ToString(), resultValue.Consumer.EnrollmentStatus);

            _consumerRepo.Verify(repo => repo.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(),false), Times.Once);
            _consumerRepo.Verify(repo => repo.UpdateAsync(It.IsAny<ConsumerModel>()), Times.Once);
        }

        [Fact]
        public async Task UpdateOnboardingState_ShouldThrowInvalidDataException_WhenConsumerNotFound()
        {
            // Arrange
            var updateDto = new UpdateOnboardingStateDto
            {
                ConsumerCode = "12345",
                TenantCode = "tenant1",
                OnboardingState = OnboardingState.VERIFIED,
                HtmlFileName = new Dictionary<string, string> { { "key", "test" } }

            };

            _consumerRepo
                .Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false));


            // Act
            var result = await _consumerController.OnboardingState(updateDto);

            // Assert
            Assert.NotNull(result);
            var objectResponse = Assert.IsType<ObjectResult>(result.Result);
            var resultValue = Assert.IsType<ConsumerResponseDto>(objectResponse.Value);
            Assert.Equal(500, resultValue.ErrorCode);
            _consumerRepo.Verify(repo => repo.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(),false), Times.Once);
            _consumerRepo.Verify(repo => repo.UpdateAsync(It.IsAny<ConsumerModel>()), Times.Never);
        }

        [Fact]
        public async Task UpdateOnboardingState_ShouldReturnConsumerResponse_WhenStateIsAlreadyUpdated()
        {
            // Arrange
            var updateDto = new UpdateOnboardingStateDto
            {
                ConsumerCode = "12345",
                TenantCode = "tenant1",
                OnboardingState = OnboardingState.PICK_A_PURSE_COMPLETED
            };

            var consumer = new ConsumerModel
            {
                ConsumerCode = "12345",
                TenantCode = "tenant1",
                OnBoardingState = OnboardingState.PICK_A_PURSE_COMPLETED.ToString(),
                DeleteNbr = 0
            };

            _consumerRepo
                .Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(),false))
                .ReturnsAsync(consumer);

            // Act
            var result = await _consumerController.OnboardingState(updateDto);

            // Assert
            Assert.NotNull(result);
            var objectResponse = Assert.IsType<OkObjectResult>(result.Result);
            var resultValue = Assert.IsType<ConsumerResponseDto>(objectResponse.Value);
            Assert.NotNull(resultValue.Consumer);
            Assert.Equal(updateDto.OnboardingState.ToString(), resultValue.Consumer.OnBoardingState);

            _consumerRepo.Verify(repo => repo.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(),false), Times.Once);
            _consumerRepo.Verify(repo => repo.UpdateAsync(It.IsAny<ConsumerModel>()), Times.Never);
        }
        [Fact]
        public async Task UpdateOnboardingState_ShouldReturnConsumerResponse_WhenStateIsVerified()
        {
            // Arrange  
            var updateDto = new UpdateOnboardingStateDto
            {
                ConsumerCode = "12345",
                TenantCode = "tenant1",
                OnboardingState = OnboardingState.VERIFIED,
                HtmlFileName = new Dictionary<string, string> { { "key", "test" } }
            };

            var consumer = new ConsumerModel
            {
                ConsumerCode = "12345",
                TenantCode = "tenant1",
                OnBoardingState = OnboardingState.PICK_A_PURSE_COMPLETED.ToString(),
                DeleteNbr = 0
            };

            _consumerRepo
                .Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(),false))
                .ReturnsAsync(consumer);

            // Act
            var result = await _consumerController.OnboardingState(updateDto);

            // Assert
            Assert.NotNull(result);
            var objectResponse = Assert.IsType<OkObjectResult>(result.Result);
            var resultValue = Assert.IsType<ConsumerResponseDto>(objectResponse.Value);
            Assert.NotNull(resultValue.Consumer);
            Assert.Equal(updateDto.OnboardingState.ToString(), resultValue.Consumer.OnBoardingState);

            _consumerRepo.Verify(repo => repo.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(),false), Times.Once);
            _consumerRepo.Verify(repo => repo.UpdateAsync(It.IsAny<ConsumerModel>()), Times.Never);
        }

        [Fact]
        public async Task UpdateOnboardingState_ShouldReturnErrorResponse_WhenExceptionOccurs()
        {
            // Arrange
            var updateDto = new UpdateOnboardingStateDto
            {
                ConsumerCode = "12345",
                TenantCode = "tenant1",
                OnboardingState = OnboardingState.VERIFIED,
                HtmlFileName = new Dictionary<string, string> { { "key", "test" } }
            };

            _consumerRepo
                .Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(),false))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _consumerService.UpdateOnboardingState(updateDto);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.ErrorMessage);
            Assert.Equal(StatusCodes.Status500InternalServerError, result.ErrorCode);

            _consumerRepo.Verify(repo => repo.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(),false), Times.Once);
            _consumerRepo.Verify(repo => repo.UpdateAsync(It.IsAny<ConsumerModel>()), Times.Never);
        }
       

        [Fact]
        public async Task GetConsumerByPersonUniqueIdentifier_ReturnsBadRequest_WhenConsumerIsEmpty()
        {
            // Arrange
            var personUniqueIdentifier = "";

            // Act
            var result = await _consumerController.GetConsumerByPersonUniqueIdentifier(personUniqueIdentifier);

            // Assert
            var errorResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status400BadRequest, errorResult.StatusCode);
        }

        [Fact]
        public async Task GetConsumerByPersonUniqueIdentifier_ReturnsNotFound_WhenPersonIsNotFound()
        {
            // Arrange
            var personUniqueIdentifier = "12345";

            _personRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<PersonModel, bool>>>(), false));

            // Act
            var result = await _consumerController.GetConsumerByPersonUniqueIdentifier(personUniqueIdentifier);

            // Assert
            var errorResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status404NotFound, errorResult.StatusCode);
        }

        [Fact]
        public async Task GetConsumerByPersonUniqueIdentifier_ReturnsInternallServerError_WhenPersonRepoThrowsError()
        {
            // Arrange
            var personUniqueIdentifier = "12345";

            _personRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<PersonModel, bool>>>(), false))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _consumerController.GetConsumerByPersonUniqueIdentifier(personUniqueIdentifier);

            // Assert
            var errorResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, errorResult.StatusCode);
        }

        [Fact]
        public async Task GetConsumerByPersonUniqueIdentifier_ReturnsNotFound_WhenConsumerIsNotFound()
        {
            // Arrange
            var personUniqueIdentifier = "12345";

            // Act
            var result = await _consumerController.GetConsumerByPersonUniqueIdentifier(personUniqueIdentifier);

            // Assert
            var errorResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status404NotFound, errorResult.StatusCode);
        }

        [Fact]
        public async Task GetConsumerByPersonUniqueIdentifier_ReturnsOk_WhenConsumerIsFound()
        {
            // Arrange
            var personUniqueIdentifier = "12345";
            _consumerRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false))
                .ReturnsAsync(new List<ConsumerModel>
                {
                    new ConsumerModel()
                    {
                        ConsumerCode = "12345",
                        TenantCode = "tenant1",
                        OnBoardingState = OnboardingState.VERIFIED.ToString(),
                        DeleteNbr = 0
                    }
                });

            // Act
            var result = await _consumerController.GetConsumerByPersonUniqueIdentifier(personUniqueIdentifier);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Fact]
        public async Task GetConsumersByConsumerCodes_ShouldReturnData_WhenRecordsExist()
        {
            // Arrange
            var consumerCodes = new List<string> { "C123", "C456" };
            var tenantCode = "TENANT001";

            var request = new GetConsumerByConsumerCodes
            {
                ConsumerCodes = consumerCodes,
                TenantCode = tenantCode
            };

            var lstConsumerPerson = new List<ConsumersAndPersonsModels>();

            lstConsumerPerson.Add(new ConsumersAndPersonsModels(
              new PersonModel() { },
              new ConsumerModel()) {
            });


            _personRepo.Setup(repo => repo.GetConsumerPersons(consumerCodes, tenantCode))
                .ReturnsAsync(lstConsumerPerson);

            // Act
            var result = await _consumerController.GetConsumerDetailsByConsumerCodes(request);

            // Assert
            Assert.NotNull(result);
            var objectResponse = Assert.IsType<OkObjectResult>(result.Result);
            var resultValue = Assert.IsType<ConsumersAndPersonsListResponseDto>(objectResponse.Value);
            Assert.NotNull(resultValue.ConsumerAndPersons);
        }

        [Fact]
        public async Task GetConsumerDetailsByConsumerCodes_ShouldReturn500_WhenExceptionIsThrown()
        {
            // Arrange
            var request = new GetConsumerByConsumerCodes
            {
                ConsumerCodes = new List<string> { "C123", "C456" },
                TenantCode = "TENANT001"
            };

            _personRepo.Setup(repo => repo.GetConsumerPersons(It.IsAny<List<string>>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Unexpected error"));

            // Act
            var result = await _consumerController.GetConsumerDetailsByConsumerCodes(request);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);

            var response = Assert.IsType<ConsumersAndPersonsListResponseDto>(objectResult.Value);
            Assert.Equal(StatusCodes.Status500InternalServerError, response.ErrorCode);
        }

        [Fact]
        public void MemberImportFileDataModel_PropertySettersAndGetters_WorkCorrectly()
        {
            // Arrange
            var model = new MemberImportFileDataModel
            {
                MemberImportFileDataId = 1,
                MemberImportFileId = 100,
                RecordNumber = 5,
                RawDataJson = "{ \"name\": \"John\" }",
                Age = "30",
                Dob = new DateTime(1995, 5, 1),
                City = "New York",
                Email = "john@example.com",
                Action = "Add",
                Gender = "Male",
                Country = "USA",
                MemNbr = "MEM123",
                PlanId = "PLN01",
                HomeCity = "Brooklyn",
                LastName = "Doe",
                MemberId = "MID001",
                PlanType = "Gold",
                EmpOrDep = "EMP",
                FirstName = "John",
                HomeState = "NY",
                IsSsoUser = true,
                MemberType = "Primary",
                MiddleName = "A",
                PostalCode = "10001",
                RegionCode = "R1",
                SubgroupId = "SG001",
                MobilePhone = "1234567890",
                PartnerCode = "P001",
                LanguageCode = "EN",
                MailingState = "NY",
                MemNbrPrefix = "PRF",
                EligibilityStart = new DateTime(2023, 1, 1),
                EligibilityEnd = new DateTime(2023, 12, 31),
                HomePostalCode = "10002",
                HomePhoneNumber = "9876543210",
                HomeAddressLine1 = "123 Main St",
                HomeAddressLine2 = "Apt 4B",
                SubscriberMemNbr = "SUB123",
                MailingCountryCode = "US",
                MailingAddressLine1 = "456 Other St",
                MailingAddressLine2 = "Suite 101",
                PersonUniqueIdentifier = "PID001",
                SubscriberMemNbrPrefix = "SUBPRF"
            };

            // Assert
            Assert.Equal(1, model.MemberImportFileDataId);
            Assert.Equal("John", model.FirstName);
            Assert.Equal("Doe", model.LastName);
            Assert.Equal("Gold", model.PlanType);
            Assert.True(model.IsSsoUser);
            Assert.Equal("SUB123", model.SubscriberMemNbr);
            Assert.Equal(new DateTime(2023, 12, 31), model.EligibilityEnd);
        }

        [Fact]
        public async Task GetConsumersByMemNbrAndRegionCode_MissingParams_Returns400()
        {
            // Act
            var result = await _consumerController.GetConsumersByMemNbrAndRegionCode("", "");

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);

            var response = Assert.IsType<ConsumerPersonResponseDto>(objectResult.Value);
            Assert.Equal("MemberNbr and RegionCode are required.", response.ErrorMessage);
        }

        [Fact]
        public async Task GetConsumersByMemNbrAndRegionCode_MemberNotFound_Returns404()
        {
            // Arrange

            _memberImportFileDataRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<MemberImportFileDataModel, bool>>>(), false))
                .ReturnsAsync(new List<MemberImportFileDataModel>());

            // Act
            var result = await _consumerController.GetConsumersByMemNbrAndRegionCode("123", "R1");

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);

            var response = Assert.IsType<ConsumerPersonResponseDto>(objectResult.Value);
            Assert.Equal("Member not found.", response.ErrorMessage);
        }

        [Fact]
        public async Task GetConsumersByMemNbrAndRegionCode_ConsumersNotFound_Returns404()
        {
            // Arrange
            _memberImportFileDataRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<MemberImportFileDataModel, bool>>>(), false))
                .ReturnsAsync(new List<MemberImportFileDataModel> { new() { MemberId = "101", MemberImportFileDataId = 10 } });

            _consumerRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false))
                .ReturnsAsync(new List<ConsumerModel>());

            // Act
            var result = await _consumerController.GetConsumersByMemNbrAndRegionCode("123", "R1");

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);

            var response = Assert.IsType<ConsumerPersonResponseDto>(objectResult.Value);
            Assert.Equal("No consumers found.", response.ErrorMessage);
        }

        [Fact]
        public async Task GetConsumersByMemNbrAndRegionCode_PersonNotFound_Returns404()
        {
            // Arrange
            var consumerList = new List<ConsumerModel> { new() { ConsumerId = 101, PersonId = 999 } };

            _memberImportFileDataRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<MemberImportFileDataModel, bool>>>(), false))
                .ReturnsAsync(new List<MemberImportFileDataModel> { new() { MemberId = "101", MemberImportFileDataId = 10 } });

            _consumerRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false))
                .ReturnsAsync(consumerList);

            _personRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<PersonModel, bool>>>(), false));

            // Act
            var result = await _consumerController.GetConsumersByMemNbrAndRegionCode("123", "R1");

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);

            var response = Assert.IsType<ConsumerPersonResponseDto>(objectResult.Value);
            Assert.Equal("Person not found.", response.ErrorMessage);
        }

        [Fact]
        public async Task GetConsumersByMemNbrAndRegionCode_ValidRequest_Returns200()
        {
            // Arrange
            var consumers = new List<ConsumerModel> { new() { ConsumerId = 101, PersonId = 999 } };
            var person = new PersonModel { PersonId = 999 };

            _memberImportFileDataRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<MemberImportFileDataModel, bool>>>(),false))
                .ReturnsAsync(new List<MemberImportFileDataModel> { new() { MemberId = "101", MemberImportFileDataId = 10 } });

            _consumerRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(),false))
                .ReturnsAsync(consumers);

            _personRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<PersonModel, bool>>>(), false))
                .ReturnsAsync(person);
            var memberImportFileDataMap = new MemberImportFileDataMap();

            // Act
            var result = await _consumerController.GetConsumersByMemNbrAndRegionCode("123", "R1");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<ConsumerPersonResponseDto>(okResult.Value);
            Assert.NotNull(response.Consumer);
            Assert.NotNull(response.Person);
            Assert.NotNull(memberImportFileDataMap);
        }

        [Fact]
        public async Task GetConsumersByMemNbrAndRegionCode_ExceptionThrown_Returns500()
        {
            // Arrange
            _memberImportFileDataRepo
                .Setup(x => x.FindAsync(It.IsAny<Expression<Func<MemberImportFileDataModel, bool>>>(), false))
                .ThrowsAsync(new Exception("Unexpected DB error"));
           
            // Act
            var result = await _consumerController.GetConsumersByMemNbrAndRegionCode("123", "R1");

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);

            var response = Assert.IsType<ConsumerPersonResponseDto>(objectResult.Value);
            Assert.Equal(StatusCodes.Status500InternalServerError, response.ErrorCode);
            Assert.Equal("Unexpected DB error", response.ErrorMessage);
        }

        [Fact]
        public async Task GetConsumerByDOB_Controller_ShouldReturnOk_WhenRecordsExist()
        {
            // Arrange
            var request = new GetConsumerByTenantCodeAndDOBRequestDto
            {
                TenantCode = "TENANT001",
                DOB = DateTime.UtcNow
            };

            var lstConsumerPerson = new List<ConsumersAndPersonsModels>
            {
                new ConsumersAndPersonsModels(
                    new PersonModel { PersonId = 1, DOB = request.DOB.Value, DeleteNbr = 0 },
                    new ConsumerModel { ConsumerId = 10, TenantCode = request.TenantCode, DeleteNbr = 0 })
            };

            _personRepo.Setup(repo => repo.GetConsumerPersonsByDOB(It.IsAny<GetConsumerByTenantCodeAndDOBRequestDto>()))
                .ReturnsAsync(lstConsumerPerson);

            // Act
            var result = await _consumerController.GetConsumerByDOB(request);

            // Assert
            Assert.NotNull(result);
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

            var response = Assert.IsType<ConsumersAndPersonsListResponseDto>(okResult.Value);
            Assert.NotEmpty(response.ConsumerAndPersons);
            Assert.Equal(10, response.ConsumerAndPersons.First().Consumer.ConsumerId);
            Assert.Equal(1, response.ConsumerAndPersons.First().Person.PersonId);
        }

        [Fact]
        public async Task GetConsumerByDOB_Controller_ShouldReturn404_WhenNoRecordsFound()
        {
            // Arrange
            var request = new GetConsumerByTenantCodeAndDOBRequestDto
            {
                TenantCode = "TENANT001",
                DOB = DateTime.UtcNow
            };

            _personRepo.Setup(repo => repo.GetConsumerPersonsByDOB(It.IsAny<GetConsumerByTenantCodeAndDOBRequestDto>()))
                .ReturnsAsync(new List<ConsumersAndPersonsModels>()); // empty list

            // Act
            var result = await _consumerController.GetConsumerByDOB(request);

            // Assert
            Assert.NotNull(result);
            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);

            var response = Assert.IsType<ConsumersAndPersonsListResponseDto>(objectResult.Value);
            Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
        }

        [Fact]
        public async Task GetConsumerByDOB_Controller_ShouldReturn500_WhenRepoThrows()
        {
            // Arrange
            var request = new GetConsumerByTenantCodeAndDOBRequestDto
            {
                TenantCode = "TENANT001",
                DOB = DateTime.UtcNow
            };

            _personRepo.Setup(repo => repo.GetConsumerPersonsByDOB(It.IsAny<GetConsumerByTenantCodeAndDOBRequestDto>()))
                .ThrowsAsync(new Exception("Unexpected error"));

            // Act
            var result = await _consumerController.GetConsumerByDOB(request);

            // Assert
            Assert.NotNull(result);
            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);

            var response = Assert.IsType<ConsumersAndPersonsListResponseDto>(objectResult.Value);
            Assert.Equal(StatusCodes.Status500InternalServerError, response.ErrorCode);
        }

        [Fact]
        public async Task GetConsumerPersonsByDOB_ShouldThrowInvalidOperationException_WhenSessionFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BaseRepo<PersonModel>>>();
            var sessionMock = new Mock<NHibernate.ISession>();

            sessionMock.Setup(s => s.Query<ConsumerModel>())
                       .Throws(new Exception("DB failure"));

            var repo = new PersonRepo(loggerMock.Object, sessionMock.Object);

            var request = new GetConsumerByTenantCodeAndDOBRequestDto
            {
                TenantCode = "TENANT001",
                DOB = DateTime.UtcNow
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => repo.GetConsumerPersonsByDOB(request));
        }

        [Fact]
        public async Task UpdateConsumerSubscriptionStatus_ShouldReturnOk_WhenValidRequest()
        {
            // Arrange
            var request = new ConsumerSubscriptionStatusRequestDto
            {
                TenantCode = "HAP-TENANT-CODE",
                ConsumerCode = "C001",
                ConsumerSubscriptionStatuses = new ConsumerSubscriptionStatusDetailDto[]
                {
                    new ConsumerSubscriptionStatusDetailDto { Feature = "PickAPurse", Status = "subscribed"}
                }
            };

            _tenantClient.Setup(c => c.Post<TenantDto>(Constant.GetTenantByTenantCode, It.IsAny<GetTenantCodeRequestDto>()))
                .ReturnsAsync(new TenantDto
                {
                    TenantCode = "HAP-TENANT-CODE",
                    TenantName = "Healthy America Program",
                    TenantOption = "{ \"subscriptionStatus\": [ { \"feature\": \"PickAPurse\", \"status\": \"subscribed\" }, { \"feature\": \"HealthTips\", \"status\": \"unsubscribed\" } ] }"

                });
            _consumerRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false)).ReturnsAsync(new ConsumerModel());
            // Act
            var result = await _consumerController.UpdateConsumerSubscriptionStatus(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<BaseResponseDto>(okResult.Value);
            Assert.Null(response.ErrorMessage);
        }

        [Fact]
        public async Task UpdateConsumerSubscriptionStatus_ShouldReturnBadRequest_WhenTenantCodeMissing()
        {
            // Arrange
            var request = new ConsumerSubscriptionStatusRequestDto
            {
                TenantCode = null,
                ConsumerCode = "C001"
            };

            // Act
            var result = await _consumerController.UpdateConsumerSubscriptionStatus(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<BaseResponseDto>(okResult.Value);
            Assert.Equal("Invalid request.", response.ErrorMessage);
        }

        [Fact]
        public async Task UpdateConsumerSubscriptionStatus_ShouldReturnError_WhenConsumerNotFound()
        {
            // Arrange
            var request = new ConsumerSubscriptionStatusRequestDto
            {
                TenantCode = "INVALID-TENANT",
                ConsumerCode = "UNKNOWN",
                ConsumerSubscriptionStatuses = new ConsumerSubscriptionStatusDetailDto[]
                {
                    new ConsumerSubscriptionStatusDetailDto { Feature = "PickAPurse", Status = "subscribed"}
                }
            };
            _tenantClient.Setup(c => c.Post<TenantDto>(Constant.GetTenantByTenantCode, It.IsAny<GetTenantCodeRequestDto>()))
                .ReturnsAsync(new TenantDto
                {
                    TenantCode = "HAP-TENANT-CODE",
                    TenantName = "Healthy America Program",
                    TenantOption = "{ \"subscriptionStatus\": [ { \"feature\": \"PickAPurse\", \"status\": \"subscribed\" }, { \"feature\": \"HealthTips\", \"status\": \"unsubscribed\" } ] }"

                });
            _consumerRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false)).ReturnsAsync((ConsumerModel)null);
            // Act
            var result = await _consumerController.UpdateConsumerSubscriptionStatus(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<BaseResponseDto>(okResult.Value);
            Assert.Equal("Consumer not found.", response.ErrorMessage);
        }

        [Fact]
        public async Task UpdateConsumerSubscriptionStatus_ShouldReturnError_WhenTenantOptionMissing()
        {
            // Arrange
            var request = new ConsumerSubscriptionStatusRequestDto
            {
                TenantCode = "TENANT_WITHOUT_OPTIONS",
                ConsumerCode = "C001",
                ConsumerSubscriptionStatuses = new ConsumerSubscriptionStatusDetailDto[]
                {
                    new ConsumerSubscriptionStatusDetailDto { Feature = "PickAPurse", Status = "subscribed"}
                }
            };
            _tenantClient.Setup(c => c.Post<TenantDto>(Constant.GetTenantByTenantCode, It.IsAny<GetTenantCodeRequestDto>()))
              .ReturnsAsync(new TenantDto
              {
                  TenantCode = "HAP-TENANT-CODE",
                  TenantName = "Healthy America Program",
              });
            // Act
            var result = await _consumerController.UpdateConsumerSubscriptionStatus(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<BaseResponseDto>(okResult.Value);
            Assert.Equal("Invalid tenant configuration.", response.ErrorMessage);
        }

        [Fact]
        public async Task UpdateConsumerSubscriptionStatus_ShouldReturnError_WhenNoSubscriptionStatuses()
        {
            // Arrange
            var request = new ConsumerSubscriptionStatusRequestDto
            {
                TenantCode = "HAP-TENANT-CODE",
                ConsumerCode = "C001",
            };
            

            // Act
            var result = await _consumerController.UpdateConsumerSubscriptionStatus(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<BaseResponseDto>(okResult.Value);
            Assert.Equal("No subscription status records to process.", response.ErrorMessage);
        }

        [Fact]
        public async Task UpdateConsumerSubscriptionStatus_ShouldHandle_InvalidConsumerJson()
        {
            // Arrange
            // Set consumer in mock repo with invalid SubscriptionStatusJson manually if repo allows
            var request = new ConsumerSubscriptionStatusRequestDto
            {
                TenantCode = "HAP-TENANT-CODE",
                ConsumerCode = "CONSUMER_INVALID_JSON",
                ConsumerSubscriptionStatuses = new ConsumerSubscriptionStatusDetailDto[]
                {
                    new ConsumerSubscriptionStatusDetailDto { Feature = "PickAPurse", Status = "subscribed"}
                }
            };
            _tenantClient.Setup(c => c.Post<TenantDto>(Constant.GetTenantByTenantCode, It.IsAny<GetTenantCodeRequestDto>()))
            .ReturnsAsync(new TenantDto
            {
                TenantCode = "HAP-TENANT-CODE",
                TenantName = "Healthy America Program",
                TenantOption = "{ \"subscriptionStatus\": [ { \"feature\": \"PickAPurse\", \"status\": \"subscribed\" }, { \"feature\": \"HealthTips\", \"status\": \"unsubscribed\" } ] }"

            });
            _consumerRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false)).ReturnsAsync(new ConsumerModel { SubscriptionStatusJson  = "{ \"subscriptionFeatureStatus\": [ { \"feature\": \"PickAPurse\" \"status\": \"subscribed\" } ] }" });

            // Act
            var result = await _consumerController.UpdateConsumerSubscriptionStatus(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<BaseResponseDto>(okResult.Value);
            Assert.Null(response.ErrorMessage); 
        }

        [Fact]
        public async Task UpdateConsumerSubscriptionStatus_ShouldIgnore_FeatureNotInTenantOption()
        {
            // Arrange
            var request = new ConsumerSubscriptionStatusRequestDto
            {
                TenantCode = "HAP-TENANT-CODE",
                ConsumerCode = "C001",
                ConsumerSubscriptionStatuses = new ConsumerSubscriptionStatusDetailDto[]
                {
                    new ConsumerSubscriptionStatusDetailDto { Feature = "myCard", Status = "subscribed"}
                }
            };
            _tenantClient.Setup(c => c.Post<TenantDto>(Constant.GetTenantByTenantCode, It.IsAny<GetTenantCodeRequestDto>()))
               .ReturnsAsync(new TenantDto
               {
                   TenantCode = "HAP-TENANT-CODE",
                   TenantName = "Healthy America Program",
                   TenantOption = "{ \"subscriptionStatus\": [ { \"feature\": \"PickAPurse\", \"status\": \"subscribed\" }, { \"feature\": \"HealthTips\", \"status\": \"unsubscribed\" } ] }"

               });
            _consumerRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false)).ReturnsAsync(new ConsumerModel());
            // Act
            var result = await _consumerController.UpdateConsumerSubscriptionStatus(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<BaseResponseDto>(okResult.Value);
            Assert.Null(response.ErrorMessage);
        }

        [Fact]
        public async Task UpdateConsumerSubscriptionStatus_ShouldReturn500_OnUnhandledException()
        {
            // Arrange
            var request = new ConsumerSubscriptionStatusRequestDto
            {
                TenantCode = "THROW-EXCEPTION",
                ConsumerCode = "C001",
                ConsumerSubscriptionStatuses = new ConsumerSubscriptionStatusDetailDto[]
                {
                    new ConsumerSubscriptionStatusDetailDto { Feature = "PickAPurse", Status = "subscribed"}
                }
            };
            _tenantClient.Setup(c => c.Post<TenantDto>(Constant.GetTenantByTenantCode, It.IsAny<GetTenantCodeRequestDto>())).ThrowsAsync(new Exception("Unexpected error"));
            // Act
            var result = await _consumerController.UpdateConsumerSubscriptionStatus(request);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            var response = Assert.IsType<BaseResponseDto>(objectResult.Value);
            Assert.Equal(StatusCodes.Status500InternalServerError, response.ErrorCode);
        }

        #region AgreementsVerified Event Publishing Tests

        [Fact]
        public async Task UpdateOnboardingState_ShouldPublishAgreementsVerifiedEvent_WhenOnboardingStateIsAgreementVerified()
        {
            // Arrange
            var consumerCode = "test-consumer-123";
            var tenantCode = "test-tenant-123";
            var agreementStatus = "VERIFIED";

            var consumer = new ConsumerMockModel
            {
                ConsumerCode = consumerCode,
                TenantCode = tenantCode,
                OnBoardingState = OnboardingState.NOT_STARTED.ToString(),
                AgreementStatus = Constant.NotVerified,
                DeleteNbr = 0
            };

            var updateOnboardingStateDto = new UpdateOnboardingStateDto
            {
                ConsumerCode = consumerCode,
                TenantCode = tenantCode,
                OnboardingState = OnboardingState.AGREEMENT_VERIFIED,
                LanguageCode = "en-US",
                HtmlFileName = new Dictionary<string, string> { { "test-key", "test-file.html" } }
            };

            _consumerRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false))
                .ReturnsAsync(consumer);

            _uploadpdfService.Setup(x => x.UploadAgreementPDf(It.IsAny<UpdateOnboardingStateDto>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new Dictionary<string, string> { { "test-key", "test-file.pdf" } });

            _consumerRepo.Setup(x => x.UpdateAsync(It.IsAny<ConsumerModel>()))
                .ReturnsAsync(consumer);

            _eventService.Setup(x => x.CreateConsumerHistoryEvent(It.IsAny<List<ConsumerDto>>(), "test"))
                .ReturnsAsync(new BaseResponseDto());

            // Act
            var result = await _consumerService.UpdateOnboardingState(updateOnboardingStateDto);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.ErrorCode);
        }

        [Fact]
        public async Task UpdateOnboardingState_ShouldNotPublishAgreementsVerifiedEvent_WhenOnboardingStateIsNotAgreementVerified()
        {
            // Arrange
            var consumerCode = "test-consumer-123";
            var tenantCode = "test-tenant-123";

            var consumer = new ConsumerMockModel
            {
                ConsumerCode = consumerCode,
                TenantCode = tenantCode,
                OnBoardingState = OnboardingState.NOT_STARTED.ToString(),
                AgreementStatus = Constant.NotVerified,
                DeleteNbr = 0
            };

            var updateOnboardingStateDto = new UpdateOnboardingStateDto
            {
                ConsumerCode = consumerCode,
                TenantCode = tenantCode,
                OnboardingState = OnboardingState.VERIFIED,
                LanguageCode = "en-US"
            };

            _consumerRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false))
                .ReturnsAsync(consumer);

            _consumerRepo.Setup(x => x.UpdateAsync(It.IsAny<ConsumerModel>()))
                .ReturnsAsync(consumer);

            _eventService.Setup(x => x.CreateConsumerHistoryEvent(It.IsAny<List<ConsumerDto>>(), "test"))
                .ReturnsAsync(new BaseResponseDto());

            // Act
            var result = await _consumerService.UpdateOnboardingState(updateOnboardingStateDto);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.ErrorCode);
        }

        [Fact]
        public async Task UpdateOnboardingState_ShouldLogError_WhenEventPublishingFails()
        {
            // Arrange
            var consumerCode = "test-consumer-123";
            var tenantCode = "test-tenant-123";

            var consumer = new ConsumerMockModel
            {
                ConsumerCode = consumerCode,
                TenantCode = tenantCode,
                OnBoardingState = OnboardingState.NOT_STARTED.ToString(),
                AgreementStatus = Constant.NotVerified,
                DeleteNbr = 0
            };

            var updateOnboardingStateDto = new UpdateOnboardingStateDto
            {
                ConsumerCode = consumerCode,
                TenantCode = tenantCode,
                OnboardingState = OnboardingState.AGREEMENT_VERIFIED,
                LanguageCode = "en-US",
                HtmlFileName = new Dictionary<string, string> { { "test-key", "test-file.html" } }
            };

            _consumerRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false))
                .ReturnsAsync(consumer);

            _uploadpdfService.Setup(x => x.UploadAgreementPDf(It.IsAny<UpdateOnboardingStateDto>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new Dictionary<string, string> { { "test-key", "test-file.pdf" } });

            _consumerRepo.Setup(x => x.UpdateAsync(It.IsAny<ConsumerModel>()))
                .ReturnsAsync(consumer);

            _eventService.Setup(x => x.CreateConsumerHistoryEvent(It.IsAny<List<ConsumerDto>>(), "test"))
                .ReturnsAsync(new BaseResponseDto());

            // Act
            var result = await _consumerService.UpdateOnboardingState(updateOnboardingStateDto);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.ErrorCode);
        }

        [Fact]
        public async Task UpdateOnboardingState_ShouldHandleException_WhenEventPublishingThrows()
        {
            // Arrange
            var consumerCode = "test-consumer-123";
            var tenantCode = "test-tenant-123";

            var consumer = new ConsumerMockModel
            {
                ConsumerCode = consumerCode,
                TenantCode = tenantCode,
                OnBoardingState = OnboardingState.NOT_STARTED.ToString(),
                AgreementStatus = Constant.NotVerified,
                DeleteNbr = 0
            };

            var updateOnboardingStateDto = new UpdateOnboardingStateDto
            {
                ConsumerCode = consumerCode,
                TenantCode = tenantCode,
                OnboardingState = OnboardingState.AGREEMENT_VERIFIED,
                LanguageCode = "en-US",
                HtmlFileName = new Dictionary<string, string> { { "test-key", "test-file.html" } }
            };

            _consumerRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false))
                .ReturnsAsync(consumer);

            _uploadpdfService.Setup(x => x.UploadAgreementPDf(It.IsAny<UpdateOnboardingStateDto>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new Dictionary<string, string> { { "test-key", "test-file.pdf" } });

            _consumerRepo.Setup(x => x.UpdateAsync(It.IsAny<ConsumerModel>()))
                .ReturnsAsync(consumer);

            _eventService.Setup(x => x.CreateConsumerHistoryEvent(It.IsAny<List<ConsumerDto>>(), "test"))
                .ReturnsAsync(new BaseResponseDto());

            // Act
            var result = await _consumerService.UpdateOnboardingState(updateOnboardingStateDto);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.ErrorCode);

            // Verify exception logging
            _consumerServiceLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Exception occurred while publishing AgreementsVerified event")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task UpdateOnboardingState_ShouldCreateCorrectEventHeader_WhenPublishingAgreementsVerifiedEvent()
        {
            // Arrange
            var consumerCode = "test-consumer-123";
            var tenantCode = "test-tenant-123";

            var consumer = new ConsumerMockModel
            {
                ConsumerCode = consumerCode,
                TenantCode = tenantCode,
                OnBoardingState = OnboardingState.NOT_STARTED.ToString(),
                AgreementStatus = Constant.NotVerified,
                DeleteNbr = 0
            };

            var updateOnboardingStateDto = new UpdateOnboardingStateDto
            {
                ConsumerCode = consumerCode,
                TenantCode = tenantCode,
                OnboardingState = OnboardingState.AGREEMENT_VERIFIED,
                LanguageCode = "en-US",
                HtmlFileName = new Dictionary<string, string> { { "test-key", "test-file.html" } }
            };

            _consumerRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false))
                .ReturnsAsync(consumer);

            _uploadpdfService.Setup(x => x.UploadAgreementPDf(It.IsAny<UpdateOnboardingStateDto>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new Dictionary<string, string> { { "test-key", "test-file.pdf" } });

            _consumerRepo.Setup(x => x.UpdateAsync(It.IsAny<ConsumerModel>()))
                .ReturnsAsync(consumer);

            _eventService.Setup(x => x.CreateConsumerHistoryEvent(It.IsAny<List<ConsumerDto>>(), "test"))
                .ReturnsAsync(new BaseResponseDto());

            // Act
            var result = await _consumerService.UpdateOnboardingState(updateOnboardingStateDto);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.ErrorCode);
        }

        [Fact]
        public async Task UpdateOnboardingState_ShouldCreateCorrectEventData_WhenPublishingAgreementsVerifiedEvent()
        {
            // Arrange
            var consumerCode = "test-consumer-123";
            var tenantCode = "test-tenant-123";
            var expectedAgreementStatus = "VERIFIED";

            var consumer = new ConsumerMockModel
            {
                ConsumerCode = consumerCode,
                TenantCode = tenantCode,
                OnBoardingState = OnboardingState.NOT_STARTED.ToString(),
                AgreementStatus = expectedAgreementStatus,
                DeleteNbr = 0
            };

            var updateOnboardingStateDto = new UpdateOnboardingStateDto
            {
                ConsumerCode = consumerCode,
                TenantCode = tenantCode,
                OnboardingState = OnboardingState.AGREEMENT_VERIFIED,
                LanguageCode = "en-US",
                HtmlFileName = new Dictionary<string, string> { { "test-key", "test-file.html" } }
            };

            _consumerRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false))
                .ReturnsAsync(consumer);

            _uploadpdfService.Setup(x => x.UploadAgreementPDf(It.IsAny<UpdateOnboardingStateDto>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new Dictionary<string, string> { { "test-key", "test-file.pdf" } });

            _consumerRepo.Setup(x => x.UpdateAsync(It.IsAny<ConsumerModel>()))
                .ReturnsAsync(consumer);

            _eventService.Setup(x => x.CreateConsumerHistoryEvent(It.IsAny<List<ConsumerDto>>(), "test"))
                .ReturnsAsync(new BaseResponseDto());

            // Act
            var result = await _consumerService.UpdateOnboardingState(updateOnboardingStateDto);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.ErrorCode);
        }

        [Fact]
        public async Task UpdateOnboardingState_ShouldNotPublishEvent_WhenAgreementUploadFails()
        {
            // Arrange
            var consumerCode = "test-consumer-123";
            var tenantCode = "test-tenant-123";

            var consumer = new ConsumerMockModel
            {
                ConsumerCode = consumerCode,
                TenantCode = tenantCode,
                OnBoardingState = OnboardingState.NOT_STARTED.ToString(),
                AgreementStatus = Constant.NotVerified,
                DeleteNbr = 0
            };

            var updateOnboardingStateDto = new UpdateOnboardingStateDto
            {
                ConsumerCode = consumerCode,
                TenantCode = tenantCode,
                OnboardingState = OnboardingState.AGREEMENT_VERIFIED,
                LanguageCode = "en-US",
                HtmlFileName = new Dictionary<string, string> { { "test-key", "test-file.html" } }
            };

            _consumerRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false))
                .ReturnsAsync(consumer);

            _uploadpdfService.Setup(x => x.UploadAgreementPDf(It.IsAny<UpdateOnboardingStateDto>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new Dictionary<string, string>()); // Empty dictionary indicates failure

            // Act
            var result = await _consumerService.UpdateOnboardingState(updateOnboardingStateDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(500, result.ErrorCode);
            Assert.Equal("Agreement pdf upload failed", result.ErrorMessage);
        }

        [Fact]
        public async Task UpdateOnboardingState_ShouldNotPublishEvent_WhenConsumerNotFound()
        {
            // Arrange
            var consumerCode = "test-consumer-123";
            var tenantCode = "test-tenant-123";

            var updateOnboardingStateDto = new UpdateOnboardingStateDto
            {
                ConsumerCode = consumerCode,
                TenantCode = tenantCode,
                OnboardingState = OnboardingState.AGREEMENT_VERIFIED,
                LanguageCode = "en-US",
                HtmlFileName = new Dictionary<string, string> { { "test-key", "test-file.html" } }
            };

            _consumerRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false))
                .ReturnsAsync((ConsumerModel)null);

            // Act
            var result = await _consumerService.UpdateOnboardingState(updateOnboardingStateDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(500, result.ErrorCode);
            Assert.Equal("Consumer not found", result.ErrorMessage);
        }

        [Fact]
        public async Task UpdateOnboardingState_ShouldNotPublishEvent_WhenOnboardingStateIsAlreadyAgreementVerified()
        {
            // Arrange
            var consumerCode = "test-consumer-123";
            var tenantCode = "test-tenant-123";

            var consumer = new ConsumerMockModel
            {
                ConsumerCode = consumerCode,
                TenantCode = tenantCode,
                OnBoardingState = OnboardingState.AGREEMENT_VERIFIED.ToString(),
                AgreementStatus = Constant.NotVerified,
                DeleteNbr = 0
            };

            var updateOnboardingStateDto = new UpdateOnboardingStateDto
            {
                ConsumerCode = consumerCode,
                TenantCode = tenantCode,
                OnboardingState = OnboardingState.AGREEMENT_VERIFIED,
                LanguageCode = "en-US"
            };

            _consumerRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false))
                .ReturnsAsync(consumer);

            // Act
            var result = await _consumerService.UpdateOnboardingState(updateOnboardingStateDto);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.ErrorCode);
        }

        [Theory]
        [InlineData("VERIFIED")]
        [InlineData("NOT_VERIFIED")]
        [InlineData("DECLINED")]
        [InlineData("")]
        [InlineData(null)]
        public async Task UpdateOnboardingState_ShouldPublishEventWithCorrectAgreementStatus_WhenOnboardingStateIsAgreementVerified(string agreementStatus)
        {
            // Arrange
            var consumerCode = "test-consumer-123";
            var tenantCode = "test-tenant-123";

            var consumer = new ConsumerMockModel
            {
                ConsumerCode = consumerCode,
                TenantCode = tenantCode,
                OnBoardingState = OnboardingState.NOT_STARTED.ToString(),
                AgreementStatus = agreementStatus,
                DeleteNbr = 0
            };

            var updateOnboardingStateDto = new UpdateOnboardingStateDto
            {
                ConsumerCode = consumerCode,
                TenantCode = tenantCode,
                OnboardingState = OnboardingState.AGREEMENT_VERIFIED,
                LanguageCode = "en-US",
                HtmlFileName = new Dictionary<string, string> { { "test-key", "test-file.html" } }
            };

            _consumerRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false))
                .ReturnsAsync(consumer);

            _uploadpdfService.Setup(x => x.UploadAgreementPDf(It.IsAny<UpdateOnboardingStateDto>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new Dictionary<string, string> { { "test-key", "test-file.pdf" } });

            _consumerRepo.Setup(x => x.UpdateAsync(It.IsAny<ConsumerModel>()))
                .ReturnsAsync(consumer);

            _eventService.Setup(x => x.CreateConsumerHistoryEvent(It.IsAny<List<ConsumerDto>>(), "test"))
                .ReturnsAsync(new BaseResponseDto());

            // Act
            var result = await _consumerService.UpdateOnboardingState(updateOnboardingStateDto);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.ErrorCode);
        }

        #endregion

    }
}