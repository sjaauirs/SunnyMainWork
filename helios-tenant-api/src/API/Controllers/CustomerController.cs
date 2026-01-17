using Microsoft.AspNetCore.Mvc;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;
using SunnyRewards.Helios.Tenant.Infrastructure.Services.Interfaces;

namespace SunnyRewards.Helios.Tenant.Api.Controllers
{
    [Route("api/v1/customer")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly ILogger<CustomerController> _customerLogger;
        public readonly ICustomerService _customerService;
        private const string ErrorLogTemplate = "{className}.{methodName}: ERROR Msg:{msg}, Error Code:{errorCode}";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="customerLogger"></param>
        /// <param name="customerService"></param>
        public CustomerController(ILogger<CustomerController> customerLogger, ICustomerService customerService)
        {
            _customerLogger = customerLogger;
            _customerService = customerService;
        }

        const string className = nameof(CustomerController);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="customerRequestDto"></param>
        /// <returns></returns>
        [HttpPost("get-tenant-customer-details")]
        public async Task<ActionResult<CustomerResponseDto>> GetTenantCustomerDetails([FromBody] CustomerRequestDto customerRequestDto)
        {
            const string methodName = nameof(GetTenantCustomerDetails);
            try
            {
                _customerLogger.LogInformation("{className}.{methodName}: API - Started With Customer code : {customerCode}", className, methodName, customerRequestDto.CustomerCode);
                var response = await _customerService.GetTenantCustomerDetails(customerRequestDto);
                return response.ErrorCode switch
                {
                    404 => NotFound(response),
                    500 => StatusCode(StatusCodes.Status500InternalServerError, response),
                    _ => Ok(response)
                };
            }
            catch (Exception ex)
            {
                _customerLogger.LogError(ex, ErrorLogTemplate, className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                throw;
            }
        }

        /// <summary>
        /// Get customer by tenant code
        /// </summary>
        /// <param name="customerRequestDto"></param>
        /// <returns></returns>
        [HttpPost("get-customer-by-tenant")]
        public async Task<ActionResult<CustomerResponseDto>> GetSponsorCustomerByTenant(GetCustomerByTenantRequestDto getCustomerByTenantRequestDto)
        {
            const string methodName = nameof(GetSponsorCustomerByTenant);
            try
            {
                _customerLogger.LogInformation("{className}.{methodName}: API - Started With Tenant : {tenantCode}", className, methodName, getCustomerByTenantRequestDto.TenantCode);
                var response = await _customerService.GetSponsorCustomerByTenant(getCustomerByTenantRequestDto.TenantCode);
                if (response.ErrorCode != null)
                {
                    var errorCode = response.ErrorCode;
                    _customerLogger.LogError("{className}.{methodName}: API - Error occurred while processing product, Request Data: {request}, Error Code: {ErrorCode}, Error Msg: {msg}", className, methodName, getCustomerByTenantRequestDto.ToJson(), response.ErrorCode, response.ErrorMessage);
                    return StatusCode((int)errorCode, response);
                }

                _customerLogger.LogInformation("{className}.{methodName}: API - Successfully fetched customer For Tenant Code:{tenantCode}",className, methodName, getCustomerByTenantRequestDto.TenantCode);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _customerLogger.LogError(ex, ErrorLogTemplate, className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, new CustomerResponseDto() { ErrorMessage = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves all customers from the database.
        /// Logs the operation progress and handles errors if they occur.
        /// Returns a list of all customers or an error response if the operation fails.
        /// </summary>
        /// <returns>
        /// An ActionResult containing either a successful response with a list of customers 
        /// (GetAllCustomersReponseDto) or an error response with a status code and message.
        /// </returns>
        [HttpGet("get-customers")]
        public async Task<ActionResult<CustomersReponseDto>> GetAllCustomers()
        {
            const string methodName = nameof(GetAllCustomers);
            try
            {
                _customerLogger.LogInformation("{ClassName}.{MethodName}: API Started fetching all customers", className, methodName);
                var response = await _customerService.GetAllCustomers();
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
                return StatusCode(StatusCodes.Status500InternalServerError, new CustomersReponseDto() {ErrorCode = StatusCodes.Status500InternalServerError, ErrorMessage = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves all sponsors from the database.
        /// Logs the operation progress and handles errors if they occur.
        /// Returns a list of all sponsors or an error response if the operation fails.
        /// </summary>
        /// <returns>
        /// An ActionResult containing either a successful response with a list of sponsors 
        /// (GetAllSponsorResponseDto) or an error response with a status code and message.
        /// </returns>
        [HttpGet("get-sponsors")]
        public async Task<ActionResult<SponsorsResponseDto>> GetAllSponsors()
        {
            const string methodName = nameof(GetAllSponsors);
            try
            {
                _customerLogger.LogInformation("{ClassName}.{MethodName}: API Started fetching all Sponsors", className, methodName);
                var response = await _customerService.GetAllSponsors();
                if (response.ErrorCode != null)
                {
                    var errorCode = response.ErrorCode;
                    _customerLogger.LogError("{className}.{methodName}: API - Error occurred while processing all sponsors, Error Code: {ErrorCode}, Error Msg: {msg}", className, methodName, response.ErrorCode, response.ErrorMessage);
                    return StatusCode((int)errorCode, response);
                }
                _customerLogger.LogInformation("{className}.{methodName}: API - Successfully fetched all sponsors", className, methodName);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _customerLogger.LogError(ex, ErrorLogTemplate, className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, new SponsorsResponseDto() { ErrorMessage = ex.Message });
            }
        }

        /// <summary>
        /// Creates a new customer in the database.
        /// Logs the operation progress, including request details and success/failure states.
        /// Returns a success response with customer details or an error response if the operation fails.
        /// </summary>
        /// <param name="requestDto">
        /// The request data containing the details of the customer to be created 
        /// (CreateCustomerRequestDto).
        /// </param>
        /// <returns>
        /// An IActionResult containing either a successful response with customer creation details 
        /// or an error response with a status code and message.
        /// </returns>
        [HttpPost("create-customer")]
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
        /// Creates a new sponsor for a specific customer in the database.
        /// Logs the operation progress, including request details and success/failure states.
        /// Returns a success response with sponsor details or an error response if the operation fails.
        /// </summary>
        /// <param name="requestDto">
        /// The request data containing the customer code and sponsor details 
        /// (CreateSponsorRequestDto).
        /// </param>
        /// <returns>
        /// An IActionResult containing either a successful response with sponsor creation details 
        /// or an error response with a status code and message.
        /// </returns>
        [HttpPost("create-sponsor")]
        public async Task<IActionResult> CreateSponsor([FromBody] CreateSponsorDto requestDto)
        {
            const string methodName = nameof(CreateSponsor);
            try
            {
                _customerLogger.LogInformation("{ClassName}.{MethodName}: Request started with SponsorCode:{Sponsor}", className, methodName, requestDto.SponsorCode);

                var response = await _customerService.CreateSponsor(requestDto);

                if (response.ErrorCode != null)
                {
                    _customerLogger.LogError("{ClassName}.{MethodName}: Error occurred while creating Sponsor. Request: {RequestData}, Response: {ResponseData}, ErrorCode: {ErrorCode}", className, methodName, requestDto.ToJson(), response.ToJson(), response.ErrorCode);
                    return StatusCode((int)response.ErrorCode, response);
                }

                _customerLogger.LogInformation("{ClassName}.{MethodName}: Sponsor created successful, SponsorCode: {SponsorCode}", className, methodName, requestDto.SponsorCode);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _customerLogger.LogError(ex, "{ClassName}.{MethodName}: An error occurred while create Sponsor. Error Message: {ErrorMessage}, ErrorCode: {ErrorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
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

        /// <summary>
        /// This method processes a list of customer, sponsor, and tenant requests to retrieve corresponding data. 
        /// It validates that at least one of the required properties (CustomerCode, SponsorCode, or TenantCode) is provided for each request.
        /// For each valid request, it fetches the corresponding Tenant, Sponsor, and Customer information from the repositories.
        /// The method returns a response containing a list of results, including error messages if any validation or data retrieval fails.
        /// </summary>
        /// <param name="requestDtos">The request object containing a list of customer, sponsor, and tenant information to be processed.</param>
        /// <returns>A response DTO containing a list of results for each processed customer, sponsor, and tenant request.</returns>
        [HttpPost("customer-sponsor-tenants")]
        public async Task<ActionResult<CustomerSponsorTenantsResponseDto>> GetCustomerSponsorTenants(CustomerSponsorTenantsRequestDto requestDto)
        {
            const string methodName = nameof(GetCustomerSponsorTenants);
            try
            {
                _customerLogger.LogInformation("{className}.{methodName}: API - Started With Request : {Request}", className, methodName, requestDto.ToJson);
                var response = await _customerService.GetCustomerSponsorTenants(requestDto);
                if (response.ErrorCode != null)
                {
                    var errorCode = response.ErrorCode;
                    _customerLogger.LogError("{className}.{methodName}: API - Error occurred while processing product, Request: {Request}, Error Code: {ErrorCode}, Error Msg: {msg}", className, methodName, requestDto.ToJson, response.ErrorCode, response.ErrorMessage);
                    return StatusCode((int)errorCode, response);
                }

                _customerLogger.LogInformation("{className}.{methodName}: API - Successfully fetched customer For Request:{Request}", className, methodName, requestDto.ToJson);
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
