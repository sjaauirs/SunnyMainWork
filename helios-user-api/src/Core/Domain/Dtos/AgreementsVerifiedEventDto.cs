using System.Runtime.Serialization;

namespace SunnyRewards.Helios.User.Core.Domain.Dtos
{
    public class AgreementsVerifiedEventDto : ISerializable
    {
        public string? AgreementStatus { get; set; }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("AgreementStatus", AgreementStatus);
        }
    }
}
