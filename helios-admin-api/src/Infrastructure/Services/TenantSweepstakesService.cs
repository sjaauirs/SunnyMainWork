using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Admin.Core.Domain.Constants;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Sweepstakes.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services
{
    public class TenantSweepstakesService : ITenantSweepstakesService
    {
        private readonly ILogger<TenantSweepstakesService> _logger;
        private readonly ISweepstakesClient _sweepstakesClient;
        private const string className = nameof(TenantSweepstakesService);
        public TenantSweepstakesService(ILogger<TenantSweepstakesService> logger, ISweepstakesClient sweepstakesClient)
        {
            _logger = logger;
            _sweepstakesClient = sweepstakesClient;
        }
        public async Task<BaseResponseDto> CreateTenantSweepStakes(TenantSweepstakesRequestDto tenantSweepstakesRequestDto)
        {
            const string methodName = nameof(CreateTenantSweepStakes);
            try
            {

                _logger.LogInformation("{ClassName}.{MethodName} - Started processing create tenant sweepstakes with SweepstakesCode:{Code},TenantCode:{Tenant}",
                        className, methodName, tenantSweepstakesRequestDto.SweepstakesCode, tenantSweepstakesRequestDto.TenantSweepstakes.TenantCode);

                var response = await _sweepstakesClient.Post<BaseResponseDto>(Constant.CreateTenantSweepStakesUrl, tenantSweepstakesRequestDto);

                if (response.ErrorCode != null)
                {
                    _logger.LogError("{ClassName}.{MethodName}: Error occurred during tenant sweepstakes creation. Request: {RequestData}, Response: {ResponseData}, ErrorCode: {ErrorCode}",
                        className, methodName, tenantSweepstakesRequestDto.TenantSweepstakes.ToJson(), response.ToJson(), response.ErrorCode);
                    return response;
                }


                _logger.LogInformation("{ClassName}.{MethodName} - Successfully created tenant sweepstakes with sweestakesCode:{Code},TenantCode:{Tenant}",
                    className, methodName, tenantSweepstakesRequestDto.SweepstakesCode, tenantSweepstakesRequestDto.TenantSweepstakes.TenantCode);

                return new BaseResponseDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Error occured while creating tenant sweepstakes with SweestakesCode:{Code},TenantCode:{TenantCode},ErrorCode:{Code},ERROR:{Msg}",
                        className, methodName, tenantSweepstakesRequestDto.SweepstakesCode, tenantSweepstakesRequestDto.TenantSweepstakes.TenantCode, StatusCodes.Status500InternalServerError, ex.Message);
                throw;
            }
        }
    }
}
