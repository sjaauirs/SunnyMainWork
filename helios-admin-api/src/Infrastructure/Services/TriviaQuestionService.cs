using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Admin.Core.Domain.Constants;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services
{
    public class TriviaQuestionService : ITriviaQuestionService
    {
        public readonly ILogger<TriviaQuestionService> _logger;
        public readonly ITaskClient _taskClient;
        public const string className = nameof(TriviaQuestionService);

        public TriviaQuestionService(ILogger<TriviaQuestionService> logger, ITaskClient taskClient)
        {
            _logger = logger;
            _taskClient = taskClient;
        }
        public async Task<BaseResponseDto> CreateTriviaQuestion(TriviaQuestionRequestDto triviaQuestionDto)
        {
            const string methodName = nameof(CreateTriviaQuestion);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName}: Create triviaQuestion process started for code: {TaskCode}", className, methodName, triviaQuestionDto.TriviaQuestionCode);

                var taskResponse = await _taskClient.Post<BaseResponseDto>(Constant.CreateTriviaQuestionRequest, triviaQuestionDto);
                if (taskResponse.ErrorCode != null)
                {
                    _logger.LogWarning("{ClassName}.{MethodName}: Error occurred while creating triviaQuestion, triviaQuestioncode: {Trivia}, ErrorCode: {ErrorCode}", className, methodName, triviaQuestionDto.TriviaQuestionCode, taskResponse.ErrorCode);
                    return taskResponse;
                }
                _logger.LogInformation("{ClassName}.{MethodName}: triviaQuestion created successfully, TriviaQuestionCode: {TaskCode}", className, methodName, triviaQuestionDto.TriviaQuestionCode);
                return new BaseResponseDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: Exception occurred while creating triviaQuestion. ErrorMessage: {ErrorMessage}, StackTrace: {StackTrace},", className, methodName, ex.Message, ex.StackTrace);
                throw;
            }
        }

        /// <summary>
        /// Gets all trivia questions.
        /// </summary>
        /// <returns></returns>
        public async Task<TriviaQuestionResponseDto> GetAllTriviaQuestions(string? languageCode)
        {
            IDictionary<string, string> parameters = new Dictionary<string, string>();
            return await _taskClient.GetId<TriviaQuestionResponseDto>($"{Constant.TriviaQuestionsAPIUrl}?languageCode={languageCode}", parameters);
        }

        /// <summary>
        /// Updates the trivia question.
        /// </summary>
        /// <param name="triviaQuestionCode">The trivia question code.</param>
        /// <param name="updateRequest">The update request.</param>
        /// <returns></returns>
        public async Task<TriviaQuestionUpdateResponseDto> UpdateTriviaQuestion(string triviaQuestionCode, TriviaQuestionData updateRequest)
        {
            return await _taskClient.Put<TriviaQuestionUpdateResponseDto>($"{Constant.TriviaQuestionsAPIUrl}/{triviaQuestionCode}", updateRequest);
        }
    }

}
