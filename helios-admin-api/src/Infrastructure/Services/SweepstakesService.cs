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
    public class SweepstakesService : ISweepstakesService
    {
        private readonly ILogger<SweepstakesService> _logger;
        private readonly ISweepstakesClient _sweepstakesClient;
        private const string className = nameof(SweepstakesService);
        public SweepstakesService(ILogger<SweepstakesService> logger, ISweepstakesClient sweepstakesClient)
        {
            _logger = logger;
            _sweepstakesClient = sweepstakesClient;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sweepstakesRequestDto"></param>
        /// <returns></returns>

        public async Task<BaseResponseDto> CreateSweepstakes(SweepstakesRequestDto sweepstakesRequestDto)
        {
            const string methodName = nameof(CreateSweepstakes);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName} - Started processing create sweepstakes with sweestakesCode:{Code},SweepStakesName:{Name}",
                        className, methodName, sweepstakesRequestDto.SweepstakesCode, sweepstakesRequestDto.SweepstakesName);

                var response = await _sweepstakesClient.Post<BaseResponseDto>(Constant.CreateSweepStakesUrl, sweepstakesRequestDto);

                if (response.ErrorCode != null)
                {
                    _logger.LogError("{ClassName}.{MethodName}: Error occurred during Sweepstakes creation. Request: {RequestData}, Response: {ResponseData}, ErrorCode: {ErrorCode}",
                        className, methodName, sweepstakesRequestDto.ToJson(), response.ToJson(), response.ErrorCode);
                    return response;
                }

                _logger.LogInformation("{ClassName}.{MethodName} - Successfully created sweepstakes with sweestakesCode:{Code},SweepStakesName:{Name}",
                        className, methodName, sweepstakesRequestDto.SweepstakesCode, sweepstakesRequestDto.SweepstakesName);

                return new BaseResponseDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Error occured while creating sweepstakes with sweestakesCode:{Code},SweepStakesName:{Name},ErrorCode:{ErrorCode},ERROR:{Msg}",
                        className, methodName, sweepstakesRequestDto.SweepstakesCode, sweepstakesRequestDto.SweepstakesName, StatusCodes.Status500InternalServerError, ex.Message);
                throw;

            }
        }

        public async Task<UpdateSweepstakesResponseDto> UpdateSweepStakes(UpdateSweepstakesRequestDto updateSweepstakesRequestDto)
        {
            const string methodName = nameof(UpdateSweepStakes);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName} - Started processing update sweepstakes with sweestakesCode:{Code},SweepStakesName:{Name}",
                        className, methodName, updateSweepstakesRequestDto.SweepstakesCode, updateSweepstakesRequestDto.SweepstakesName);

                var response = await _sweepstakesClient.Put<UpdateSweepstakesResponseDto>(Constant.UpdateSweepStakesUrl, updateSweepstakesRequestDto);

                if (response.ErrorCode != null)
                {
                    _logger.LogError("{ClassName}.{MethodName}: Error occurred during Sweepstakes update. Request: {RequestData}, Response: {ResponseData}, ErrorCode: {ErrorCode}",
                        className, methodName, updateSweepstakesRequestDto.ToJson(), response.ToJson(), response.ErrorCode);
                    return response;
                }

                _logger.LogInformation("{ClassName}.{MethodName} - Successfully updated sweepstakes with sweestakesCode:{Code},SweepStakesName:{Name}",
                        className, methodName, updateSweepstakesRequestDto.SweepstakesCode, updateSweepstakesRequestDto.SweepstakesName);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Error occured while updating sweepstakes with sweestakesCode:{Code},SweepStakesName:{Name},ErrorCode:{ErrorCode},ERROR:{Msg}",
                        className, methodName, updateSweepstakesRequestDto.SweepstakesCode, updateSweepstakesRequestDto.SweepstakesName, StatusCodes.Status500InternalServerError, ex.Message);
                throw;

            }
        }
    }
}
