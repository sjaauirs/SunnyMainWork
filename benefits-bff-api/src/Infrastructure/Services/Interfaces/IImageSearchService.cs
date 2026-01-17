using Sunny.Benefits.Bff.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sunny.Benefits.Bff.Infrastructure.Services.Interfaces
{
    public interface IImageSearchService
    {
        Task<ImageSearchResponseDto> AnalyseImageSearch(ImageSearchRequestDto requestDto);

    }
}
