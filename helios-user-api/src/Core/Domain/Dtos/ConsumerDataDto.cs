namespace SunnyRewards.Helios.User.Core.Domain.Dtos
{
    public class ConsumerDataDto
    {
        public PersonDto Person { get; set; } = new PersonDto();
        public List<PersonAddressDto> PersonAddresses { get; set; } = new List<PersonAddressDto>();
        public List<PhoneNumberDto> PhoneNumbers { get; set; } = new List<PhoneNumberDto>();
        public ConsumerDto Consumer  { get; set; } = new ConsumerDto();
    }
}
