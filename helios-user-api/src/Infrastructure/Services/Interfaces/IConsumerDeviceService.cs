using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Dtos;

namespace SunnyRewards.Helios.User.Infrastructure.Services.Interfaces
{
    public interface IConsumerDeviceService
    {
        /// <summary>
        /// Processes the creation of a new consumer device in the system.
        /// Validates for existing devices, hashes and encrypts the device ID, and persists the device details in the database.
        /// </summary>
        /// <param name="postConsumerDeviceRequestDto">
        /// The request DTO containing the consumer device details, including TenantCode, ConsumerCode, DeviceId, DeviceType, and DeviceAttrJson.
        /// </param>
        /// <returns>
        /// Returns a base responsedto response if the device is created successfully.
        /// </returns>
        Task<BaseResponseDto> CreateConsumerDevice(PostConsumerDeviceRequestDto postConsumerDeviceRequestDto);
        /// <summary>
        /// Fetches consumer devices based on the provided tenant and consumer codes.
        /// </summary>
        /// <param name="getConsumerDeviceRequestDto">
        /// The request data containing tenant and consumer codes to filter consumer devices.
        /// </param>
        /// <returns>
        ///  - Retuns List of consumer devices if found.
        /// </returns>
        Task<GetConsumerDeviceResponseDto> GetConsumerDevices(GetConsumerDeviceRequestDto getConsumerDeviceRequestDto);
    }
}
