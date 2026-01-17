using SunnyRewards.Helios.User.Core.Domain.Dtos;
using System.Diagnostics.CodeAnalysis;

namespace Sunny.Benefits.Bff.Core.Domain.Dtos
{
    [ExcludeFromCodeCoverage]
    public class ConsumerFilter : ConsumerDto
    {

        public int? EligibilityMonths
        {
            get
            {
                return DateTime.UtcNow.Month - EligibleStartTs.Month;
            }
        }
    }
}
