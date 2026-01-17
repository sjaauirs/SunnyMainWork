using SunnyRewards.Helios.Task.Core.Domain.Models;

namespace SunnyRewards.Helios.Task.UnitTest.Fixtures.MockModel
{
    public class TermsOfServiceMockModel : TermsOfServiceModel
    {
        public TermsOfServiceMockModel()

        {
            TermsOfServiceId = 1;
            TermsOfServiceText = "We provide you access and use of our websites, including and other Internet sites, mobile applications";
            LanguageCode = "en-us";
            CreateTs = DateTime.UtcNow;
            UpdateTs = DateTime.UtcNow;
            CreateUser = "sunny";
            UpdateUser = "sunny rewards";
            DeleteNbr = 0;
        }
        public List<TermsOfServiceModel> termofservice()
        {
            return new List<TermsOfServiceModel>()
            {
             new TermsOfServiceMockModel()
             };
        }
    }
}