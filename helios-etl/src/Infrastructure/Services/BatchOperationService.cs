using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces;
using Newtonsoft.Json.Serialization;
using SunnyRewards.Helios.Etl.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos.Enums;
using Microsoft.AspNetCore.Http;
using SunnyRewards.Helios.ETL.Common.CustomException;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos.Json;
using SunnyRewards.Helios.Tenant.Core.Domain.Models;
using SunnyRewards.Helios.ETL.Common.Constants;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services
{
    public class BatchOperationService : IBatchOperationService
    {
        private readonly IBatchOperationRepo _batchOperationRepo;
        private readonly ILogger<BatchOperationService> _logger;
        private readonly ITenantRepo _tenantRepo;
        const string className = nameof(BatchOperationService);
        public BatchOperationService(ILogger<BatchOperationService> logger, IBatchOperationRepo batchOperationRepo, ITenantRepo tenantRepo)
        {
            _logger = logger;
            _batchOperationRepo = batchOperationRepo;
            _tenantRepo = tenantRepo;
        }

        public async Task<IList<EtlBatchOperationModel>> GetBatchOperationsRecords(string batchOperationGroupCode, List<BatchActions> actions)
        {
            var actionString = string.Join(',', actions.Select(s => s.ToString()));
            var results = await _batchOperationRepo.FindAsync(x => x.BatchOperationGroupCode == batchOperationGroupCode
           && actionString.Contains(x.BatchAction) && x.DeleteNbr == 0);
            return results;
        }


        public async Task SaveBatchOperationGenerateRecord(string batchOperationGroupCode, string storageName, string? folderName, string filename)
        {
            if (String.IsNullOrWhiteSpace(folderName))
            {
                folderName = "";
            }
            var generateAction = new GenerateActionDto
            {
                BatchAction = BatchActions.GENERATE.ToString(),
                Location = new ETLlocationDto(storageName, folderName, filename)
            };

            await SaveBatchOperation(batchOperationGroupCode, generateAction);
        }

        public async Task SaveBatchOperation(string batchOperationGroupCode, BatchActionDtoBase batchAction)
        {
            if (String.IsNullOrWhiteSpace(batchOperationGroupCode))
            {
                batchOperationGroupCode = "bgc-" + Guid.NewGuid().ToString("N");
            }
            else if (!batchOperationGroupCode.StartsWith("bgc-"))
            {
                batchOperationGroupCode = "bgc-" + batchOperationGroupCode;
            }

            var batchOperation = new EtlBatchOperationModel()
            {
                CreateUser = "ETL",
                CreateTs = DateTime.UtcNow,
                DeleteNbr = 0,
                BatchAction = batchAction.BatchAction,
                BatchOperationCode = "bat-" + Guid.NewGuid().ToString("N"),
                BatchOperationGroupCode = batchOperationGroupCode,
                action_description_json = JsonConvert.SerializeObject(batchAction, new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                })
            };
            var batchOperationResult = await _batchOperationRepo.CreateAsync(batchOperation);

            if (batchOperationResult != null)
            {
                _logger.LogInformation($"Batch Operation is recorded successfully in DB for batchGroupCode: {batchOperationGroupCode}");
            }
            else
            {
                _logger.LogError($"BatchOperationService -Batch Operation failed for batchGroupCode: {batchOperationGroupCode}");
            }

        }

        public async Task ValidateTenant(string tenantCode)
        {
            var methodName = nameof(ValidateTenant);

            // Check if a valid tenant code is provided
            if (string.IsNullOrEmpty(tenantCode))
            {
                var msg = $"{className}.{methodName}: No tenant code provided. Error Code:{StatusCodes.Status400BadRequest}";
                _logger.LogError(msg);
                throw new ETLException(ETLExceptionCodes.NullValue, " No tenant code provided");
            }

            // Retrieve tenant information
            var tenant = await _tenantRepo.FindOneAsync(x => x.TenantCode == tenantCode && x.DeleteNbr == 0);
            if (tenant == null)
            {
                var msg = $"{className}.{methodName}: Invalid tenant code: {tenantCode}, Error Code:{StatusCodes.Status404NotFound}";
                _logger.LogError(msg);
                throw new ETLException(ETLExceptionCodes.NotFoundInDb, $"Invalid tenant code: {tenantCode}");
            }
            if (tenant.TenantOption == null)
            {
                throw new ETLException(ETLExceptionCodes.NotFoundInDb, $"Invalid tenant Options: {tenantCode}");
            }
            // Deserialize tenant configuration
            var tenantOption = JsonConvert.DeserializeObject<TenantOption>(tenant.TenantOption);
            if(tenantOption==null || !tenantOption.Apps.Contains(Constants.Benefits))
            {
                throw new ETLException(ETLExceptionCodes.NotFoundInDb, $"Tenant Options Apps needs to be Benefits for card 30 request: {tenantCode}");

            }
        }

        public void ValidateBatchOperationGroupCode(EtlExecutionContext excontext)
        {
            if (string.IsNullOrEmpty(excontext.BatchOperationGroupCode))
            {
                var msg = $"Missing BatchOperationGroupCode cannot Encrypt/Copy/Archive ,tenantCode : {excontext.TenantCode}";
                _logger.LogError(msg);
                throw new ETLException(ETLExceptionCodes.NullValue, msg);
            }

            // Ensure BatchOperationGroupCode starts with "bgc-" and format the GUID part in "N" format
            excontext.BatchOperationGroupCode = "bgc-" + Guid.Parse(
                excontext.BatchOperationGroupCode.StartsWith("bgc-")
                ? excontext.BatchOperationGroupCode.Substring(4) : excontext.BatchOperationGroupCode).ToString("N");
        }

    }
}
