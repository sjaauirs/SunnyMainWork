using Microsoft.AspNetCore.Mvc;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Api.Controllers
{
    [Route("api/v1/")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly ILogger<CustomerController> _customerLogger;
        public readonly ICustomerService _customerService;

        const string className = nameof(CustomerController);
        private const string ErrorLogTemplate = "{className}.{methodName}: ERROR Msg:{msg}, Error Code:{errorCode}";
        public CustomerController(ILogger<CustomerController> customerLogger, ICustomerService customerService)
        {
            _customerLogger = customerLogger;
            _customerService = customerService;
        }

        /// <summary>
        /// Retrieves all customers from the database.
        /// Logs the operation progress and handles errors if they occur.
        /// </summary>
        /// <returns>
        /// An ActionResult containing either a successful response with a list of customers 
        /// </returns>
        [HttpGet("customers")]
        public async Task<ActionResult<CustomersReponseDto>> GetCustomers()
        {
            const string methodName = nameof(GetCustomers);
            try
            {
                _customerLogger.LogInformation("{ClassName}.{MethodName}: API Started fetching all customers", className, methodName);
                var response = await _customerService.GetCustomers();
                if (response.ErrorCode != null)
                {
                    var errorCode = response.ErrorCode;
                    _customerLogger.LogError("{className}.{methodName}: API - Error occurred while processing all customers, Error Code: {ErrorCode}, Error Msg: {msg}", className, methodName, response.ErrorCode, response.ErrorMessage);
                    return StatusCode((int)errorCode, response);
                }
                _customerLogger.LogInformation("{className}.{methodName}: API - Successfully fetched all customers", className, methodName);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _customerLogger.LogError(ex, ErrorLogTemplate, className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, new CustomersReponseDto() { ErrorCode = StatusCodes.Status500InternalServerError, ErrorMessage = ex.Message });
            }
        }

        /// <summary>
        /// Creates a new customer in the database.
        /// Logs the operation progress, including request details and success/failure states.
        /// </summary>
        /// <param name="requestDto">The request data containing the details of the customer to be created 
        /// </param>
        /// <returns>
        /// An IActionResult containing either a successful response with customer creation details or an error response with a status code and message.
        /// </returns>
        [HttpPost("customer")]
        public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerDto requestDto)
        {
            const string methodName = nameof(CreateCustomer);
            try
            {
                _customerLogger.LogInformation("{ClassName}.{MethodName}: Request started with CustomerCode: {Customer}", className, methodName, requestDto.CustomerCode);

                var response = await _customerService.CreateCustomer(requestDto);

                if (response.ErrorCode != null)
                {
                    _customerLogger.LogError("{ClassName}.{MethodName}: Error occurred while creating Customer. Request: {RequestData}, Response: {ResponseData}, ErrorCode: {ErrorCode}", className, methodName, requestDto.ToJson(), response.ToJson(), response.ErrorCode);
                    return StatusCode((int)response.ErrorCode, response);
                }

                _customerLogger.LogInformation("{ClassName}.{MethodName}: Customer created successful, CustomerCode: {CustomerCode}", className, methodName, requestDto.CustomerCode);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _customerLogger.LogError(ex, "{ClassName}.{MethodName}: An error occurred while create Customer. Error Message: {ErrorMessage}, ErrorCode: {ErrorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError });
            }
        }

        /// <summary>
        /// Get tenant sponsor customer by tenant code
        /// </summary>
        /// <param name="tenantCode"></param>
        /// <returns></returns>
        [HttpGet("tenant-sponsor-customer/{tenantCode}")]
        public async Task<ActionResult<TenantSponsorCustomerResponseDto>> GetTenantSponsorCustomer(string tenantCode)
        {
            const string methodName = nameof(GetTenantSponsorCustomer);
            try
            {
                _customerLogger.LogInformation("{className}.{methodName}: API - Started With Tenant : {tenantCode}", className, methodName, tenantCode);
                var response = await _customerService.GetTenantSponsorCustomer(tenantCode);
                if (response.ErrorCode != null)
                {
                    var errorCode = response.ErrorCode;
                    _customerLogger.LogError("{className}.{methodName}: API - Error occurred while processing product, Request Data: {request}, Error Code: {ErrorCode}, Error Msg: {msg}", className, methodName, tenantCode, response.ErrorCode, response.ErrorMessage);
                    return StatusCode((int)errorCode, response);
                }

                _customerLogger.LogInformation("{className}.{methodName}: API - Successfully fetched customer For Tenant Code:{tenantCode}", className, methodName, tenantCode);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _customerLogger.LogError(ex, ErrorLogTemplate, className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, new CustomerResponseDto() { ErrorMessage = ex.Message });
            }
        }
    }
}
