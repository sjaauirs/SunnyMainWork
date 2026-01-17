using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Common.Core.Services;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;
using SunnyRewards.Helios.Tenant.Core.Domain.Models;
using SunnyRewards.Helios.Tenant.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Tenant.Infrastructure.Services.Interfaces;

namespace SunnyRewards.Helios.Tenant.Infrastructure.Services
{
    public class CustomerService : BaseService, ICustomerService
    {
        public readonly ILogger<CustomerService> _loggerCustomer;
        public readonly ICustomerRepo _customerRepo;
        public readonly IMapper _mapper;

        public readonly ITenantRepo _tenantRepo;
        public readonly ISponsorRepo _sponsorRepo;
        private const string ErrorLogTemplate = "{className}.{methodName}: ERROR Msg:{msg}, Error Code:{errorCode}";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="loggerCustomer"></param>
        /// <param name="customerRepo"></param>
        public CustomerService(ILogger<CustomerService> loggerCustomer, ICustomerRepo customerRepo, IMapper mapper
            , ITenantRepo tenantRepo, ISponsorRepo sponsorRepo)
        {
            _loggerCustomer = loggerCustomer;
            _customerRepo = customerRepo;
            _mapper = mapper;
            _tenantRepo = tenantRepo;
            _sponsorRepo = sponsorRepo;
        }
        const string className = nameof(CustomerService);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="customerRequestDto"></param>
        /// <returns></returns>
        public async Task<CustomerResponseDto> GetTenantCustomerDetails(CustomerRequestDto customerRequestDto)
        {
            const string methodName = nameof(GetTenantCustomerDetails);
            try
            {

                var customerData = await _customerRepo.FindOneAsync(x => x.CustomerCode.ToLower() == customerRequestDto.CustomerCode.ToLower() &&
                x.CustomerName.ToLower() == customerRequestDto.CustomerLabel.ToLower() && x.DeleteNbr == 0);

                if (customerData == null)
                {
                    _loggerCustomer.LogError("{className}.{methodName}: CustomerData is not found :{CustomerCode}, Error Code:{ErrorCode}", className, methodName,
                        customerRequestDto.CustomerCode, StatusCodes.Status404NotFound);
                    return new CustomerResponseDto() { ErrorCode = StatusCodes.Status404NotFound };
                }
                var customer = _mapper.Map<CustomerDto>(customerData);
                _loggerCustomer.LogInformation("{className}.{methodName}: Retrieved customer Data by CustomerCode  Successfully for Request :" +
                    "\n{CustomerCode}", className, methodName, customerRequestDto.CustomerCode);
                return new CustomerResponseDto() { customer = customer };
            }
            catch (Exception ex)
            {
                _loggerCustomer.LogError(ex, "{className}.{methodName}: - ERROR :{msg}", className, methodName, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Get customer by tenant code
        /// </summary>
        /// <param name="tenantCode"></param>
        /// <returns></returns>
        public async Task<CustomerResponseDto> GetSponsorCustomerByTenant(string tenantCode)
        {
            const string methodName = nameof(GetSponsorCustomerByTenant);
            try
            {

                var tenantData = await _tenantRepo.FindOneAsync(x => x.TenantCode!.ToLower() == tenantCode.ToLower() && x.DeleteNbr == 0);

                if (tenantData == null)
                {
                    _loggerCustomer.LogError("{className}.{methodName}: Tenant not found :{tenantCode}, Error Code:{errorCode}", className, methodName, tenantCode, StatusCodes.Status404NotFound);
                    return new CustomerResponseDto() { ErrorCode = StatusCodes.Status404NotFound };
                }

                var sponsor = await _sponsorRepo.FindOneAsync(x => x.SponsorId == tenantData.SponsorId && x.DeleteNbr == 0);
                if (sponsor == null)
                {
                    _loggerCustomer.LogError("{className}.{methodName}: sponsor not found, Sponsor Id :{sponsor} Error Code:{errorCode}", className, methodName, tenantData.SponsorId, StatusCodes.Status404NotFound);
                    return new CustomerResponseDto() { ErrorCode = StatusCodes.Status404NotFound };
                }
                var customerData = await _customerRepo.FindOneAsync(x => x.CustomerId == sponsor.CustomerId && x.DeleteNbr == 0);
                if (customerData != null)
                {
                    var customer = _mapper.Map<CustomerDto>(customerData);
                    _loggerCustomer.LogInformation("{className}.{methodName}: Retrieved sponsor customer Data by Tenant Successfully for Request :\n{tenantCode}", className, methodName, tenantCode);
                    return new CustomerResponseDto() { customer = customer };
                }
                else
                {
                    _loggerCustomer.LogInformation("{className}.{methodName}: Customer Not Found for ,Tenant :  {TenantCode} ,  SponsorId :  {SponsorId} , CustomerId : {CustomerId}, Error Code:{errorCode}", className, methodName, tenantCode, sponsor.SponsorId, sponsor.CustomerId, StatusCodes.Status404NotFound);
                    return new CustomerResponseDto()
                    {
                        ErrorCode = StatusCodes.Status404NotFound,
                        ErrorMessage = "Customer Not Found"
                    };
                }
            }
            catch (Exception ex)
            {
                _loggerCustomer.LogError(ex, "{className}.{methodName}: - Error :{msg}", className, methodName, ex.Message);
                throw;
            }
        }
        /// <summary>
        /// Retrieves all customers from the database and filters out those with a `DeleteNbr` value of 0.
        /// </summary>
        /// <returns>
        /// A <see cref="CustomersReponseDto"/> containing a list of filtered customers as <see cref="CustomerDto"/> objects.
        /// If no customers are found, it returns a response with a 404 status code and an error message.
        /// </returns>
        public async Task<CustomersReponseDto> GetAllCustomers()
        {
            const string methodName = nameof(GetAllCustomers);
            try
            {
                _loggerCustomer.LogInformation("{ClassName}.{MethodName}: Fetching all the Customers", className, methodName);
                var allCustomers = await _customerRepo.FindAllAsync();

                var filteredCustomers = allCustomers.Where(c => c.DeleteNbr == 0).ToList();

                if (filteredCustomers.Count == 0)
                {
                    return new CustomersReponseDto { ErrorCode = StatusCodes.Status404NotFound, ErrorMessage = $"No Customers found in database" };
                }
                var customerDtos = _mapper.Map<List<CustomerDto>>(filteredCustomers);
                return new CustomersReponseDto
                {
                    Customers = customerDtos
                };

            }
            catch (Exception ex)
            {
                _loggerCustomer.LogError(ex, "{className}.{methodName}: - Error occured while fetching customers Error :{msg}", className, methodName, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Retrieves all sponsors from the database and filters out those with a `DeleteNbr` value of 0.
        /// </summary>
        /// A <see cref="SponsorsResponseDto"/> containing a list of filtered sponsors as <see cref="SponsorDto"/> objects.
        /// If no sponsors are found, it returns a response with a 404 status code and an error message.
        public async Task<SponsorsResponseDto> GetAllSponsors()
        {
            const string methodName = nameof(GetAllSponsors);
            try
            {
                _loggerCustomer.LogInformation("{ClassName}.{MethodName}: Fetching all the Sponsors", className, methodName);
                var allSponsors = await _sponsorRepo.FindAllAsync();

                var filteredSponsors = allSponsors.Where(c => c.DeleteNbr == 0).ToList();

                if (filteredSponsors.Count == 0)
                {
                    return new SponsorsResponseDto { ErrorCode = StatusCodes.Status404NotFound, ErrorMessage = $"No Sponsors found in database" };
                }
                var sponsorDtos = _mapper.Map<List<SponsorDto>>(filteredSponsors);
                return new SponsorsResponseDto
                {
                    Sponsors = sponsorDtos
                };

            }
            catch (Exception ex)
            {
                _loggerCustomer.LogError(ex, "{className}.{methodName}: - Error occurred while fetching sponsors Error :{msg}", className, methodName, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Creates the Customer when no customer found with Customer code.
        /// </summary>
        /// <param name="customerRequestDto">Request contains the data to create customer </param>
        /// <returns>
        /// 409: when customer already exist
        /// 200:(baseResponse) when customer creates
        /// </returns>
        public async Task<BaseResponseDto> CreateCustomer(CreateCustomerDto customerRequestDto)
        {
            const string methodName = nameof(CreateCustomer);
            try
            {
                var customerCode = customerRequestDto.CustomerCode;
                _loggerCustomer.LogInformation("{ClassName}.{MethodName}: Started Customer creation for CustomerCode:{Customer}", className, methodName, customerCode);
                var customer = await _customerRepo.FindOneAsync(x  => x.CustomerCode == customerCode && x.DeleteNbr == 0);
                if(customer != null)
                {
                    return new BaseResponseDto { ErrorCode = StatusCodes.Status409Conflict, ErrorMessage = $"Customer already exist with CustomerCode:{customerCode}" };
                }
                var customerModel = _mapper.Map<CustomerModel>(customerRequestDto);
                customerModel.DeleteNbr = 0;
                customerModel.CreateTs = DateTime.UtcNow;
                customerModel.CustomerId = 0;
                customerModel.CreateUser = "SYSTEM";
                await _customerRepo.CreateAsync(customerModel);
                _loggerCustomer.LogInformation("{ClassName}.{MethodName}: Customer created successfully with CustomerCode:{Customer}", className, methodName, customerCode);
                return new BaseResponseDto();

            }
            catch (Exception ex)
            {
                _loggerCustomer.LogError(ex, "{className}.{methodName}: - Error occurred while Creating Customer Error :{msg}", className, methodName, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Checks if the customer exists and if the sponsor already exists.
        /// If valid, maps the sponsor data and saves it to the database.
        /// </summary>
        /// <param name="sponsorRequestDto">request data that contains sponsor details to create</param>
        /// <returns>Returns a response indicating success or failure.</returns>
        public async Task<BaseResponseDto> CreateSponsor(CreateSponsorDto createSponsorDto)
        {
            const string methodName = nameof(CreateSponsor);
            try
            {
                _loggerCustomer.LogInformation("{ClassName}.{MethodName}: Fetching Sponsor details for SponsorCode:{Sponsor}", className, methodName, createSponsorDto.SponsorCode);
                var customer = await _customerRepo.FindOneAsync(x => x.CustomerId == createSponsorDto.CustomerId && x.DeleteNbr == 0);
                if (customer == null)
                {
                    return new BaseResponseDto { ErrorCode = StatusCodes.Status404NotFound, ErrorMessage = $"Customer Not found with CustomerId:{createSponsorDto.CustomerId}" };
                }
                var sponsorCode = createSponsorDto.SponsorCode;
                var sponsor = await _sponsorRepo.FindOneAsync(x => x.SponsorCode == sponsorCode && x.DeleteNbr == 0);
                if (sponsor != null)
                {
                    return new BaseResponseDto { ErrorCode = StatusCodes.Status409Conflict, ErrorMessage = $"Sponsor already exist with SponsorCode:{createSponsorDto.SponsorCode}" };
                }
                var sponsorModel = _mapper.Map<SponsorModel>(createSponsorDto);
                sponsorModel.CustomerId = customer.CustomerId;
                sponsorModel.DeleteNbr = 0;
                sponsorModel.CreateTs = DateTime.UtcNow;
                sponsorModel.CreateUser = "SYSTEM";
                sponsorModel.SponsorId = 0;
                await _sponsorRepo.CreateAsync(sponsorModel);
                _loggerCustomer.LogInformation("{ClassName}.{MethodName}: Sponsor created successfully with SponsorCode:{Sponsor}", className, methodName, sponsorCode);
                return new BaseResponseDto();

            }
            catch (Exception ex)
            {
                _loggerCustomer.LogError(ex, "{className}.{methodName}: - Error occurred while Creating Sponsor Error :{msg}", className, methodName, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Get tenant sponsor customer by tenant code
        /// </summary>
        /// <param name="tenantCode"></param>
        /// <returns></returns>
        public async Task<TenantSponsorCustomerResponseDto> GetTenantSponsorCustomer(string tenantCode)
        {
            const string methodName = nameof(GetTenantCustomerDetails);

            _loggerCustomer.LogInformation("{className}.{methodName}: Started processing for TenantCode :{tenantCode}", className, methodName, tenantCode);
            try
            {
                var tenantModel = await _tenantRepo.FindOneAsync(x => x.TenantCode == tenantCode && x.DeleteNbr == 0);
                if (tenantModel == null)
                {
                    _loggerCustomer.LogError("{className}.{methodName}: Invaid TenantCode :{tenantCode}, Error Code:{ErrorCode}", className, methodName, tenantCode, StatusCodes.Status404NotFound);
                    return new TenantSponsorCustomerResponseDto() { ErrorCode = StatusCodes.Status404NotFound, ErrorMessage = "Invaid Tenant" };
                }

                var sponsorModel = await _sponsorRepo.FindOneAsync(x => x.SponsorId == tenantModel.SponsorId && x.DeleteNbr == 0);
                if (sponsorModel == null)
                {
                    _loggerCustomer.LogError("{className}.{methodName}: Invaid SponsorId :{SponsorId}, Error Code:{ErrorCode}", className, methodName, tenantModel.SponsorId, StatusCodes.Status404NotFound);
                    return new TenantSponsorCustomerResponseDto() { ErrorCode = StatusCodes.Status404NotFound, ErrorMessage = "Invaid Sponsor" };
                }

                var customerModel = await _customerRepo.FindOneAsync(x => x.CustomerId == sponsorModel.CustomerId && x.DeleteNbr == 0);
                if (customerModel == null)
                {
                    _loggerCustomer.LogError("{className}.{methodName}: Invaid CustomerId :{CustomerId}, Error Code:{ErrorCode}", className, methodName, sponsorModel.CustomerId, StatusCodes.Status404NotFound);
                    return new TenantSponsorCustomerResponseDto() { ErrorCode = StatusCodes.Status404NotFound, ErrorMessage = "Invaid Customer" };
                }

                return new TenantSponsorCustomerResponseDto
                {
                    Tenant = _mapper.Map<TenantDto>(tenantModel),
                    Sponsor = _mapper.Map<SponsorDto>(sponsorModel),
                    Customer = _mapper.Map<CustomerDto>(customerModel)
                };

            }
            catch (Exception ex)
            {
                _loggerCustomer.LogError("{className}.{methodName}: Error procesing for TenantCode :{tenantCode}, Error Code:{ErrorCode}", className, methodName, tenantCode, StatusCodes.Status500InternalServerError);
                _loggerCustomer.LogError(ex, "{className}.{methodName}: - ERROR :{msg}", className, methodName, ex.Message);
                return new TenantSponsorCustomerResponseDto
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = $"Error procesing for TenantCode :{tenantCode}"
                };
            }
        }

        public async Task<CustomerSponsorTenantsResponseDto> GetCustomerSponsorTenants(CustomerSponsorTenantsRequestDto requestDto)
        {
            const string methodName = nameof(GetCustomerSponsorTenants);
            var customerSponsorTenants = new List<CustomerSponsorTenantResponseDto>();

            // Log the start of the method execution with request details
            _loggerCustomer.LogInformation("{className}.{methodName}: Started processing for CustomerSponsorTenants: {CustomerSponsorTenants}", className, methodName, requestDto.ToJson);

            try
            {
                // Validate that at least one list contains data; otherwise, return a bad request error
                if (requestDto == null || requestDto?.CustomerSponsorTenants == null || requestDto?.CustomerSponsorTenants?.Count == 0)
                {
                    _loggerCustomer.LogError("{className}.{methodName}: Invalid request: {requestDto}", className, methodName, requestDto.CustomerSponsorTenants.ToJson());
                    return new CustomerSponsorTenantsResponseDto()
                    {
                        ErrorCode = StatusCodes.Status400BadRequest,
                        ErrorMessage = "Bad Request.CustomerSponsorTenant Array is required."
                    };
                }

                foreach (var item in requestDto?.CustomerSponsorTenants)
                {
                    // Process TenantCode if present in the request
                    if (!string.IsNullOrEmpty(item.TenantCode))
                    {
                        var tenantCode = item.TenantCode;
                        _loggerCustomer.LogInformation("{className}.{methodName}: Processing tenantCode :{tenantCode}", className, methodName, tenantCode);
                        
                        // Fetch tenant associated sponsor and customer from repository
                        var tenantModel = await _tenantRepo.FindOneAsync(x => x.TenantCode == tenantCode && x.DeleteNbr == 0);
                        if (tenantModel != null && tenantModel.TenantId > 0)
                        {
                            var sponsorModel = await _sponsorRepo.FindOneAsync(x => x.SponsorId == tenantModel.SponsorId && x.DeleteNbr == 0);
                            if (sponsorModel?.CustomerId > 0)
                            {
                                var customerModel = await _customerRepo.FindOneAsync(x => x.CustomerId == sponsorModel.CustomerId && x.DeleteNbr == 0);
                                if (customerModel?.CustomerId > 0)
                                {
                                    customerSponsorTenants.Add(new CustomerSponsorTenantResponseDto
                                    {
                                        Tenant = _mapper.Map<TenantDto>(tenantModel),
                                        Sponsor = _mapper.Map<SponsorDto>(sponsorModel),
                                        Customer = _mapper.Map<CustomerDto>(customerModel),
                                    });
                                }
                            }
                        }
                    }

                    // Process SponsorCode if present in the request
                    else if (!string.IsNullOrEmpty(item.SponsorCode))
                    {
                        var sponsorCode = item.SponsorCode;
                        _loggerCustomer.LogInformation("{className}.{methodName}: Processing sponsorCode :{sponsorCode}", className, methodName, sponsorCode);

                        // Fetch sponsor associated customer from repository
                        var sponsorModel = await _sponsorRepo.FindOneAsync(x => x.SponsorCode == sponsorCode && x.DeleteNbr == 0);
                        if (sponsorModel?.CustomerId > 0)
                        {
                            var customerModel = await _customerRepo.FindOneAsync(x => x.CustomerId == sponsorModel.CustomerId && x.DeleteNbr == 0);
                            if (customerModel?.CustomerId > 0)
                            {
                                customerSponsorTenants.Add(new CustomerSponsorTenantResponseDto
                                {
                                    Sponsor = _mapper.Map<SponsorDto>(sponsorModel),
                                    Customer = _mapper.Map<CustomerDto>(customerModel),
                                });
                            }
                        }
                    }

                    // Process CustomerCode if present in the request
                    else if (!string.IsNullOrEmpty(item.CustomerCode))
                    {
                        var customerCode = item.CustomerCode;
                        _loggerCustomer.LogInformation("{className}.{methodName}: Processing customerCode :{customerCode}", className, methodName, customerCode);

                        // Fetch sponsor associated customer from repository
                        var customerModel = await _customerRepo.FindOneAsync(x => x.CustomerCode == customerCode && x.DeleteNbr == 0);
                        if (customerModel?.CustomerId > 0)
                        {
                            customerSponsorTenants.Add(new CustomerSponsorTenantResponseDto
                            {
                                Customer = _mapper.Map<CustomerDto>(customerModel),
                            });
                        }
                    }
                }

                return new CustomerSponsorTenantsResponseDto()
                {
                    CustomerSponsorTenants = customerSponsorTenants
                };
            }
            catch (Exception ex)
            {
                // Log the error and return a generic server error response
                _loggerCustomer.LogError(ex, ErrorLogTemplate, className, methodName, ex.Message, StatusCodes.Status500InternalServerError);

                return new CustomerSponsorTenantsResponseDto()
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = "Internal Server Error"
                };
            }
        }
    }
}
