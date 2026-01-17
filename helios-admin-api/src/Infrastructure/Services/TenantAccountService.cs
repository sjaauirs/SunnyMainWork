using AutoMapper;
using Grpc.Net.Client.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SunnyBenefits.Fis.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Core.Domain.Constants;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Core.Domain.Models;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos.Json;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services
{
    public class TenantAccountService : ITenantAccountService
    {
        private readonly ILogger<TenantAccountService> _logger;
        private readonly IWalletClient _walletClient;
        private readonly IMapper _mapper;

        private readonly IFisClient _fisClient;
        private const string _className = nameof(TenantAccountService);
        public TenantAccountService(ILogger<TenantAccountService> logger, IWalletClient walletClient, IFisClient fisClient, IMapper mapper)
        {
            _logger = logger;
            _walletClient = walletClient;
            _fisClient = fisClient;
            _mapper = mapper;


        }
        /// <summary>
        /// Creates the tenant acount if not exists . if tenant account is created then creating the benitis wallets
        /// </summary>
        /// <param name="createTenantAccountRequestDto">The create request dto</param>
        /// <returns></returns>
        public async Task<BaseResponseDto> CreateTenantAccount(CreateTenantAccountRequestDto createTenantAccountRequestDto)
        {
            const string methodName = nameof(CreateTenantAccount);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName}: Create tenantaccount process started for TenantCode: {TenantCode}",
                    _className, methodName, createTenantAccountRequestDto.TenantAccount.TenantCode);

                // calling fis for tenantaccount creation
                var tenantResponse = await _fisClient.Post<BaseResponseDto>(Constant.CreateTenantAcountAPIUrl, createTenantAccountRequestDto);
                if (tenantResponse.ErrorCode != null)
                {
                    _logger.LogWarning("{ClassName}.{MethodName}: Error occurred while creating tenantaccount, TenantCode: {TenantCode}, ErrorCode: {ErrorCode},Error:{Msg}",
                        _className, methodName, createTenantAccountRequestDto.TenantAccount?.TenantCode, tenantResponse.ErrorCode, tenantResponse.ErrorMessage);
                    return tenantResponse;
                }

                _logger.LogInformation("{ClassName}.{MethodName}: Tenant account created successfully, TenantCode: {TenantCode}",
                _className, methodName, createTenantAccountRequestDto.TenantAccount?.TenantCode);


                var tenantAccountRequestDto = _mapper.Map<TenantAccountRequestDto>(createTenantAccountRequestDto.TenantAccount);
                var masterWalletResponseDto = await CreateMasterWallets(tenantAccountRequestDto, createTenantAccountRequestDto.CustomerCode, createTenantAccountRequestDto.SponsorCode, createTenantAccountRequestDto.TenantAccount.CreateUser);
                _logger.LogInformation("{ClassName}.{MethodName}: Tenant master wallets created successfully, TenantCode: {TenantCode}",
                    _className, methodName, createTenantAccountRequestDto.TenantAccount?.TenantCode);

                return new BaseResponseDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: Exception occurred while creating tenant or tenant master wallets for Rewards App. ErrorMessage: {ErrorMessage}, StackTrace: {StackTrace}",
                    _className, methodName, ex.Message, ex.StackTrace);
                throw;
            }
        }
        public async Task<GetTenantAccountResponseDto> GetTenantAccount(string tenantCode)
        {
            var parameters = new Dictionary<string, string>();
            return await _fisClient.GetId<GetTenantAccountResponseDto>($"{Constant.TenantAccount}/{tenantCode}", parameters);
        }
         public async Task<BaseResponseDto> CreateMasterWallets(TenantAccountRequestDto tenantAccountRequestDto,string customerCode,string sponsorCode, string createUser)
        {
            const string methodName = nameof(CreateMasterWallets);

            var tenantConfig = tenantAccountRequestDto.TenantConfigJson != null ?
                      JsonConvert.DeserializeObject<TenantConfig>(tenantAccountRequestDto.TenantConfigJson) : new TenantConfig();

            var createMasterWalletsRequest = new CreateTenantMasterWalletsRequestDto
            {
                CustomerCode = customerCode,
                SponsorCode = sponsorCode,
                TenantCode = tenantAccountRequestDto.TenantCode,
                Apps = [Constant.Apps.Benefits],
                PurseConfig = tenantConfig?.PurseConfig,
                CreateUser = createUser
            };

            // calling for creating benifits wallets
            var createMasterWalletsResponse = await _walletClient.Post<BaseResponseDto>(Constant.CreateTenantMasterWalletsAPIUrl, createMasterWalletsRequest);
            if (createMasterWalletsResponse.ErrorCode != null)
            {
                _logger.LogWarning("{ClassName}.{MethodName}: Error occurred while creating tenant master wallets, TenantCode: {TenantCode}, ErrorCode: {ErrorCode}",
                    _className, methodName, tenantAccountRequestDto?.TenantCode, createMasterWalletsResponse.ErrorCode);
                return createMasterWalletsResponse;
            }
            return new BaseResponseDto();

        }

        public async Task<TenantAccountUpdateResponseDto> UpdateTenantAccount(string tenantCode, TenantAccountRequestDto tenantAccountDto)
        {
            return await _fisClient.Put<TenantAccountUpdateResponseDto>($"{Constant.TenantAccount}/{tenantCode}", tenantAccountDto);
        }

        public async Task<BaseResponseDto> SaveTenantAccount(TenantAccountRequestDto tenantAccountDto)
        {
            return await _fisClient.Post<BaseResponseDto>(Constant.TenantAccount, tenantAccountDto);
        }
    }
}
