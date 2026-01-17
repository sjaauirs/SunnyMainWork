using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    public class AddressTypeService : IAddressTypeService
    {
        private readonly IAddressTypeRepo _addressTypeRepo;
        private readonly ILogger<AddressTypeService> _logger;
        private readonly IMapper _mapper;
        private readonly IReadOnlySession? _readOnlySession;
        private const string className = nameof(AddressTypeService);
        
        private string DbSource => _readOnlySession != null ? "ReadReplica" : "Primary";

        public AddressTypeService(
            IAddressTypeRepo addressTypeRepo,
            ILogger<AddressTypeService> logger,
            IMapper mapper,
            IReadOnlySession? readOnlySession = null)
        {
            _addressTypeRepo = addressTypeRepo;
            _logger = logger;
            _mapper = mapper;
            _readOnlySession = readOnlySession;
        }

        public async Task<GetAllAddressTypesResponseDto> GetAllAddressTypes()
        {
            const string methodName = nameof(GetAllAddressTypes);
            try
            {
                _logger.LogInformation("{className}.{methodName} - Querying from {dbSource} database", className, methodName, DbSource);

                IList<AddressTypeModel> addressTypes;
                if (_readOnlySession != null)
                {
                    addressTypes = await _readOnlySession.Session.QueryOver<AddressTypeModel>()
                        .Where(x => x.DeleteNbr == 0)
                        .ListAsync();
                }
                else
                {
                    addressTypes = await _addressTypeRepo.FindAsync(x => x.DeleteNbr == 0);
                }

                if (addressTypes == null || !addressTypes.Any())
                {
                    return new GetAllAddressTypesResponseDto
                    {
                        ErrorCode = StatusCodes.Status404NotFound,
                        ErrorMessage = "No address types found."
                    };
                }

                _logger.LogInformation("{className}.{methodName} - Successfully fetched all address types.", className, methodName);

                return new GetAllAddressTypesResponseDto
                {
                    AddressTypesList = _mapper.Map<IList<AddressTypeDto>>(addressTypes)
                };
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "{className}.{methodName} - Error occurred while getting all address types.", className, methodName);
                throw;
            }
        }

        public async Task<GetAddressTypeResponseDto> GetAddressTypeById(long addressTypeId)
        {
            const string methodName = nameof(GetAddressTypeById);
            try
            {
                _logger.LogInformation("{className}.{methodName} - Querying from {dbSource} database for addressTypeId {addressTypeId}", className, methodName, DbSource, addressTypeId);

                AddressTypeModel? addressType;
                if (_readOnlySession != null)
                {
                    addressType = await _readOnlySession.Session.QueryOver<AddressTypeModel>()
                        .Where(x => x.DeleteNbr == 0 && x.AddressTypeId == addressTypeId)
                        .SingleOrDefaultAsync();
                }
                else
                {
                    addressType = await _addressTypeRepo.FindOneAsync(x => x.DeleteNbr == 0 && x.AddressTypeId == addressTypeId);
                }

                if (addressType == null)
                {
                    return new GetAddressTypeResponseDto
                    {
                        ErrorCode = StatusCodes.Status404NotFound,
                        ErrorMessage = $"No address type found for addressTypeId {addressTypeId}."
                    };
                }
                _logger.LogInformation("{className}.{methodName} - Successfully fetched address type by id {addressTypeName}.", className, methodName, addressTypeId);
                return new GetAddressTypeResponseDto
                {
                    AddressType = _mapper.Map<AddressTypeDto>(addressType)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{className}.{methodName} - Error occurred while getting address type by id {addressTypeName}.", className, methodName, addressTypeId);
                throw;
            }
        }
    }
}
