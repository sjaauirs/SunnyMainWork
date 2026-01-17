using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SunnyBenefits.Fis.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Common.CustomException;
using SunnyRewards.Helios.ETL.Core.Domain.Constants;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos.Enums;
using SunnyRewards.Helios.ETL.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services
{
    public class TenantConfigSyncService : ITenantConfigSyncService
    {
        private readonly ILogger<TenantConfigSyncService> _logger;
        private readonly IJobReportService _jobReportService;
        private readonly IAdminClient _adminClient;
        private readonly IWalletSyncService _walletSyncService;
        private readonly IConsumerAccountSyncService _consumerAccountSyncService;
        private const string className = nameof(TenantConfigSyncService);

        public TenantConfigSyncService(ILogger<TenantConfigSyncService> logger, IJobReportService jobReportService,
             IAdminClient adminClient, IWalletSyncService walletSyncService, IConsumerAccountSyncService consumerAccountSyncService)
        {
            _logger = logger;
            _jobReportService = jobReportService;
            _adminClient = adminClient;
            _walletSyncService = walletSyncService;
            _consumerAccountSyncService = consumerAccountSyncService;
        }

        /// <summary>
        /// Sync the tenant config options
        /// </summary>
        /// <param name="etlExecutionContext">The etl execution context.</param>
        /// <exception cref="SunnyRewards.Helios.ETL.Common.CustomException.ETLException">
        /// No tenant code supplied.
        /// or
        /// No syncTenantConfigOption supplied.
        /// or
        /// Tenant not found in DB with tenant_code: {tenantCode}.
        /// or
        /// TenantAccount not found in DB with tenant_code: {tenantCode}.
        /// </exception>
        public async Task SyncAsync(EtlExecutionContext etlExecutionContext)
        {
            const string methodName = nameof(SyncAsync);
            _logger.LogInformation($"{className}.{methodName}: Starting tenant config sync for TenantCode: {etlExecutionContext.TenantCode}");
            try
            {
                var tenantCode = etlExecutionContext.TenantCode;
                var syncTenantConfigOption = etlExecutionContext.SyncTenantConfigOptions;
                if (string.IsNullOrEmpty(tenantCode))
                {
                    _logger.LogError("{ClassName}.{MethodName}  - No tenant code supplied.", className, methodName);
                    throw new ETLException(ETLExceptionCodes.NullValue, "No tenant code supplied.");
                }
                if (string.IsNullOrEmpty(syncTenantConfigOption))
                {
                    _logger.LogError("{ClassName}.{MethodName}  - No syncTenantConfigOption supplied.", className, methodName);
                    throw new ETLException(ETLExceptionCodes.NullValue, "No syncTenantConfigOption supplied.");
                }

                var tenant = await GetTenantDetails(tenantCode);
                if (tenant == null || tenant.ErrorCode != null || tenant.Tenant == null)
                {
                    _logger.LogError("{ClassName}.{MethodName}  - Invalid tenant code supplied.", className, methodName);
                    throw new ETLException(ETLExceptionCodes.NotFoundInDb, $"Tenant not found in DB with tenant_code: {tenantCode}.");
                }

                var tenantAccount = await GetTenantAccount(tenantCode);
                if (tenantAccount == null || tenantAccount.ErrorCode != null || tenantAccount.TenantAccount == null)
                {
                    _logger.LogError("{ClassName}.{MethodName}  - TenantAccount not found.", className, methodName);
                    throw new ETLException(ETLExceptionCodes.NotFoundInDb, $"TenantAccount not found in DB with tenant_code: {tenantCode}.");
                }
                if (syncTenantConfigOption.Contains(TenantSyncOption.WALLET.ToString()))
                {
                   await _walletSyncService.SyncWalletsForTenantAsync(tenant.Tenant, tenantAccount.TenantAccount, etlExecutionContext.ConsumerCodes);
                }

                if (syncTenantConfigOption.Contains(TenantSyncOption.CONSUMER_ACCOUNT_CONFIG.ToString()))
                {
                    await _consumerAccountSyncService.SyncConsumerAccountAsync(tenant.Tenant, tenantAccount.TenantAccount, etlExecutionContext.ConsumerCodes);
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}  - Failed processing tenant config sync. ErrorCode:{Code},ERROR: {Message}", className, methodName, StatusCodes.Status500InternalServerError, ex.Message);
                throw;
            }
        }

        private async Task<TenantResponseDto> GetTenantDetails(string tenantCode)
        {
            IDictionary<string, long> parameters = new Dictionary<string, long>();
            return await _adminClient.Get<TenantResponseDto>($"{AdminConstants.GetTenant}?tenantCode={tenantCode}", parameters);
        }

        private async Task<GetTenantAccountResponseDto> GetTenantAccount(string tenantCode)
        {
            IDictionary<string, long> parameters = new Dictionary<string, long>();
            return await _adminClient.Get<GetTenantAccountResponseDto>($"{AdminConstants.TenantAccount}/{tenantCode}", parameters);
        }

    }
}
