using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Task.Infrastructure.Services.Interface
{
    public interface ITriviaService
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskRewardId"></param>
        /// <param name="consumerCode"></param>
        /// <returns></returns>
        Task<GetTriviaResponseDto> GetTrivia(long taskRewardId, string consumerCode, string? languageCode);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="postTaskProgressUpdateRequestDto"></param>
        /// <returns></returns>
        Task<PostTaskProgressUpdateResponseDto> TaskProgressUpdate(PostTaskProgressUpdateRequestDto postTaskProgressUpdateRequestDto);
        Task<BaseResponseDto> CreateTrivia(TriviaRequestDto triviaDto);
        Task<BaseResponseDto> CreateTriviaQuestion(TriviaQuestionRequestDto triviaQuestionDto);
        Task<BaseResponseDto> CreateTriviaQuestionGroup(TriviaQuestionGroupRequestDto requestDto);
        /// <summary>
        /// Gets all trivia.
        /// </summary>
        /// <returns></returns>
        Task<TriviaResponseDto> GetAllTrivia();
        Task<BaseResponseDto> UpdateTrivia(TriviaRequestDto requestDto);

        string? FilterTriviaJsonByLanguage(string? triviaJson, string? language);
    }
}
