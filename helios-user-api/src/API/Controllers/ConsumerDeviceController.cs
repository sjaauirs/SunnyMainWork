using Microsoft.AspNetCore.Mvc;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Infrastructure.Services.Interfaces;

namespace SunnyRewards.Helios.User.Api.Controllers
{
    [Route("api/v1/")]
    [ApiController]
    public class ConsumerDeviceController : ControllerBase
    {
        private readonly ILogger<ConsumerDeviceController> _logger;
        private readonly IConsumerDeviceService _consumerDeviceService;
        private const string className = nameof(ConsumerDeviceController);
        public ConsumerDeviceController(ILogger<ConsumerDeviceController> logger, IConsumerDeviceService consumerDeviceService)
        {
            _logger = logger;
            _consumerDeviceService = consumerDeviceService;
        }

        /// <summary>
        /// Handles the creation of a consumer device.
        /// Logs the process and manages responses based on the service result.
        /// </summary>
        /// <param name="postConsumerDeviceRequestDto">The request data containing details about the consumer device to be created.</param>
        /// <returns>
        /// A status code response:
        /// - 200 (OK) if the device is created successfully.
        /// - Appropriate error code if there is an issue during the creation.
        /// - 500 (Internal Server Error) if an unexpected exception occurs.
        /// </returns>
        /// <remarks>
        /// This method uses logging to track the process and outputs errors for debugging purposes.
        /// </remarks>
        [HttpPost("create-consumer-device")]
        public async Task<ActionResult<BaseResponseDto>> CreateConsumerDevice(PostConsumerDeviceRequestDto postConsumerDeviceRequestDto)
        {
            const string methodName = nameof(CreateConsumerDevice);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName} -  Started processing create consumer device with TenantCode:{Code},ConsumerCode:{Consumer}",
                   className, methodName, postConsumerDeviceRequestDto.TenantCode, postConsumerDeviceRequestDto.ConsumerCode);

                var response = await _consumerDeviceService.CreateConsumerDevice(postConsumerDeviceRequestDto);

                if (response.ErrorCode != null)
                {
                    _logger.LogError("{ClassName}.{MethodName}: Error occurred while inserting consumer device. Request: {RequestData}, Response: {ResponseData}, ErrorCode: {ErrorCode}",
                        className, methodName, postConsumerDeviceRequestDto.ToJson(), response.ToJson(), response.ErrorCode);
                    return StatusCode((int)response.ErrorCode, response);
                }
                _logger.LogInformation("{ClassName}.{MethodName} -  Consumer device created sucessfully with TenantCode:{Code},ConsumerCode:{Consumer}",
                  className, methodName, postConsumerDeviceRequestDto.TenantCode, postConsumerDeviceRequestDto.ConsumerCode);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Error occured while processing  create consumer device with TenantCode:{Code},ConsumerCode:{Consumer},ERROR:{Msg}",
                    className, methodName, postConsumerDeviceRequestDto.TenantCode, postConsumerDeviceRequestDto.ConsumerCode, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError });
            }
        }
        /// <summary>
        /// Retrieves consumer device details based on the provided tenant and consumer codes.
        /// </summary>
        /// <param name="getConsumerDeviceRequestDto">
        /// The request data containing the tenant code and consumer code to fetch the consumer devices.
        /// </param>
        /// <returns>
        /// A status code response:
        /// - 200 (OK) if the consumer devices fetched successfully with list of consumer devices.
        /// - Appropriate error code if there is an issue during the creation.
        /// - 500 (Internal Server Error) if an unexpected exception occurs.
        /// </returns>
        /// <remarks>
        /// Logs the start, success, and error events for the operation. Handles both service-level errors and unexpected exceptions.
        /// Returns appropriate HTTP status codes based on the result:
        /// </remarks>

        [HttpPost("get-consumer-devices")]
        public async Task<ActionResult<GetConsumerDeviceResponseDto>> GetConsumerDevices(GetConsumerDeviceRequestDto getConsumerDeviceRequestDto)
        {
            const string methodName = nameof(CreateConsumerDevice);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName} -  Started fetching consumer devices with TenantCode:{Code},ConsumerCode:{Consumer}",
                        className, methodName, getConsumerDeviceRequestDto.TenantCode, getConsumerDeviceRequestDto.ConsumerCode);

                var response = await _consumerDeviceService.GetConsumerDevices(getConsumerDeviceRequestDto);

                if (response.ErrorCode != null)
                {
                    _logger.LogError("{ClassName}.{MethodName}: Error occurred while fectching consumer devices. Request: {RequestData}, Response: {ResponseData}, ErrorCode: {ErrorCode}",
                        className, methodName, getConsumerDeviceRequestDto.ToJson(), response.ToJson(), response.ErrorCode);
                    return StatusCode((int)response.ErrorCode, response);
                }
                _logger.LogInformation("{ClassName}.{MethodName} -  Consumer devices fetched sucessfully with TenantCode:{Code},ConsumerCode:{Consumer}",
                  className, methodName, getConsumerDeviceRequestDto.TenantCode, getConsumerDeviceRequestDto.ConsumerCode);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Error occured while fetching consumer devices with TenantCode:{Code},ConsumerCode:{Consumer},ERROR:{Msg}",
                    className, methodName, getConsumerDeviceRequestDto.TenantCode, getConsumerDeviceRequestDto.ConsumerCode, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError });
            }
        }
    }
}
