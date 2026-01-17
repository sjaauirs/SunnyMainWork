using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Common.Core.Services;
using SunnyRewards.Helios.User.Core.Domain.Constant;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Models;
using SunnyRewards.Helios.User.Infrastructure.ReadReplica;
using SunnyRewards.Helios.User.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.User.Infrastructure.Services.Interfaces;
using Constant = SunnyRewards.Helios.User.Core.Domain.Constant.Constant;

namespace SunnyRewards.Helios.User.Infrastructure.Services
{
    public class PersonRoleService : BaseService, IPersonRoleService
    {
        private readonly ILogger<PersonRoleService> _logger;
        private readonly IMapper _mapper;
        private readonly IPersonRoleRepo _personRoleRepo;
        private readonly IPersonRepo _personRepo;
        private readonly IConsumerRepo _consumerRepo;
        private readonly IRoleRepo _roleRepo;
        private readonly IReadOnlySession? _readOnlySession;
        public const string className = nameof(PersonRoleService);

        public PersonRoleService(
            ILogger<PersonRoleService> logger,
            IMapper mapper,
            IPersonRoleRepo personRoleRepo,
            IPersonRepo personRepo,
            IConsumerRepo consumerRepo,
            IRoleRepo roleRepo,
            IReadOnlySession? readOnlySession = null)
        {
            _logger = logger;
            _mapper = mapper;
            _personRoleRepo = personRoleRepo;
            _personRepo = personRepo;
            _consumerRepo = consumerRepo;
            _roleRepo = roleRepo;
            _readOnlySession = readOnlySession;
        }

        public async Task<GetPersonRolesResponseDto> GetPersonRoles(GetPersonRolesRequestDto getPersonRolesRequestDto)
        {
            const string methodName = nameof(GetPersonRoles);
            var response = new GetPersonRolesResponseDto();

            try
            {
                _logger.LogInformation("{ClassName}.{MethodName}: Fetching PersonRoles", className, methodName);

                // Validate request input
                if (string.IsNullOrWhiteSpace(getPersonRolesRequestDto.Email) && string.IsNullOrWhiteSpace(getPersonRolesRequestDto.PersonCode))
                {
                    return CreateErrorResponse(StatusCodes.Status400BadRequest, "Either Email or PersonCode must be provided.");
                }

                // Fetch person
                var person = await GetPersonByEmailOrPersonCodeAsync(getPersonRolesRequestDto);
                if (person == null)
                {
                    return CreateErrorResponse(StatusCodes.Status404NotFound, "No person found with the given Email or PersonCode.");
                }

                // Fetch person roles
                var personRoles = await _personRoleRepo.FindAsync(x => x.PersonId == person.PersonId && x.DeleteNbr == 0);
                if (personRoles == null || personRoles.Count == 0)
                {
                    return CreateErrorResponse(StatusCodes.Status404NotFound, "No person roles found.");
                }

                response.PersonRoles = _mapper.Map<IList<PersonRoleDto>>(personRoles);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: An error occurred while fetching person roles", className, methodName);
                return CreateErrorResponse(StatusCodes.Status500InternalServerError, "An unexpected error occurred while fetching person roles");
            }
        }

        /// <summary>
        /// Fetch the access control list for the specified consumer code.
        /// </summary>
        /// <param name="consumerCode"></param>
        /// <returns></returns>
        public async Task<AccessControlListResponseDTO> GetAccessControlList(string consumerCode)
        {
            const string methodName = nameof(GetAccessControlList);
            var response = new AccessControlListResponseDTO();

            try
            {
                _logger.LogInformation("{ClassName}.{MethodName}: Fetching PersonRoles", className, methodName);

                // Validate input
                if (string.IsNullOrWhiteSpace(consumerCode))
                {
                    _logger.LogWarning("{ClassName}.{MethodName}: ConsumerCode is null or empty", className, methodName);
                    response.ErrorCode = StatusCodes.Status404NotFound;
                    response.ErrorMessage = "ConsumerCode is null or empty";
                    return response;
                }

                // Fetch consumer model
                var consumerModel = await GetConsumerModel(consumerCode);
                if (consumerModel == null || consumerModel.PersonId <= 0)
                {
                    response.ErrorCode = StatusCodes.Status404NotFound;
                    response.ErrorMessage = $"Consumer not found for ConsumerCode {consumerCode}";
                    return response;
                }

                // Fetch person model
                var personModel = await GetPersonModel(consumerModel.PersonId);
                if (personModel == null)
                {
                    response.ErrorCode = StatusCodes.Status404NotFound;
                    response.ErrorMessage = $"Person not found for PersonId {consumerModel.PersonId}";
                    return response;
                }

                // Fetch person roles
                var personRoles = await _personRoleRepo.FindAsync(x => x.PersonId == consumerModel.PersonId && x.DeleteNbr == 0);
                if (personRoles == null || !personRoles.Any())
                {
                    _logger.LogWarning("{ClassName}.{MethodName}: No roles found for PersonId {PersonId}", className, methodName, consumerModel.PersonId);
                    return response;
                }

                // Populate response
                response.IsSuperAdmin = await CheckIfSuperAdmin(personRoles);
                response.IsSubscriber = await CheckIfSubscriber(personRoles);
                response.IsReportUser = await CheckIfReportUser(personRoles);
                response.CustomerAdminCustomerCodes = await GetDistinctCustomerCodesForCustomerAdmin(personRoles);
                response.SponsorAdminSponsorCodes = await GetDistinctSponsorCodesForSponsorAdmin(personRoles);
                response.TenantAdminTenantCodes = await GetDistinctTenantCodesForTenantAdmin(personRoles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: An error occurred while fetching person roles", className, methodName);
            }

            return response;
        }

        /// <summary>
        /// Fetch consumer model by consumer code.
        /// </summary>
        private async Task<ConsumerModel?> GetConsumerModel(string consumerCode)
        {
            const string methodName = nameof(GetAccessControlList);

            try
            {
                return await _consumerRepo.FindOneAsync(x => x.ConsumerCode == consumerCode && x.DeleteNbr == 0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: An error occurred while fetching the consumer model.", className, methodName);
                return null;
            }
        }

        /// <summary>
        /// Fetch person model by person ID.
        /// </summary>
        private async Task<PersonModel?> GetPersonModel(long personId)
        {
            const string methodName = nameof(GetPersonModel);
            try
            {
                return await _personRepo.FindOneAsync(x => x.PersonId == personId && x.DeleteNbr == 0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: An error occurred while fetching the person model.", className, methodName);
                return null;
            }
        }

        /// <summary>
        /// Check if the user is a Super Admin.
        /// </summary>
        private async Task<bool> CheckIfSuperAdmin(IEnumerable<PersonRoleModel> personRoles)
        {
            // Fetch the adminR role
            var adminRole = await _roleRepo.FindOneAsync(x => x.RoleCode != null && x.RoleCode.ToLower() == Constant.Admin.ToLower() && x.DeleteNbr == 0);
            
            // Check if the user is a Super Admin.
            return personRoles.Any(x => adminRole != null && x.RoleId == adminRole.RoleId
                && x.CustomerCode != null && x.CustomerCode.ToLower() == Constant.All.ToLower()
                && x.SponsorCode != null && x.SponsorCode.ToLower() == Constant.All.ToLower()
                && x.TenantCode != null && x.TenantCode.ToLower() == Constant.All.ToLower());
        }

        /// <summary>
        /// Check if the user is a Subscriber.
        /// </summary>
        private async Task<bool> CheckIfSubscriber(IEnumerable<PersonRoleModel> personRoles)
        {
            // Fetch the subscriber role
            var subscriberRole = await _roleRepo.FindOneAsync(x => x.RoleCode != null && x.RoleCode.ToLower() == Constant.Subscriber.ToLower() && x.DeleteNbr == 0);

            // Check if the user is a Subscriber.
            return personRoles.Any(x => subscriberRole != null && x.RoleId == subscriberRole.RoleId && x.CustomerCode != null && x.SponsorCode != null && x.TenantCode != null);
        }

        /// <summary>
        /// Check if the user is a Report User.
        /// </summary>
        private async Task<bool> CheckIfReportUser(IEnumerable<PersonRoleModel> personRoles)
        {
            // Fetch the report user role
            var reportUserRole = await _roleRepo.FindOneAsync(x => x.RoleCode != null && x.RoleCode.ToLower() == Constant.ReportUser.ToLower() && x.DeleteNbr == 0);

            // Check if the user is a Report User.
            return personRoles.Any(x => reportUserRole != null && x.RoleId == reportUserRole.RoleId);
        }

        /// <summary>
        /// Fetch distinct CustomerCode values where the user is a Customer Admin
        /// </summary>
        /// <param name="personRoles"></param>
        /// <returns></returns>
        private async Task<IList<string>?> GetDistinctCustomerCodesForCustomerAdmin(IEnumerable<PersonRoleModel> personRoles)
        {
            // Fetch the customer admin role
            var customerAdminRole = await _roleRepo.FindOneAsync(x => x.RoleCode != null && x.RoleCode.ToLower() == Constant.CustomerAdmin.ToLower() && x.DeleteNbr == 0);

            // Filter and return distinct CustomerCode values
            var customerCodes = personRoles.Where(x => x.RoleId == customerAdminRole.RoleId
                    && x.CustomerCode != null && x.CustomerCode.ToLower() != Constant.All.ToLower()
                    && x.SponsorCode != null && x.SponsorCode.ToLower() == Constant.All.ToLower()
                    && x.TenantCode != null && x.TenantCode.ToLower() == Constant.All.ToLower())
                .Select(x => x.CustomerCode)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            return customerCodes;
        }

        /// <summary>
        /// Fetch distinct SponsorCode values where the user is a Sponsor Admin.
        /// </summary>
        private async Task<IList<string>?> GetDistinctSponsorCodesForSponsorAdmin(IEnumerable<PersonRoleModel> personRoles)
        {
            // Fetch the sponsor admin role
            var sponsorAdminRole = await _roleRepo.FindOneAsync(x => x.RoleCode != null && x.RoleCode.ToLower() == Constant.SponsorAdmin.ToLower() && x.DeleteNbr == 0);

            // Filter and return distinct CustomerCode, SponsorCode values
            var sponsorCodes = personRoles.Where(x => sponsorAdminRole != null && x.RoleId == sponsorAdminRole.RoleId
                    && x.CustomerCode != null && x.CustomerCode.ToLower() != Constant.All.ToLower()
                    && x.SponsorCode != null && x.SponsorCode.ToLower() != Constant.All.ToLower()
                    && x.TenantCode != null && x.TenantCode.ToLower() == Constant.All.ToLower())
                .Select(x => x.SponsorCode)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            return sponsorCodes;
        }

        /// <summary>
        /// Fetch distinct TenantCode values where the user is a Tenant Admin.
        /// </summary>
        private async Task<IList<string>?> GetDistinctTenantCodesForTenantAdmin(IEnumerable<PersonRoleModel> personRoles)
        {
            // Fetch the tenant admin role
            var tenantAdminRole = await _roleRepo.FindOneAsync(x => x.RoleCode != null && x.RoleCode.ToLower() == Constant.TenantAdmin.ToLower() && x.DeleteNbr == 0);

            // Filter and return distinct CustomerCode, SponsorCode, TenantCode values
            var tenantCodes = personRoles
                .Where(x => tenantAdminRole != null && x.RoleId == tenantAdminRole.RoleId
                    && x.CustomerCode != null && x.CustomerCode.ToLower() != Constant.All.ToLower()
                    && x.SponsorCode != null && x.SponsorCode.ToLower() != Constant.All.ToLower()
                    && x.TenantCode != null && x.TenantCode.ToLower() != Constant.All.ToLower())
                .Select(x => x.TenantCode)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            
            return tenantCodes;
        }

        private async Task<PersonDto> GetPersonByEmailOrPersonCodeAsync(GetPersonRolesRequestDto request)
        {
            PersonDto personDto = null;

            if (!string.IsNullOrEmpty(request.Email) && !string.IsNullOrEmpty(request.PersonCode))
            {
                var personModel = await _personRepo.FindOneAsync(x => x.DeleteNbr == 0 && x.Email != null &&
                                                                    x.Email == request.Email &&
                                                                    x.PersonCode == request.PersonCode);
                personDto = _mapper.Map<PersonDto>(personModel);
            }
            else if (!string.IsNullOrEmpty(request.Email))
            {
                var personModel = await _personRepo.FindOneAsync(x => x.DeleteNbr == 0 && x.Email != null && x.Email == request.Email);
                personDto = _mapper.Map<PersonDto>(personModel);
            }
            else
            {
                var personModel = await _personRepo.FindOneAsync(x => x.DeleteNbr == 0 && x.PersonCode == request.PersonCode);
                personDto = _mapper.Map<PersonDto>(personModel);
            }

            return personDto;
        }

        private GetPersonRolesResponseDto CreateErrorResponse(int statusCode, string errorMessage)
        {
            return new GetPersonRolesResponseDto
            {
                ErrorCode = statusCode,
                ErrorMessage = errorMessage
            };
        }
    }
}
