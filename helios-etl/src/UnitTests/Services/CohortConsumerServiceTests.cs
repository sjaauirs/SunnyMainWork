using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos.Json;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Services;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces;
using System.Linq.Expressions;
using System.Text;

namespace SunnyRewards.Helios.ETL.UnitTests.Services
{
    public class CohortConsumerServiceTests
    {
        private readonly Mock<ILogger<CohortConsumerService>> _loggerMock;
        private readonly Mock<IJobReportService> _jobReportServiceMock;
        private readonly Mock<ICohortRepo> _cohortRepoMock;
        private readonly Mock<ICohortConsumerRepo> _cohortConsumerRepoMock;
        private readonly Mock<ITenantRepo> _tenantRepoMock;
        private readonly CohortConsumerService _service;
        private readonly Mock<IMapper> _mapper;

        public CohortConsumerServiceTests()
        {
            _loggerMock = new Mock<ILogger<CohortConsumerService>>();
            _jobReportServiceMock = new Mock<IJobReportService>();
            _cohortRepoMock = new Mock<ICohortRepo>();
            _cohortConsumerRepoMock = new Mock<ICohortConsumerRepo>();
            _tenantRepoMock = new Mock<ITenantRepo>();
            _mapper = new Mock<IMapper>();
            _service = new CohortConsumerService(
                _loggerMock.Object,
                _jobReportServiceMock.Object,
                _cohortRepoMock.Object,
                _cohortConsumerRepoMock.Object,
                _tenantRepoMock.Object,
                _mapper.Object
            );
        }

        [Fact]
        public async System.Threading.Tasks.Task Import_ValidFile_ShouldProcessSuccessfully()
        {
            // Arrange
            var fileContents = "CohortCode\tConsumerCode\tDetectDescription\nCohort1\tConsumer1\tDetectDescription1";
            var context = new EtlExecutionContext
            {
                CohortConsumerImportFilePath = "some-file-path",
                CohortConsumerImportFileContents = Encoding.UTF8.GetBytes(fileContents)
            };

            _jobReportServiceMock.Setup(x => x.BatchJobRecords).Returns(new BatchJobRecordsDto { JobType = nameof(CohortConsumerService) });
            _jobReportServiceMock.Setup(x => x.JobResultDetails).Returns(new JobResultDetails { Files = new List<string>() });
            _tenantRepoMock.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ETLTenantModel, bool>>>(), false))
                .ReturnsAsync(new ETLTenantModel { TenantId = 1, TenantCode = "TenantCode1" });
            _cohortRepoMock.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ETLCohortModel, bool>>>(), false))
                .ReturnsAsync(new ETLCohortModel { CohortId = 1, CohortCode = "Cohort1" });
            _cohortConsumerRepoMock.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ETLCohortConsumerModel, bool>>>(), false));
            _cohortConsumerRepoMock.Setup(x => x.CreateAsync(It.IsAny<ETLCohortConsumerModel>()))
                .ReturnsAsync(new ETLCohortConsumerModel { CohortConsumerId = 1 });

            // Act
            await _service.Import(context);

            // Assert
            Assert.Single(_jobReportServiceMock.Object.JobResultDetails.Files);
            Assert.Equal(1, _jobReportServiceMock.Object.JobResultDetails.RecordsReceived);
            Assert.Equal(1, _jobReportServiceMock.Object.JobResultDetails.RecordsProcessed);
        }

        [Fact]
        public async System.Threading.Tasks.Task Import_InvalidFile_ShouldLogError_When_Tenant_Not_Found()
        {
            // Arrange
            var fileContents = "CohortCode\tConsumerCode\tDetectDescription\nCohort1\tConsumer1\tDetectDescription1";
            var context = new EtlExecutionContext
            {
                CohortConsumerImportFilePath = "some-file-path",
                CohortConsumerImportFileContents = Encoding.UTF8.GetBytes(fileContents)
            };
            _jobReportServiceMock.Setup(x => x.BatchJobRecords).Returns(new BatchJobRecordsDto { JobType = nameof(CohortConsumerService) });
            _jobReportServiceMock.Setup(x => x.JobResultDetails).Returns(new JobResultDetails { Files = new List<string>() });
            _tenantRepoMock.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ETLTenantModel, bool>>>(), false));

            // Act
            await _service.Import(context);

            // Assert
            Assert.Equal(0, _jobReportServiceMock.Object.JobResultDetails.RecordsReceived);
        }

        [Fact]
        public async System.Threading.Tasks.Task ProcessImportAsync_CohortNotFound_ShouldLogError()
        {
            // Arrange
            var fileContents = "CohortCode\tConsumerCode\tDetectDescription\nCohort1\tConsumer1\tDetectDescription1\n\t\tDetectDescription1";
            var context = new EtlExecutionContext
            {
                CohortConsumerImportFilePath = "some-file-path",
                CohortConsumerImportFileContents = Encoding.UTF8.GetBytes(fileContents)
            };
            _jobReportServiceMock.Setup(x => x.BatchJobRecords).Returns(new BatchJobRecordsDto { JobType = nameof(CohortConsumerService) });
            _jobReportServiceMock.Setup(x => x.JobResultDetails).Returns(new JobResultDetails { Files = new List<string>() });
            _tenantRepoMock.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ETLTenantModel, bool>>>(), false))
                .ReturnsAsync(new ETLTenantModel { TenantId = 1, TenantCode = "TenantCode1" });
            _cohortRepoMock.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ETLCohortModel, bool>>>(), false))
                .ReturnsAsync(new ETLCohortModel { CohortId = 1, CohortCode = "Cohort1" });
            _cohortConsumerRepoMock.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ETLCohortConsumerModel, bool>>>(), false));
            _cohortConsumerRepoMock.Setup(x => x.CreateAsync(It.IsAny<ETLCohortConsumerModel>()))
                .ReturnsAsync(new ETLCohortConsumerModel { CohortConsumerId = 1 });

            // Act
            await _service.Import(context);

            // Assert
            Assert.Single(_jobReportServiceMock.Object.JobResultDetails.Files);
            Assert.Equal(2, _jobReportServiceMock.Object.JobResultDetails.RecordsReceived);
            Assert.Equal(2, _jobReportServiceMock.Object.JobResultDetails.RecordsProcessed);
            Assert.Equal(1, _jobReportServiceMock.Object.JobResultDetails.RecordsErrorCount);
            Assert.Equal(1, _jobReportServiceMock.Object.JobResultDetails.RecordsSuccessCount);
        }

    }
}
