using Microsoft.Extensions.Logging;
using Sunny.Benefits.Bff.Infrastructure.HttpClients.Interfaces;
using Sunny.Benefits.Bff.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;
namespace Sunny.Benefits.Bff.Infrastructure.Services
{
    public class WalletCategoryService : IWalletCategoryService
    {
        private readonly ILogger<WalletCategoryService> _logger;
        private readonly ITenantClient _tenantClient;
        private const string className = nameof(WalletCategoryService);

        public WalletCategoryService(
            ILogger<WalletCategoryService> logger,
            ITenantClient tenantClient)
        {
            _logger = logger;
            _tenantClient = tenantClient;
        }

        public async Task<IEnumerable<WalletCategoryResponseDto>> GetByTenant(string tenantCode)
        {
            const string methodName = nameof(GetByTenant);
            _logger.LogInformation("{ClassName}.{MethodName} : Started for TenantCode:{TenantCode}",
                className, methodName, tenantCode);

            try
            {
                var response = await _tenantClient.Get<IEnumerable<WalletCategoryResponseDto>>(
                    $"wallet-category/tenant/{tenantCode}",
                    new Dictionary<string, long>());

                return response ?? new List<WalletCategoryResponseDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} : Failed for TenantCode:{TenantCode}, {error}",
                    className, methodName, tenantCode, ex.Message);

                return new List<WalletCategoryResponseDto>();
            }
        }

        public async Task<WalletCategoryResponseDto?> GetById(long id)
        {
            const string methodName = nameof(GetById);
            _logger.LogInformation("{ClassName}.{MethodName} : Started for Id:{Id}",
                className, methodName, id);

            try
            {
                var response = await _tenantClient.Get<WalletCategoryResponseDto>(
                    $"wallet-category/{id}",
                    new Dictionary<string, long>());

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} : Failed for Id:{Id} , {error}",
                    className, methodName, id, ex.Message);

                return null;
            }
        }

        public async Task<IEnumerable<WalletCategoryResponseDto>> GetByTenantAndWallet(string tenantCode, long walletTypeId)
        {
            const string methodName = nameof(GetByTenantAndWallet);
            _logger.LogInformation("{ClassName}.{MethodName} : Started for TenantCode:{TenantCode}, WalletTypeId:{WalletTypeId}",
                className, methodName, tenantCode, walletTypeId);

            try
            {
                var response = await _tenantClient.Get<IEnumerable<WalletCategoryResponseDto>>(
                    $"wallet-category/tenant/{tenantCode}/wallet/{walletTypeId}",
                    new Dictionary<string, long>());

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} : Failed for TenantCode:{TenantCode}, WalletTypeId:{WalletTypeId} , {error}",
                    className, methodName, tenantCode, walletTypeId, ex.Message);

                return new List<WalletCategoryResponseDto>();
            }
        }
    }
}
