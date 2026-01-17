using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Common.Core.Services;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;
using SunnyRewards.Helios.Tenant.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Tenant.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Helpers;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Tenant.Core.Domain.Models;

namespace SunnyRewards.Helios.Tenant.Infrastructure.Services
{
    public class TenantService : BaseService, ITenantService
    {
        public readonly ILogger<TenantService> _loggerTenant;
        public readonly IMapper _mapper;
        public readonly ITenantRepo _tenantRepo;
        private readonly IVault _vault;
        private const string dfApiEncKey = "DF_API_ENC_KEY";
        public readonly ICustomerRepo _customerRepo;
        private readonly ISponsorRepo _sponsorRepo;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="loggerTenant"></param>
        /// <param name="mapper"></param>
        /// <param name="tenantRepo"></param>
        /// <param name="customerRepo"></param>
        public TenantService(ILogger<TenantService> loggerTenant,
            IMapper mapper,
            ITenantRepo tenantRepo,
            ICustomerRepo customerRepo, IVault vault, ISponsorRepo sponsorRepo)
        {
            _loggerTenant = loggerTenant;
            _mapper = mapper;
            _tenantRepo = tenantRepo;
            _customerRepo = customerRepo;
            _vault = vault;
            _sponsorRepo = sponsorRepo;
        }
        const string className = nameof(TenantService);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tenantByPartnerCodeRequestDto"></param>
        /// <returns></returns>
        public async Task<GetTenantByPartnerCodeResponseDto> GetTenantByPartnerCode(GetTenantByPartnerCodeRequestDto tenantByPartnerCodeRequestDto)
        {
            const string methodName = nameof(GetTenantByPartnerCode);
            try
            {
                var tenantData = await _tenantRepo.FindOneAsync(x => x.PartnerCode == tenantByPartnerCodeRequestDto.PartnerCode && x.DeleteNbr == 0);
                _loggerTenant.LogInformation("{className}.{methodName}: Retrieved Tenant Data By PartnerCode Successfully for Request :\n{PartnerCode}", className, methodName, tenantByPartnerCodeRequestDto.PartnerCode);

                if (tenantData == null)
                {
                    _loggerTenant.LogError("{className}.{methodName}: Tenant Data Not Found For Partner Code:{tenant}, Error Code:{errorCode}", className, methodName,tenantByPartnerCodeRequestDto.PartnerCode, StatusCodes.Status404NotFound);
                    return new GetTenantByPartnerCodeResponseDto();
                }
                    

                var response = new GetTenantByPartnerCodeResponseDto()
                {
                    Tenant = _mapper.Map<TenantDto>(tenantData)
                };
                return response;
            }
            catch (Exception ex)
            {
                _loggerTenant.LogError(ex, "{className}.{methodName}: - Error :{msg}", className, methodName, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="getTenantCodeRequestDto"></param>
        /// <returns></returns>
        public async Task<TenantDto> GetByTenantCode(GetTenantCodeRequestDto getTenantCodeRequestDto)
        {
            const string methodName = nameof(GetByTenantCode);
            try
            {
                var tenantData = await _tenantRepo.FindOneAsync(x => x.TenantCode == getTenantCodeRequestDto.TenantCode && x.DeleteNbr == 0);
                _loggerTenant.LogInformation("{className}.{methodName}: Retrieved Tenant data by TenantCode Successfully for Request :\n{TenantCode}", className, methodName, getTenantCodeRequestDto.TenantCode);

                if (tenantData == null)
                {
                    _loggerTenant.LogError("{className}.{methodName}: Tenant Data Not Found For Tenant Code:{tenant}, Error Code:{errorCode}", className, methodName, getTenantCodeRequestDto.TenantCode, StatusCodes.Status404NotFound);
                    return new TenantDto();
                }
                var tenant = _mapper.Map<TenantDto>(tenantData);

                return tenant;
            }
            catch (Exception ex)
            {
                _loggerTenant.LogError(ex, "{className}.{methodName}: - Error :{msg}", className, methodName, ex.Message);
                throw;
            }

        }

        /// <summary>
        /// Get Tenant by encryption key id
        /// </summary>
        /// <param name="getTenantByEncryptionKeyIdRequestDto"></param>
        /// <returns></returns>
        public async Task<GetTenantByEncKeyIdResponseDto> GetTenantByEncKeyId(GetTenantByEncKeyIdRequestDto getTenantByEncKeyIdRequestDto)
        {
            const string methodName = nameof(GetTenantByEncKeyId);
            try
            {
                if (string.IsNullOrEmpty(getTenantByEncKeyIdRequestDto.EncKeyId))
                {
                    _loggerTenant.LogError("{className}.{methodName}: EncKey Id is NullOrEmpty. EncKeyId:{encKeyId}, Error Code:{errorCode}", className, methodName, getTenantByEncKeyIdRequestDto.EncKeyId, StatusCodes.Status400BadRequest);
                    return new GetTenantByEncKeyIdResponseDto() { ErrorCode = StatusCodes.Status400BadRequest };
                }

                var tenantData = await _tenantRepo.FindOneAsync(x => x.EncKeyId == getTenantByEncKeyIdRequestDto.EncKeyId && x.DeleteNbr == 0);
                _loggerTenant.LogInformation("{className}.{methodName}: Retrieved Tenant data by EncKeyId Successfully for Request :\n{EncKeyId}", className, methodName, getTenantByEncKeyIdRequestDto.EncKeyId);

                if (tenantData == null)
                {
                    _loggerTenant.LogError("{className}.{methodName}:Tenant Data Not Found For EncKeyId:{encKeyId}, Error Code:{errorCode}", className, methodName, getTenantByEncKeyIdRequestDto.EncKeyId, StatusCodes.Status404NotFound);
                    return new GetTenantByEncKeyIdResponseDto() { ErrorCode = StatusCodes.Status404NotFound };
                }

                var response = new GetTenantByEncKeyIdResponseDto()
                {
                    Tenant = _mapper.Map<TenantDto>(tenantData)
                };
                return response;
            }
            catch (Exception ex)
            {
                _loggerTenant.LogError(ex, "{className}.{methodName}: - ERROR :{msg}", className, methodName, ex.Message);
                throw;
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="apiKey"></param>
        /// <returns></returns>
        public async Task<bool> ValidateApiKey(string apiKey)
        {
            const string methodName = nameof(ValidateApiKey);   
            try
            {
                var encryptionHelper = new EncryptionHelper();
                var dfApiEncKeyValue = await _vault.GetSecret(dfApiEncKey);
                if (string.IsNullOrEmpty(dfApiEncKeyValue) || dfApiEncKeyValue == _vault.InvalidSecret)
                    return false;

                var decryptedKey = encryptionHelper.Decrypt(apiKey, Convert.FromBase64String(dfApiEncKeyValue));

                var xApiKey = decryptedKey.Split(":");
                if (xApiKey.Length != 2)
                    return false;

                var tenantCode = xApiKey[0];
                var tenantApiKey = xApiKey[1];

                var tenantData = await _tenantRepo.FindOneAsync(x => x.TenantCode == tenantCode && x.DeleteNbr == 0);
                _loggerTenant.LogInformation("{className}.{methodName}: Validate Api Key Successfully for Request :\n{apiKey}", className, methodName, apiKey);

                if (tenantData == null || tenantData.ApiKey != tenantApiKey)
                    return false;

                return true;
            }
            catch (Exception ex)
            {
                _loggerTenant.LogError(ex, "{className}.{methodName}: - ERROR :{msg}", className, methodName, ex.Message);
                throw;
            }

        }


        /// <summary>
        /// Creates the tenant.
        /// </summary>
        /// <param name="createTenantRequest">The create tenant request.</param>
        /// <returns></returns>
        public async Task<BaseResponseDto> CreateTenant(CreateTenantRequestDto createTenantRequest)
        {
            const string methodName = nameof(CreateTenant);
            try
            {
                var customer = await _customerRepo.FindOneAsync(x => x.CustomerCode == createTenantRequest.CustomerCode && x.DeleteNbr == 0);
                if(customer == null)
                {
                    return new BaseResponseDto() { ErrorCode = StatusCodes.Status404NotFound, ErrorMessage = $"Customer not found with coustmerCode: {createTenantRequest.CustomerCode}" };
                }

                var sponsor = await _sponsorRepo.FindOneAsync(x => x.SponsorCode == createTenantRequest.SponsorCode && x.DeleteNbr == 0);
                if (sponsor == null)
                {
                    return new BaseResponseDto() { ErrorCode = StatusCodes.Status404NotFound, ErrorMessage = $"Sponsor not found with sponsorCode: {createTenantRequest.SponsorCode}" };
                }
                var tenantName = createTenantRequest.Tenant.TenantName.Trim().ToLower();
                var tenants = await _tenantRepo.FindAsync(x => x.TenantName !=null && x.TenantName.Trim().ToLower() == tenantName && x.DeleteNbr == 0 && x.SponsorId == sponsor.SponsorId);
                var tenantWithSameTenantCode = tenants?.FirstOrDefault(x => x.TenantCode == createTenantRequest.Tenant.TenantCode);
                var tenantWithSamePartnerCode =await _tenantRepo.FindAsync(x => x.PartnerCode == createTenantRequest.Tenant.PartnerCode);
                var tenantWithOtherCode = tenants?.FirstOrDefault(x => x.TenantCode != createTenantRequest.Tenant.TenantCode);

                if (tenantWithSameTenantCode != null)
                {
                    return new BaseResponseDto() { ErrorCode = StatusCodes.Status409Conflict, ErrorMessage = $"Tenant is already Existed with Tenant Code: {createTenantRequest.Tenant.TenantCode}" };
                }
                
                if (tenantWithOtherCode != null)
                {
                    return new BaseResponseDto() { ErrorCode = StatusCodes.Status422UnprocessableEntity, ErrorMessage = $"Tenant name already exists, but with a different Tenant Code. Existing Tenant Code: {tenantWithOtherCode.TenantCode}, Given Tenant Code: {createTenantRequest.Tenant.TenantCode}." };
                }
                var tenantModel = _mapper.Map<TenantModel>(createTenantRequest.Tenant);
                if (tenantWithSamePartnerCode != null && tenantWithSamePartnerCode.Count>0)
                {
                    var guid = Guid.NewGuid();
                    tenantModel.PartnerCode = $"par-{guid:N}";
                    
                    _loggerTenant.LogInformation("{ClassName}.{MethodName}: tenant with same partner code exists:{TenantCode}", className, methodName, tenantModel.TenantCode);


                }
                tenantModel.SponsorId = sponsor.SponsorId;
                tenantModel.CreateTs = DateTime.UtcNow;
                tenantModel.UpdateTs = null;
                tenantModel.DeleteNbr = 0;
                tenantModel.TenantId = 0;
                 await _tenantRepo.CreateAsync(tenantModel);
                _loggerTenant.LogInformation("{className}.{methodName}: Tenant created Successfully. TenantCode:{TenantCode}", className, methodName, tenantModel.TenantCode);
                return new BaseResponseDto();
              
            }
            catch (Exception ex)
            {
                _loggerTenant.LogError(ex, "{className}.{methodName}: Error occurred while creating tenant, ErrorMessage:{ErrorMessage}", className, methodName, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Retrieves all Tenants from the database and filters out those with a `DeleteNbr` value of 0.
        /// </summary>
        /// A <see cref="TenantsResponseDto"/> containing a list of filtered Tenants as <see cref="TenantDto"/> objects.
        /// If no Tenants are found, it returns a response with a 404 status code and an error message.
        public async Task<TenantsResponseDto> GetAllTenants()
        {
            const string methodName = nameof(GetAllTenants);
            try
            {
                _loggerTenant.LogInformation("{ClassName}.{MethodName}: Fetching all the Tenants", className, methodName);
                var allTenants = await _tenantRepo.FindAllAsync();

                var filteredTenants = allTenants.Where(c => c.DeleteNbr == 0).ToList();

                if (filteredTenants.Count == 0)
                {
                    return new TenantsResponseDto { ErrorCode = StatusCodes.Status404NotFound, ErrorMessage = $"No Tenants found in database" };
                }
                var tenantDtos = _mapper.Map<List<TenantDto>>(filteredTenants);
                return new TenantsResponseDto
                {
                    Tenants = tenantDtos
                };

            }
            catch (Exception ex)
            {
                _loggerTenant.LogError(ex, "{className}.{methodName}: - Error occured while fetching Tenants Error :{msg}", className, methodName, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Updates the details of a tenant based on the provided request.
        /// Logs the process and handles any errors.
        /// </summary>
        /// <param name="updateTenantRequest">The request containing updated tenant details.</param>
        /// <returns>Returns a response DTO with the updated tenant information or an error message.</returns>
        public async Task<UpdateTenantResponseDto> UpdateTenant(string tenantCode, UpdateTenantDto updateTenantRequest)
        {
            const string methodName = nameof(UpdateTenant);
            try
            {
                if(tenantCode != updateTenantRequest.TenantCode)
                {
                    return new UpdateTenantResponseDto() { UpdateTenant = _mapper.Map<TenantDto>(updateTenantRequest), ErrorCode = StatusCodes.Status400BadRequest, ErrorMessage = $"Tenant Code mis match in the path and body must match. path TenantCode: {tenantCode} and Body tenantCode:{updateTenantRequest.TenantCode}" };
                }
                _loggerTenant.LogInformation("{ClassName}.{MethodName}: Started fetching Tenant with TenantCode:{TenantCode}", className, methodName, tenantCode);
                var tenant = await _tenantRepo.FindOneAsync(x => x.TenantCode == tenantCode.Trim() && x.DeleteNbr == 0);
                if (tenant == null)
                {
                    return new UpdateTenantResponseDto() {  UpdateTenant = _mapper.Map<TenantDto>(updateTenantRequest), ErrorCode = StatusCodes.Status404NotFound, ErrorMessage = $"Tenant not found with TenantCode: {tenantCode}" };
                }
                var planYear = updateTenantRequest.PlanYear;
                _mapper.Map(updateTenantRequest, tenant);
                var tenantWithSamePartnerCode =await _tenantRepo.FindAsync(x => x.PartnerCode == updateTenantRequest.PartnerCode && x.TenantCode!= tenant.TenantCode && x.DeleteNbr == 0);
                if (tenantWithSamePartnerCode != null && tenantWithSamePartnerCode.Count > 0)
                {
                    var guid = Guid.NewGuid();
                    tenant.PartnerCode = $"par-{guid:N}";
                    _loggerTenant.LogInformation("{ClassName}.{MethodName}: tenant with same partner code exists:{TenantCode}", className, methodName, tenantCode);

                }
                tenant.PeriodStartTs = new DateTime(planYear, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                tenant.PeriodEndTs = new DateTime(planYear, 12, 31, 23, 59, 59, DateTimeKind.Utc);
                tenant.UpdateTs = DateTime.UtcNow;
                tenant.UpdateUser = updateTenantRequest.UpdateUser??string.Empty;

                await _tenantRepo.UpdateAsync(tenant);
                _loggerTenant.LogInformation("{ClassName}.{MethodName}: Tenant Updated successfully for TenantCode:{TenantCode}", className, methodName, tenantCode);
                return new UpdateTenantResponseDto()
                {
                    UpdateTenant = _mapper.Map<TenantDto>(tenant)
                };

            }
            catch (Exception ex)
            {
                _loggerTenant.LogError(ex, "{className}.{methodName}: Error occurred while Updating Tenant, ErrorMessage:{ErrorMessage}", className, methodName, ex.Message);
                return new UpdateTenantResponseDto() { UpdateTenant = _mapper.Map<TenantDto>(updateTenantRequest), ErrorCode = StatusCodes.Status500InternalServerError, ErrorMessage = ex.Message };
            }
        }

        /// <summary>
        /// Get tenant details by tenant code
        /// </summary>
        /// <param name="tenantCode"></param>
        /// <returns></returns>
        public async Task<TenantResponseDto> GetTenantDetails(string tenantCode)
        {
            const string methodName = nameof(GetTenantDetails);
            try
            {
                var tenantData = await _tenantRepo.FindOneAsync(x => x.TenantCode == tenantCode && x.DeleteNbr == 0);
                _loggerTenant.LogInformation("{className}.{methodName}: Retrieved Tenant data by TenantCode Successfully for Request : {TenantCode}", className, methodName, tenantCode);

                if (tenantData == null)
                {
                    _loggerTenant.LogError("{className}.{methodName}: Tenant Data Not Found For Tenant Code:{tenant}, Error Code:{errorCode}", className, methodName, tenantCode, StatusCodes.Status404NotFound);
                    return new TenantResponseDto() 
                    { 
                        ErrorCode = StatusCodes.Status404NotFound,
                        ErrorMessage = "Tenant Not Found"
                    };
                }
                var tenant = _mapper.Map<TenantDto>(tenantData);

                return new TenantResponseDto() 
                { 
                    Tenant = tenant,
                };
            }
            catch (Exception ex)
            {
                _loggerTenant.LogError(ex, "{className}.{methodName}: - Error occurred while fetching tenant data, ErrorCode: {ErrorCode}, ErrorMessage: {ErrorMessage}", className, methodName, StatusCodes.Status500InternalServerError, ex.Message);
                throw;
            }
        }
    }
}
