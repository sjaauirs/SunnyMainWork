using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Models;

namespace SunnyRewards.Helios.Task.Infrastructure.Helpers.Interface
{
    public interface IQuestionnaireHelper
    {
        string? FilterQuestionnaireJsonByLanguage(string? questionnaireJson, string? language);
        System.Threading.Tasks.Task GetTaskRewardDetails(GetQuestionnaireResponseDto response, TaskRewardModel? ctaTaskreward, string? languageCode);

        string? NormalizeJsonInput(object input);
    }
}
