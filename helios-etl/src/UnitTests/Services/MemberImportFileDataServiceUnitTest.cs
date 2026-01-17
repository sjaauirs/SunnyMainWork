using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NHibernate;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using SunnyRewards.Helios.ETL.Infrastructure.AwsConfig;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Services;
using SunnyRewards.Helios.ETL.Infrastructure.Services.FIS;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.User.Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using static SunnyRewards.Helios.ETL.Core.Domain.Dtos.PreRunValidationJson;
using static SunnyRewards.Helios.ETL.UnitTests.FlatFileGeneratorTests;

namespace SunnyRewards.Helios.ETL.UnitTests.Services
{
    public class MemberImportFileDataServiceUnitTest
    {
        private Mock<ILogger<MemberImportFileDataService>> _loggerMock;
        private Mock<ITenantRepo> _tenantRepoMock;
        private Mock<IConsumerRepo> _consumerRepoMock;
        private Mock<IPersonRepo> _personRepoMock;
        private Mock<IETLMemberImportFileDataRepo> _MemberImportFileDataRepoMock;
        private Mock<IETLMemberImportFileRepo> _MemberImportFileDataMock;
        private readonly Mock<IVault> _mockVault;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private Mock<IAwsS3Service> _s3ServiceMock;
        private Mock<ISession> _session;
        private readonly Mock<BatchJobReportValidationJson> _mockBatchJobReportValidationJson;

        private MemberImportFileDataService memberImportFileDataService;

        public MemberImportFileDataServiceUnitTest()
        {
            _mockVault = new Mock<IVault>();
            _loggerMock = new Mock<ILogger<MemberImportFileDataService>>();
            _mockConfiguration = new Mock<IConfiguration>();
            _consumerRepoMock = new Mock<IConsumerRepo>();
            _tenantRepoMock = new Mock<ITenantRepo>();
            _personRepoMock = new Mock<IPersonRepo>();
            _MemberImportFileDataRepoMock = new Mock<IETLMemberImportFileDataRepo>();
            _MemberImportFileDataMock = new Mock<IETLMemberImportFileRepo>();
            _s3ServiceMock = new Mock<IAwsS3Service>();
            _session = new Mock<ISession>();
            _mockBatchJobReportValidationJson = new Mock<BatchJobReportValidationJson>();



            memberImportFileDataService = new MemberImportFileDataService(_mockVault.Object, _loggerMock.Object, _mockConfiguration.Object, _tenantRepoMock.Object, _consumerRepoMock.Object, _personRepoMock.Object, _session.Object
               , _MemberImportFileDataRepoMock.Object, _MemberImportFileDataMock.Object, _s3ServiceMock.Object);
        }

        [Fact]
        public async System.Threading.Tasks.Task SaveMemberImportFileData_FileContentsProvided_SavesData()
        {
            // Arrange
            var fileContents = new byte[] { 1, 2, 3 }; // Replace with realistic test data
            var etlExecutionContext = new EtlExecutionContext
            {
                MemberImportFilePath = null,
                MemberImportFileContents = fileContents
            };

            var mockTransaction = new Mock<ITransaction>();
            _session.Setup(s => s.BeginTransaction()).Returns(mockTransaction.Object);
            _session.Setup(s => s.SaveAsync(It.IsAny<ETLMemberImportFileModel>(),default)).ReturnsAsync(1);
            _session.Setup(s => s.SaveAsync(It.IsAny<ETLMemberImportFileDataModel>(), default)).ReturnsAsync(1);

            // Act
            var result = await memberImportFileDataService.saveMemberImportFileData(etlExecutionContext);

            // Assert
            Assert.Equal(1, result.Item1);
            Assert.True(result.Item2);
        }

        [Fact]
        public async System.Threading.Tasks.Task GetMemberImportFileDataRecords_ShouldReturnValidRecords_WhenRecordsExist()
        {
            // Arrange
            long memberImportFileId = 1;

            // Mock _memberImportFileDataRepo.FindAsync method to return a list of file data
            var fileDataList = new List<ETLMemberImportFileDataModel>
        {
            new ETLMemberImportFileDataModel { RecordNumber = 1, MemberImportFileId = memberImportFileId },
            new ETLMemberImportFileDataModel { RecordNumber = 2, MemberImportFileId = memberImportFileId }
        };
            var tenant = new ETLTenantModel { TenantCode = "validTenantCode", DeleteNbr = 0 };
            _MemberImportFileDataRepoMock
                .Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<ETLMemberImportFileDataModel, bool>>>(), false))
                .ReturnsAsync(fileDataList);
            _tenantRepoMock.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<ETLTenantModel, bool>>>(), false))
                .ReturnsAsync(tenant);
            _personRepoMock.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<ETLPersonModel, bool>>>(), false))
                .ReturnsAsync(It.IsAny<ETLPersonModel>());
            _consumerRepoMock.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<ETLConsumerModel, bool>>>(), false))
               .ReturnsAsync(It.IsAny<ETLConsumerModel>());
            // Mock the _batchJobReportValidationJson.preRun object to avoid null reference errors

            // Mock ProcessCount to avoid calling the actual method

            int skip = 0;
            int take = 100;
            // Act
            var result = await memberImportFileDataService.GetMemberImportFileDataRecords(memberImportFileId, take);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            _MemberImportFileDataRepoMock.Verify(repo => repo.FindAsync(It.IsAny<Expression<Func<ETLMemberImportFileDataModel, bool>>>(),false), Times.Once);
        }
        [Fact]
        public async System.Threading.Tasks.Task GetMemberImportFileDataRecords_ShouldReturnEmptyList_WhenNoRecordsFound()
        {
            // Arrange
            long memberImportFileId = 1;

            // Mock _memberImportFileDataRepo.FindAsync method to return an empty list


            _MemberImportFileDataRepoMock
             .Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<ETLMemberImportFileDataModel, bool>>>(), false))
            .ReturnsAsync(new List<ETLMemberImportFileDataModel>());
            var tenant = new ETLTenantModel { TenantCode = "validTenantCode", DeleteNbr = 0 };

            _tenantRepoMock.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<ETLTenantModel, bool>>>(), false))
                .ReturnsAsync(tenant);
            _personRepoMock.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<ETLPersonModel, bool>>>(), false))
                .ReturnsAsync(It.IsAny<ETLPersonModel>());
            _consumerRepoMock.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<ETLConsumerModel, bool>>>(), false))
               .ReturnsAsync(It.IsAny<ETLConsumerModel>());
            // Mock the _batchJobReportValidationJson.preRun object to avoid null reference errors


            // Act
            int skip = 0;
            int take = 100;
            var result = await memberImportFileDataService.GetMemberImportFileDataRecords(memberImportFileId, take);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result); // Expect empty list when no records found
        }
        [Fact]
        public async System.Threading.Tasks.Task GetMemberImportFileDataRecords_ShouldLogError_WhenExceptionOccurs()
        {
            // Arrange
            long memberImportFileId = 1;

            // Mock _memberImportFileDataRepo.FindAsync method to throw an exception

            _MemberImportFileDataRepoMock
             .Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<ETLMemberImportFileDataModel, bool>>>(), false))
                .ThrowsAsync(new Exception("Database error"));

            var tenant = new ETLTenantModel { TenantCode = "validTenantCode", DeleteNbr = 0 };

            _tenantRepoMock.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<ETLTenantModel, bool>>>(), false))
                .ReturnsAsync(tenant);
            _personRepoMock.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<ETLPersonModel, bool>>>(), false))
                .ReturnsAsync(It.IsAny<ETLPersonModel>());
            _consumerRepoMock.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<ETLConsumerModel, bool>>>(), false))
               .ReturnsAsync(It.IsAny<ETLConsumerModel>());
            // Mock the _batchJobReportValidationJson.preRun object to avoid null reference errors

            // Act
            int skip = 0;
            int take = 100;
            var result = await memberImportFileDataService.GetMemberImportFileDataRecords(memberImportFileId, take);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result); // Expect empty list after exception
        }

    }
}
