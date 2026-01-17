using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NHibernate;
using SunnyRewards.Helios.Etl.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using SunnyRewards.Helios.ETL.Infrastructure.Logs.Interface;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.S3FileProcessor;
using SunnyRewards.Helios.ETL.Infrastructure.S3FileProcessor.Interface;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces.DynamoDb;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces.FIS;
using System.Linq.Expressions;

namespace SunnyRewards.Helios.ETL.UnitTests.Services
{
    public class S3FileProcessorCoreTests
    {
        private readonly Mock<IAwsS3Service> _awsS3ServiceMock;
        private readonly Mock<ITaskUpdateService> _taskUpdateServiceMock;
        private readonly Mock<ILogger<S3FileProcessorCore>> _loggerMock;
        private readonly Mock<ICohortService> _cohortServiceMock;
        private readonly Mock<IPersonRepo> _personRepoMock;
        private readonly Mock<IIngestConsumerAttrService> _ingestConsumerAttrServiceMock;
        private readonly Mock<IEnrollmentService> _enrollmentServiceMock;
        private readonly Mock<IMemberImportService> _memberImportServiceMock;
        private readonly Mock<ITenantRepo> _tenantRepoMock;
        private readonly Mock<ISession> _sessionMock;
        private readonly Mock<IPldParser> _pldParserMock;
        private readonly Mock<IS3FileLogger> _s3FileLoggerMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<IMonetaryTransactionsFileReadService> _monetaryTransactionsFileReadServiceMock;
        private readonly Mock<IConsumerNonMonetaryTransactionsFileReadService> _nonMonetaryTransactionsFileReadServiceMock;
        private readonly IS3FileProcessorCore _s3FileProcessorCore;
        private readonly Mock<IWalletService> _walletServiceMock;
        private readonly Mock<IJobReportService> _jobReportServiceMock;
        private readonly Mock<IMemberImportFileDataService> _memberrImportFileDataServiceMock;
        private readonly Mock<ITriviaImportService> _triviaImportServiceMock;
        private readonly Mock<ICohortConsumerService> _cohortConsumerService;
        private readonly Mock<IJobHistoryService> _jobHistoryService;
        private readonly Mock<ITaskImportService> _taskImportService;
        private readonly Mock<IQuestionnaireImportService> _questionnaireImportService;
        private readonly Mock<IDepositInstructionService> _depositInstructionService;

        public S3FileProcessorCoreTests()
        {
            _awsS3ServiceMock = new Mock<IAwsS3Service>();
            _taskUpdateServiceMock = new Mock<ITaskUpdateService>();
            _loggerMock = new Mock<ILogger<S3FileProcessorCore>>();
            _cohortServiceMock = new Mock<ICohortService>();
            _personRepoMock = new Mock<IPersonRepo>();
            _ingestConsumerAttrServiceMock = new Mock<IIngestConsumerAttrService>();
            _enrollmentServiceMock = new Mock<IEnrollmentService>();
            _memberImportServiceMock = new Mock<IMemberImportService>();
            _tenantRepoMock = new Mock<ITenantRepo>();
            _sessionMock = new Mock<ISession>();
            _pldParserMock = new Mock<IPldParser>();
            _s3FileLoggerMock = new Mock<IS3FileLogger>();
            _configurationMock = new Mock<IConfiguration>();
            _monetaryTransactionsFileReadServiceMock = new Mock<IMonetaryTransactionsFileReadService>();
            _nonMonetaryTransactionsFileReadServiceMock = new Mock<IConsumerNonMonetaryTransactionsFileReadService>();
            _walletServiceMock = new Mock<IWalletService>();
            _jobReportServiceMock = new Mock<IJobReportService>();
            _memberrImportFileDataServiceMock = new Mock<IMemberImportFileDataService>();
            _triviaImportServiceMock = new Mock<ITriviaImportService>();
            _cohortConsumerService = new Mock<ICohortConsumerService>();
            _taskImportService = new Mock<ITaskImportService>();
            _jobHistoryService = new Mock<IJobHistoryService>();
            _questionnaireImportService = new Mock<IQuestionnaireImportService>();
            _depositInstructionService = new Mock<IDepositInstructionService>();

            _s3FileProcessorCore = new S3FileProcessorCore(
                _awsS3ServiceMock.Object,
                _pldParserMock.Object,
                _sessionMock.Object,
                _loggerMock.Object,
                _taskUpdateServiceMock.Object,
                _cohortServiceMock.Object,
                _personRepoMock.Object,
                _tenantRepoMock.Object,
                _ingestConsumerAttrServiceMock.Object,
                _enrollmentServiceMock.Object,
                _s3FileLoggerMock.Object,
                _memberImportServiceMock.Object,
                _configurationMock.Object,
                _monetaryTransactionsFileReadServiceMock.Object,
                _nonMonetaryTransactionsFileReadServiceMock.Object,
                _walletServiceMock.Object,
                _jobReportServiceMock.Object,
                _triviaImportServiceMock.Object,
                _taskImportService.Object,
                _cohortConsumerService.Object, _memberrImportFileDataServiceMock.Object,
                _jobHistoryService.Object,
                _questionnaireImportService.Object,
                _depositInstructionService.Object
                );
        }


        [Fact]
        public async System.Threading.Tasks.Task StartScanAndProcessFiles_ShouldReturn_WhenTenantNotFound()
        {
            // Arrange
            var etlExecutionContext = new EtlExecutionContext { TenantCode = "invalid", ScanS3FileTypes = "MEMBER_IMPORT" };
            _tenantRepoMock.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ETLTenantModel, bool>>>(), It.IsAny<bool>()));

            // Act
            await _s3FileProcessorCore.StartScanAndProcessFiles(etlExecutionContext);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(logLevel => logLevel == LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Invalid tenant code: invalid")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public async System.Threading.Tasks.Task StartScanAndProcessFiles_ShouldReturn_WhenNoFilesFound()
        {
            // Arrange
            var etlExecutionContext = new EtlExecutionContext { TenantCode = "valid", ScanS3FileTypes = "MEMBER_IMPORT" };
            _tenantRepoMock.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ETLTenantModel, bool>>>(), It.IsAny<bool>())).ReturnsAsync(new ETLTenantModel());
            _awsS3ServiceMock.Setup(x => x.GetAllFileNames(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new List<string>());

            // Act
            await _s3FileProcessorCore.StartScanAndProcessFiles(etlExecutionContext);

            // Assert
            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(logLevel => logLevel == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("StartScanAndProcessFiles : Nothing to process")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.AtLeastOnce);
        }

        [Fact]
        public async System.Threading.Tasks.Task StartScanAndProcessFiles_ShouldProcessMemberFiles()
        {
            // Arrange
            var etlExecutionContext = new EtlExecutionContext { TenantCode = "valid", ScanS3FileTypes = "MEMBER_TENANT_ENROLLMENT" };
            _tenantRepoMock.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ETLTenantModel, bool>>>(), It.IsAny<bool>())).ReturnsAsync(new ETLTenantModel());
            _awsS3ServiceMock.Setup(x => x.GetAllFileNames(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new List<string> { "memberFile1", "enrollmentFile1" });
            var consumerList = new List<ETLConsumerModel> { new ETLConsumerModel() };
            var personList = new List<ETLPersonModel> { new ETLPersonModel() };

            _enrollmentServiceMock.Setup(x => x.ProcessTenantEnrollments(It.IsAny<EtlExecutionContext>())).ReturnsAsync(() => (consumerList, personList));

            // Act
            await _s3FileProcessorCore.StartScanAndProcessFiles(etlExecutionContext);

            // Assert
            _enrollmentServiceMock.Verify(x => x.ProcessTenantEnrollments(It.IsAny<EtlExecutionContext>()), Times.AtLeastOnce);
            _awsS3ServiceMock.Verify(x => x.GetAllFileNames(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async System.Threading.Tasks.Task StartScanAndProcessFiles_ShouldProcessMemberImportFiles()
        {
            // Arrange
            var etlExecutionContext = new EtlExecutionContext { TenantCode = "valid", ScanS3FileTypes = "MEMBER_IMPORT" };
            _tenantRepoMock.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ETLTenantModel, bool>>>(), It.IsAny<bool>())).ReturnsAsync(new ETLTenantModel());
            _awsS3ServiceMock.Setup(x => x.GetAllFileNames(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new List<string> { "etl_member_import_File1" });
            var consumerList = new List<ETLConsumerModel> { new ETLConsumerModel() };
            var personList = new List<ETLPersonModel> { new ETLPersonModel() };

            _memberImportServiceMock.Setup(x => x.Import(It.IsAny<EtlExecutionContext>())).ReturnsAsync(() => (consumerList, personList));

            // Act
            await _s3FileProcessorCore.StartScanAndProcessFiles(etlExecutionContext);

            // Assert
            _awsS3ServiceMock.Verify(x => x.GetAllFileNames(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            _memberImportServiceMock.Verify(x => x.Import(It.IsAny<EtlExecutionContext>()), Times.Once);
        }

        [Fact]
        public async System.Threading.Tasks.Task StartScanAndProcessFiles_ShouldProcessPldFiles()
        {
            // Arrange
            var etlExecutionContext = new EtlExecutionContext { TenantCode = "valid", ScanS3FileTypes = "PROCESS_PLD" };
            _tenantRepoMock.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ETLTenantModel, bool>>>(), It.IsAny<bool>())).ReturnsAsync(new ETLTenantModel());
            _awsS3ServiceMock.Setup(x => x.GetAllFileNames(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new List<string> { "pldFile1" });


            // Act
            await _s3FileProcessorCore.StartScanAndProcessFiles(etlExecutionContext);

            // Assert
            _awsS3ServiceMock.Verify(x => x.GetAllFileNames(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            _awsS3ServiceMock.Verify(x => x.GetFileFromAwsS3(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async System.Threading.Tasks.Task StartScanAndProcessFiles_ShouldProcessConsumerAttributeFiles()
        {
            // Arrange
            var etlExecutionContext = new EtlExecutionContext { TenantCode = "valid", ScanS3FileTypes = "CONSUMER_ATTRIBUTES" };
            _tenantRepoMock.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ETLTenantModel, bool>>>(), It.IsAny<bool>())).ReturnsAsync(new ETLTenantModel());
            _awsS3ServiceMock.Setup(x => x.GetAllFileNames(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new List<string> { "consumer_attr" });
            var consumerList = new List<ETLConsumerModel> { new ETLConsumerModel() };

            _ingestConsumerAttrServiceMock.Setup(x => x.Ingest(It.IsAny<string>(), It.IsAny<byte[]>())).ReturnsAsync(consumerList);

            // Act
            await _s3FileProcessorCore.StartScanAndProcessFiles(etlExecutionContext);

            // Assert
            _awsS3ServiceMock.Verify(x => x.GetAllFileNames(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            _ingestConsumerAttrServiceMock.Verify(x => x.Ingest(It.IsAny<string>(), It.IsAny<byte[]>()), Times.Once);
        }

        [Fact]
        public async System.Threading.Tasks.Task StartScanAndProcessFiles_ShouldProcessTaskUpdateFiles()
        {
            // Arrange
            var etlExecutionContext = new EtlExecutionContext { TenantCode = "valid", ScanS3FileTypes = "TASK_UPDATE" };
            _tenantRepoMock.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ETLTenantModel, bool>>>(), It.IsAny<bool>())).ReturnsAsync(new ETLTenantModel());
            _awsS3ServiceMock.Setup(x => x.GetAllFileNames(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new List<string> { "task_update_file" });


            // Act
            await _s3FileProcessorCore.StartScanAndProcessFiles(etlExecutionContext);

            // Assert
            _awsS3ServiceMock.Verify(x => x.GetAllFileNames(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            _taskUpdateServiceMock.Verify(x => x.ProcessTaskUpdates(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<EtlExecutionContext>()), Times.Once);
        }
    }
}
