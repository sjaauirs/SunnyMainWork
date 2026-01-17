using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Task.Core.Domain.Constants;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Models;
using SunnyRewards.Helios.Task.Infrastructure.Helpers.Interface;
using SunnyRewards.Helios.Task.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Task.Infrastructure.Services.Interface;

namespace SunnyRewards.Helios.Task.Infrastructure.Services
{
    public class QuestionnaireQuestionService : IQuestionnaireQuestionService
    {
        private readonly IQuestionnaireQuestionRepo _questionnaireQuestionRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<QuestionnaireQuestionService> _logger;
        private readonly IQuestionnaireService _questionnaireService;
        private readonly IQuestionnaireHelper _questionnaireHelper;
        const string className = nameof(QuestionnaireQuestionService);

        public QuestionnaireQuestionService(
            IQuestionnaireQuestionRepo questionnaireQuestionRepo,
            IMapper mapper,
            ILogger<QuestionnaireQuestionService> logger, IQuestionnaireService questionnaireService,
            IQuestionnaireHelper questionnaireHelper)
        {
            _questionnaireQuestionRepo = questionnaireQuestionRepo;
            _mapper = mapper;
            _logger = logger;
            _questionnaireService = questionnaireService;
            _questionnaireHelper = questionnaireHelper;
        }

        /// <summary>
        /// Gets all questionnaire questions.
        /// </summary>
        /// <param name="languageCode"></param>
        /// <returns></returns>
        public async Task<QuestionnaireQuestionResponseDto> GetAllQuestionnaireQuestions(string? languageCode)
        {
            var questionnaireQuestions = await _questionnaireQuestionRepo.FindAsync(x => x.DeleteNbr == 0);

            foreach (var questionnaireQuestion in questionnaireQuestions)
            {
                questionnaireQuestion.QuestionnaireJson = _questionnaireHelper.FilterQuestionnaireJsonByLanguage(questionnaireQuestion.QuestionnaireJson, languageCode);
            }
            return new QuestionnaireQuestionResponseDto
            {
                QuestionnaireQuestions = _mapper.Map<List<QuestionnaireQuestionData>>(questionnaireQuestions)
            };
        }

        /// <summary>
        /// Updates the questionnaire question.
        /// </summary>
        /// <param name="questionnaireQuestionCode"></param>
        /// <param name="updateRequest"></param>
        /// <returns></returns>
        public async Task<QuestionnaireQuestionUpdateResponseDto> UpdateQuestionnaireQuestion(string questionnaireQuestionCode, QuestionnaireQuestionData updateRequest)
        {
            const string methodName = nameof(UpdateQuestionnaireQuestion);
            try
            {
                if (questionnaireQuestionCode != updateRequest.QuestionnaireQuestionCode)
                {
                    _logger.LogError("{ClassName}.{MethodName}: QuestionnaireQuestionCode mismatch between path and body. Path: {PathCode}, Body: {BodyCode}",
                        className, methodName, questionnaireQuestionCode, updateRequest.QuestionnaireQuestionCode);
                    return new QuestionnaireQuestionUpdateResponseDto { ErrorCode = StatusCodes.Status400BadRequest, ErrorMessage = "QuestionnaireQuestionCode in the path and body must match." };
                }
                //update do not need laguage check
                var questionnaireQuestion = await _questionnaireQuestionRepo.FindOneAsync(x => x.QuestionnaireQuestionCode == questionnaireQuestionCode && x.DeleteNbr == 0);
                if (questionnaireQuestion == null)
                {
                    _logger.LogError("{ClassName}.{MethodName}: QuestionnaireQuestion not found for Code: {QuestionnaireQuestionCode}", className, methodName, questionnaireQuestionCode);
                    return new QuestionnaireQuestionUpdateResponseDto
                    {
                        IsSuccess = false,
                        ErrorCode = StatusCodes.Status404NotFound,
                        ErrorMessage = "Questionnaire question not found."
                    };
                }

                if (updateRequest.QuestionnaireJson != null)
                {
                    // Normalize QuestionnaireJson if it's a JSON string otherwise, serialize the object to JSON
                    questionnaireQuestion.QuestionnaireJson = updateRequest.QuestionnaireJson is string jsonString
                                                            ? _questionnaireHelper.NormalizeJsonInput(jsonString)
                                                            : JsonConvert.SerializeObject(updateRequest.QuestionnaireJson);
                }

                // Update properties
                questionnaireQuestion.QuestionExternalCode = updateRequest.QuestionExternalCode;
                questionnaireQuestion.UpdateUser = updateRequest.UpdateUser ?? Constant.SystemUser;
                questionnaireQuestion.UpdateTs = DateTime.UtcNow;
                await _questionnaireQuestionRepo.UpdateAsync(questionnaireQuestion);

                _logger.LogInformation("{ClassName}.{MethodName}: QuestionnaireQuestion updated successfully for Code: {QuestionnaireQuestionCode}", className, methodName, questionnaireQuestionCode);
                return new QuestionnaireQuestionUpdateResponseDto { IsSuccess = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: Error while updating QuestionnaireQuestion. Code: {QuestionnaireQuestionCode}, Error: {ErrorMessage}",
                    className, methodName, questionnaireQuestionCode, ex.Message);
                return new QuestionnaireQuestionUpdateResponseDto
                {
                    IsSuccess = false,
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = "An error occurred while updating questionnaire question."
                };
            }
        }

        public async Task<BaseResponseDto> CreateQuestionnaireQuestion(QuestionnaireQuestionRequestDto questionnaireQuestionDto)
        {
            const string methodName = nameof(CreateQuestionnaireQuestion);
            try
            {
                if (questionnaireQuestionDto == null)
                {
                    _logger.LogError("{className}.{methodName}: Failed to Saved data for Questionnaire Question  for request: {requestDto}", className, methodName, questionnaireQuestionDto?.ToJson());
                    return new BaseResponseDto { ErrorCode = StatusCodes.Status404NotFound, ErrorMessage = "Questionnaire Question record Not Found" };
                }


                var questionnaireModel = await _questionnaireQuestionRepo.FindOneAsync(x => x.QuestionExternalCode == questionnaireQuestionDto.QuestionExternalCode && x.DeleteNbr == 0);
                if (questionnaireModel != null)
                {
                    _logger.LogInformation("{className}.{methodName}: record exists for request: {requestDto}", className, methodName, questionnaireQuestionDto.ToJson());
                    return new BaseResponseDto { ErrorCode = StatusCodes.Status409Conflict, ErrorMessage = "Questionnaire Question already exists" };
                }

                QuestionnaireQuestionModel questionnaireQuestion = new QuestionnaireQuestionModel();
                questionnaireQuestion = _mapper.Map<QuestionnaireQuestionModel>(questionnaireQuestionDto);
                questionnaireQuestion.CreateTs = DateTime.Now;
                if (questionnaireQuestionDto?.QuestionnaireJson != null)
                {
                    // Normalize QuestionnaireJson if it's a JSON string otherwise, serialize the object to JSON
                    questionnaireQuestion.QuestionnaireJson = questionnaireQuestionDto.QuestionnaireJson is string jsonString
                                                            ? _questionnaireHelper.NormalizeJsonInput(jsonString)
                                                            : JsonConvert.SerializeObject(questionnaireQuestionDto.QuestionnaireJson);
                }
                questionnaireQuestion.CreateUser = questionnaireQuestionDto?.CreateUser ?? Constant.SystemUser;
                questionnaireQuestion = await _questionnaireQuestionRepo.CreateAsync(questionnaireQuestion);
                if (questionnaireQuestion.QuestionnaireQuestionId <= 0)
                {
                    _logger.LogError("{className}.{methodName}: Failed to Saved data for Questionnaire Question  request: {requestDto}", className, methodName, questionnaireQuestionDto.ToJson());
                    return new BaseResponseDto { ErrorCode = StatusCodes.Status404NotFound, ErrorMessage = "Questionnaire Question record Not Created" };
                }
                _logger.LogInformation("{className}.{methodName}: Successfully Saved data for Questionnaire Question  request: {requestDto}", className, methodName, questionnaireQuestionDto.ToJson());
                return new BaseResponseDto();


            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{className}.{methodName}: ERROR - msg : {msg}, for requestDto: {RequestDto}", className, methodName, ex.Message, questionnaireQuestionDto.ToJson());
                return new BaseResponseDto { ErrorCode = StatusCodes.Status500InternalServerError, ErrorMessage = "Questionnaire Question Not Created" };

            }
        }

    }
}
