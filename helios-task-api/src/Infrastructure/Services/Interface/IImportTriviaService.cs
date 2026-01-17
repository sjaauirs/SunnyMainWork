using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Task.Infrastructure.Services.Interface
{
    public interface IImportTriviaService
    {
        Task<BaseResponseDto> ImportTrivia(ImportTriviaRequestDto triviaRequestDto);

    }
}
