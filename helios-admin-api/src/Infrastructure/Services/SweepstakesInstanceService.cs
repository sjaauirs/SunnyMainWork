using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Admin.Core.Domain.Constants;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Sweepstakes.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services
{
    public class SweepstakesInstanceService : ISweepstakesInstanceService
    {
        private readonly ILogger<SweepstakesInstanceService> _logger;
        private readonly ISweepstakesClient _sweepstakesClient;

        public SweepstakesInstanceService(ILogger<SweepstakesInstanceService> logger, ISweepstakesClient sweepstakesClient)
        {
            _logger = logger;
            _sweepstakesClient = sweepstakesClient;
        }
        public async Task<SweepstakesInstanceResponseDto> CreateSweepstakesInstance(SweepstakesInstanceRequestDto requestDto)
        {
            try
            {
                var responseDto = await _sweepstakesClient.Post<SweepstakesInstanceResponseDto>(Constant.SweepstakesInstanceUrl, requestDto);
                if (responseDto == null || responseDto.SweepstakesInstanceId <= 0)
                {
                    var errorMessage = $"Sweepstakes Instance creation failed. SweepstakesId: {requestDto.SweepstakesId}";
                    _logger.LogError(errorMessage);
                    return new SweepstakesInstanceResponseDto
                    {
                        ErrorCode = StatusCodes.Status500InternalServerError,
                        ErrorMessage = errorMessage
                    };
                }
                if (responseDto.ErrorCode == null)
                    _logger.LogInformation($"CreateSweepstakesInstance: Sweepstakes Instance created successfully.SweepstakesId: {requestDto.SweepstakesId}");
                  else
                    _logger.LogError($"CreateSweepstakesInstance: Sweepstakes Instance creation failed. request: {requestDto.ToJson()}, response: {responseDto.ToJson()}");
                return responseDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateSweepstakesInstance: Error:{Message}", ex.Message);
                return new SweepstakesInstanceResponseDto
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <summary>
        /// Get sweepstakes instance by sweepstakes instance code
        /// </summary>
        /// <param name="sweepstakesInstanceCode"></param>
        /// <returns></returns>
        public async Task<SweepstakesInstanceDto> GetSweepstakesInstance(string sweepstakesInstanceCode)
        {
            try
            {
                var url = $"{Constant.SweepstakesInstanceGetUrl}?sweepstakesInstanceCode={sweepstakesInstanceCode}";
                var responseDto = await _sweepstakesClient.Get<SweepstakesInstanceDto>(url, null);
                if (responseDto == null || responseDto.SweepstakesInstanceId <= 0)
                {
                    var errorMessage = $"Sweepstakes Instance not found with sweepstakes instance code: {sweepstakesInstanceCode}";
                    _logger.LogError(errorMessage);
                    return new SweepstakesInstanceDto
                    {
                        ErrorCode = StatusCodes.Status500InternalServerError,
                        ErrorMessage = errorMessage
                    };
                }
                if (responseDto.ErrorCode == null)
                    _logger.LogInformation($"GetSweepstakesInstance: fetching Sweepstakes Instance successfully.Sweepstakes instance code: {sweepstakesInstanceCode}");
                else
                    _logger.LogError($"GetSweepstakesInstance: Error occurred during fetching sweepstakes instance for sweepstakes instance code: {sweepstakesInstanceCode}, response: {responseDto.ToJson()}");
                return responseDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetSweepstakesInstance: Error:{Message}", ex.Message);
                return new SweepstakesInstanceDto
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = ex.Message
                };
            }
        }
    }
}
