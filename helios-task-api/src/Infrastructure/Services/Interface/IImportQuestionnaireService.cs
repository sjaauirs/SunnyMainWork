using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Task.Infrastructure.Services.Interface
{
    public interface IImportQuestionnaireService
    {
        Task<BaseResponseDto> ImportQuestionnaire(ImportQuestionnaireRequestDto questionnaireRequestDto);
    }
}
