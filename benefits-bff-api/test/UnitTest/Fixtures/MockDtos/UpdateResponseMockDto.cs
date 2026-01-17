using Sunny.Benefits.Bff.Core.Domain.Dtos;

namespace Sunny.Benefits.Bff.UnitTest.Fixtures.MockDtos
{
    public class UpdateResponseMockDto : UpdateResponseDto
    {
        public UpdateResponseMockDto()
        {
            created_at = DateTime.Now;
            email = "test29@sunnyrewards.com";
            email_verified = true;
            identities = new List<Identity>
            {
                new Identity
                {
                     user_id ="auth0|6564a33ce3178efd5b0e9892",
                     provider ="testt",
                      connection="ok",
                     isSocial = true,
                },
            };
            name = "test";
            nickname = "sunny";
            picture = "https://example.com/profile-picture.jpg";
            updated_at = DateTime.UtcNow;
            user_id = "auth0|6564a33ce3178efd5b0e9892";
            UserMetadata user_metadata = new UserMetadata()
            {
                member_nbr = "mbr-6564a33ce3178efd5b0e9892"
            };
            AppMetadata app_metadata = new AppMetadata()
            {
                ConsumerCode = "cmr-f9c419da974c4bbb99eab99fd3b490e0",
                Env = "test",
                Role = "admin",
                TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4"
            };
            last_ip = "";
            last_login = DateTime.UtcNow;
            logins_count = 5;
        }
    }
}
