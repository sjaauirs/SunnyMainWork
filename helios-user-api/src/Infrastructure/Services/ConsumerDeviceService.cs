using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Common.Core.Domain;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using SunnyRewards.Helios.Common.Core.Services.Interfaces;
using SunnyRewards.Helios.User.Core.Domain.Constant;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Models;
using SunnyRewards.Helios.User.Infrastructure.ReadReplica;
using SunnyRewards.Helios.User.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.User.Infrastructure.Services.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace SunnyRewards.Helios.User.Infrastructure.Services
{
    public class ConsumerDeviceService : IConsumerDeviceService
    {
        private readonly ILogger<ConsumerDeviceService> _logger;
        private readonly IConsumerDeviceRepo _consumerDeviceRepo;
        private readonly IEncryptionHelper _encryptionHelper;
        private readonly IVault _vault;
        private readonly IMapper _mapper;
        private readonly IHashingService _hashingService;
        private readonly IReadOnlySession? _readOnlySession;
        private const string className = nameof(ConsumerDeviceService);

        private NHibernate.ISession? ReadSession => _readOnlySession?.Session;
        private string DbSource => _readOnlySession != null ? "ReadReplica" : "Primary";

        public ConsumerDeviceService(
            ILogger<ConsumerDeviceService> logger,
            IConsumerDeviceRepo consumerDeviceRepo,
            IEncryptionHelper encryptionHelper,
            IVault vault,
            IMapper mapper,
            IHashingService hashingService,
            IReadOnlySession? readOnlySession = null)
        {
            _logger = logger;
            _consumerDeviceRepo = consumerDeviceRepo;
            _encryptionHelper = encryptionHelper;
            _vault = vault;
            _mapper = mapper;
            _hashingService = hashingService;
            _readOnlySession = readOnlySession;
        }
        /// <summary>
        /// Processes the creation of a new consumer device in the system.
        /// Validates for existing devices, hashes and encrypts the device ID, and persists the device details in the database.
        /// </summary>
        /// <param name="postConsumerDeviceRequestDto">
        /// The request DTO containing the consumer device details, including TenantCode, ConsumerCode, DeviceId, DeviceType, and DeviceAttrJson.
        /// </param>
        /// <returns>
        /// A <see cref="BaseResponseDto"/> indicating the result of the operation:
        /// - Returns a status code 409 (Conflict) if the device already exists.
        /// - Returns a status code 400 (Bad Request) if there is an error during encryption.
        /// - Returns a base responsedto response if the device is created successfully.
        /// </returns>
        /// <exception cref="Exception">
        /// Throws an exception if an unexpected error occurs during processing.
        /// </exception>
        public async Task<BaseResponseDto> CreateConsumerDevice(PostConsumerDeviceRequestDto postConsumerDeviceRequestDto)
        {
            const string methodName = nameof(CreateConsumerDevice);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName} -  Started processing create consumer device with TenantCode:{Code},ConsumerCode:{Consumer}",
                    className, methodName, postConsumerDeviceRequestDto.TenantCode, postConsumerDeviceRequestDto.ConsumerCode);

                var deviceIdHash = _hashingService.ComputeSHA256Hash(postConsumerDeviceRequestDto.DeviceId);

                var consumerDevice = await _consumerDeviceRepo.FindOneAsync(x => x.ConsumerCode == postConsumerDeviceRequestDto.ConsumerCode &&
                                                   x.DeviceIdHash == deviceIdHash && x.DeleteNbr == 0);
                if (consumerDevice != null)
                {
                    _logger.LogWarning("{ClassName}.{MethodName} - Consumer device is already exist with ConsumerCode:{Code},DeviceId:{Id}", className, methodName,
                        postConsumerDeviceRequestDto.ConsumerCode, postConsumerDeviceRequestDto.DeviceId);
                    return new BaseResponseDto() { ErrorCode = StatusCodes.Status409Conflict };
                }

                var encryptedDeviceId = await GetEncryptedDeviceId(postConsumerDeviceRequestDto.DeviceId, postConsumerDeviceRequestDto.TenantCode);

                if (string.IsNullOrEmpty(encryptedDeviceId))
                {
                    _logger.LogError("{ClassName}.{MethodName} - Error occurred while encrypting device id with ConsumerCode:{Code},DeviceId:{Id}", className, methodName,
                        postConsumerDeviceRequestDto.ConsumerCode, postConsumerDeviceRequestDto.DeviceId);
                    return new BaseResponseDto() { ErrorCode = StatusCodes.Status400BadRequest };
                }

                var consumerDeviceModel = new ConsumerDeviceModel()
                {
                    ConsumerDeviceCode = $"cdc-{Guid.NewGuid().ToString().Replace("-", "")}",
                    TenantCode = postConsumerDeviceRequestDto.TenantCode,
                    ConsumerCode = postConsumerDeviceRequestDto.ConsumerCode,
                    DeviceIdHash = deviceIdHash,
                    DeviceIdEnc = encryptedDeviceId,
                    DeviceType = postConsumerDeviceRequestDto.DeviceType,
                    DeviceAttrJson = postConsumerDeviceRequestDto.DeviceAttrJson,
                    CreateTs = DateTime.UtcNow,
                    CreateUser = Constants.CreateUser,
                    DeleteNbr = postConsumerDeviceRequestDto.DeleteNbr
                };

                _logger.LogInformation("{ClassName}.{MethodName} - Writing to Primary database", className, methodName);
                await _consumerDeviceRepo.CreateAsync(consumerDeviceModel);

                _logger.LogInformation("{ClassName}.{MethodName} -  Consumer device created successfully with TenantCode:{Code},ConsumerCode:{Consumer}",
                   className, methodName, postConsumerDeviceRequestDto.TenantCode, postConsumerDeviceRequestDto.ConsumerCode);

                return new BaseResponseDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Error occurred while processing  create consumer device with TenantCode:{Code},ConsumerCode:{Consumer},ERROR:{Msg}",
                   className, methodName, postConsumerDeviceRequestDto.TenantCode, postConsumerDeviceRequestDto.ConsumerCode, ex.Message);
                throw;
            }
        }

        private async Task<string> GetEncryptedDeviceId(string deviceId, string tenantCode)
        {
            const string methodName = nameof(GetEncryptedDeviceId);

            var symmetricEncryptionKey = await _vault.GetTenantSecret(tenantCode, SecretName.SymmetricEncryptionKey);

            if (string.IsNullOrEmpty(symmetricEncryptionKey) || symmetricEncryptionKey == _vault.InvalidSecret)
            {
                _logger.LogError("{className}.{methodName}: ERROR - Invalid symmetric Encryption key, for tenant code:{code} , Error Code:{errorCode}", className, methodName, tenantCode, StatusCodes.Status400BadRequest);
                return String.Empty;
            }

            var deviceid = _encryptionHelper.Encrypt(deviceId, Convert.FromBase64String(symmetricEncryptionKey));

            return deviceid;
        }
        /// <summary>
        /// Fetches consumer devices based on the provided tenant and consumer codes.
        /// </summary>
        /// <param name="getConsumerDeviceRequestDto">
        /// The request data containing tenant and consumer codes to filter consumer devices.
        /// </param>
        /// <returns>
        ///  - Returns 404 Not found response if no consumer devices found for tenant code and consumer code
        ///  - Returns List of consumer devices if found.
        /// </returns>
        public async Task<GetConsumerDeviceResponseDto> GetConsumerDevices(GetConsumerDeviceRequestDto getConsumerDeviceRequestDto)
        {
            const string methodName = nameof(GetConsumerDevices);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName} - Querying from {DbSource} database with TenantCode:{Code},ConsumerCode:{Consumer}",
                        className, methodName, DbSource, getConsumerDeviceRequestDto.TenantCode, getConsumerDeviceRequestDto.ConsumerCode);

                IList<ConsumerDeviceModel> consumerDeviceModels;
                if (_readOnlySession != null)
                {
                    consumerDeviceModels = await ReadSession!.QueryOver<ConsumerDeviceModel>()
                        .Where(x => x.TenantCode == getConsumerDeviceRequestDto.TenantCode
                            && x.ConsumerCode == getConsumerDeviceRequestDto.ConsumerCode
                            && x.DeleteNbr == 0)
                        .ListAsync();
                }
                else
                {
                    consumerDeviceModels = await _consumerDeviceRepo.FindAsync(x => x.TenantCode == getConsumerDeviceRequestDto.TenantCode && x.ConsumerCode ==
                                                   getConsumerDeviceRequestDto.ConsumerCode && x.DeleteNbr == 0);
                }

                if (consumerDeviceModels == null || !consumerDeviceModels.Any())
                {
                    _logger.LogError("{ClassName}.{MethodName} - Consumer devices not found with TenantCode:{TenantCode},ConsumerCode:{Code}", className, methodName,
                        getConsumerDeviceRequestDto.ConsumerCode, getConsumerDeviceRequestDto.ConsumerCode);
                    return new GetConsumerDeviceResponseDto()
                    {
                        ErrorCode = StatusCodes.Status404NotFound,
                        ErrorMessage = $"Consumer devices not found with TenantCode:{getConsumerDeviceRequestDto.TenantCode},ConsumerCode:{getConsumerDeviceRequestDto.ConsumerCode}"
                    };
                }
                var consumerDeviceDtos = _mapper.Map<IList<ConsumerDeviceDto>>(consumerDeviceModels);

                return new GetConsumerDeviceResponseDto() { ConsumerDevices = consumerDeviceDtos };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Error occurred while processing  fetching consumer devices with TenantCode:{Code},ConsumerCode:{Consumer},ERROR:{Msg}",
                   className, methodName, getConsumerDeviceRequestDto.TenantCode, getConsumerDeviceRequestDto.ConsumerCode, ex.Message);
                throw;
            }
        }
    }
}
