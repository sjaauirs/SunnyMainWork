using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using SunnyRewards.Helios.ETL.Common.Helpers;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using SunnyRewards.Helios.ETL.Core.Domain.Constants.FIS;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos.FIS;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using SunnyRewards.Helios.ETL.Infrastructure.Helpers;
using SunnyRewards.Helios.ETL.Infrastructure.Helpers.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Services.FIS;
using SunnyRewards.Helios.Tenant.Core.Domain.Models;
using Xunit;
using SunnyRewards.Helios.ETL.Common.Helpers.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Services;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos.Json;
using AutoMapper;
using SunnyRewards.Helios.ETL.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.HttpClients;

namespace SunnyRewards.Helios.ETL.UnitTests
{
    public class ConsumerNonMonetaryTransactionsFileReadServiceTests
    {
        private readonly IFlatFileReader _FlatFileReader;
        private readonly Mock<ILogger<ConsumerNonMonetaryTransactionsFileReadService>> _mockLogger;
        private readonly Mock<IPgpS3FileEncryptionHelper> _mockS3FileEncryptionHelper;
        private readonly Mock<ITenantRepo> _mockTenantRepo;
        private readonly Mock<ITenantAccountRepo> _mockTenantAccountRepo;
        private readonly Mock<IConsumerAccountRepo> _mockConsumerAccountRepo;
        private readonly Mock<IVault> _mockVault;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<IJobReportService> _mockJobReportService;
        private readonly ConsumerNonMonetaryTransactionsFileReadService _service;
        private readonly Mock<IS3Helper> _s3Helper;
        private readonly Mock<IAdminClient> _adminClient;

        public ConsumerNonMonetaryTransactionsFileReadServiceTests()
        {
            _FlatFileReader = new FlatFileReader();
            _mockLogger = new Mock<ILogger<ConsumerNonMonetaryTransactionsFileReadService>>();
            _mockS3FileEncryptionHelper = new Mock<IPgpS3FileEncryptionHelper>();
            _mockTenantRepo = new Mock<ITenantRepo>();
            _mockTenantAccountRepo = new Mock<ITenantAccountRepo>();
            _mockConsumerAccountRepo = new Mock<IConsumerAccountRepo>();
            _mockVault = new Mock<IVault>();
            _mockConfiguration = new Mock<IConfiguration>();
            _s3Helper= new Mock<IS3Helper>();
            _mockJobReportService = new Mock<IJobReportService>();
            _adminClient = new Mock<IAdminClient>();

            _service = new ConsumerNonMonetaryTransactionsFileReadService(
                _FlatFileReader,
                _mockLogger.Object,
                _s3Helper.Object,
                _mockTenantRepo.Object,
                _mockVault.Object,
                _mockConfiguration.Object,
                _mockS3FileEncryptionHelper.Object,
                _mockConsumerAccountRepo.Object,
                _mockTenantAccountRepo.Object, _mockJobReportService.Object,
                _adminClient.Object
            );
        }

        [Fact]
        public async System.Threading.Tasks.Task ImportConsumerNonMonetaryTransactions_InvalidTenant_Returns()
        {
            // Arrange
            var etlExecutionContext = new EtlExecutionContext
            {
                TenantCode = "InvalidTenantCode",
                ConsumerNonMonetaryTransactionsFileName = "testfile.txt"
            };
            _mockJobReportService.Setup(x => x.BatchJobRecords).Returns(new BatchJobRecordsDto { JobType = nameof(CohortConsumerService) });
            _mockJobReportService.Setup(x => x.JobResultDetails).Returns(new JobResultDetails { Files = new List<string>() });

            _mockTenantRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ETLTenantModel, bool>>>(), false));

            // Act
            await _service.ImportConsumerNonMonetaryTransactions(etlExecutionContext);

            // Assert
            _mockLogger.Verify(x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Invalid tenant code")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async System.Threading.Tasks.Task ImportConsumerNonMonetaryTransactions_ValidTenant_ProcessesFile()
        {
            // Arrange
            var etlExecutionContext = new EtlExecutionContext
            {
                TenantCode = "ValidTenantCode",
                ConsumerNonMonetaryTransactionsFileName = "testfile.txt"
            };
            _mockJobReportService.Setup(x => x.BatchJobRecords).Returns(new BatchJobRecordsDto { JobType = nameof(CohortConsumerService) });
            _mockJobReportService.Setup(x => x.JobResultDetails).Returns(new JobResultDetails { Files = new List<string>() });

            var fileReadString = "D|1234500|EFPS|121528|Crimson Training|504115|Hybrid Reloadable|873343|Hybrid Instant Issue|468203|USD|840|182503|EFPSTestPackage2|******2120090841|******2120091234|06152345|1|CSAFNm|CSALNm|A|Addr1Value|Addr2Value|Sunrise|IA|4323|FLCountry|840|6534234567|3333|06152023|06152025|5|BuxxCard|1700|170|06152023 12:34:56|||||||||||||||||||6902031683149|||||||||\r\nD|1234500|EFPS|121528|Crimson Training|504115|Hybrid Reloadable|873343|Hybrid Instant Issue|468203|USD|840|182503|EFPSTestPackage2|******2120090841|******2120093456|06154567|1|CSAFNm|CSALNm|A|Addr1Value|Addr2Value|Sunrise|IA|4323|FLCountry|840|6534234567|3333|06152023|06152025|5|BuxxCard|1700|170|06152023 12:34:56|||||||||||||||||||3719316982204|||||||||\r\nD|1234501|EFPS2|121529|Crimson Learning|504116|Hybrid Non-Reloadable|873343|Hybrid Issue|468204|EUR|978|182504|EFPSTestPackage3|******2120090842|******2120090842|06252023|2|CSBFNm|CSBLNm|B|Addr1Value2|Addr2Value2|Sunset|TX|4324|TXCountry|841|6544234567|3334|06252023|06252025|6|BuxxCard2|1701|171|06252023 13:45:67|||||||||||||||||||1234434|||||||||\r\nD|1234500|EFPS3|121530|Crimson Testing|504117|Hybrid Temporary|873343|Hybrid Quick Issue|468205|GBP|826|182505|EFPSTestPackage4|******2120090843|******2120090843|07302023|3|CSCFNm|CSCLNm|C|Addr1Value3|Addr2Value3|Moonrise|CA|4325|CACountry|842|6554234567|3335|07302023|07302025|7|BuxxCard3|1702|172|07302023 14:56:78|||||||||||||||||||1234434|||||||||\r\n";

            var tenantModel = new ETLTenantModel { TenantCode = "ValidTenantCode", DeleteNbr = 0 };
            var tenantAccountModel = new ETLTenantAccountModel { TenantConfigJson = JsonConvert.SerializeObject(new FISTenantConfigDto { FISProgramDetail = new FISProgramDetailDto { CompanyId = "1234500", SubprogramId = "873343", PackageId = "182503", ClientId = "121528" } }) };
            var proxyNumbers = new List<string?> { "6902031683149", "3719316982204", "9902031683149", "999999999999", "88888888888" };
            var response = Encoding.UTF8.GetBytes(fileReadString);

            var configSectionMock = new Mock<IConfigurationSection>();
            configSectionMock.Setup(x => x.Value).Returns("SECRET");

            _mockConfiguration.Setup(x => x.GetSection(It.IsAny<string>())).Returns(configSectionMock.Object);
            _mockTenantRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ETLTenantModel, bool>>>(), false))
                .ReturnsAsync(tenantModel);
            _mockTenantAccountRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ETLTenantAccountModel, bool>>>(), false))
                .ReturnsAsync(tenantAccountModel);
            _mockConsumerAccountRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ETLConsumerAccountModel, bool>>>(), false))
                .ReturnsAsync(new ETLConsumerAccountModel() { ProxyNumber = "6902031683149" });
            _mockS3FileEncryptionHelper.Setup(x => x.DownloadAndDecryptFile(It.IsAny<SecureFileTransferRequestDto>()))
                .ReturnsAsync(response);
            _s3Helper.Setup(x => x.MoveFileToFolder(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));

            // Act
            await _service.ImportConsumerNonMonetaryTransactions(etlExecutionContext);

            // Assert
            // 2 records have correct client Id and SubProgram Id and matching Proxy number - update will called twice
            _mockConsumerAccountRepo.Verify(x => x.UpdateAsync( It.IsAny<ETLConsumerAccountModel>()), Times.Exactly(2));
        }

        [Fact]
        public async System.Threading.Tasks.Task ImportConsumerNonMonetaryTransactions_With_Unique_Client_Id()
        {
            // Arrange
            var etlExecutionContext = new EtlExecutionContext
            {
                TenantCode = "ValidTenantCode",
                ConsumerNonMonetaryTransactionsFileName = "testfile.txt"
            };
            _mockJobReportService.Setup(x => x.BatchJobRecords).Returns(new BatchJobRecordsDto { JobType = nameof(CohortConsumerService) });
            _mockJobReportService.Setup(x => x.JobResultDetails).Returns(new JobResultDetails { Files = new List<string>() });

            var fileReadString = "D|1204185|Sunny Benefits Inc|1234500|Sunny Healthplan UAT Demo|1717022118|Sunny Healthplan UAT Demo|873343|Sunny Healthplan UAT PPO|471928|USD|840|721736|Test Package1|4719280000026353|4719280000026353|Null|1|TomMY|Johnson|Null|123 Main Street|test|Nashua|NH|03062|United States|840|1234567890|Null|11212024|05312030|4|Insurance|1530|12|11212024 16:34:20|Null|Null|Null|Null|11251985|Null|Null|tomMYjohnson@test.com|Null|Null|Null|Null|*****0608|Null|Ready for activation|3|11212024|05312030|3808265117356|Null|Null|Null|Null|Null|Null|Null|Null|Null|Null|Null|Null|Null|Null|Null|Null|Null|Null|Null|Null|Null|Null|Null|Null|Null|Self|Create a new account|Manual batch loader|0|allapp|InternalUse|AllApp|1103151919|300042546|Null|Sunny Healthplan UAT PPO|Null|Null|Null|Null|Null|Null|Null|Null|Null|Null|Null|Null|Null|Null|3808265117356|Null|Null|Null|Null|Null|Null|Null|11212024|Null|Null|Null|Null|Null|Null|Null|Null|Null|Null|Null|Null|Null|Null|Null|Null|Null|Null|Null|Null|Null|Null|Null|Null|Null|Null|Null|Null|Null|Null|Null|Null|Null|Null|0|Undefined (catch all)|Null||||";

            var tenantModel = new ETLTenantModel { TenantCode = "ValidTenantCode", DeleteNbr = 0 };
            var tenantAccountModel = new ETLTenantAccountModel { TenantConfigJson = JsonConvert.SerializeObject(new FISTenantConfigDto { FISProgramDetail = new FISProgramDetailDto { CompanyId = "1204185", SubprogramId = "873343", PackageId = "721736", ClientId = "1234500" } }) };
            var response = Encoding.UTF8.GetBytes(fileReadString);

            var configSectionMock = new Mock<IConfigurationSection>();
            configSectionMock.Setup(x => x.Value).Returns("SECRET");

            _mockConfiguration.Setup(x => x.GetSection(It.IsAny<string>())).Returns(configSectionMock.Object);
            _mockTenantRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ETLTenantModel, bool>>>(), false))
                .ReturnsAsync(tenantModel);
            _mockTenantAccountRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ETLTenantAccountModel, bool>>>(), false))
                .ReturnsAsync(tenantAccountModel);
            _mockConsumerAccountRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ETLConsumerAccountModel, bool>>>(), false))
                .ReturnsAsync(new ETLConsumerAccountModel() { ClientUniqueId = "300042546" });
            _mockS3FileEncryptionHelper.Setup(x => x.DownloadAndDecryptFile(It.IsAny<SecureFileTransferRequestDto>()))
                .ReturnsAsync(response);
            _s3Helper.Setup(x => x.MoveFileToFolder(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));

            // Act
            await _service.ImportConsumerNonMonetaryTransactions(etlExecutionContext);

            // Assert
            // 2 records have correct client Id and SubProgram Id and matching Proxy number - update will called twice
            _mockConsumerAccountRepo.Verify(x => x.UpdateAsync(It.IsAny<ETLConsumerAccountModel>()), Times.Exactly(1));
        }
    }
}
