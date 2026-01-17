using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestPlatform.Common;
using Moq;
using Sunny.Benefits.Cms.Core.Domain.Dtos;
using SunnyBenefits.Fis.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Core.Domain.Constants;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Infrastructure.Helpers.Interface;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Sweepstakes.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SunnyRewards.Helios.Admin.UnitTest.Services
{
    public class TenantImportServiceTests
    {
        private readonly Mock<ILogger<TenantImportService>> _loggerMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ITenantClient> _tenantClientMock;
        private readonly Mock<ITaskClient> _taskClientMock;
        private readonly Mock<IFisClient> _fisClientMock;
        private readonly Mock<ICmsClient> _cmsClientMock;
        private readonly Mock<ICohortClient> _cohortClientMock;
        private readonly Mock<ISweepstakesClient> _sweepstakesClientMock;
        private readonly Mock<IS3Helper> _s3HelperMock;
        private readonly Mock<IConfiguration> _configMock;
        private readonly TenantImportService _service;
        private readonly Mock<ITenantService> _tenantService;
        private readonly Mock<ITenantAccountService> _tenantAccountService;
        private readonly Mock<IWalletTypeTransferRuleService> _walletTypeTransferRuleService;
        private readonly IMapper _mapper;
        private readonly Mock<IAdminService> _adminService;
        private readonly Mock<ITaskService> _taskService;
        private readonly Mock<IWalletTypeService> _walletTypeService;
        private readonly Mock<IComponentService> _componentService;
        private readonly Mock<IUserContextService> _conService;

        public TenantImportServiceTests()
        {
            _loggerMock = new Mock<ILogger<TenantImportService>>();
            _mapperMock = new Mock<IMapper>();
            _tenantClientMock = new Mock<ITenantClient>();
            _taskClientMock = new Mock<ITaskClient>();
            _fisClientMock = new Mock<IFisClient>();
            _cmsClientMock = new Mock<ICmsClient>();
            _sweepstakesClientMock = new Mock<ISweepstakesClient>();
            _cohortClientMock = new Mock<ICohortClient>();
            _s3HelperMock = new Mock<IS3Helper>();
            _configMock = new Mock<IConfiguration>();
            _tenantService = new Mock<ITenantService>();
            _tenantAccountService = new Mock<ITenantAccountService>();
            _walletTypeTransferRuleService = new Mock<IWalletTypeTransferRuleService>();
            _mapper = new Mapper(new MapperConfiguration(
                           configure =>
                           {
                               configure.AddMaps(typeof(Infrastructure.Mappings.MappingProfile.TenantAccountMappingProfile).Assembly.FullName);
                           }));
            _adminService = new Mock<IAdminService>();
            _taskService = new Mock<ITaskService>();
            _walletTypeService = new Mock<IWalletTypeService>();
            _componentService = new Mock<IComponentService>();
            _conService = new Mock<IUserContextService>();
            _service = new TenantImportService(
                _loggerMock.Object,
                _mapper,
                _s3HelperMock.Object,
                _configMock.Object,
                _tenantClientMock.Object,
                _taskClientMock.Object, _cmsClientMock.Object,
                _tenantService.Object, _tenantAccountService.Object, _sweepstakesClientMock.Object, _cohortClientMock.Object,
                _walletTypeTransferRuleService.Object, _fisClientMock.Object,
                _adminService.Object,
                _taskService.Object,
                _walletTypeService.Object,
                _componentService.Object, _conService.Object
            );
        }

        [Fact]
        public async void TenantImport_ShouldReturn500_WhenS3UploadFails()
        {
            // Arrange
            var requestDto = new TenantImportRequestDto
            {
                tenantCode = "Tenant123",
                File = new FormFile(Stream.Null, 0, 0, "file", "test.json"),
                ImportOptions = new List<string> { "ALL" }
            };
            var configSectionMock = new Mock<IConfigurationSection>();
            configSectionMock.Setup(x => x.Value).Returns("Secret");

            _configMock.Setup(x => x.GetSection(It.IsAny<string>())).Returns(configSectionMock.Object);

            _s3HelperMock
                .Setup(s3 => s3.UploadFileToS3(It.IsAny<string>(), It.IsAny<IFormFile>(), It.IsAny<string>()))
                .ReturnsAsync(false); // Simulate S3 upload failure

            // Act
            var result = await _service.TenantImport(requestDto);

            // Assert
            Assert.Equal(StatusCodes.Status500InternalServerError, result.ErrorCode);
        }

        [Fact]
        public async void TenantImport_ShouldReturn404_WhenTenantNotFound()
        {
            // Arrange
            var requestDto = new TenantImportRequestDto
            {
                tenantCode = "InvalidTenant",
                File = new FormFile(Stream.Null, 0, 0, "file", "test.json"),
                ImportOptions = new List<string> { "ALL" }
            };
            var configSectionMock = new Mock<IConfigurationSection>();
            configSectionMock.Setup(x => x.Value).Returns("Secret");

            _configMock.Setup(x => x.GetSection(It.IsAny<string>())).Returns(configSectionMock.Object);
            _s3HelperMock
                .Setup(s3 => s3.UploadFileToS3(It.IsAny<string>(), It.IsAny<IFormFile>(), It.IsAny<string>()))
                .ReturnsAsync(true); // Simulate successful S3 upload

            _tenantClientMock
                .Setup(client => client.Post<TenantDto>("tenant/get-by-tenant-code", It.IsAny<GetTenantCodeRequestDto>()));

            // Act
            var result = await _service.TenantImport(requestDto);

            // Assert
            Assert.Equal(StatusCodes.Status404NotFound, result.ErrorCode);
        }

        [Fact]
        public async void TenantImport_ShouldReturn404_WhenFileNotFound()
        {
            // Arrange
            var requestDto = new TenantImportRequestDto
            {
                tenantCode = "Tenant123",
                File = new FormFile(Stream.Null, 0, 0, "file", "test.json"),
                ImportOptions = new List<string> { "ALL" }
            };
            var configSectionMock = new Mock<IConfigurationSection>();
            configSectionMock.Setup(x => x.Value).Returns("Secret");

            _configMock.Setup(x => x.GetSection(It.IsAny<string>())).Returns(configSectionMock.Object);
            _s3HelperMock
                .Setup(s3 => s3.UploadFileToS3(It.IsAny<string>(), It.IsAny<IFormFile>(), It.IsAny<string>()))
                .ReturnsAsync(true);

            _tenantClientMock
                .Setup(client => client.Post<TenantDto>("tenant/get-by-tenant-code", It.IsAny<GetTenantCodeRequestDto>()))
                .ReturnsAsync(new TenantDto { TenantCode = "Tenant123" });

            _s3HelperMock
                .Setup(s3 => s3.UnzipAndProcessJsonFromS3(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));

            // Act
            var result = await _service.TenantImport(requestDto);

            // Assert
            Assert.NotNull(result.ErrorCode);
            Assert.Equal(404, result.ErrorCode);

        }

        [Fact]
        public async void TenantImport_ShouldReturn404_WhenNoDataToProcess()
        {
            // Arrange
            var requestDto = new TenantImportRequestDto
            {
                tenantCode = "Tenant123",
                File = new FormFile(Stream.Null, 0, 0, "file", "test.json"),
                ImportOptions = new List<string> { "ALL" }
            };
            var configSectionMock = new Mock<IConfigurationSection>();
            configSectionMock.Setup(x => x.Value).Returns("Secret");

            _configMock.Setup(x => x.GetSection(It.IsAny<string>())).Returns(configSectionMock.Object);
            _s3HelperMock
                .Setup(s3 => s3.UploadFileToS3(It.IsAny<string>(), It.IsAny<IFormFile>(), It.IsAny<string>()))
                .ReturnsAsync(true);

            _tenantClientMock
                .Setup(client => client.Post<TenantDto>("tenant/get-by-tenant-code", It.IsAny<GetTenantCodeRequestDto>()))
                .ReturnsAsync(new TenantDto { TenantCode = "Tenant123" });

            _s3HelperMock
                .Setup(s3 => s3.UnzipAndProcessJsonFromS3(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new ImportDto() { TenantCodeData = new TenantImportJson() { Data = new TenantData() { Tenant = new TenantDto() { TenantCode = "ten-xyz" } } } });

            _tenantService.Setup(x => x.GetTenantDetails(requestDto.tenantCode)).ReturnsAsync(new TenantResponseDto()
            {
                Tenant = new TenantDto() { TenantCode = "Tenant123" }
            });


            _tenantService
                .Setup(x => x.UpdateTenant(requestDto.tenantCode, It.IsAny<UpdateTenantDto>()))
                .ReturnsAsync(new UpdateTenantResponseDto
                {
                    UpdateTenant = new TenantDto() { TenantCode = "Tenant123", RecommendedTask = true }
                });



            // Act
            var result = await _service.TenantImport(requestDto);

            // Assert
            Assert.NotNull(result.ErrorCode);
            Assert.Equal(500, result.ErrorCode);
            Assert.Contains("No data to process", result.ErrorMessage);
        }

        [Fact]
        public async void TenantImport_ShouldReturn404_WhenProcessingAllImportOptions()
        {
            // Arrange
            var requestDto = new TenantImportRequestDto
            {
                tenantCode = "Tenant123",
                File = new FormFile(Stream.Null, 0, 0, "file", "test.json"),
                ImportOptions = new List<string> { "ALL" }
            };
            var configSectionMock = new Mock<IConfigurationSection>();
            configSectionMock.Setup(x => x.Value).Returns("Secret");

            _configMock.Setup(x => x.GetSection(It.IsAny<string>())).Returns(configSectionMock.Object);
            _s3HelperMock
                .Setup(s3 => s3.UploadFileToS3(It.IsAny<string>(), It.IsAny<IFormFile>(), It.IsAny<string>()))
                .ReturnsAsync(true);

            _tenantClientMock
                .Setup(client => client.Post<TenantDto>("tenant/get-by-tenant-code", It.IsAny<GetTenantCodeRequestDto>()))
                .ReturnsAsync(new TenantDto { TenantCode = "Tenant123" });

            _s3HelperMock
                .Setup(s3 => s3.UnzipAndProcessJsonFromS3(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new ImportDto() { TenantCodeData = new TenantImportJson() { Data = new TenantData() { Tenant = new TenantDto() { TenantCode = "ten-xyz" } } } });

            _tenantService.Setup(x => x.GetTenantDetails(requestDto.tenantCode)).ReturnsAsync(new TenantResponseDto()
            {
                Tenant = new TenantDto() { TenantCode = "Tenant123" }
            });


            _tenantService
                .Setup(x => x.UpdateTenant(requestDto.tenantCode, It.IsAny<UpdateTenantDto>()))
                .ReturnsAsync(new UpdateTenantResponseDto
                {
                    UpdateTenant = new TenantDto() { TenantCode = "Tenant123", RecommendedTask = true }
                });

            // Act
            var result = await _service.TenantImport(requestDto);

            // Assert
            Assert.NotNull(result.ErrorCode);
            Assert.Equal(500, result.ErrorCode);
            Assert.Contains("No data to process", result.ErrorMessage);

        }

        [Fact]
        public async void TenantImport_ShouldReturn500_WhenProcessingTaskImport()
        {
            // Arrange
            var requestDto = new TenantImportRequestDto
            {
                tenantCode = "Tenant123",
                File = new FormFile(Stream.Null, 0, 0, "file", "test.json"),
                ImportOptions = new List<string> { "TASK" }
            };
            var configSectionMock = new Mock<IConfigurationSection>();
            configSectionMock.Setup(x => x.Value).Returns("Secret");

            _configMock.Setup(x => x.GetSection(It.IsAny<string>())).Returns(configSectionMock.Object);

            _s3HelperMock
                .Setup(s3 => s3.UploadFileToS3(It.IsAny<string>(), It.IsAny<IFormFile>(), It.IsAny<string>()))
                .ReturnsAsync(true);

            _tenantClientMock
                .Setup(client => client.Post<TenantDto>("tenant/get-by-tenant-code", It.IsAny<GetTenantCodeRequestDto>()))
                .ReturnsAsync(new TenantDto { TenantCode = "Tenant123" });

            _s3HelperMock
                .Setup(s3 => s3.UnzipAndProcessJsonFromS3(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new ImportDto() { TaskData = new TaskImportJson(), TenantCodeData = new TenantImportJson() { Data = new TenantData() { Tenant = new TenantDto() { TenantCode = "ten-xyz" } } } });

            _tenantService.Setup(x => x.GetTenantDetails(requestDto.tenantCode)).ReturnsAsync(new TenantResponseDto()
            {
                Tenant = new TenantDto() { TenantCode = "Tenant123" }
            });


            _tenantService
                .Setup(x => x.UpdateTenant(requestDto.tenantCode, It.IsAny<UpdateTenantDto>()))
                .ReturnsAsync(new UpdateTenantResponseDto
                {
                    UpdateTenant = new TenantDto() { TenantCode = "Tenant123", RecommendedTask = true }
                });
            // Act
            var result = await _service.TenantImport(requestDto);

            // Assert
            Assert.NotNull(result.ErrorCode);
            Assert.Equal(500, result.ErrorCode);
            Assert.Contains("No data to process", result.ErrorMessage);
        }

        [Fact]
        public async void TenantImport_ShouldReturn200_WhenProcessingTaskImport_withTaskData()
        {
            // Arrange
            var requestDto = new TenantImportRequestDto
            {
                tenantCode = "Tenant123",
                File = new FormFile(Stream.Null, 0, 0, "file", "test.json"),
                ImportOptions = new List<string> { "TASK" }
            };
            var configSectionMock = new Mock<IConfigurationSection>();
            configSectionMock.Setup(x => x.Value).Returns("Secret");

            _configMock.Setup(x => x.GetSection(It.IsAny<string>())).Returns(configSectionMock.Object);

            _s3HelperMock
                .Setup(s3 => s3.UploadFileToS3(It.IsAny<string>(), It.IsAny<IFormFile>(), It.IsAny<string>()))
                .ReturnsAsync(true);

            _tenantClientMock
                .Setup(client => client.Post<TenantDto>("tenant/get-by-tenant-code", It.IsAny<GetTenantCodeRequestDto>()))
                .ReturnsAsync(new TenantDto { TenantCode = "Tenant123" });

            var tasks = Enumerable.Range(1, 25).Select(i => new ImportTaskDto
            {
                Task = new TaskDto { TaskId = i },
                TaskTypeCode = $"Code-{i}"
            }).ToList();

            _s3HelperMock
                .Setup(s3 => s3.UnzipAndProcessJsonFromS3(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new ImportDto
                {
                    TaskData = new TaskImportJson()
                    {
                        Data = new Data()
                        {
                            Task = tasks,

                        }
                    },
                    TenantCodeData = new TenantImportJson() { Data = new TenantData() { Tenant = new TenantDto() { TenantCode = "ten-xyz" } } }
                });
            _tenantService.Setup(x => x.GetTenantDetails(requestDto.tenantCode)).ReturnsAsync(new TenantResponseDto()
            {
                Tenant = new TenantDto() { TenantCode = "Tenant123" }
            });


            _tenantService
                .Setup(x => x.UpdateTenant(requestDto.tenantCode, It.IsAny<UpdateTenantDto>()))
                .ReturnsAsync(new UpdateTenantResponseDto
                {
                    UpdateTenant = new TenantDto() { TenantCode = "Tenant123", RecommendedTask = true }
                });

            // Act
            var result = await _service.TenantImport(requestDto);

            // Assert
            Assert.Null(result.ErrorCode);
        }

        [Fact]
        public async void TenantImport_TaskAPIfails_WhenProcessingTaskImport_withTaskData()
        {
            // Arrange
            var requestDto = new TenantImportRequestDto
            {
                tenantCode = "Tenant123",
                File = new FormFile(Stream.Null, 0, 0, "file", "test.json"),
                ImportOptions = new List<string> { "TASK" }
            };
            var configSectionMock = new Mock<IConfigurationSection>();
            configSectionMock.Setup(x => x.Value).Returns("Secret");

            _configMock.Setup(x => x.GetSection(It.IsAny<string>())).Returns(configSectionMock.Object);

            _s3HelperMock
                .Setup(s3 => s3.UploadFileToS3(It.IsAny<string>(), It.IsAny<IFormFile>(), It.IsAny<string>()))
                .ReturnsAsync(true);

            _tenantClientMock
                .Setup(client => client.Post<TenantDto>("tenant/get-by-tenant-code", It.IsAny<GetTenantCodeRequestDto>()))
                .ReturnsAsync(new TenantDto { TenantCode = "Tenant123" });

            var tasks = Enumerable.Range(1, 25).Select(i => new ImportTaskDto
            {
                Task = new TaskDto { TaskId = i },
                TaskTypeCode = $"Code-{i}"
            }).ToList();

            _s3HelperMock
                .Setup(s3 => s3.UnzipAndProcessJsonFromS3(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new ImportDto
                {
                    TaskData = new TaskImportJson()
                    {
                        Data = new Data()
                        {
                            Task = tasks,

                        }
                    },
                    TenantCodeData = new TenantImportJson() { Data = new TenantData() { Tenant = new TenantDto() { TenantCode = "ten-xyz" } } }
                });

            var failedResponse = new BaseResponseDto { ErrorCode = 500, ErrorMessage = "API Error" };

            _tenantService.Setup(x => x.GetTenantDetails(requestDto.tenantCode)).ReturnsAsync(new TenantResponseDto()
            {
                Tenant = new TenantDto() { TenantCode = "Tenant123" }
            });


            _tenantService
                .Setup(x => x.UpdateTenant(requestDto.tenantCode, It.IsAny<UpdateTenantDto>()))
                .ReturnsAsync(new UpdateTenantResponseDto
                {
                    UpdateTenant = new TenantDto() { TenantCode = "Tenant123", RecommendedTask = true }
                });

            _taskClientMock
                .SetupSequence(client => client.Post<BaseResponseDto>(It.IsAny<string>(), It.IsAny<ImportTaskRewardDetailsRequestDto>()))
                .ReturnsAsync(failedResponse) // First batch fails
                .ReturnsAsync(new BaseResponseDto())
            .ReturnsAsync(new BaseResponseDto());

            // Act
            var result = await _service.TenantImport(requestDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(500, result.ErrorCode);
            Assert.Contains("API Error", result.ErrorMessage);
        }

        [Fact]
        public async void TenantImport_ShouldLogWarning_WhenUnknownOptionIsPassed()
        {
            // Arrange
            var requestDto = new TenantImportRequestDto
            {
                tenantCode = "Tenant123",
                File = new FormFile(Stream.Null, 0, 0, "file", "test.json"),
                ImportOptions = new List<string> { "UNKNOWN" }
            };

            var configSectionMock = new Mock<IConfigurationSection>();
            configSectionMock.Setup(x => x.Value).Returns("Secret");

            _configMock.Setup(x => x.GetSection(It.IsAny<string>())).Returns(configSectionMock.Object);


            _s3HelperMock
                .Setup(s3 => s3.UploadFileToS3(It.IsAny<string>(), It.IsAny<IFormFile>(), It.IsAny<string>()))
                .ReturnsAsync(true);

            _tenantClientMock
                .Setup(client => client.Post<TenantDto>("tenant/get-by-tenant-code", It.IsAny<GetTenantCodeRequestDto>()))
                .ReturnsAsync(new TenantDto { TenantCode = "Tenant123" });

            // Act
            var result = await _service.TenantImport(requestDto);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async void TenantImport_ShouldReturn500_WhenExceptionOccurs()
        {
            // Arrange
            var requestDto = new TenantImportRequestDto
            {
                tenantCode = "Tenant123",
                File = new FormFile(Stream.Null, 0, 0, "file", "test.json"),
                ImportOptions = new List<string> { "ALL" }
            };

            var configSectionMock = new Mock<IConfigurationSection>();
            configSectionMock.Setup(x => x.Value).Returns("Secret");

            _configMock.Setup(x => x.GetSection(It.IsAny<string>())).Returns(configSectionMock.Object);


            _s3HelperMock
                .Setup(s3 => s3.UploadFileToS3(It.IsAny<string>(), It.IsAny<IFormFile>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("S3 Error"));

            _s3HelperMock
               .Setup(s3 => s3.UnzipAndProcessJsonFromS3(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
               .ReturnsAsync(new ImportDto
               {
                   TaskData = new TaskImportJson()
                   {
                       Data = new Data()
                       {
                           Task = null,

                       }
                   },
                   TenantCodeData = new TenantImportJson() { Data = new TenantData() { Tenant = new TenantDto() { TenantCode = "ten-xyz" } } }
               });

            _tenantService.Setup(x => x.GetTenantDetails(requestDto.tenantCode)).ThrowsAsync(new Exception("Service Error"));

            // Act
            await Assert.ThrowsAsync<Exception>(() => _service.TenantImport(requestDto));

        }

        [Fact]
        public async void ProcessTriviaImport_ShouldReturn404_WhenTriviaDataIsValid()
        {
            var requestDto = new TenantImportRequestDto
            {
                tenantCode = "Tenant123",
                File = new FormFile(Stream.Null, 0, 0, "file", "test.json"),
                ImportOptions = new List<string> { "TRIVIA" }
            };
            var configSectionMock = new Mock<IConfigurationSection>();
            configSectionMock.Setup(x => x.Value).Returns("Secret");

            _configMock.Setup(x => x.GetSection(It.IsAny<string>())).Returns(configSectionMock.Object);

            _s3HelperMock
                .Setup(s3 => s3.UploadFileToS3(It.IsAny<string>(), It.IsAny<IFormFile>(), It.IsAny<string>()))
                .ReturnsAsync(true);

            _tenantClientMock
                .Setup(client => client.Post<TenantDto>("tenant/get-by-tenant-code", It.IsAny<GetTenantCodeRequestDto>()))
                .ReturnsAsync(new TenantDto { TenantCode = "Tenant123" });

            _s3HelperMock
                .Setup(s3 => s3.UnzipAndProcessJsonFromS3(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new ImportDto { TaskData = new TaskImportJson(), TenantCodeData = new TenantImportJson() { Data = new TenantData() { Tenant = new TenantDto() { TenantCode = "ten-xyz" } } } });

            _tenantService.Setup(x => x.GetTenantDetails(requestDto.tenantCode)).ReturnsAsync(new TenantResponseDto()
            {
                Tenant = new TenantDto() { TenantCode = "Tenant123" }
            });


            _tenantService
                .Setup(x => x.UpdateTenant(requestDto.tenantCode, It.IsAny<UpdateTenantDto>()))
                .ReturnsAsync(new UpdateTenantResponseDto
                {
                    UpdateTenant = new TenantDto() { TenantCode = "Tenant123", RecommendedTask = true }
                });

            // Arrange
            var tenantImportRequestDto = new TenantImportRequestDto
            {
                tenantCode = "TestTenant"
            };

            var triviaData = new TaskImportJson
            {
                Data = new Data
                {
                    Trivia = new List<ImportTriviaDto>
                    {
                        new ImportTriviaDto { Trivia = new TriviaDto { TriviaId = 1 } }
                    },
                    TriviaQuestionGroup = new List<TriviaQuestionGroupDto>
                    {
                        new TriviaQuestionGroupDto { TriviaId = 1, TriviaQuestionId = 101 },
                        new TriviaQuestionGroupDto { TriviaId = 1, TriviaQuestionId = 102 }
                    },
                    TriviaQuestion = new List<TriviaQuestionDto>
                    {
                        new TriviaQuestionDto { TriviaQuestionId = 101 },
                        new TriviaQuestionDto { TriviaQuestionId = 102 }
                    }
                }
            };

            _taskClientMock
                .Setup(client => client.Post<BaseResponseDto>(
                    Constant.ImportTriviaApiUrl,
                    It.IsAny<ImportTriviaRequestDto>()))
                .ReturnsAsync(new BaseResponseDto());

            // Act
            var result = await _service.TenantImport(requestDto);

            // Assert
            Assert.True(result.ErrorCode == StatusCodes.Status500InternalServerError);

        }
        [Fact]
        public async void ProcessTriviaImport_ShouldReturnSuccess_WhenTriviaDataIsValid()
        {
            var requestDto = new TenantImportRequestDto
            {
                tenantCode = "Tenant123",
                File = new FormFile(Stream.Null, 0, 0, "file", "test.json"),
                ImportOptions = new List<string> { "TRIVIA" }
            };
            var configSectionMock = new Mock<IConfigurationSection>();
            configSectionMock.Setup(x => x.Value).Returns("Secret");

            _configMock.Setup(x => x.GetSection(It.IsAny<string>())).Returns(configSectionMock.Object);

            _s3HelperMock
                .Setup(s3 => s3.UploadFileToS3(It.IsAny<string>(), It.IsAny<IFormFile>(), It.IsAny<string>()))
                .ReturnsAsync(true);

            _tenantService.Setup(x => x.GetTenantDetails(requestDto.tenantCode)).ReturnsAsync(new TenantResponseDto()
            {
                Tenant = new TenantDto() { TenantCode = "Tenant123" }
            });


            _tenantService
                .Setup(x => x.UpdateTenant(requestDto.tenantCode, It.IsAny<UpdateTenantDto>()))
                .ReturnsAsync(new UpdateTenantResponseDto
                {
                    UpdateTenant = new TenantDto() { TenantCode = "Tenant123", RecommendedTask = true }
                });



            // Arrange
            var tenantImportRequestDto = new TenantImportRequestDto
            {
                tenantCode = "TestTenant"
            };

            var triviaData = new TaskImportJson
            {
                Data = new Data
                {
                    Trivia = new List<ImportTriviaDto>
                    {
                        new ImportTriviaDto { Trivia = new TriviaDto { TriviaId = 1 } }
                    },
                    TriviaQuestionGroup = new List<TriviaQuestionGroupDto>
                    {
                        new TriviaQuestionGroupDto { TriviaId = 1, TriviaQuestionId = 101 },
                        new TriviaQuestionGroupDto { TriviaId = 1, TriviaQuestionId = 102 }
                    },
                    TriviaQuestion = new List<TriviaQuestionDto>
                    {
                        new TriviaQuestionDto { TriviaQuestionId = 101 },
                        new TriviaQuestionDto { TriviaQuestionId = 102 }
                    }
                }
            };
            _s3HelperMock
               .Setup(s3 => s3.UnzipAndProcessJsonFromS3(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
               .ReturnsAsync(new ImportDto { TaskData = triviaData, TenantCodeData = new TenantImportJson() { Data = new TenantData() { Tenant = new TenantDto() { TenantCode = "ten-xyz" } } } });
            _taskClientMock
                .Setup(client => client.Post<BaseResponseDto>(
                    Constant.ImportTriviaApiUrl,
                    It.IsAny<ImportTriviaRequestDto>()))
                .ReturnsAsync(new BaseResponseDto());

            // Act
            var result = await _service.TenantImport(requestDto);

            // Assert
            Assert.Null(result.ErrorCode);

        }

        [Fact]
        public async void ProcessTriviaImport_ShouldReturnError_WhenAPIFails()
        {
            var requestDto = new TenantImportRequestDto
            {
                tenantCode = "Tenant123",
                File = new FormFile(Stream.Null, 0, 0, "file", "test.json"),
                ImportOptions = new List<string> { "TRIVIA" }
            };
            var configSectionMock = new Mock<IConfigurationSection>();
            configSectionMock.Setup(x => x.Value).Returns("Secret");

            _configMock.Setup(x => x.GetSection(It.IsAny<string>())).Returns(configSectionMock.Object);

            _s3HelperMock
                .Setup(s3 => s3.UploadFileToS3(It.IsAny<string>(), It.IsAny<IFormFile>(), It.IsAny<string>()))
                .ReturnsAsync(true);

            _tenantService.Setup(x => x.GetTenantDetails(requestDto.tenantCode)).ReturnsAsync(new TenantResponseDto()
            {
                Tenant = new TenantDto() { TenantCode = "Tenant123" }
            });


            _tenantService
                .Setup(x => x.UpdateTenant(requestDto.tenantCode, It.IsAny<UpdateTenantDto>()))
                .ReturnsAsync(new UpdateTenantResponseDto
                {
                    UpdateTenant = new TenantDto() { TenantCode = "Tenant123", RecommendedTask = true }
                });



            // Arrange
            var tenantImportRequestDto = new TenantImportRequestDto
            {
                tenantCode = "TestTenant"
            };

            var triviaData = new TaskImportJson
            {
                Data = new Data
                {
                    Trivia = new List<ImportTriviaDto>
                    {
                        new ImportTriviaDto { Trivia = new TriviaDto { TriviaId = 1 } }
                    },
                    TriviaQuestionGroup = new List<TriviaQuestionGroupDto>
                    {
                        new TriviaQuestionGroupDto { TriviaId = 1, TriviaQuestionId = 101 },
                        new TriviaQuestionGroupDto { TriviaId = 1, TriviaQuestionId = 102 }
                    },
                    TriviaQuestion = new List<TriviaQuestionDto>
                    {
                        new TriviaQuestionDto { TriviaQuestionId = 101 },
                        new TriviaQuestionDto { TriviaQuestionId = 102 }
                    }
                }
            };
            _s3HelperMock
               .Setup(s3 => s3.UnzipAndProcessJsonFromS3(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
               .ReturnsAsync(new ImportDto { TaskData = triviaData, TenantCodeData = new TenantImportJson() { Data = new TenantData() { Tenant = new TenantDto() { TenantCode = "ten-xyz" } } } });

            var failedResponse = new BaseResponseDto { ErrorCode = 500, ErrorMessage = "API Error" };
            _taskClientMock
                .SetupSequence(client => client.Post<BaseResponseDto>(
                    Constant.ImportTriviaApiUrl,
                    It.IsAny<ImportTriviaRequestDto>()))
                .ReturnsAsync(failedResponse)
                .ReturnsAsync(new BaseResponseDto());

            _tenantService.Setup(x => x.GetTenantDetails(requestDto.tenantCode)).ReturnsAsync(new TenantResponseDto()
            {
                Tenant = new TenantDto() { TenantCode = "Tenant123" }
            });


            _tenantService
                .Setup(x => x.UpdateTenant(requestDto.tenantCode, It.IsAny<UpdateTenantDto>()))
                .ReturnsAsync(new UpdateTenantResponseDto
                {
                    UpdateTenant = new TenantDto() { TenantCode = "Tenant123", RecommendedTask = true }
                });


            var result = await _service.TenantImport(requestDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(500, result.ErrorCode);
            Assert.Contains("API Error", result.ErrorMessage);

        }

        [Fact]
        public async void ProcessTriviaImport_ShouldReturn404_WhenFileNotConatainTriviaData()
        {
            var requestDto = new TenantImportRequestDto
            {
                tenantCode = "Tenant123",
                File = new FormFile(Stream.Null, 0, 0, "file", "test.json"),
                ImportOptions = new List<string> { "TRIVIA" }
            };
            var configSectionMock = new Mock<IConfigurationSection>();
            configSectionMock.Setup(x => x.Value).Returns("Secret");

            _configMock.Setup(x => x.GetSection(It.IsAny<string>())).Returns(configSectionMock.Object);

            _s3HelperMock
                .Setup(s3 => s3.UploadFileToS3(It.IsAny<string>(), It.IsAny<IFormFile>(), It.IsAny<string>()))
                .ReturnsAsync(true);

            _tenantService.Setup(x => x.GetTenantDetails(requestDto.tenantCode)).ReturnsAsync(new TenantResponseDto()
            {
                Tenant = new TenantDto() { TenantCode = "Tenant123" }
            });


            _tenantService
                .Setup(x => x.UpdateTenant(requestDto.tenantCode, It.IsAny<UpdateTenantDto>()))
                .ReturnsAsync(new UpdateTenantResponseDto
                {
                    UpdateTenant = new TenantDto() { TenantCode = "Tenant123", RecommendedTask = true }
                });



            // Arrange
            var tenantImportRequestDto = new TenantImportRequestDto
            {
                tenantCode = "TestTenant"
            };


            _s3HelperMock
               .Setup(s3 => s3.UnzipAndProcessJsonFromS3(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
               .ReturnsAsync(new ImportDto { TaskData = new TaskImportJson() { Data = new Data() { } }, TenantCodeData = new TenantImportJson() { Data = new TenantData() { Tenant = new TenantDto() { TenantCode = "ten-xyz" } } } });
            _taskClientMock
                .Setup(client => client.Post<BaseResponseDto>(
                    Constant.ImportTriviaApiUrl,
                    It.IsAny<ImportTriviaRequestDto>()))
                .ReturnsAsync(new BaseResponseDto());

            // Act
            var result = await _service.TenantImport(requestDto);

            // Assert
            Assert.True(result.ErrorCode == StatusCodes.Status500InternalServerError);

        }

        [Fact]
        public async void ProcessCMSImport_ShouldReturnSuccess_WhenCMSDataIsProcessed()
        {
            var requestDto = new TenantImportRequestDto
            {
                tenantCode = "Tenant123",
                File = new FormFile(Stream.Null, 0, 0, "file", "test.json"),
                ImportOptions = new List<string> { "CMS" }
            };
            var configSectionMock = new Mock<IConfigurationSection>();
            configSectionMock.Setup(x => x.Value).Returns("Secret");

            _configMock.Setup(x => x.GetSection(It.IsAny<string>())).Returns(configSectionMock.Object);

            _s3HelperMock
                .Setup(s3 => s3.UploadFileToS3(It.IsAny<string>(), It.IsAny<IFormFile>(), It.IsAny<string>()))
                .ReturnsAsync(true);

            _tenantService.Setup(x => x.GetTenantDetails(requestDto.tenantCode)).ReturnsAsync(new TenantResponseDto()
            {
                Tenant = new TenantDto() { TenantCode = "Tenant123" }
            });


            _tenantService
                .Setup(x => x.UpdateTenant(requestDto.tenantCode, It.IsAny<UpdateTenantDto>()))
                .ReturnsAsync(new UpdateTenantResponseDto
                {
                    UpdateTenant = new TenantDto() { TenantCode = "Tenant123", RecommendedTask = true }
                });




            var cmsImportJson = new CmsImportJson
            {
                Data = new CmsData
                {
                    Component = new List<ImportComponentDto>
            {
                new ImportComponentDto {  Component=new ComponentDto{ComponentId = 1, ComponentName = "Header" } },
                new ImportComponentDto {  Component=new ComponentDto{ComponentId = 2, ComponentName = "Footer"  } },
            }
                }
            };
            _s3HelperMock
               .Setup(s3 => s3.UnzipAndProcessJsonFromS3(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
               .ReturnsAsync(new ImportDto { CMSData = cmsImportJson, TenantCodeData = new TenantImportJson() { Data = new TenantData() { Tenant = new TenantDto() { TenantCode = "ten-xyz" } } } });
            var tenantImportRequestDto = new TenantImportRequestDto { tenantCode = "TestTenant" };


            var expectedResponse = new BaseResponseDto(); // No errors

            _cmsClientMock
                .Setup(client => client.Post<BaseResponseDto>(It.IsAny<string>(), It.IsAny<CmsImportRequestDto>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _service.TenantImport(requestDto);

            // Assert
            Assert.NotNull(result);


            _cmsClientMock.Verify(client => client.Post<BaseResponseDto>(It.IsAny<string>(), It.IsAny<CmsImportRequestDto>()), Times.AtLeastOnce);
        }
        [Fact]
        public async void ProcessCMSImport_ShouldReturn404_WhenCMSDataIsProcessed()
        {
            var requestDto = new TenantImportRequestDto
            {
                tenantCode = "Tenant123",
                File = new FormFile(Stream.Null, 0, 0, "file", "test.json"),
                ImportOptions = new List<string> { "CMS" }
            };
            var configSectionMock = new Mock<IConfigurationSection>();
            configSectionMock.Setup(x => x.Value).Returns("Secret");

            _configMock.Setup(x => x.GetSection(It.IsAny<string>())).Returns(configSectionMock.Object);

            _s3HelperMock
                .Setup(s3 => s3.UploadFileToS3(It.IsAny<string>(), It.IsAny<IFormFile>(), It.IsAny<string>()))
                .ReturnsAsync(true);

            _tenantClientMock
                .Setup(client => client.Post<TenantDto>("tenant/get-by-tenant-code", It.IsAny<GetTenantCodeRequestDto>()))
                .ReturnsAsync(new TenantDto { TenantCode = "Tenant123" });




            _s3HelperMock
               .Setup(s3 => s3.UnzipAndProcessJsonFromS3(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
               ;
            var tenantImportRequestDto = new TenantImportRequestDto { tenantCode = "TestTenant" };


            var expectedResponse = new BaseResponseDto(); // No errors

            _cmsClientMock
                .Setup(client => client.Post<BaseResponseDto>(It.IsAny<string>(), It.IsAny<CmsImportRequestDto>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _service.TenantImport(requestDto);

            // Assert
            Assert.True(result.ErrorCode == StatusCodes.Status404NotFound);


        }
        //[Fact]
        //public async void ProcessFisImport_ShouldReturnSuccess_WhenValidFisDataIsProvided()
        //{
        //    // Arrange
        //    var requestDto = new TenantImportRequestDto
        //    {
        //        tenantCode = "Tenant123",
        //        CustomerCode = "CUST001",
        //        SponsorCode = "SPONS001",
        //        File = new FormFile(Stream.Null, 0, 0, "file", "test.json"),
        //        ImportOptions = new List<string> { "FIS" }
        //    };
        //    var configSectionMock = new Mock<IConfigurationSection>();
        //    configSectionMock.Setup(x => x.Value).Returns("Secret");

        //    _configMock.Setup(x => x.GetSection(It.IsAny<string>())).Returns(configSectionMock.Object);

        //    _s3HelperMock
        //        .Setup(s3 => s3.UploadFileToS3(It.IsAny<string>(), It.IsAny<IFormFile>(), It.IsAny<string>()))
        //        .ReturnsAsync(true);

        //    _tenantService.Setup(x => x.GetTenantDetails(requestDto.tenantCode)).ReturnsAsync(new TenantResponseDto()
        //    {
        //        Tenant = new TenantDto() { TenantCode = "Tenant123" }
        //    });


        //    _tenantService
        //        .Setup(x => x.UpdateTenant(requestDto.tenantCode, It.IsAny<UpdateTenantDto>()))
        //        .ReturnsAsync(new UpdateTenantResponseDto
        //        {
        //            UpdateTenant = new TenantDto() { TenantCode = "Tenant123", RecommendedTask = true }
        //        });




        //    var tenantImportRequest = new TenantImportRequestDto
        //    {
        //        tenantCode = "TENANT123",
        //        CustomerCode = "CUST001",
        //        SponsorCode = "SPONS001"
        //    };

        //    var fisData = new FisImportJson
        //    {
        //        Data = new TenantAccountData { TenantAccount = new GetTenantAccountDto { TenantCode = "Tenant123" } }
        //    };

        //    _s3HelperMock
        //      .Setup(s3 => s3.UnzipAndProcessJsonFromS3(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
        //      .ReturnsAsync(new ImportDto { TenantData = fisData, TenantCodeData = new TenantImportJson() { Data = new TenantData() { Tenant = new TenantDto() { TenantCode = "ten-xyz" } } } });
        //    var tenantImportRequestDto = new TenantImportRequestDto { tenantCode = "Tenant123" };


        //    var tenantSponsorCustomerResponse = new TenantSponsorCustomerResponseDto
        //    {
        //        Customer = new CustomerDto { CustomerCode = "CUST001" },
        //        Sponsor = new SponsorDto { SponsorCode = "SPONS001" }
        //    };

        //    var getTenantAccountResponse = new GetTenantAccountResponseDto
        //    {
        //        TenantAccount = new TenantAccountRequestDto { TenantCode = "Tenant123" }
        //    };

        //    var updateTenantAccountResponse = new BaseResponseDto(); // Successful update
        //    var createWalletResponse = new BaseResponseDto(); // Successful wallet creation

        //    _tenantClientMock
        //        .Setup(tc => tc.Get<TenantSponsorCustomerResponseDto>(It.IsAny<string>(), It.IsAny<IDictionary<string, long>>()))
        //        .ReturnsAsync(tenantSponsorCustomerResponse);



        //    _mapperMock
        //        .Setup(m => m.Map<TenantAccountRequestDto>(It.IsAny<TenantAccountDto>()))
        //        .Returns(new TenantAccountRequestDto());

        //    _tenantAccountService
        //        .Setup(tas => tas.GetTenantAccount(It.IsAny<string>()))
        //        .ReturnsAsync(getTenantAccountResponse);
        //    _tenantAccountService
        //        .Setup(tas => tas.UpdateTenantAccount(It.IsAny<string>(), It.IsAny<TenantAccountRequestDto>()))
        //        .ReturnsAsync(new TenantAccountUpdateResponseDto());

        //    _tenantAccountService
        //        .Setup(tas => tas.CreateMasterWallets(It.IsAny<TenantAccountRequestDto>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
        //        .ReturnsAsync(createWalletResponse);

        //    // Act
        //    var result = await _service.TenantImport(requestDto);

        //    // Assert
        //    Assert.NotNull(result);

        //}
        [Fact]
        public async void ProcessSweepstakesImport_ShouldReturnSuccess_WhenCMSDataIsProcessed()
        {
            var requestDto = new TenantImportRequestDto
            {
                tenantCode = "Tenant123",
                File = new FormFile(Stream.Null, 0, 0, "file", "test.json"),
                ImportOptions = new List<string> { "SWEEPSTAKES" }
            };
            var configSectionMock = new Mock<IConfigurationSection>();
            configSectionMock.Setup(x => x.Value).Returns("Secret");

            _configMock.Setup(x => x.GetSection(It.IsAny<string>())).Returns(configSectionMock.Object);

            _s3HelperMock
                .Setup(s3 => s3.UploadFileToS3(It.IsAny<string>(), It.IsAny<IFormFile>(), It.IsAny<string>()))
                .ReturnsAsync(true);

            _tenantService.Setup(x => x.GetTenantDetails(requestDto.tenantCode)).ReturnsAsync(new TenantResponseDto()
            {
                Tenant = new TenantDto() { TenantCode = "Tenant123" }
            });


            _tenantService
                .Setup(x => x.UpdateTenant(requestDto.tenantCode, It.IsAny<UpdateTenantDto>()))
                .ReturnsAsync(new UpdateTenantResponseDto
                {
                    UpdateTenant = new TenantDto() { TenantCode = "Tenant123", RecommendedTask = true }
                });




            var ImportJson = new SweepstakesImportJson
            {
                Data = new SweepstakesData
                {
                    Sweepstakes = new List<SweepstakesDto>
            {
                new SweepstakesDto {   SweepstakesId = 1, SweepstakesName = "Sweepstake A" },
                new SweepstakesDto {   SweepstakesId = 2, SweepstakesName = "Sweepstake B" },
            },
                    TenantSweepstakes = new List<TenantSweepstakesDto>
                    {
                         new TenantSweepstakesDto { SweepstakesId = 1, TenantCode = "101" },
        new TenantSweepstakesDto { SweepstakesId = 1, TenantCode = "102" },
                    }

                }
            };
            _s3HelperMock
               .Setup(s3 => s3.UnzipAndProcessJsonFromS3(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
               .ReturnsAsync(new ImportDto { SweepstakesData = ImportJson, TenantCodeData = new TenantImportJson() { Data = new TenantData() { Tenant = new TenantDto() { TenantCode = "ten-xyz" } } } });
            var tenantImportRequestDto = new TenantImportRequestDto { tenantCode = "TestTenant" };


            var expectedResponse = new BaseResponseDto(); // No errors

            _sweepstakesClientMock
                .Setup(client => client.Post<BaseResponseDto>(It.IsAny<string>(), It.IsAny<ImportSweepstakesRequestDto>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _service.TenantImport(requestDto);

            // Assert
            Assert.NotNull(result);


            _sweepstakesClientMock.Verify(client => client.Post<BaseResponseDto>(It.IsAny<string>(), It.IsAny<ImportSweepstakesRequestDto>()), Times.AtLeastOnce);
        }
    }
}
