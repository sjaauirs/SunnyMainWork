using SunnyRewards.Helios.User.Core.Domain.Models;

namespace SunnyRewards.Helios.User.UnitTest.Fixtures.MockModels
{
    public class ConsumerMockModel : ConsumerModel
    {
        public ConsumerMockModel()
        {
            ConsumerId = 2;
            PersonId = 2;
            TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4";
            ConsumerCode = "cmr-c69905fc68ce4f36851f877bae38f22e";
            RegistrationTs = DateTime.UtcNow;
            EligibleStartTs = DateTime.UtcNow;
            EligibleEndTs = DateTime.UtcNow;
            CreateTs = DateTime.UtcNow;
            UpdateTs = DateTime.UtcNow;
            CreateUser = "Parshant";
            UpdateUser = "Parshant Sood";
            DeleteNbr = 0;
            Registered = true;
            Eligible = true;
            MemberNbr = "6c267e72-b55d-4ab7-90e2-6ead89074e81";
            SubscriberMemberNbr = "6c267e72-b55d-4ab7-90e2-6ead89074e81";
            ConsumerAttribute = null;
            Person = new PersonMockModel();
        }
    }
}
