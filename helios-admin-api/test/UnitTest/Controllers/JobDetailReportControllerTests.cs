using AutoMapper;
using Azure;
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
using SunnyRewards.Helios.Task.Core.Domain.Models;
using System.Linq.Expressions;
using Xunit;
using TaskAsync = System.Threading.Tasks.Task;

namespace SunnyRewards.Helios.Admin.UnitTest.Controllers
{
    public class JobDetailReportControllerTests
    {
        private readonly JobDetailReportController _jobDetailReportController;
        private readonly JobDetailReportService _jobDetailReportService;
        private readonly Mock<ILogger<JobDetailReportService>> _loggerMock;
        private readonly Mock<ILogger<JobDetailReportController>> _loggerControllerMock;
        private readonly Mock<IBatchJobDetailReportRepo> _batchJobDetailReportRepoMock;
        private readonly IMapper _mapper;

        public JobDetailReportControllerTests()
        {
            // Mock the repositories
            _loggerMock = new Mock<ILogger<JobDetailReportService>>();
            _loggerControllerMock = new Mock<ILogger<JobDetailReportController>>();
            _batchJobDetailReportRepoMock = new Mock<IBatchJobDetailReportRepo>();

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
            _jobDetailReportService = new JobDetailReportService(_loggerMock.Object, _batchJobDetailReportRepoMock.Object, _mapper);

            // Initialize the controller with the real service
            _jobDetailReportController = new JobDetailReportController(_loggerControllerMock.Object,_jobDetailReportService);
        }


        [Fact]
        public async TaskAsync GetJobDetailReport_WithINValidJobReportCode_404()
        {
            // Arrange
            var jobDetailReportRequestDto = new JobDetailReportRequestDto
            {
                JobReportCode = "JobReportCode1"
            };

            var batchJobDetailReportModels = new List<BatchJobDetailReportModel>
        {
            new BatchJobDetailReportModel { BatchJobReportId = 1 }
        };

            _batchJobDetailReportRepoMock
                .Setup(repo => repo.GetBatchJobDetailsByReportCode(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()));

            // Act
            var result = await _jobDetailReportController.GetJobDetailReport(jobDetailReportRequestDto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);
        }

        [Fact]
        public async TaskAsync GetJobDetailReport_WithInvalidJobReportCode_ServerError()
        {
            // Arrange
            var jobDetailReportRequestDto = new JobDetailReportRequestDto
            {
                JobReportCode = "InvalidCode"
            };

            _batchJobDetailReportRepoMock
                .Setup(repo => repo.GetBatchJobDetailsByReportCode(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>())).Throws<ArgumentException>();

            // Act
            var result = await _jobDetailReportController.GetJobDetailReport(jobDetailReportRequestDto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        [Fact]
        public async TaskAsync GetJobDetailReport_WithInvalidJobReportCode_400()
        {
            // Arrange
            var jobDetailReportRequestDto = new JobDetailReportRequestDto
            {
                JobReportCode = ""
            };

            _batchJobDetailReportRepoMock
                .Setup(repo => repo.GetBatchJobDetailsByReportCode(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>())).Throws<ArgumentException>();

            // Act
            var result = await _jobDetailReportController.GetJobDetailReport(jobDetailReportRequestDto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
        }


        [Fact]
        public async TaskAsync GetJobDetailReport_WithValidJobReportCode_okResult_200()
        {
            // Arrange
            var jobDetailReportRequestDto = new JobDetailReportRequestDto
            {
                JobReportCode = "ValidData"
            };
            var batchJobDetailReport = new List<BatchJobDetailReportModel>
        {
            new BatchJobDetailReportModel { BatchJobReportId = 1 }
        };
            var paginatedResult = new PaginatedBatchJobDetailReport() { JobDetailReports = batchJobDetailReport, TotalRecords =1 };

            _batchJobDetailReportRepoMock
                .Setup(repo => repo.GetBatchJobDetailsByReportCode(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(paginatedResult);

            // Act
            var result = await _jobDetailReportController.GetJobDetailReport(jobDetailReportRequestDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var responseDto = Assert.IsType<BatchJobDetailReportResponseDto>(okResult.Value);
            Assert.NotNull(responseDto.BatchJobDetails);
            Assert.NotEmpty(responseDto.BatchJobDetails);
        }

        [Fact]
        public async TaskAsync SaveJobDetailReport_ValidRequest_ReturnsOk()
        {
            // Arrange
            var requestDto = new BatchJobDetailReportRequestDto
            {
                BatchJobDetailReportDtos = new List<BatchJobDetailReportDto>
                {
                    new BatchJobDetailReportDto { BatchJobReportId = 1 , RecordResultJson ="{\"Files\": [\"8733430801202401.issuance.txt\"], \"RecordsReceived\": 10}" , RecordNum =1 }
                }
            };

            var responseModel = new BatchJobDetailReportModel
            {
                BatchJobDetailReportId = 1 ,
                CreateUser = "ETL",
                RecordNum = 1 ,
                RecordResultJson = "{\"Files\": [\"8733430801202401.issuance.txt\"], \"RecordsReceived\": 10}",
                BatchJobReportId=1
             };

            _batchJobDetailReportRepoMock
                .Setup(repo => repo.CreateAsync(It.IsAny<BatchJobDetailReportModel>()))
                .ReturnsAsync(responseModel);

            // Act
            var result = await _jobDetailReportController.SaveJobDetailReport(requestDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
            var responseDto = Assert.IsType<BatchJobDetailReportResponseDto>(okResult.Value);
            Assert.NotNull(responseDto.BatchJobDetails);
            Assert.NotEmpty(responseDto.BatchJobDetails);

        }

        [Fact]
        public async TaskAsync SaveJobDetailReport_NoDataToSave_ReturnsBadRequest()
        {
            // Arrange
            var requestDto = new BatchJobDetailReportRequestDto
            {
                BatchJobDetailReportDtos = null // No data to save
            };



            _batchJobDetailReportRepoMock
                .Setup(repo => repo.CreateAsync(It.IsAny<BatchJobDetailReportModel>()));

            // Act
            var result = await _jobDetailReportController.SaveJobDetailReport(requestDto) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        }

        [Fact]
        public async TaskAsync SaveJobDetailReport_ServiceReturnsServerError_ReturnsServerError()
        {
            // Arrange
            var requestDto = new BatchJobDetailReportRequestDto
            {
                BatchJobDetailReportDtos = new List<BatchJobDetailReportDto>
                {
                    new BatchJobDetailReportDto { BatchJobReportId = 1 , RecordResultJson ="{\"Files\": [\"8733430801202401.issuance.txt\"], \"RecordsReceived\": 10}" , RecordNum =1 }
                }
            };
            //server returns nothing
            _batchJobDetailReportRepoMock
                .Setup(repo => repo.CreateAsync(It.IsAny<BatchJobDetailReportModel>()));

            // Act
            var result = await _jobDetailReportController.SaveJobDetailReport(requestDto) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, result.StatusCode);

        }

        [Fact]
        public async TaskAsync SaveJobDetailReport_ExceptionThrown_ReturnsInternalServerError()
        {
            // Arrange
            var requestDto = new BatchJobDetailReportRequestDto
            {
                BatchJobDetailReportDtos = new List<BatchJobDetailReportDto>
                {
                    new BatchJobDetailReportDto { BatchJobReportId = 1 , RecordResultJson ="{\"Files\": [\"8733430801202401.issuance.txt\"], \"RecordsReceived\": 10}" , RecordNum =1 }
                }
            };

            _batchJobDetailReportRepoMock
                .Setup(repo => repo.CreateAsync(It.IsAny<BatchJobDetailReportModel>()))
                .ThrowsAsync(new Exception("Test Exception"));
            // Act
            var result = await _jobDetailReportController.SaveJobDetailReport(requestDto) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, result.StatusCode);
        }

    }
}

