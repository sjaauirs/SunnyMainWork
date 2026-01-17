using System.Xml.Serialization;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace Sunny.Benefits.Bff.Core.Domain.Dtos
{
    public class FisGetNotificationsEnrollmentResponseDto : BaseResponseDto
    {
        public string EnrollmentUid { get; set; }
        public List<FisNotificationsEnrollment> EnrolledNotifications { get; set; }
    }
    public class FisNotificationsEnrollment
    {
        public string MessageId { get; set; }
        public string MessageType { get; set; }
        public string MessageDescription { get; set; }
    }
}
