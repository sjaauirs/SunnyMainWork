using SunnyRewards.Helios.Task.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Task.Infrastructure.Services.Interface
{
    public interface ITriviaQuestionService
    {
        /// <summary>
        /// Gets all trivia questions.
        /// </summary>
        /// <returns></returns>
        Task<TriviaQuestionResponseDto> GetAllTriviaQuestions(string? languageCode);

        /// <summary>
        /// Updates the trivia question.
        /// </summary>
        /// <param name="triviaQuestionCode">The trivia question code.</param>
        /// <param name="updateRequest">The update request.</param>
        /// <returns></returns>
        Task<TriviaQuestionUpdateResponseDto> UpdateTriviaQuestion(string triviaQuestionCode, TriviaQuestionData updateRequest);
    }

}
