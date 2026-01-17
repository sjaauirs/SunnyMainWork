using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces
{
    public interface ITriviaQuestionService
    {
        Task<BaseResponseDto> CreateTriviaQuestion(TriviaQuestionRequestDto triviaQuestionDto);

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
