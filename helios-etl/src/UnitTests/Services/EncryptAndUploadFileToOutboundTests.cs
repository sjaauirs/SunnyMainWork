using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using SunnyRewards.Helios.ETL.Infrastructure.Helpers.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Services;
using SunnyRewards.Helios.ETL.Infrastructure.Services.FIS;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.ETL.UnitTests.Services
{
    public class EncryptAndUploadFileToOutboundTests
    {
        private Mock<ILogger<EncryptAndUploadFileToOutboundService>> _loggerMock;
        private Mock<ITenantRepo> _tenantRepoMock;
        private Mock<IPgpS3FileEncryptionHelper> _s3FileEncryptionHelperMock;
        private Mock<IConfiguration> _configurationMock;
        private Mock<IVault> _vaultMock;
        private EncryptAndUploadFileToOutboundService _encryptAndUploadFileToOutbound;
        private Mock <IBatchOperationService> _batchOperationService;

        public EncryptAndUploadFileToOutboundTests()
        {
           _loggerMock = new Mock<ILogger<EncryptAndUploadFileToOutboundService>>();
           _s3FileEncryptionHelperMock = new Mock<IPgpS3FileEncryptionHelper>();
           _tenantRepoMock = new Mock<ITenantRepo>();
           _configurationMock = new Mock<IConfiguration>();
           _vaultMock = new Mock<IVault>();
            _batchOperationService = new Mock<IBatchOperationService>();
            _encryptAndUploadFileToOutbound = new EncryptAndUploadFileToOutboundService(_loggerMock.Object, _s3FileEncryptionHelperMock.Object
               , _tenantRepoMock.Object, _configurationMock.Object, _vaultMock.Object , _batchOperationService.Object);
        }
        [Fact]
        public async System.Threading.Tasks.Task EncryptAndCopyToOutbound_ValidTenant_Success()
        {
            var etlExecutionContext = new EtlExecutionContext
            {
                TenantCode = "validTenantCode"
            };
            var tenant = new ETLTenantModel { TenantCode = "validTenantCode", DeleteNbr = 0 };
            _tenantRepoMock.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<ETLTenantModel, bool>>>(), false))
                .ReturnsAsync(tenant);


            _s3FileEncryptionHelperMock.Setup(x => x.EncryptGeneratedFile(etlExecutionContext.BatchOperationGroupCode, etlExecutionContext.TenantCode,
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));
            var configSectionMock = new Mock<IConfigurationSection>();
            configSectionMock.Setup(x => x.Value).Returns("SECRET");

            _configurationMock.Setup(x => x.GetSection(It.IsAny<string>())).Returns(configSectionMock.Object);
            // Act
            await _encryptAndUploadFileToOutbound.EncryptAndCopyToOutbound(etlExecutionContext);

            _s3FileEncryptionHelperMock.Verify(
               x => x.EncryptGeneratedFile(etlExecutionContext.BatchOperationGroupCode, etlExecutionContext.TenantCode,
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
               Times.Once);

        }

    }
}
