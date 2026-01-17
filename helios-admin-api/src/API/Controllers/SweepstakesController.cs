using Microsoft.AspNetCore.Mvc;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Sweepstakes.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Api.Controllers
{
    [Route("api/v1/sweepstakes")]
    [ApiController]
    public class SweepstakesController : ControllerBase
    {
        private readonly ILogger<SweepstakesController> _logger;
        private readonly ISweepstakesService _sweepstakesService;
        private const string className = nameof(SweepstakesController);
        public SweepstakesController(ILogger<SweepstakesController> logger, ISweepstakesService sweepstakesService)
        {
            _logger = logger;
            _sweepstakesService = sweepstakesService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateSweepstakes([FromBody] SweepstakesRequestDto sweepstakesRequestDto)
        {
            const string methodName = nameof(CreateSweepstakes);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName} - Started processing create sweepstakes with sweestakesCode:{Code},SweepStakesName:{Name}",
                        className, methodName, sweepstakesRequestDto.SweepstakesCode, sweepstakesRequestDto.SweepstakesName);

                var response = await _sweepstakesService.CreateSweepstakes(sweepstakesRequestDto);

                if (response.ErrorCode != null)
                {
                    _logger.LogError("{ClassName}.{MethodName}: Error occurred during Sweepstakes creation. Request: {RequestData}, Response: {ResponseData}, ErrorCode: {ErrorCode}",
                        className, methodName, sweepstakesRequestDto.ToJson(), response.ToJson(), response.ErrorCode);
                    return StatusCode((int)response.ErrorCode, response);
                }

                _logger.LogInformation("{ClassName}.{MethodName} - Successfully created sweepstakes with sweestakesCode:{Code},SweepStakesName:{Name}",
                        className, methodName, sweepstakesRequestDto.SweepstakesCode, sweepstakesRequestDto.SweepstakesName);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Error occured while creating sweepstakes with sweestakesCode:{Code},SweepStakesName:{Name},ErrorCode:{ErrorCode},ERROR:{Msg}",
                        className, methodName, sweepstakesRequestDto.SweepstakesCode, sweepstakesRequestDto.SweepstakesName, StatusCodes.Status500InternalServerError, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponseDto { ErrorCode = StatusCodes.Status500InternalServerError });
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateSweepstakes([FromBody] UpdateSweepstakesRequestDto updateSweepstakesRequestDto)
        {
            const string methodName = nameof(UpdateSweepstakes);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName} - Started processing create sweepstakes with sweepstakesCode:{Code},SweepStakesName:{Name}",
                        className, methodName, updateSweepstakesRequestDto.SweepstakesCode, updateSweepstakesRequestDto.SweepstakesName);

                var response = await _sweepstakesService.UpdateSweepStakes(updateSweepstakesRequestDto);

                if (response.ErrorCode != null)
                {
                    _logger.LogError("{ClassName}.{MethodName}: Error occurred during Sweepstakes creation. Request: {RequestData}, Response: {ResponseData}, ErrorCode: {ErrorCode}",
                        className, methodName, updateSweepstakesRequestDto.ToJson(), response.ToJson(), response.ErrorCode);
                    return StatusCode((int)response.ErrorCode, response);
                }

                _logger.LogInformation("{ClassName}.{MethodName} - Successfully updated sweepstakes with sweepstakesCode:{Code},SweepStakesName:{Name}",
                        className, methodName, updateSweepstakesRequestDto.SweepstakesCode, updateSweepstakesRequestDto.SweepstakesName);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Error occured while updating sweepstakes with sweepstakesCode:{Code},SweepStakesName:{Name},ErrorCode:{ErrorCode},ERROR:{Msg}",
                        className, methodName, updateSweepstakesRequestDto.SweepstakesCode, updateSweepstakesRequestDto.SweepstakesName, StatusCodes.Status500InternalServerError, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new UpdateSweepstakesResponseDto { ErrorCode = StatusCodes.Status500InternalServerError });
            }
        }
    }
}
