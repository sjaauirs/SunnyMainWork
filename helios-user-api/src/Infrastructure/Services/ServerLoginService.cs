using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Common.Core.Domain;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Common.Core.Services;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Models;
using SunnyRewards.Helios.User.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.User.Infrastructure.Services.Interfaces;
using System.Data;

namespace SunnyRewards.Helios.User.Infrastructure.Services
{
    public class ServerLoginService : BaseService, IServerLoginService
    {
        private readonly ILogger<ServerLoginService> _logger;
        private readonly NHibernate.ISession _session;
        private readonly IServerLoginRepo _serverLoginRepo;
        public ServerLoginService(
          ILogger<ServerLoginService> logger,
          NHibernate.ISession session,
          IServerLoginRepo serverLoginRepo)
        {
            _logger = logger;
            _session = session;
            _serverLoginRepo = serverLoginRepo;
        }
        const string className = nameof(ServerLoginService);
        /// <summary>
        /// Creates an API token for the specified server login request.
        /// </summary>
        /// <param name="serverLoginRequestDto"></param>
        /// <returns></returns>
        public async Task<ServerLoginResponseDto> CreateApiToken(ServerLoginRequestDto serverLoginRequestDto)
        {
            const string methodName = nameof(CreateApiToken);
            try
            {
                if (string.IsNullOrEmpty(serverLoginRequestDto.TenantCode))
                {
                    _logger.LogError("{className}.{methodName}: ERROR - Tenant Not Found, because tenant code is null or empty for Tenant code:{tenant} , Error Code:{errorCode}", className, methodName, serverLoginRequestDto.TenantCode, StatusCodes.Status400BadRequest);
                    return new ServerLoginResponseDto() { ErrorCode = 400, ErrorMessage = "Please provide a valid Tenant Code" };
                }
                    
                var serverLoginModel = await _serverLoginRepo.FindAsync(x => x.TenantCode == serverLoginRequestDto.TenantCode && x.DeleteNbr == 0);
                var latestServerLogin = serverLoginModel?.OrderByDescending(x => x.ServerLoginId).FirstOrDefault();

                if (latestServerLogin != null && IsTokenValid(latestServerLogin))
                {
                    // Return existing ApiToken if not expired
                    latestServerLogin.UpdateTs = DateTime.UtcNow;
                    await _session.UpdateAsync(latestServerLogin);
                    _logger.LogInformation("{className}.{methodName}: Valid ApiToken Found for Tenant Code: {tenant}", className,methodName, latestServerLogin.TenantCode);

                    return new ServerLoginResponseDto() { ApiToken = latestServerLogin.ApiToken };
                }
                else
                {
                    using var transaction = _session.BeginTransaction();
                    try
                    {
                        // Soft delete and re-create ApiToken if expired
                        if (latestServerLogin != null)
                        {
                            latestServerLogin.DeleteNbr = latestServerLogin.ServerLoginId;
                            await _session.UpdateAsync(latestServerLogin);
                        }

                        // Create and Save ApiToken
                        var apiToken = Guid.NewGuid().ToString("N");
                        await CreateServerLogin(serverLoginRequestDto.TenantCode, apiToken);
                        await transaction.CommitAsync();
                        return new ServerLoginResponseDto { ApiToken = apiToken };
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "{className}.{methodName}: Error occurred while creating API Token, error:{ErrorMessage}",className, methodName, ex.Message);
                        transaction.Rollback();
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{className}.{methodName}: Error occurred while creating API Token", className, methodName);
                throw;
            }
        }

        private async Task CreateServerLogin(string tenantCode, string apiToken)
        {
            var serverLogin = new ServerLoginModel()
            {
                TenantCode = tenantCode,
                LoginTs = DateTime.UtcNow,
                RefreshTokenTs = DateTime.UtcNow,
                ApiToken = apiToken,
                CreateTs = DateTime.UtcNow,
                CreateUser = Constants.CreateUser,
                DeleteNbr = 0
            };
            await _session.SaveAsync(serverLogin);
            _logger.LogInformation($"{className}.CreateServerLogin: with server login model ={serverLogin.ToJson()}");
        }

        private static bool IsTokenValid(ServerLoginModel serverLogin)
        {
            var isTokenValid = (DateTime.UtcNow - serverLogin.RefreshTokenTs).TotalSeconds < Constants.USER_JWT_EXPIRY_SECONDS;
            return isTokenValid;
        }
    }
}
