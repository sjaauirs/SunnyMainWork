using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Infrastructure.AwsConfig;
using SunnyRewards.Helios.ETL.Infrastructure.Helpers.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces.FIS;
using Constants = SunnyRewards.Helios.ETL.Common.Constants.Constants;
using Microsoft.Extensions.Configuration;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using Microsoft.AspNetCore.Http;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services.FIS
{
    public class EncryptAndUploadFileToOutboundService : AwsConfiguration, IEncryptAndUploadFileToOutboundService
    {
        private readonly ILogger<EncryptAndUploadFileToOutboundService> _logger;
        private readonly IPgpS3FileEncryptionHelper _s3FileEncryptionHelper;
        private readonly ITenantRepo _tenantRepo;
        private readonly IBatchOperationService _batchOperationService;
        const string className = nameof(EncryptAndUploadFileToOutboundService);

        public EncryptAndUploadFileToOutboundService(ILogger<EncryptAndUploadFileToOutboundService> logger, IPgpS3FileEncryptionHelper s3FileEncryptionHelper
            , ITenantRepo tenantRepo, IConfiguration configuration, IVault vault, IBatchOperationService batchOperationService) : base(vault, configuration)
        {
            _logger = logger;
            _s3FileEncryptionHelper = s3FileEncryptionHelper;
            _tenantRepo = tenantRepo;
            _batchOperationService = batchOperationService;
        }

        public async Task EncryptAndCopyToOutbound(EtlExecutionContext etlExecutionContext)
        {
            const string methodName = nameof(EncryptAndCopyToOutbound);
            _logger.LogInformation($"{className}.{methodName}: Starting to Encript and move the file to Outbound Folder...");
            try
            {
                await _batchOperationService.ValidateTenant(etlExecutionContext.TenantCode);
                var TargetFileName = etlExecutionContext.FISEncryptAndCopyFileName + ".pgp";
                
                await _batchOperationService.SaveBatchOperationGenerateRecord(etlExecutionContext.BatchOperationGroupCode, GetAwsTmpS3BucketName(), null, etlExecutionContext.FISEncryptAndCopyFileName);

                if (!etlExecutionContext.BatchOperationGroupCode.StartsWith("bgc-"))
                {
                    etlExecutionContext.BatchOperationGroupCode = "bgc-" + etlExecutionContext.BatchOperationGroupCode;
                }

                await _s3FileEncryptionHelper.EncryptGeneratedFile(etlExecutionContext.BatchOperationGroupCode, etlExecutionContext.TenantCode,
                    GetAwsFisSftpS3BucketName(), Constants.FIS_OUTBOUND_FOLDER, TargetFileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{className}.{methodName}: Error occurred while processing encryption and uploading to outbound., Error: {Message}", className, methodName, ex.Message);
                throw;
            }

        }
    }
}
