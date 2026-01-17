using AutoMapper;
using FluentNHibernate.Conventions.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SunnyRewards.Helios.Task.Core.Domain.Constants;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Task.Infrastructure.Services.Interface;

namespace SunnyRewards.Helios.Task.Infrastructure.Services
{
    public class TriviaQuestionService : ITriviaQuestionService
    {
        private readonly ITriviaQuestionRepo _triviaQuestionRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<TriviaQuestionService> _logger;
        private readonly ITriviaService _triviaService;
        const string className = nameof(TriviaQuestionService);
        public TriviaQuestionService(
            ITriviaQuestionRepo triviaQuestionRepo,
            IMapper mapper,
            ILogger<TriviaQuestionService> logger , ITriviaService triviaService)
        {
            _triviaQuestionRepo = triviaQuestionRepo;
            _mapper = mapper;
            _logger = logger;
            _triviaService = triviaService;
        }

        /// <summary>
        /// Gets all trivia questions.
        /// </summary>
        /// <returns></returns>
        public async Task<TriviaQuestionResponseDto> GetAllTriviaQuestions(string? languageCode)
        {
            var triviaQuestions = await _triviaQuestionRepo.FindAsync(x => x.DeleteNbr == 0);

            foreach (var triviaQuestion in triviaQuestions)
            {
                triviaQuestion.TriviaJson = _triviaService.FilterTriviaJsonByLanguage(triviaQuestion.TriviaJson, languageCode);
            }
            return new TriviaQuestionResponseDto
            {
                TriviaQuestions = _mapper.Map<List<TriviaQuestionData>>(triviaQuestions)
            };
        }

        /// <summary>
        /// Updates the trivia question.
        /// </summary>
        /// <param name="triviaQuestionCode">The trivia question code.</param>
        /// <param name="updateRequest">The update request.</param>
        /// <returns></returns>
        public async Task<TriviaQuestionUpdateResponseDto> UpdateTriviaQuestion(string triviaQuestionCode, TriviaQuestionData updateRequest)
        {
            const string methodName = nameof(UpdateTriviaQuestion);
            try
            {
                if (triviaQuestionCode != updateRequest.TriviaQuestionCode)
                {
                    _logger.LogError("{ClassName}.{MethodName}: TriviaQuestionCode mismatch between path and body. Path: {PathCode}, Body: {BodyCode}",
                        className, methodName, triviaQuestionCode, updateRequest.TriviaQuestionCode);
                    return new TriviaQuestionUpdateResponseDto { ErrorCode = StatusCodes.Status400BadRequest, ErrorMessage = "TriviaQuestionCode in the path and body must match." };
                }
                //update do not need laguage check
                var triviaQuestion = await _triviaQuestionRepo.FindOneAsync(x => x.TriviaQuestionCode == triviaQuestionCode && x.DeleteNbr == 0);
                if (triviaQuestion == null)
                {
                    _logger.LogError("{ClassName}.{MethodName}: TriviaQuestion not found for Code: {TriviaQuestionCode}", className, methodName, triviaQuestionCode);
                    return new TriviaQuestionUpdateResponseDto
                    {
                        IsSuccess = false,
                        ErrorCode = StatusCodes.Status404NotFound,
                        ErrorMessage = "Trivia question not found."
                    };
                }

                // Update properties
                triviaQuestion.QuestionExternalCode = updateRequest.QuestionExternalCode;
                triviaQuestion.TriviaJson = updateRequest.TriviaJson;
                triviaQuestion.UpdateUser = updateRequest.UpdateUser ?? Constant.SystemUser;
                triviaQuestion.UpdateTs = DateTime.UtcNow;

                await _triviaQuestionRepo.UpdateAsync(triviaQuestion);

                _logger.LogInformation("{ClassName}.{MethodName}: TriviaQuestion updated successfully for Code: {TriviaQuestionCode}", className, methodName, triviaQuestionCode);
                return new TriviaQuestionUpdateResponseDto { IsSuccess = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: Error while updating TriviaQuestion. Code: {TriviaQuestionCode}, Error: {ErrorMessage}",
                    className, methodName, triviaQuestionCode, ex.Message);
                return new TriviaQuestionUpdateResponseDto
                {
                    IsSuccess = false,
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = "An error occurred while updating trivia question."
                };
            }
        }

    }
}
