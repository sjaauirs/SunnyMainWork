using SunnyBenefits.Fis.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace Sunny.Benefits.Bff.Core.Domain.Dtos
{
    public class FisGetClientConfigResponseDto : BaseResponseDto
    {
        public ServicePayloadClientConfiguration FisResponse { get; set; }
    }

    public class ServicePayloadClientConfiguration
    {
        public ServiceResponse ServiceResponse { get; set; }

        public ClientConfigurationData ClientConfigurationData { get; set; }
    }

    public class ServiceResponse
    {
        public int ErrorNumber { get; set; }
        public string ErrorDescription { get; set; }
        public string ExceptionType { get; set; }
        public int Duration { get; set; }
        public DateTime RequestReceived { get; set; }
        public DateTime ResponseSent { get; set; }
    }

    public class ClientConfigurationData
    {
        public bool Isconfigured { get; set; }
        public string Fees { get; set; }

        public ConfigRecordData ConfigRecordData { get; set; }
    }

    public class ConfigRecordData
    {
        public ConfigRecord ConfigRecord { get; set; }
    }

    public class ConfigRecord
    {
        public string LanguageData { get; set; }

        public MessageData MessageData { get; set; }

        public string ReturnAddressData { get; set; }
    }

    public class AccessLevelConfigData
    {
        public AccessLevelConfig AccessLevelConfig { get; set; }
    }

    public class AccessLevelConfig
    {
        public int MsgOptionId { get; set; }
        public string AccessLvlOverride { get; set; }
        public Relation Relation { get; set; }
    }

    public class Relation
    {
        public string Description { get; set; }
        public int Value { get; set; }
    }
}
