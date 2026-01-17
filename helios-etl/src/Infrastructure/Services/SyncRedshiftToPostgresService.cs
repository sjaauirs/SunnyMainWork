using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.ETL.Common.Constants;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services
{
    public class SyncRedshiftToPostgresService : ISyncRedshiftToPostgresService
    {
        private readonly ILogger<SyncRedshiftToPostgresService> _logger;
        private readonly ISyncMembersFromRedshiftToPostgresService _membersSyncService;
        public SyncRedshiftToPostgresService(
            ILogger<SyncRedshiftToPostgresService> logger,
            ISyncMembersFromRedshiftToPostgresService membersSyncService)
        {
            _logger = logger;
            _membersSyncService = membersSyncService;
        }
        public async Task SyncAsync(EtlExecutionContext etlExecutionContext)
        {
            _logger.LogInformation("Starting Redshift to Postgres sync.");

            try
            {
                var syncType = etlExecutionContext?.SyncDataType;
                if (string.IsNullOrEmpty(syncType))
                {
                    _logger.LogError("SyncDataType is null or empty. Aborting sync operation.");
                    return;
                }

                if (string.Equals(syncType, Constants.RedshiftSyncMemberImportDataType, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogInformation("Syncing member import data from Redshift to Postgres.");
                    await _membersSyncService.SyncAsync(etlExecutionContext);
                }
                else
                {
                    _logger.LogWarning("Unrecognized SyncDataType '{SyncType}'. No sync operation executed.", syncType);
                }

                // Placeholder: other sync operations can go here in the future.
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during Redshift to Postgres sync.");
                throw;
            }
        }
    }
}
