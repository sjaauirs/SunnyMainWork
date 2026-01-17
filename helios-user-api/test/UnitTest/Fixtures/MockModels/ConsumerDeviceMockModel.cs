using SunnyRewards.Helios.User.Core.Domain.Models;

namespace SunnyRewards.Helios.User.UnitTest.Fixtures.MockModels
{
    public class ConsumerDeviceMockModel : ConsumerDeviceModel
    {
        public ConsumerDeviceMockModel()
        {
            ConsumerDeviceCode = "cdc-eb0463946af74883a845e6903ca8b9da";
            TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4";
            ConsumerCode = "cmr-58b6d93e24284a0db8bbe7fd782362ed";
            DeviceIdHash = "986c0dc956dc822b5d8f698661b9eb1ef880786ff9043c16744d2a420e99e9bb";
            DeviceIdEnc = "vnOphbTxJOEhU/tAPe/U2zSbagWVRj4yOKXEwEgdGJp39irc9IB8cWMXP5OFZskg6gZP8JfNMQ3lGMEzbMxfhA==";
            DeviceType = "PHONE";
            DeviceAttrJson = "{\"screen_size\": \"6.7 inches\", \"screen_resolution\": \"2560x1440\", \"device_description\": \"Flagship smartphone with advanced camera\", \"device_platform\": \"Android\", \"platform_version\": \"14.0\"}";
            CreateTs = DateTime.UtcNow;
            CreateUser = "SYSTEM";
            DeleteNbr = 0;
        }
    }
}
