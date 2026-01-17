using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SunnyRewards.Helios.User.Api.Controllers;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Models;
using SunnyRewards.Helios.User.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.User.Infrastructure.Services;
using SunnyRewards.Helios.User.Infrastructure.Services.Interfaces;
using System.Linq.Expressions;
using Xunit;

namespace SunnyRewards.Helios.User.UnitTest.Controllers
{
    public class PersonRoleControllerUnitTest
    {
        private readonly Mock<ILogger<PersonRoleController>> _personRoleControllerLogger;
        private readonly Mock<ILogger<PersonRoleService>> _personRoleServiceLogger;
        private readonly IMapper _mapper;
        private readonly Mock<IPersonRepo> _personRepo;
        private readonly Mock<IPersonRoleRepo> _personRoleRepo;
        private readonly Mock<IConsumerRepo> _consumerRepo;
        private readonly Mock<IRoleRepo> _roleRepo;
        private readonly IPersonRoleService _personRoleService;
        private readonly PersonRoleController _personRoleController;

        public PersonRoleControllerUnitTest()
        {
            _personRoleControllerLogger = new Mock<ILogger<PersonRoleController>>();
            _personRoleServiceLogger = new Mock<ILogger<PersonRoleService>>();
            _mapper = new Mapper(new MapperConfiguration(
                configure =>
                {
                    configure.AddMaps(typeof(Infrastructure.Mappings.MappingProfile.PersonMapping).Assembly.FullName);
                    configure.AddMaps(typeof(Infrastructure.Mappings.MappingProfile.PersonRoleMapping).Assembly.FullName);
                }));
            _personRepo = new Mock<IPersonRepo>();
            _personRoleRepo = new Mock<IPersonRoleRepo>();
            _consumerRepo = new Mock<IConsumerRepo>();
            _roleRepo = new Mock<IRoleRepo>();
            _personRoleService = new PersonRoleService(_personRoleServiceLogger.Object, _mapper, _personRoleRepo.Object, _personRepo.Object, _consumerRepo.Object, _roleRepo.Object);
            _personRoleController = new PersonRoleController(_personRoleControllerLogger.Object, _personRoleService);
        }
        [Fact]
        public async Task GetPersonRoles_ReturnsValidResponse_WhenPersonFound_EmailAndPersonCodeGiven()
        {
            // Arrange
            var request = new GetPersonRolesRequestDto { Email = "test-email", PersonCode = "Test" };
            var expectedPersonDto = new PersonDto { PersonId = 123 };

            _personRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<PersonModel, bool>>>(), false))
                           .ReturnsAsync(new PersonModel { PersonId = 123 });
            Mock<IMapper> mapper = new Mock<IMapper>();
            mapper.Setup(mapper => mapper.Map<PersonDto>(It.IsAny<PersonModel>()))
                       .Returns(expectedPersonDto);

            _personRoleRepo.Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<PersonRoleModel, bool>>>(), false))
                           .ReturnsAsync(new List<PersonRoleModel>
                           {
                               new PersonRoleModel { PersonId = 123, PersonRoleId = 123, DeleteNbr = 0 },
                               new PersonRoleModel { PersonId = 123, PersonRoleId = 456, DeleteNbr = 0 }
                           });

            // Act
            var result = await _personRoleController.GetPersonRoles(request);

            // Assert
            Assert.NotNull(result);
            var actionResult = Assert.IsType<ActionResult<GetPersonRolesResponseDto>>(result);
            var objectResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            Assert.Equal(StatusCodes.Status200OK, objectResult.StatusCode);

        }
        [Fact]
        public async Task GetPersonRoles_ReturnsValidResponse_WhenPersonFound_Email_Provided()
        {
            // Arrange
            var request = new GetPersonRolesRequestDto { Email = "test-email", PersonCode = "" };
            var expectedPersonDto = new PersonDto { PersonId = 123 };

            _personRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<PersonModel, bool>>>(), false))
                           .ReturnsAsync(new PersonModel { PersonId = 123 });
            Mock<IMapper> mapper = new Mock<IMapper>();
            mapper.Setup(mapper => mapper.Map<PersonDto>(It.IsAny<PersonModel>()))
                       .Returns(expectedPersonDto);

            _personRoleRepo.Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<PersonRoleModel, bool>>>(), false))
                           .ReturnsAsync(new List<PersonRoleModel>
                           {
                               new PersonRoleModel { PersonId = 123, PersonRoleId = 123, DeleteNbr = 0 },
                               new PersonRoleModel { PersonId = 123, PersonRoleId = 456, DeleteNbr = 0 }
                           });

            // Act
            var result = await _personRoleController.GetPersonRoles(request);

            // Assert
            Assert.NotNull(result);
            var actionResult = Assert.IsType<ActionResult<GetPersonRolesResponseDto>>(result);
            var objectResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            Assert.Equal(StatusCodes.Status200OK, objectResult.StatusCode);

        }
        [Fact]
        public async Task GetPersonRoles_ReturnsValidResponse_WhenPersonFound_PersonCode_Provided()
        {
            // Arrange
            var request = new GetPersonRolesRequestDto { Email = "", PersonCode = "test-personCode" };
            var expectedPersonDto = new PersonDto { PersonId = 123 };

            _personRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<PersonModel, bool>>>(), false))
                           .ReturnsAsync(new PersonModel { PersonId = 123 });
            Mock<IMapper> mapper = new Mock<IMapper>();
            mapper.Setup(mapper => mapper.Map<PersonDto>(It.IsAny<PersonModel>()))
                       .Returns(expectedPersonDto);

            _personRoleRepo.Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<PersonRoleModel, bool>>>(), false))
                           .ReturnsAsync(new List<PersonRoleModel>
                           {
                               new PersonRoleModel { PersonId = 123, PersonRoleId = 123, DeleteNbr = 0 },
                               new PersonRoleModel { PersonId = 123, PersonRoleId = 456, DeleteNbr = 0 }
                           });

            // Act
            var result = await _personRoleController.GetPersonRoles(request);

            // Assert
            Assert.NotNull(result);
            var actionResult = Assert.IsType<ActionResult<GetPersonRolesResponseDto>>(result);
            var objectResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            Assert.Equal(StatusCodes.Status200OK, objectResult.StatusCode);

        }
        [Fact]
        public async Task GetPersonRoles_ReturnsValidResponse_WhenPerson_EmailAndPersonCode_Provided_PersonRoles_NotFound()
        {
            // Arrange
            var request = new GetPersonRolesRequestDto { Email = "test-email", PersonCode = "test-personCode" };
            var expectedPersonDto = new PersonDto { PersonId = 123 };

            _personRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<PersonModel, bool>>>(), false))
                           .ReturnsAsync(new PersonModel { PersonId = 123 });
            Mock<IMapper> mapper = new Mock<IMapper>();
            mapper.Setup(mapper => mapper.Map<PersonDto>(It.IsAny<PersonModel>()))
                       .Returns(expectedPersonDto);

            _personRoleRepo.Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<PersonRoleModel, bool>>>(), false));

            // Act
            var result = await _personRoleController.GetPersonRoles(request);

            // Assert
            Assert.NotNull(result);
            var actionResult = Assert.IsType<ActionResult<GetPersonRolesResponseDto>>(result);
            var objectResult = Assert.IsType<ObjectResult>(actionResult.Result);
            Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);

        }
        [Fact]
        public async Task GetPersonRoles_ReturnsResponse_WhenPerson_NotFound()
        {
            // Arrange
            var request = new GetPersonRolesRequestDto { Email = "test-email", PersonCode = "test-personCode" };

            _personRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<PersonModel, bool>>>(), false));

            // Act
            var result = await _personRoleController.GetPersonRoles(request);

            // Assert
            Assert.NotNull(result);
            var actionResult = Assert.IsType<ActionResult<GetPersonRolesResponseDto>>(result);
            var objectResult = Assert.IsType<ObjectResult>(actionResult.Result);
            Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);

        }
        [Fact]
        public async Task GetPersonRoles_ReturnsResponse_When_requestEmpty_BadRequest()
        {
            // Arrange
            var request = new GetPersonRolesRequestDto { Email = "", PersonCode = "" };

            _personRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<PersonModel, bool>>>(), false));

            // Act
            var result = await _personRoleController.GetPersonRoles(request);

            // Assert
            Assert.NotNull(result);
            var actionResult = Assert.IsType<ActionResult<GetPersonRolesResponseDto>>(result);
            var objectResult = Assert.IsType<ObjectResult>(actionResult.Result);
            Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);

        }
        [Fact]
        public async Task GetPersonRoles_ThrowsExceptionWhenPersonRolesGetException()
        {
            // Arrange
            var request = new GetPersonRolesRequestDto { Email = "test-email", PersonCode = "test-personCode" };
            var expectedPersonDto = new PersonDto { PersonId = 123 };

            _personRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<PersonModel, bool>>>(), false))
                           .ReturnsAsync(new PersonModel { PersonId = 123 });
            Mock<IMapper> mapper = new Mock<IMapper>();
            mapper.Setup(mapper => mapper.Map<PersonDto>(It.IsAny<PersonModel>()))
                       .Returns(expectedPersonDto);

            _personRoleRepo.Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<PersonRoleModel, bool>>>(), false))
                           .ThrowsAsync(new Exception("test-exception"));

            // Act
            var result = await _personRoleController.GetPersonRoles(request);

            // Assert
            Assert.NotNull(result);
            var actionResult = Assert.IsType<ActionResult<GetPersonRolesResponseDto>>(result);
            var objectResult = Assert.IsType<ObjectResult>(actionResult.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);

        }

        [Fact]
        public async Task GetAccessControlList_ReturnsNotFound_WhenConsumerCodeIsNull()
        {
            // Act
            var result = await _personRoleController.GetAccessControlList(null);

            // Assert
            var actionResult = Assert.IsType<ActionResult<AccessControlListResponseDTO>>(result);
            var objectResult = Assert.IsType<ObjectResult>(actionResult.Result);
            Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);
        }

        [Fact]
        public async Task GetAccessControlList_ReturnsNotFound_WhenConsumerNotFound()
        {
            // Arrange
            _consumerRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false))
                       .ReturnsAsync((ConsumerModel)null);

            // Act
            var result = await _personRoleController.GetAccessControlList("invalid-code");

            // Assert
            var actionResult = Assert.IsType<ActionResult<AccessControlListResponseDTO>>(result);
            var objectResult = Assert.IsType<ObjectResult>(actionResult.Result);
            Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);
        }

        [Fact]
        public async Task GetAccessControlList_ReturnsNotFound_WhenPersonNotFound()
        {
            // Arrange
            _consumerRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false))
                       .ReturnsAsync(new ConsumerModel { PersonId = 123 });
            _personRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<PersonModel, bool>>>(), false))
                       .ReturnsAsync((PersonModel)null);

            // Act
            var result = await _personRoleController.GetAccessControlList("test-code");

            // Assert
            var actionResult = Assert.IsType<ActionResult<AccessControlListResponseDTO>>(result);
            var objectResult = Assert.IsType<ObjectResult>(actionResult.Result);
            Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);
        }

        [Fact]
        public async Task GetAccessControlList_ReturnsOk_WhenRolesAreFound()
        {
            // Arrange
            var request = new GetPersonRolesRequestDto { Email = "", PersonCode = "test-personCode" };
            var expectedPersonDto = new PersonDto { PersonId = 123 };

            _personRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<PersonModel, bool>>>(), false))
                       .ReturnsAsync(new PersonModel { PersonId = 123 });
            Mock<IMapper> mapper = new Mock<IMapper>();
            mapper.Setup(mapper => mapper.Map<PersonDto>(It.IsAny<PersonModel>()))
                   .Returns(expectedPersonDto);

            _personRoleRepo.Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<PersonRoleModel, bool>>>(), false))
                           .ReturnsAsync(new List<PersonRoleModel>
                           {
                           new PersonRoleModel { PersonId = 123, PersonRoleId = 123, DeleteNbr = 0 },
                           new PersonRoleModel { PersonId = 123, PersonRoleId = 456, DeleteNbr = 0 }
                           });

            // Act
            var result = await _personRoleController.GetPersonRoles(request);

            // Assert
            Assert.NotNull(result);
            var actionResult = Assert.IsType<ActionResult<GetPersonRolesResponseDto>>(result);
            var objectResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            Assert.Equal(StatusCodes.Status200OK, objectResult.StatusCode);
        }

        [Fact]
        public async Task GetAccessControlList_ReturnsWarning_WhenNoRolesFound()
        {
            // Arrange
            _consumerRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false))
                       .ReturnsAsync(new ConsumerModel { PersonId = 123 });
            _personRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<PersonModel, bool>>>(), false))
                       .ReturnsAsync(new PersonModel { PersonId = 123 });
            _personRoleRepo.Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<PersonRoleModel, bool>>>(), false))
                           .ReturnsAsync(new List<PersonRoleModel>());

            // Act
            var result = await _personRoleController.GetAccessControlList("test-code");

            // Assert
            var actionResult = Assert.IsType<ActionResult<AccessControlListResponseDTO>>(result);
            var objectResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var response = Assert.IsType<AccessControlListResponseDTO>(objectResult.Value);
            Assert.Null(response.CustomerAdminCustomerCodes);
        }

        [Fact]
        public async Task GetAccessControlList_ThrowsException_WhenNoConsumerFound()
        {
            // Arrange
            _consumerRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false))
                         .ThrowsAsync(new Exception("Test Exception"));

            // Act
            var result = await _personRoleController.GetAccessControlList("test-code");

            // Assert
            var actionResult = Assert.IsType<ActionResult<AccessControlListResponseDTO>>(result);
            var objectResult = Assert.IsType<ObjectResult>(actionResult.Result);
            var response = Assert.IsType<AccessControlListResponseDTO>(objectResult.Value);
            Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
            Assert.False(response.IsSuperAdmin);
            Assert.False(response.IsSubscriber);
            Assert.False(response.IsReportUser);
            Assert.Null(response.CustomerAdminCustomerCodes);
            Assert.Null(response.SponsorAdminSponsorCodes);
            Assert.Null(response.TenantAdminTenantCodes);
        }

        [Fact]
        public async Task GetAccessControlList_ThrowsException_WhenNoPersonFound()
        {

            // Arrange
            var consumerModel = new ConsumerModel { PersonId = 123 };
            var personModel = new PersonModel { PersonId = 123 };
            var personRoles = new List<PersonRoleModel>
            {
                new PersonRoleModel
                {
                    PersonId = 123,
                    RoleId = 1, // Super Admin role ID
                    CustomerCode = "All",
                    SponsorCode = "All",
                    TenantCode = "All",
                    DeleteNbr = 0
                }
            };

            _consumerRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false))
                         .ReturnsAsync(consumerModel);
            _personRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<PersonModel, bool>>>(), false))
                       .ThrowsAsync(new Exception("Test Exception"));
            
            // Act
            var result = await _personRoleController.GetAccessControlList("test-code");

            // Assert
            var actionResult = Assert.IsType<ActionResult<AccessControlListResponseDTO>>(result);
            var objectResult = Assert.IsType<ObjectResult>(actionResult.Result);
            var response = Assert.IsType<AccessControlListResponseDTO>(objectResult.Value);
            Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
            Assert.False(response.IsSuperAdmin);
            Assert.False(response.IsSubscriber);
            Assert.False(response.IsReportUser);
            Assert.Null(response.CustomerAdminCustomerCodes);
            Assert.Null(response.SponsorAdminSponsorCodes);
            Assert.Null(response.TenantAdminTenantCodes);
        }

        [Fact]
        public async Task GetAccessControlList_ReturnsIsSuperAdmin_True_WhenSuperAdminRoleExists()
        {
            // Arrange
            var consumerModel = new ConsumerModel { PersonId = 123 };
            var personModel = new PersonModel { PersonId = 123 };
            var personRoles = new List<PersonRoleModel>
            {
                new PersonRoleModel
                {
                    PersonId = 123,
                    RoleId = 1, // Super Admin role ID
                    CustomerCode = "All",
                    SponsorCode = "All",
                    TenantCode = "All",
                    DeleteNbr = 0
                }
            };

            _consumerRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false))
                         .ReturnsAsync(consumerModel);
            _personRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<PersonModel, bool>>>(), false))
                       .ReturnsAsync(personModel);
            _personRoleRepo.Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<PersonRoleModel, bool>>>(), false))
                           .ReturnsAsync(personRoles);
            _roleRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<RoleModel, bool>>>(), false))
                     .ReturnsAsync(new RoleModel { RoleId = 1, RoleCode = "superadmin" });

            // Act
            var result = await _personRoleController.GetAccessControlList("test-code");

            // Assert
            var actionResult = Assert.IsType<ActionResult<AccessControlListResponseDTO>>(result);
            var okObjectResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var response = Assert.IsType<AccessControlListResponseDTO>(okObjectResult.Value);

            Assert.True(response.IsSuperAdmin);
        }

        [Fact]
        public async Task GetAccessControlList_ReturnsIsSubscriber_True_WhenSubscriberRoleExists()
        {
            // Arrange
            var consumerModel = new ConsumerModel { PersonId = 123 };
            var personModel = new PersonModel { PersonId = 123 };
            var personRoles = new List<PersonRoleModel>
            {
                new PersonRoleModel
                {
                    PersonId = 123,
                    RoleId = 2, // Subscriber role ID
                    CustomerCode = "Customer123",
                    SponsorCode = "Sponsor123",
                    TenantCode = "Tenant123",
                    DeleteNbr = 0
                }
            };

            _consumerRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false))
                         .ReturnsAsync(consumerModel);
            _personRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<PersonModel, bool>>>(), false))
                       .ReturnsAsync(personModel);
            _personRoleRepo.Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<PersonRoleModel, bool>>>(), false))
                           .ReturnsAsync(personRoles);
            _roleRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<RoleModel, bool>>>(), false))
                     .ReturnsAsync(new RoleModel { RoleId = 2, RoleCode = "subscriber" });

            // Act
            var result = await _personRoleController.GetAccessControlList("test-code");

            // Assert
            var actionResult = Assert.IsType<ActionResult<AccessControlListResponseDTO>>(result);
            var okObjectResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var response = Assert.IsType<AccessControlListResponseDTO>(okObjectResult.Value);

            Assert.True(response.IsSubscriber);
        }

        [Fact]
        public async Task GetAccessControlList_ReturnsIsReportUser_True_WhenReportUserRoleExists()
        {
            // Arrange
            var consumerModel = new ConsumerModel { PersonId = 123 };
            var personModel = new PersonModel { PersonId = 123 };
            var personRoles = new List<PersonRoleModel>
            {
                new PersonRoleModel
                {
                    PersonId = 123,
                    RoleId = 3, // Report User role ID
                    CustomerCode = "Customer123",
                    SponsorCode = "Sponsor123",
                    TenantCode = "Tenant123",
                    DeleteNbr = 0
                }
            };

            _consumerRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false))
                         .ReturnsAsync(consumerModel);
            _personRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<PersonModel, bool>>>(), false))
                       .ReturnsAsync(personModel);
            _personRoleRepo.Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<PersonRoleModel, bool>>>(), false))
                           .ReturnsAsync(personRoles);
            _roleRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<RoleModel, bool>>>(), false))
                     .ReturnsAsync(new RoleModel { RoleId = 3, RoleCode = "reportuser" });

            // Act
            var result = await _personRoleController.GetAccessControlList("test-code");

            // Assert
            var actionResult = Assert.IsType<ActionResult<AccessControlListResponseDTO>>(result);
            var okObjectResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var response = Assert.IsType<AccessControlListResponseDTO>(okObjectResult.Value);

            Assert.True(response.IsReportUser);
        }

        [Fact]
        public async Task GetAccessControlList_ReturnsCustomerAdminCustomerCodes_WhenCustomerAdminRoleExists()
        {
            // Arrange
            var consumerModel = new ConsumerModel { PersonId = 123 };
            var personModel = new PersonModel { PersonId = 123 };
            var personRoles = new List<PersonRoleModel>
            {
                new PersonRoleModel
                {
                    PersonId = 123,
                    RoleId = 4, // Customer Admin role ID
                    CustomerCode = "CustomerAdminCode",
                    SponsorCode = "All",
                    TenantCode = "All",
                    DeleteNbr = 0
                }
            };

            _consumerRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false))
                         .ReturnsAsync(consumerModel);
            _personRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<PersonModel, bool>>>(), false))
                       .ReturnsAsync(personModel);
            _personRoleRepo.Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<PersonRoleModel, bool>>>(), false))
                           .ReturnsAsync(personRoles);
            _roleRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<RoleModel, bool>>>(), false))
                     .ReturnsAsync(new RoleModel { RoleId = 4, RoleCode = "customeradmin" });

            // Act
            var result = await _personRoleController.GetAccessControlList("test-code");

            // Assert
            var actionResult = Assert.IsType<ActionResult<AccessControlListResponseDTO>>(result);
            var okObjectResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var response = Assert.IsType<AccessControlListResponseDTO>(okObjectResult.Value);

            Assert.Contains("CustomerAdminCode", response.CustomerAdminCustomerCodes);
        }

        [Fact]
        public async Task GetAccessControlList_ReturnsSponsorAdminSponsorCodes_WhenSponsorAdminRoleExists()
        {
            // Arrange
            var consumerModel = new ConsumerModel { PersonId = 123 };
            var personModel = new PersonModel { PersonId = 123 };
            var personRoles = new List<PersonRoleModel>
            {
                new PersonRoleModel
                {
                    PersonId = 123,
                    RoleId = 5, // Sponsor Admin role ID
                    CustomerCode = "Customer123",
                    SponsorCode = "SponsorAdminCode",
                    TenantCode = "All",
                    DeleteNbr = 0
                }
            };

            _consumerRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false))
                         .ReturnsAsync(consumerModel);
            _personRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<PersonModel, bool>>>(), false))
                       .ReturnsAsync(personModel);
            _personRoleRepo.Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<PersonRoleModel, bool>>>(), false))
                           .ReturnsAsync(personRoles);
            _roleRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<RoleModel, bool>>>(), false))
                     .ReturnsAsync(new RoleModel { RoleId = 5, RoleCode = "sponsoradmin" });

            // Act
            var result = await _personRoleController.GetAccessControlList("test-code");

            // Assert
            var actionResult = Assert.IsType<ActionResult<AccessControlListResponseDTO>>(result);
            var okObjectResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var response = Assert.IsType<AccessControlListResponseDTO>(okObjectResult.Value);

            Assert.Contains("SponsorAdminCode", response.SponsorAdminSponsorCodes);
        }

        [Fact]
        public async Task GetAccessControlList_ReturnsTenantAdminTenantCodes_WhenTenantAdminRoleExists()
        {
            // Arrange
            var consumerModel = new ConsumerModel { PersonId = 123 };
            var personModel = new PersonModel { PersonId = 123 };
            var personRoles = new List<PersonRoleModel>
            {
                new PersonRoleModel
                {
                    PersonId = 123,
                    RoleId = 6, // Tenant Admin role ID
                    CustomerCode = "Customer123",
                    SponsorCode = "Sponsor123",
                    TenantCode = "TenantAdminCode",
                    DeleteNbr = 0
                }
            };

            _consumerRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false))
                         .ReturnsAsync(consumerModel);
            _personRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<PersonModel, bool>>>(), false))
                       .ReturnsAsync(personModel);
            _personRoleRepo.Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<PersonRoleModel, bool>>>(), false))
                           .ReturnsAsync(personRoles);
            _roleRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<RoleModel, bool>>>(), false))
                     .ReturnsAsync(new RoleModel { RoleId = 6, RoleCode = "tenantadmin" });

            // Act
            var result = await _personRoleController.GetAccessControlList("test-code");

            // Assert
            var actionResult = Assert.IsType<ActionResult<AccessControlListResponseDTO>>(result);
            var okObjectResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var response = Assert.IsType<AccessControlListResponseDTO>(okObjectResult.Value);

            Assert.Contains("TenantAdminCode", response.TenantAdminTenantCodes);
        }
    }
}
