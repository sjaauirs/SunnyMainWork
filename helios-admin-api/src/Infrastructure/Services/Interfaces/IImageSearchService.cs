using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SunnyRewards.Helios.Admin.Infrastructure.Services.ImageSearchService;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces
{
    public interface IImageSearchService
    {
        Task<ImageSearchResponseDto> AnalyzeImage(ImageSearchRequestDto imagePath);


    }
}
