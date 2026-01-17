using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Admin.Core.Domain.Constants;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Cohort.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Models;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services
{
    public class TenantService : ITenantService
    {
        public readonly ILogger<TenantService> _logger;
        private readonly ITenantClient _tenantClient;
        private readonly IWalletClient _walletClient;
        private readonly IUserClient _userClient;
        private const string _className = nameof(TenantService);
        public TenantService(ILogger<TenantService> logger, ITenantClient tenantClient, IWalletClient walletClient , IUserClient userClient)
        {
            _logger = logger;
            _tenantClient = tenantClient;
            _walletClient = walletClient;
            _userClient = userClient;
        }

        /// <summary>
        /// Creates a new tenant and its associated master wallets.
        /// </summary>
        /// <param name="createTenantRequest">The request data for creating a tenant.</param>
        /// <returns></returns>
        public async Task<BaseResponseDto> CreateTenant(CreateTenantRequestDto createTenantRequest)
        {
            const string methodName = nameof(CreateTenant);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName}: Create tenant process started for TenantCode: {TenantCode}", _className, methodName, createTenantRequest.Tenant.TenantCode);

                var tenantResponse = await _tenantClient.Post<BaseResponseDto>(Constant.CreateTenantAPIUrl, createTenantRequest);
                if (tenantResponse.ErrorCode != null)
                {
                    _logger.LogWarning("{ClassName}.{MethodName}: Error occurred while creating tenant, TenantCode: {TenantCode}, ErrorCode: {ErrorCode}", _className, methodName, createTenantRequest.Tenant.TenantCode, tenantResponse.ErrorCode);
                    return tenantResponse;
                }

                _logger.LogInformation("{ClassName}.{MethodName}: Tenant created successfully, TenantCode: {TenantCode}", _className, methodName, createTenantRequest.Tenant.TenantCode);

                var createMasterWalletsRequest = new CreateTenantMasterWalletsRequestDto
                {
                    CustomerCode = createTenantRequest.CustomerCode,
                    SponsorCode = createTenantRequest.SponsorCode,
                    TenantCode = createTenantRequest.Tenant.TenantCode,
                    CreateUser = createTenantRequest.Tenant.CreateUser ?? string.Empty,
                    Apps = [Constant.Apps.Rewards]
                };

                var createMasterWalletsResponse = await _walletClient.Post<BaseResponseDto>(Constant.CreateTenantMasterWalletsAPIUrl, createMasterWalletsRequest);
                if (createMasterWalletsResponse.ErrorCode != null)
                {
                    _logger.LogWarning("{ClassName}.{MethodName}: Error occurred while creating tenant master wallets, TenantCode: {TenantCode}, ErrorCode: {ErrorCode}", _className, methodName, createTenantRequest.Tenant.TenantCode, createMasterWalletsResponse.ErrorCode);
                    return createMasterWalletsResponse;
                }

                _logger.LogInformation("{ClassName}.{MethodName}: Tenant master wallets created successfully, TenantCode: {TenantCode}", _className, methodName, createTenantRequest.Tenant.TenantCode);

                return new BaseResponseDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: Exception occurred while creating tenant or tenant master wallets for Rewards App. ErrorMessage: {ErrorMessage}, StackTrace: {StackTrace}", _className, methodName, ex.Message, ex.StackTrace);
                throw;
            }
        }
        /// <summary>
        /// Retrieves consumer and person details based on the tenant code provided.
        /// </summary>
        /// <param name="consumerByTenantRequestDto">Contains tenant code, search term, and pagination parameters.</param>
        /// <returns>A paginated list of consumer and person details that match the search criteria.</returns>
        /// <remarks>This method performs optional search, and pagination.</remarks>
        public async Task<ConsumersAndPersonsListResponseDto> GetConsumersByTenantCode(GetConsumerByTenantRequestDto consumerByTenantRequestDto)
        {
            const string methodName = nameof(GetConsumersByTenantCode);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName}: Get Consumer started for TenantCode: {TenantCode}", _className, methodName, consumerByTenantRequestDto.TenantCode);
                var tenantConsumers = await _userClient.Post<ConsumersAndPersonsListResponseDto>("consumer/get-consumers-by-tenant-code", consumerByTenantRequestDto);
                if (tenantConsumers.ErrorCode != null)
                {
                    _logger.LogWarning("{ClassName}.{MethodName}: Error occurred while  Get Consumer started for TenantCode: {TenantCode}, ErrorCode: {ErrorCode}", _className, methodName, consumerByTenantRequestDto.TenantCode, tenantConsumers.ErrorCode);
                    return tenantConsumers;
                }

                _logger.LogInformation("{ClassName}.{MethodName}: Get data successfully, TenantCode: {TenantCode}", _className, methodName, consumerByTenantRequestDto.TenantCode);
                return tenantConsumers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: Exception occurred while creating cohort. ErrorMessage: {ErrorMessage}, StackTrace: {StackTrace}", _className, methodName, ex.Message, ex.StackTrace);
                throw;
            }
        }

        public async Task<TenantsResponseDto> GetTenants()
        {
            IDictionary<string, long> parameters = new Dictionary<string, long>();
            return await _tenantClient.Get<TenantsResponseDto>(Constant.Tenants, parameters);
        }

        public async Task<UpdateTenantResponseDto> UpdateTenant(string tenantCode, UpdateTenantDto updateTenantRequest)
        {
            return await _tenantClient.Put<UpdateTenantResponseDto>($"{Constant.UpdateTenant}/{tenantCode}", updateTenantRequest);
        }

        public async Task<TenantResponseDto> GetTenantDetails(string tenantCode)
        {
            IDictionary<string, long> parameters = new Dictionary<string, long>();
            return await _tenantClient.Get<TenantResponseDto>($"{Constant.GetTenant}?tenantCode={tenantCode}", parameters);
        }
    }
}
