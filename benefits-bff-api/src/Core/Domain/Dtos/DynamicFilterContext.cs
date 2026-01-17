using System.Diagnostics.CodeAnalysis;

namespace Sunny.Benefits.Bff.Core.Domain.Dtos
{
    [ExcludeFromCodeCoverage]
    public class DynamicFilterContext
    {
        public ConsumerFilter? Consumer { get; set; }
    }
}
