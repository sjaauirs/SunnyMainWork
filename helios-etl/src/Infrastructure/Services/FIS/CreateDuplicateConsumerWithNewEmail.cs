using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SunnyRewards.Helios.ETL.Common.CustomException;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Core.Domain.Enums;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces.FIS;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services.FIS
{
    public class CreateDuplicateConsumerWithNewEmail : ICreateDuplicateConsumerWithNewEmail
    {
        private readonly ILogger<CreateDuplicateConsumerWithNewEmail> _logger;
        private readonly IConsumerRepo _consumerRepo;
        private readonly ITenantRepo _tenantRepo;
        private readonly IPersonRepo _personRepo;
        private readonly ICustomerRepo _customerRepo;
        private readonly ISponsorRepo _sponsorRepo;
        private readonly IPersonAddressRepo _personAddressRepo;
        private readonly IMemberImportService _memberImportService;
        private readonly IPhoneNumberRepo _phoneNumberRepo;
        const string className = nameof(CreateDuplicateConsumerWithNewEmail);

        /// <summary>
        /// The service creats a new customer with the details of existing customer having different email address
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="sponsorRepo"></param>
        /// <param name="customerRepo"></param>
        /// <param name="consumerRepo"></param>
        /// <param name="personRepo"></param>
        /// <param name="tenantRepo"></param>
        /// <param name="memberImportService"></param>
        /// <param name="personAddressRepo"></param>
        /// <param name="phoneNumberRepo"></param>
        public CreateDuplicateConsumerWithNewEmail(ILogger<CreateDuplicateConsumerWithNewEmail> logger, ISponsorRepo sponsorRepo, ICustomerRepo customerRepo,
        IConsumerRepo consumerRepo, IPersonRepo personRepo, ITenantRepo tenantRepo, IMemberImportService memberImportService, IPersonAddressRepo personAddressRepo, IPhoneNumberRepo phoneNumberRepo)
        {
            _logger = logger;
            _consumerRepo = consumerRepo;
            _personRepo = personRepo;
            _tenantRepo = tenantRepo;
            _sponsorRepo = sponsorRepo;
            _customerRepo = customerRepo;
            _memberImportService = memberImportService;
            _personAddressRepo = personAddressRepo;
            _phoneNumberRepo = phoneNumberRepo;
        }
        /// <summary>
        /// Creates duplicate consumer with new email address
        /// </summary>
        /// <param name="etlExecutionContext"></param>
        /// <returns></returns>
        public async Task CreateDuplicateConsumer(EtlExecutionContext etlExecutionContext)
        {
            const string methodName = nameof(CreateDuplicateConsumer);
            try
            {
                if (etlExecutionContext.ConsumerCode.IsNullOrEmpty() || etlExecutionContext.NewEmail.IsNullOrEmpty() || IsInvalidEmail(etlExecutionContext.NewEmail.Trim()))
                {
                    _logger.LogError($"{className}.{methodName}:Invalid details for Consumer Code: {etlExecutionContext.ConsumerCode}, email:{etlExecutionContext.NewEmail} ");
                    throw new ETLException(ETLExceptionCodes.NullValue, $"Invalid details for Consumer Code: {etlExecutionContext.ConsumerCode}, email:{etlExecutionContext.NewEmail}");
                }
                var consumerDetails = await _consumerRepo.FindOneAsync(x => x.ConsumerCode == etlExecutionContext.ConsumerCode && x.DeleteNbr == 0);
                if (consumerDetails == null)
                {
                    _logger.LogError($"{className}.{methodName}:Consumer Details not found for Consumer Code: {etlExecutionContext.ConsumerCode}");
                    throw new ETLException(ETLExceptionCodes.NotFoundInDb, $"Consumer Details not found for Consumer Code: {etlExecutionContext.ConsumerCode}");
                }
                var personDetails = await _personRepo.FindOneAsync(x => x.PersonId == consumerDetails.PersonId && x.DeleteNbr == 0);
                if (personDetails == null)
                {
                    _logger.LogError($"{className}.{methodName}:Person Details not found for Consumer Code: {etlExecutionContext.ConsumerCode}");
                    throw new ETLException(ETLExceptionCodes.NotFoundInDb, $"Consumer Details not found for Consumer Code: {etlExecutionContext.ConsumerCode}");
                }
                var tenant = await _tenantRepo.FindOneAsync(x => x.TenantCode == etlExecutionContext.TenantCode && x.DeleteNbr == 0);
                if (tenant == null)
                {
                    _logger.LogError($"{className}.{methodName}:tenant Details not found for Consumer Code: {etlExecutionContext.ConsumerCode}");
                    throw new ETLException(ETLExceptionCodes.NotFoundInDb, $"Tenant Details not found for Consumer Code: {etlExecutionContext.ConsumerCode}");
                }
                var mem_nbr = "mem-" + Guid.NewGuid().ToString("N");

                var mailing_address = await _personAddressRepo.FindOneAsync(x => x.PersonId == personDetails.PersonId 
                    && x.AddressTypeId == (long)AddressTypeEnum.MAILING 
                    && x.IsPrimary
                    && x.DeleteNbr == 0);

                var home_address = await _personAddressRepo.FindOneAsync(x => x.PersonId == personDetails.PersonId
                    && x.AddressTypeId == (long)AddressTypeEnum.HOME
                    && x.Source == "ETL"
                    && x.DeleteNbr == 0);

                var mobile_phone = await _phoneNumberRepo.FindOneAsync(x => x.PersonId == personDetails.PersonId
                    && x.PhoneTypeId == (long)PhoneTypeEnum.MOBILE
                    && x.IsPrimary
                    && x.DeleteNbr == 0);

                var home_phone = await _phoneNumberRepo.FindOneAsync(x => x.PersonId == personDetails.PersonId
                    && x.PhoneTypeId == (long)PhoneTypeEnum.HOME
                    && x.Source == "ETL"
                    && x.DeleteNbr == 0);

                var memberDto = new MemberImportCSVDto
                {
                    first_name = personDetails.FirstName ?? string.Empty,
                    last_name = personDetails.LastName ?? string.Empty,
                    email = etlExecutionContext.NewEmail.ToLower(),
                    city = mailing_address?.City ?? string.Empty,
                    country = mailing_address?.Country ?? "US",
                    postal_code = mailing_address?.PostalCode ?? string.Empty,
                    mobile_phone = mobile_phone.PhoneNumber ?? string.Empty,
                    dob = personDetails.DOB?.ToString("MM/dd/yyyy") ?? String.Empty,
                    gender = GetGenderString(personDetails.Gender ?? string.Empty),
                    mailing_address_line1 = mailing_address?.Line1 ?? string.Empty,
                    mailing_address_line2 = mailing_address?.Line2 ?? string.Empty,
                    mailing_state = mailing_address?.State ?? string.Empty,
                    mailing_country_code = mailing_address?.CountryCode ?? string.Empty,
                    home_phone_number = home_phone.PhoneNumber ?? string.Empty,
                    partner_code = tenant.PartnerCode ?? string.Empty,
                    mem_nbr = mem_nbr,
                    subscriber_mem_nbr = mem_nbr,
                    eligibility_start = consumerDetails.EligibleStartTs?.ToString("MM/dd/yyyy")??String.Empty,
                    eligibility_end = consumerDetails.EligibleEndTs?.ToString("MM/dd/yyyy") ?? String.Empty,
                    is_sso_user = consumerDetails.IsSSOUser,
                    person_unique_identifier = etlExecutionContext.NewEmail.ToLower(),

                    middle_name = personDetails.MiddleName ?? string.Empty,
                    home_address_line1 = home_address?.Line1 ?? string.Empty,
                    home_address_line2 = home_address?.Line2 ?? string.Empty,
                    home_state = home_address?.State ?? string.Empty,
                    home_city = home_address?.City ?? string.Empty,
                    home_postal_code = home_address?.PostalCode ?? string.Empty,
                    language_code = personDetails.LanguageCode ?? string.Empty,
                    region_code = consumerDetails.RegionCode ?? string.Empty,
                    plan_id = consumerDetails.PlanId ?? string.Empty,
                    plan_type = consumerDetails.PlanType ?? string.Empty,
                    subgroup_id = consumerDetails.SubgroupId ?? string.Empty,
                    mem_nbr_prefix = consumerDetails.MemberNbrPrefix ?? string.Empty,
                    subscriber_mem_nbr_prefix = consumerDetails.SubsciberMemberNbrPrefix ?? string.Empty,
                    member_type = consumerDetails.MemberType,
                    member_id = Guid.NewGuid().ToString("N")
                };
                var sponsor = await _sponsorRepo.FindOneAsync(x => x.SponsorId == tenant.SponsorId && x.DeleteNbr == 0);
                if (sponsor == null)
                {
                    _logger.LogError($"{className}.{methodName}:Sponsor details not found for tenant Code: {etlExecutionContext.TenantCode}, email:{etlExecutionContext.NewEmail} ");
                    throw new ETLException(ETLExceptionCodes.NotFoundInDb, $"Sponsor details not found for tenant Code: {etlExecutionContext.TenantCode}, email:{etlExecutionContext.NewEmail}");
                }
                var customerDetails = await _customerRepo.FindOneAsync(x => x.CustomerId == sponsor.CustomerId && x.DeleteNbr == 0);
                if (customerDetails == null)
                {
                    _logger.LogError($"{className}.{methodName}:customer Details not found for tenant Code: {etlExecutionContext.TenantCode}, email:{etlExecutionContext.NewEmail} ");
                    throw new ETLException(ETLExceptionCodes.NotFoundInDb, $"Customer Details not found for tenant Code: {etlExecutionContext.TenantCode}, email:{etlExecutionContext.NewEmail} ");
                }
                etlExecutionContext.CustomerCode = customerDetails.CustomerCode;
                etlExecutionContext.CustomerLabel = customerDetails.CustomerName;
                List<MemberImportCSVDto> memberImportCSVDtos = new List<MemberImportCSVDto>();
                memberImportCSVDtos.Add(memberDto);
                await _memberImportService.ProcessBatchAsync(memberImportCSVDtos, etlExecutionContext);

                return;

            }
            catch (Exception ex)
            {

                _logger.LogError("{ClassName}.{MethodName} - Failed processing Create Duplicate consumer for consumer code request {code},  ERROR: {Msg}.", 
                    className, methodName, etlExecutionContext.ConsumerCode, ex.Message);
                throw;
            }
        }
        private string GetGenderString(string gender)
        {
            return gender switch
            {
                "MALE" => "M",
                "FEMALE" => "F",
                "OTHER" => "O",
                _ => string.Empty
            };
        }
        private bool IsInvalidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return true;
            string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            var value= Regex.IsMatch(email, pattern);
            return !value;

        }
    }
}
