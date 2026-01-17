using Microsoft.Extensions.Logging;
using NHibernate;
using NHibernate.Linq;
using SunnyRewards.Helios.Common.Core.Repositories;
using SunnyRewards.Helios.Tenant.Core.Domain.Models;
using SunnyRewards.Helios.Tenant.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Tenant.Infrastructure.Services.Interfaces;

namespace SunnyRewards.Helios.Tenant.Infrastructure.Services
{
    public class WalletCategoryService : IWalletCategoryService
    {
        private readonly IWalletCategoryRepo _walletCategoryRepo;
        private readonly ILogger<WalletCategoryService> _logger;

        public WalletCategoryService(
            IWalletCategoryRepo walletCategoryRepo,
            ILogger<WalletCategoryService> logger)
        {
            _walletCategoryRepo = walletCategoryRepo;
            _logger = logger;
        }

        /// <summary>
        /// Get all wallet categories for a given tenant code.
        /// </summary>
        public async Task<IEnumerable<WalletCategoryModel>> GetByTenantCodeAsync(string tenantCode)
        {
            const string methodName = nameof(GetByTenantCodeAsync);
            try
            {
                _logger.LogInformation(
                    "{Service}.{Method}: Fetching wallet categories for tenantCode={TenantCode}",
                    nameof(WalletCategoryService), methodName, tenantCode);

                var results = await _walletCategoryRepo.GetByTenantCodeAsync(tenantCode);

                _logger.LogInformation(
                    "{Service}.{Method}: Retrieved {Count} wallet categories for tenantCode={TenantCode}",
                    nameof(WalletCategoryService), methodName, results?.Count() ?? 0, tenantCode);

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "{Service}.{Method}: Error fetching wallet categories for tenantCode={TenantCode}",
                    nameof(WalletCategoryService), methodName, tenantCode);
                throw;
            }
        }

        /// <summary>
        /// Get a wallet category by Id.
        /// </summary>
        public async Task<WalletCategoryModel?> GetByIdAsync(int id)
        {
            const string methodName = nameof(GetByIdAsync);
            try
            {
                _logger.LogInformation(
                    "{Service}.{Method}: Fetching wallet category with id={Id}",
                    nameof(WalletCategoryService), methodName, id);

                var result = await _walletCategoryRepo.FindOneAsync(
                    x => x.Id == id && x.DeleteNbr == 0
                );

                if (result == null)
                {
                    _logger.LogWarning(
                        "{Service}.{Method}: No wallet category found with id={Id}",
                        nameof(WalletCategoryService), methodName, id);
                }
                else
                {
                    _logger.LogInformation(
                        "{Service}.{Method}: Successfully retrieved wallet category with id={Id}",
                        nameof(WalletCategoryService), methodName, id);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "{Service}.{Method}: Error fetching wallet category with id={Id}",
                    nameof(WalletCategoryService), methodName, id);
                throw;
            }
        }

        /// <summary>
        /// Get a wallet category by tenantCode and walletId.
        /// </summary>
        public async Task<WalletCategoryModel?> GetByTenantAndWalletAsync(string tenantCode, int walletId)
        {
            const string methodName = nameof(GetByTenantAndWalletAsync);
            try
            {
                _logger.LogInformation(
                    "{Service}.{Method}: Fetching wallet category with tenantCode={TenantCode}, walletId={WalletId}",
                    nameof(WalletCategoryService), methodName, tenantCode, walletId);

                var result = await _walletCategoryRepo.GetByTenantAndWalletAsync(tenantCode, walletId);

                if (result == null)
                {
                    _logger.LogWarning(
                        "{Service}.{Method}: No wallet category found for tenantCode={TenantCode}, walletId={WalletId}",
                        nameof(WalletCategoryService), methodName, tenantCode, walletId);
                }
                else
                {
                    _logger.LogInformation(
                        "{Service}.{Method}: Successfully retrieved wallet category for tenantCode={TenantCode}, walletId={WalletId}",
                        nameof(WalletCategoryService), methodName, tenantCode, walletId);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "{Service}.{Method}: Error fetching wallet category for tenantCode={TenantCode}, walletId={WalletId}",
                    nameof(WalletCategoryService), methodName, tenantCode, walletId);
                throw;
            }
        }
    }
}