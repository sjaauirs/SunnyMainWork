using System.Diagnostics.CodeAnalysis;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    [ExcludeFromCodeCoverage]
    public class ImageTaskCompletionCriteraDto
    {
       public ImageCriteriaDto ImageCriteria { get; set; } = null!;
    }
    [ExcludeFromCodeCoverage]
    public class ImageCriteriaDto
    {
        public long RequiredImageCount { get; set; }
    }
}
