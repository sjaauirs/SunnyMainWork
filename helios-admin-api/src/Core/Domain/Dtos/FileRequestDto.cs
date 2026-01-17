using Microsoft.AspNetCore.Http;

namespace SunnyRewards.Helios.Admin.Core.Domain.Dtos
{
    public class FileRequestDto
    {
        public string? FileType { get; set; }
        public string? FileName { get; set; }
        public IFormFile? File { get; set; }
    }
}
