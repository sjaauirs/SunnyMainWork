using Sunny.Benefits.Bff.Infrastructure.Helpers.Interface;

namespace Sunny.Benefits.Bff.Infrastructure.Helpers
{
    public class CardOperationsHelper : ICardOperationsHelper
    {

        public string? ExtractCardStatusFromFisResponse(string? fisResponse)
        {
            if (string.IsNullOrEmpty(fisResponse))
            {
                return fisResponse;
            }

            if (fisResponse.Length > 0 && fisResponse.Trim().Substring(0, 1) != "0")
            {
                var outPutParams = fisResponse.Split('|');
                var cardSatatus = outPutParams[0];
                string[] words = cardSatatus.Split(' ');
                if (words.Length > 0)
                {
                    return words[words.Length - 1];
                }
            }
            return string.Empty;
        }
    }
}
