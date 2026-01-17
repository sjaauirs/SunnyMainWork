using System.Diagnostics.CodeAnalysis;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class ImageTaskProgressDto
    {
        public string DetailType { get; set; } = null!;
        public ImageCriteriaProgressDto ImageCriteriaProgress { get; set; } = null!;
    }
    public class ImageCriteriaProgressDto
    {
        public long UploadImageCount { get; set; }
        public IList<string> UploadImagePaths { get; set; } = null!;
    }
}
