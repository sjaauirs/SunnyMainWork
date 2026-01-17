using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SunnyRewards.Helios.Admin.Api.Controllers;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Core.Domain.Models;
using SunnyRewards.Helios.Admin.Infrastructure.Repositories.HeliosRepo.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using System.Linq.Expressions;
using Xunit;
using TaskAsync = System.Threading.Tasks.Task;

namespace SunnyRewards.Helios.Admin.UnitTest.Controllers
{
    public class JobReportControllerTests
    {
        private readonly JobReportController _jobReportController;
        private readonly JobReportService _jobReportService;
        private readonly Mock<ILogger<JobReportService>> _loggerMock;
        private readonly Mock<ILogger<JobReportController>> _loggerControllerMock;
        private readonly Mock<IBatchJobReportRepo> _batchJobReportRepoMock;
        private readonly IMapper _mapper;

        public JobReportControllerTests()
        {
            // Mock the repositories
            _loggerMock = new Mock<ILogger<JobReportService>>();
            _loggerControllerMock = new Mock<ILogger<JobReportController>>();
            _batchJobReportRepoMock = new Mock<IBatchJobReportRepo>();
            // Configure AutoMapper for testing
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<BatchJobReportModel, BatchJobReportDto>();
                cfg.CreateMap<BatchJobReportDto, BatchJobReportModel>();
                cfg.CreateMap<BatchJobDetailReportModel, BatchJobDetailReportDto>();
                cfg.CreateMap<BatchJobDetailReportDto, BatchJobDetailReportModel>();
            });
            _mapper = config.CreateMapper();

            // Initialize the service with the mocked dependencies
            _jobReportService = new JobReportService(_loggerMock.Object, _batchJobReportRepoMock.Object, _mapper);

            // Initialize the controller with the real service
            _jobReportController = new JobReportController(_loggerControllerMock.Object,_jobReportService);
        }

        [Fact]
        public async TaskAsync GetJobReports_WithValidRequest_ReturnsOkResult()
        {
            // Arrange
            var jobReportRequestDto = new JobReportRequestDto
            {
                JobReportCode = "JobReportCode1",
                SearchByJobName = "TestJob",
                PageNumber = 1,
                PageSize = 10
            };

            var batchJobReportModels = new List<BatchJobReportModel>
        {
            new BatchJobReportModel { BatchJobReportCode = "JobReportCode1", DeleteNbr = 0 }
        };

            _batchJobReportRepoMock
                .Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<BatchJobReportModel, bool>>>(), false))
                .ReturnsAsync(batchJobReportModels);

            // Act
            var result = await _jobReportController.GetJobReports(jobReportRequestDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var responseDto = Assert.IsType<GetBatchJobReportResponseDto>(okResult.Value);
            Assert.NotNull(responseDto.jobReports);
            Assert.NotEmpty(responseDto.jobReports);
        }

        [Fact]
        public async TaskAsync GetJobReports_WithInvalidRequest_Status404NotFound()
        {
            // Arrange
            var jobReportRequestDto = new JobReportRequestDto
            {
                JobReportCode = "InvalidCode",
                SearchByJobName = "TestJob",
                PageNumber = 1,
                PageSize = 10
            };

            _batchJobReportRepoMock
                .Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<BatchJobReportModel, bool>>>(), false))
                .ReturnsAsync(new List<BatchJobReportModel>());

            // Act
            var result = await _jobReportController.GetJobReports(jobReportRequestDto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);
        }

        [Fact]
        public async TaskAsync GetJobReports_WithInvalidRequest_Status500()
        {
            // Arrange
            var jobReportRequestDto = new JobReportRequestDto
            {
                JobReportCode = "InvalidCode",
                SearchByJobName = "TestJob",
                PageNumber = 1,
                PageSize = 10
            };

            _batchJobReportRepoMock
                .Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<BatchJobReportModel, bool>>>(), false))
                .Throws<ArgumentException>();

            // Act
            var result = await _jobReportController.GetJobReports(jobReportRequestDto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        [Fact]
        public async TaskAsync GetJobReports_WithBlankJobReportCodeAndSearch_ReturnsAllRecord()
        {
            // Arrange
            var jobReportRequestDto = new JobReportRequestDto
            {
                JobReportCode = "",
                SearchByJobName = "",
                PageNumber = 1,
                PageSize = 10
            };
            var batchJobReportModels = new List<BatchJobReportModel>
        {
            new BatchJobReportModel { BatchJobReportCode = "JobReportCode1", DeleteNbr = 0 }
        };
            var paginatedResult = new PaginatedBatchJobReport()
            { JobReports = batchJobReportModels , TotalRecords = 1 };

            _batchJobReportRepoMock
                .Setup(repo => repo.GetPaginatedJobReport(It.IsAny<string>(),It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(paginatedResult);

            // Act
            var result = await _jobReportController.GetJobReports(jobReportRequestDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var responseDto = Assert.IsType<GetBatchJobReportResponseDto>(okResult.Value);
            Assert.NotNull(responseDto.jobReports);
            Assert.NotEmpty(responseDto.jobReports);
        }

        [Fact]
        public async TaskAsync SaveJobReports_ReturnsOk_WhenSaveSuccessful()
        {
            // Arrange
            var dto = new BatchJobReportDto {  JobType = "card30" , JobResultJson="{}" };
            var response = new BatchJobReportResponseDto
            {
                jobReport = dto,
                ErrorCode = null
            };

            var batchJobReportModel = new BatchJobReportModel
            {
                BatchJobReportCode = "TestCode",
                BatchJobReportId = 1,
                JobType = "card30"
            };

            _batchJobReportRepoMock.Setup(r => r.CreateAsync(It.IsAny<BatchJobReportModel>()))
                .ReturnsAsync(batchJobReportModel);

            // Act
            var result = await _jobReportController.SaveJobReports(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
            var responseDto = Assert.IsType<BatchJobReportResponseDto>(okResult.Value);
            Assert.Equal(response.jobReport.JobType, responseDto?.jobReport?.JobType ?? "");
        }

        [Fact]
        public async TaskAsync SaveJobReports_ReturnsErrorCode_WhenServiceReturnsError()
        {
            // Arrange
            var dto = new BatchJobReportDto { JobType = "card30", JobResultJson = "{}" };

            _batchJobReportRepoMock.Setup(r => r.CreateAsync(It.IsAny<BatchJobReportModel>()));

            // Act
            var result = await _jobReportController.SaveJobReports(dto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        [Fact]
        public async TaskAsync SaveJobReports_ReturnsInternalServerError_WhenExceptionThrown()
        {
            // Arrange
            var dto = new BatchJobReportDto {JobType = "card30", JobResultJson = "{}" };
            var batchJobReportModel = new BatchJobReportModel
            {
                BatchJobReportCode = "TestCode",
                BatchJobReportId = 1
            };

            _batchJobReportRepoMock.Setup(r => r.CreateAsync(It.IsAny<BatchJobReportModel>()))
                .ThrowsAsync(new Exception("Test Exception") );

            // Act
            var result = await _jobReportController.SaveJobReports(dto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);

            var responseDto = Assert.IsType<BaseResponseDto>(objectResult.Value);
            Assert.Equal(StatusCodes.Status500InternalServerError, responseDto.ErrorCode);
            Assert.Equal("Test Exception", responseDto.ErrorMessage);
        }

    }
}

