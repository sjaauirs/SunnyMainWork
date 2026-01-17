using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Task.Infrastructure.Services.Interface
{
    public interface ITriviaQuestionGroupService
    {
        /// <summary>
        /// Gets the trivia question groups by trivia identifier.
        /// </summary>
        /// <param name="triviaId">The trivia identifier.</param>
        /// <returns></returns>
        Task<TriviaQuestionGroupResponseDto> GetTriviaQuestionGroupsByTriviaId(long triviaId);

        /// <summary>
        /// Updates the trivia question group.
        /// </summary>
        /// <param name="triviaQuestionGroupId">The trivia question group identifier.</param>
        /// <param name="updateRequest">The update request.</param>
        /// <returns></returns>
        Task<TriviaQuestionGroupUpdateResponseDto> UpdateTriviaQuestionGroup(long triviaQuestionGroupId, TriviaQuestionGroupDto updateRequest);

        /// <summary>
        /// Deletes the trivia question group.
        /// </summary>
        /// <param name="triviaQuestionGroupId">The trivia question group identifier.</param>
        /// <returns></returns>
        Task<BaseResponseDto> DeleteTriviaQuestionGroup(long triviaQuestionGroupId);
    }
}
