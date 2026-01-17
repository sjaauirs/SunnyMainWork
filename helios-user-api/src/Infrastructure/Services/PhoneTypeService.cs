using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Models;
using SunnyRewards.Helios.User.Infrastructure.ReadReplica;
using SunnyRewards.Helios.User.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.User.Infrastructure.Services.Interfaces;

namespace SunnyRewards.Helios.User.Infrastructure.Services
{
    public class PhoneTypeService : IPhoneTypeService
    {
        private readonly IPhoneTypeRepo _phoneTypeRepo;
        private readonly ILogger<PhoneTypeService> _logger;
        private readonly IMapper _mapper;
        private readonly IReadOnlySession? _readOnlySession;
        private const string className = nameof(PhoneTypeService);
        
        private string DbSource => _readOnlySession != null ? "ReadReplica" : "Primary";

        public PhoneTypeService(
            IPhoneTypeRepo phoneTypeRepo,
            ILogger<PhoneTypeService> logger,
            IMapper mapper,
            IReadOnlySession? readOnlySession = null)
        {
            _phoneTypeRepo = phoneTypeRepo;
            _logger = logger;
            _mapper = mapper;
            _readOnlySession = readOnlySession;
        }

        public async Task<GetAllPhoneTypesResponseDto> GetAllPhoneTypes()
        {
            const string methodName = nameof(GetAllPhoneTypes);
            try
            {
                _logger.LogInformation("{className}.{methodName} - Querying from {dbSource} database", className, methodName, DbSource);

                IList<PhoneTypeModel> phoneTypes;
                if (_readOnlySession != null)
                {
                    phoneTypes = await _readOnlySession.Session.QueryOver<PhoneTypeModel>()
                        .Where(x => x.DeleteNbr == 0)
                        .ListAsync();
                }
                else
                {
                    phoneTypes = await _phoneTypeRepo.FindAsync(x => x.DeleteNbr == 0);
                }
                if (phoneTypes == null || !phoneTypes.Any())
                {
                    return new GetAllPhoneTypesResponseDto
                    {
                        ErrorCode = StatusCodes.Status404NotFound,
                        ErrorMessage = "No phone types found."
                    };
                }

                _logger.LogInformation("{className}.{methodName} - Successfully fetched all phone types.", className, methodName);

                return new GetAllPhoneTypesResponseDto
                {
                    PhoneTypesList = _mapper.Map<IList<PhoneTypeDto>>(phoneTypes)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{className}.{methodName} - Error occurred while getting all phone types.", className, methodName);
                throw;
            }
        }

        public async Task<GetPhoneTypeResponseDto> GetPhoneTypeById(long phoneTypeId)
        {
            const string methodName = nameof(GetPhoneTypeById);
            try
            {
                _logger.LogInformation("{className}.{methodName} - Querying from {dbSource} database for phoneTypeId {phoneTypeId}", className, methodName, DbSource, phoneTypeId);

                PhoneTypeModel? phoneType;
                if (_readOnlySession != null)
                {
                    phoneType = await _readOnlySession.Session.QueryOver<PhoneTypeModel>()
                        .Where(x => x.DeleteNbr == 0 && x.PhoneTypeId == phoneTypeId)
                        .SingleOrDefaultAsync();
                }
                else
                {
                    phoneType = await _phoneTypeRepo.FindOneAsync(x => x.DeleteNbr == 0 && x.PhoneTypeId == phoneTypeId);
                }
                if (phoneType == null)
                {
                    return new GetPhoneTypeResponseDto
                    {
                        ErrorCode = StatusCodes.Status404NotFound,
                        ErrorMessage = $"No phone type found for phoneTypeId {phoneTypeId}."
                    };
                }
                _logger.LogInformation("{className}.{methodName} - Successfully fetched phone type by id {phoneTypeId}.", className, methodName, phoneTypeId);
                return new GetPhoneTypeResponseDto
                {
                    PhoneType = _mapper.Map<PhoneTypeDto>(phoneType)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{className}.{methodName} - Error occurred while getting phone type by id {phoneTypeId}.", className, methodName, phoneTypeId);
                throw;
            }
        }
    }

}
