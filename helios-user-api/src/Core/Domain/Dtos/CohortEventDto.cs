using System.Runtime.Serialization;

namespace SunnyRewards.Helios.User.Core.Domain.Dtos
{
    public class CohortEventDto : ISerializable
    {
        public string EventId { get; set; } = null!;
        public string TenantCode { get; set; } = null!;
        public string ConsumerCode { get; set; } = null!;
        public string TriggeredBy { get; set; } = null!;

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("EventId", EventId);
            info.AddValue("TenantCode", TenantCode);
            info.AddValue("ConsumerCode",ConsumerCode);
            info.AddValue("TriggeredBy",TriggeredBy);
        }
    }
}
