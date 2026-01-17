using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using NHibernate;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Common.Core.Services;
using SunnyRewards.Helios.Task.Core.Domain.Constants;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Models;
using SunnyRewards.Helios.Task.Infrastructure.Helpers.Interface;
using SunnyRewards.Helios.Task.Infrastructure.Repositories;
using SunnyRewards.Helios.Task.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Task.Infrastructure.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Task.Infrastructure.Services
{
    public class QuestionnaireService : BaseService, IQuestionnaireService
    {
        private readonly ILogger<QuestionnaireService> _questionnaireLogger;
        private readonly IMapper _mapper;
        private readonly IQuestionnaireQuestionRepo _questionnaireQuestionRepo;
        private readonly IQuestionnaireRepo _questionnaireRepo;
        private readonly IQuestionnaireQuestionGroupRepo _questionnaireQuestionGroupRepo;
        private readonly ITaskRewardRepo _taskRewardRepo;
        private readonly ITaskRewardService _taskRewardService;
        private readonly IConsumerTaskRepo _consumerTaskRepo;
        private readonly NHibernate.ISession _session;
        private readonly IQuestionnaireHelper _questionnaireHelper;
        const string className = nameof(QuestionnaireService);

        public QuestionnaireService(ILogger<QuestionnaireService> questionnaireLogger, IMapper mapper, IQuestionnaireQuestionRepo questionnaireQuestionRepo,
            IQuestionnaireRepo questionnaireRepo, IQuestionnaireQuestionGroupRepo questionnaireQuestionGroupRepo, ITaskRewardRepo taskRewardRepo,
            ITaskRewardService taskRewardService, IConsumerTaskRepo consumerTaskRepo, NHibernate.ISession session, IQuestionnaireHelper questionnaireHelper)
        {
            _questionnaireLogger = questionnaireLogger;
            _mapper = mapper;
            _questionnaireQuestionRepo = questionnaireQuestionRepo;
            _questionnaireRepo = questionnaireRepo;
            _questionnaireQuestionGroupRepo = questionnaireQuestionGroupRepo;
            _taskRewardRepo = taskRewardRepo;
            _taskRewardService = taskRewardService;
            _consumerTaskRepo = consumerTaskRepo;
            _session = session;
            _questionnaireHelper = questionnaireHelper;
        }

        public async Task<GetQuestionnaireResponseDto> GetQuestionnaire(long taskRewardId, string consumerCode, string? languageCode)
        {
            const string methodName = nameof(GetQuestionnaire);
            var response = new GetQuestionnaireResponseDto();
            DateTime tsNow = DateTime.UtcNow;
            try
            {
                var questionnaire = await _questionnaireRepo.FindOneAsync(x => x.TaskRewardId == taskRewardId);

                if (questionnaire == null)
                {
                    _questionnaireLogger.LogError("{className}.{methodName}: Questionnaire not found for given taskRewardId: {taskRewardId},Error Code:{errorCode}", className, methodName, taskRewardId, StatusCodes.Status404NotFound);
                    return new GetQuestionnaireResponseDto()
                    {
                        ErrorCode = StatusCodes.Status404NotFound
                    };
                }
                var questionnaireQuestionGroup = await _questionnaireQuestionGroupRepo.FindAsync(x => x.QuestionnaireId == questionnaire.QuestionnaireId && x.DeleteNbr == 0 && DateTime.UtcNow >= x.ValidStartTs && DateTime.UtcNow < x.ValidEndTs);
                var questionnaireQuestionGroupSequence = questionnaireQuestionGroup.OrderBy(x => x.SequenceNbr);
                var res = new List<QuestionnaireQuestionDto>();
                foreach (var qqg in questionnaireQuestionGroupSequence)
                {
                    var questionnaireQuestions = await _questionnaireQuestionRepo.FindAsync(x => x.QuestionnaireQuestionId == qqg.QuestionnaireQuestionId);
                    var questionsDto = _mapper.Map<List<QuestionnaireQuestionDto>>(questionnaireQuestions);
                    res.AddRange(questionsDto);
                }

                foreach (var questionnaireQuestion in res)
                {
                    questionnaireQuestion.QuestionnaireJson = _questionnaireHelper.FilterQuestionnaireJsonByLanguage(questionnaireQuestion.QuestionnaireJson, languageCode);
                }

                _questionnaireLogger.LogInformation("{className}.{methodName}: successfully retrieved data from  GetQuestionnaire API for taskRewardId: {taskRewardId}", className, methodName, taskRewardId);

                QuestionnaireQuestionDto[] array = res.ToArray();
                response.Questions = array;

                var questionnaireDto = _mapper.Map<QuestionnaireDto>(questionnaire);

                TaskRewardModel? ctaTaskreward = null;
                ConsumerTaskModel? ctaConsumerTaskResponse = null;

                if (questionnaire.CtaTaskExternalCode != null)
                {
                    var questionnaireTaskReward = await _taskRewardRepo.FindOneAsync(x => x.TaskRewardId == questionnaire.TaskRewardId);

                    ctaTaskreward = await _taskRewardRepo.FindOneAsync(x => x.TenantCode == questionnaireTaskReward.TenantCode &&
                        x.TaskExternalCode == questionnaire.CtaTaskExternalCode);

                    if (ctaTaskreward != null)
                    {
                        var consumerTasks = await _consumerTaskRepo.FindAsync(x => x.TaskId == ctaTaskreward.TaskId &&
                            x.ConsumerCode == consumerCode && x.DeleteNbr == 0);
                        // Filtering latest consumertask
                        ctaConsumerTaskResponse = consumerTasks.OrderByDescending(x => x.ConsumerTaskId).FirstOrDefault();
                    }
                    else
                    {
                        _questionnaireLogger.LogError("{className}.{methodName}: Consumer: {consumerCode}, " +
                            "CTA task external code is not null: {ctaTaskExternalCode} but unable to find the TaskReward for it, Error Code:{errorCode}", className, methodName,
                            consumerCode, questionnaire.CtaTaskExternalCode, StatusCodes.Status404NotFound);
                    }
                }

                response.Questionnaire = questionnaireDto;

                if (ctaConsumerTaskResponse == null && ctaTaskreward != null)
                {
                    await _questionnaireHelper.GetTaskRewardDetails(response, ctaTaskreward, languageCode);
                }
                else if (ctaConsumerTaskResponse != null && ctaConsumerTaskResponse.TaskStatus != "COMPLETED")
                {
                    await _questionnaireHelper.GetTaskRewardDetails(response, ctaTaskreward, languageCode);
                }
                return response;
            }
            catch (Exception ex)
            {
                _questionnaireLogger.LogError(ex, "{className}.{methodName}: ERROR Msg:{msg},Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return new GetQuestionnaireResponseDto()
                {
                    ErrorCode = StatusCodes.Status500InternalServerError
                };
            }
        }
        public async Task<BaseResponseDto> UpdateQuestionnaire(QuestionnaireRequestDto requestDto)
        {
            const string methodName = nameof(UpdateQuestionnaire);
            try
            {
                if (requestDto == null)
                {
                    _questionnaireLogger.LogError("{className}.{methodName}: Failed to Saved data for Questionnaire for request: {requestDto}", className, methodName, requestDto?.ToJson());
                    return new BaseResponseDto { ErrorCode = StatusCodes.Status404NotFound, ErrorMessage = "Questionnaire record Not Found" };
                }
                var taskRewardModel = await _taskRewardRepo.FindOneAsync(x => x.TaskRewardCode == requestDto.TaskRewardCode && x.DeleteNbr == 0);

                if (taskRewardModel == null)
                {
                    _questionnaireLogger.LogInformation("{className}.{methodName}:  Task reward record not found for request: {requestDto}", className, methodName, requestDto?.ToJson());
                    return new BaseResponseDto { ErrorCode = StatusCodes.Status404NotFound, ErrorMessage = "Task reward record not found" };

                }
                var questionnaire = await _questionnaireRepo.FindOneAsync(x => x.QuestionnaireCode == requestDto.questionnaire.QuestionnaireCode && x.DeleteNbr == 0);
                if (questionnaire == null)
                {
                    _questionnaireLogger.LogInformation("{className}.{methodName}: record  does not exists  for request: {requestDto}", className, methodName, requestDto.ToJson());
                    return new BaseResponseDto { ErrorCode = StatusCodes.Status409Conflict, ErrorMessage = "Questionnaire does not exists" };
                }

                //questionnaire = _mapper.Map<QuestionnaireModel>(requestDto.questionnaire);
                questionnaire.UpdateTs = DateTime.Now;
                questionnaire.CtaTaskExternalCode = requestDto.questionnaire.CtaTaskExternalCode;
                questionnaire.TaskRewardId = taskRewardModel.TaskRewardId;
                if (questionnaire.ConfigJson != null)
                {
                    // Normalize ConfigJson if it's a JSON string otherwise, serialize the object to JSON
                    questionnaire.ConfigJson = requestDto.questionnaire.ConfigJson is string jsonString
                                                            ? _questionnaireHelper.NormalizeJsonInput(jsonString)
                                                            : JsonConvert.SerializeObject(requestDto.questionnaire.ConfigJson);
                }
                questionnaire.UpdateUser = requestDto?.questionnaire.UpdateUser ?? Constant.SystemUser;
                questionnaire = await _questionnaireRepo.UpdateAsync(questionnaire);
                if (questionnaire.QuestionnaireId > 0)
                {
                    _questionnaireLogger.LogInformation("{className}.{methodName}: Successfully Updated data for questionnaire request: {requestDto}", className, methodName, requestDto.ToJson());
                    return new BaseResponseDto();
                }
                else
                {
                    _questionnaireLogger.LogError("{className}.{methodName}: Failed to Update data for Questionnaire request: {requestDto}", className, methodName, requestDto.ToJson());
                    return new BaseResponseDto { ErrorCode = StatusCodes.Status404NotFound, ErrorMessage = "Questionnaire record Not Updated" };
                }

            }
            catch (Exception ex)
            {
                _questionnaireLogger.LogError(ex, "{className}.{methodName}: ERROR - msg : {msg}, for requestDto: {RequestDto}", className, methodName, ex.Message, requestDto.ToJson());
                return new BaseResponseDto { ErrorCode = StatusCodes.Status500InternalServerError, ErrorMessage = "Questionnaire record Not Updated" };

            }
        }

        public async Task<BaseResponseDto> CreateQuestionnaire(QuestionnaireRequestDto requestDto)
        {
            const string methodName = nameof(CreateQuestionnaire);
            try
            {
                if (requestDto == null)
                {
                    _questionnaireLogger.LogError("{className}.{methodName}: Failed to Saved data for Questionnaire for request: {requestDto}", className, methodName, requestDto?.ToJson());
                    return new BaseResponseDto { ErrorCode = StatusCodes.Status404NotFound, ErrorMessage = "Questionnaire record Not Found" };
                }
                var taskRewardModel = await _taskRewardRepo.FindOneAsync(x => x.TaskRewardCode == requestDto.TaskRewardCode && x.DeleteNbr == 0);

                if (taskRewardModel == null)
                {
                    _questionnaireLogger.LogInformation("{className}.{methodName}:  Task reward record not found for request: {requestDto}", className, methodName, requestDto?.ToJson());
                    return new BaseResponseDto { ErrorCode = StatusCodes.Status404NotFound, ErrorMessage = "Task reward record not found" };

                }
                var questionnaireModel = await _questionnaireRepo.FindOneAsync(x => x.TaskRewardId == taskRewardModel.TaskRewardId && x.DeleteNbr == 0);
                if (questionnaireModel != null)
                {
                    _questionnaireLogger.LogInformation("{className}.{methodName}: record exists for request: {requestDto}", className, methodName, requestDto.ToJson());
                    return new BaseResponseDto { ErrorCode = StatusCodes.Status409Conflict, ErrorMessage = "Task External Mapping already exists" };
                }

                QuestionnaireModel questionnaire = new QuestionnaireModel();
                questionnaire = _mapper.Map<QuestionnaireModel>(requestDto.questionnaire);
                questionnaire.CreateTs = DateTime.Now;
                questionnaire.TaskRewardId = taskRewardModel.TaskRewardId;
                if(questionnaire.ConfigJson != null)
                {
                    // Normalize ConfigJson if it's a JSON string; otherwise, serialize the object to JSON
                    questionnaire.ConfigJson = requestDto.questionnaire.ConfigJson is string jsonString
                                                            ? _questionnaireHelper.NormalizeJsonInput(jsonString)
                                                            : JsonConvert.SerializeObject(requestDto.questionnaire.ConfigJson);
                }
                questionnaire.CreateUser = requestDto?.questionnaire.CreateUser ?? Constant.SystemUser;
                questionnaire = await _questionnaireRepo.CreateAsync(questionnaire);
                if (questionnaire.QuestionnaireId > 0)
                {
                    _questionnaireLogger.LogInformation("{className}.{methodName}: Successfully Saved data for questionnaire request: {requestDto}", className, methodName, requestDto.ToJson());
                    return new BaseResponseDto();
                }
                else
                {
                    _questionnaireLogger.LogError("{className}.{methodName}: Failed to Saved data for Questionnaire request: {requestDto}", className, methodName, requestDto.ToJson());
                    return new BaseResponseDto { ErrorCode = StatusCodes.Status404NotFound, ErrorMessage = "Questionnaire record Not Created" };
                }

            }
            catch (Exception ex)
            {
                _questionnaireLogger.LogError(ex, "{className}.{methodName}: ERROR - msg : {msg}, for requestDto: {RequestDto}", className, methodName, ex.Message, requestDto.ToJson());
                return new BaseResponseDto { ErrorCode = StatusCodes.Status500InternalServerError, ErrorMessage = "Task External Mapping Not Created" };

            }
        }

        public async Task<BaseResponseDto> CreateQuestionnaireQuestionGroup(QuestionnaireQuestionGroupRequestDto requestDto)
        {
            const string methodName = nameof(CreateQuestionnaireQuestionGroup);
            try
            {
                if (requestDto == null)
                {
                    _questionnaireLogger.LogError("{className}.{methodName}: Failed to Saved data for Questionnaire Question group for request: {requestDto}", className, methodName, requestDto?.ToJson());
                    return new BaseResponseDto { ErrorCode = StatusCodes.Status404NotFound, ErrorMessage = "Questionnaire Question group record Not Found" };
                }


                var questionnaireQuestionModel = await _questionnaireQuestionRepo.FindOneAsync(x => x.QuestionnaireQuestionCode == requestDto.QuestionnaireQuestionCode && x.DeleteNbr == 0);
                var questionnaireModel = await _questionnaireRepo.FindOneAsync(x => x.QuestionnaireCode == requestDto.QuestionnaireCode && x.DeleteNbr == 0);
                if (questionnaireQuestionModel == null || questionnaireModel == null)
                {
                    _questionnaireLogger.LogInformation("{className}.{methodName}: Questionnaire Question and Questionnaire record not found for request: {requestDto}", className, methodName, requestDto.ToJson());
                    return new BaseResponseDto { ErrorCode = StatusCodes.Status404NotFound, ErrorMessage = " Questionnaire Question and Questionnaire record not found" };

                }
                var questionnaireQuestionGroupModel = await _questionnaireQuestionGroupRepo.FindOneAsync(x => x.QuestionnaireId == questionnaireModel.QuestionnaireId && x.QuestionnaireQuestionId == questionnaireQuestionModel.QuestionnaireQuestionId && x.DeleteNbr == 0);
                if (questionnaireQuestionGroupModel != null)
                {
                    _questionnaireLogger.LogInformation("{className}.{methodName}: record exists for request: {requestDto}", className, methodName, requestDto.ToJson());
                    return new BaseResponseDto { ErrorCode = StatusCodes.Status409Conflict, ErrorMessage = "Questionnaire Question group already exists" };
                }

                QuestionnaireQuestionGroupModel questionnaireQuestionGroup = new QuestionnaireQuestionGroupModel();
                questionnaireQuestionGroup = _mapper.Map<QuestionnaireQuestionGroupModel>(requestDto.QuestionnaireQuestionGroup);
                questionnaireQuestionGroup.CreateTs = DateTime.Now;
                questionnaireQuestionGroup.QuestionnaireQuestionId = questionnaireQuestionModel.QuestionnaireQuestionId;
                questionnaireQuestionGroup.QuestionnaireId = questionnaireModel.QuestionnaireId;
                questionnaireQuestionGroup.CreateUser = requestDto.QuestionnaireQuestionGroup.CreateUser ?? Constant.ImportUser;
                questionnaireQuestionGroup = await _questionnaireQuestionGroupRepo.CreateAsync(questionnaireQuestionGroup);
                if (questionnaireQuestionGroup.QuestionnaireQuestionGroupId > 0)
                {
                    _questionnaireLogger.LogInformation("{className}.{methodName}: Successfully Saved data for Questionnaire Question Group  request: {requestDto}", className, methodName, requestDto.ToJson());
                    return new BaseResponseDto();
                }
                _questionnaireLogger.LogError("{className}.{methodName}: Failed to Saved data for Questionnaire Question Group request: {requestDto}", className, methodName, requestDto.ToJson());
                return new BaseResponseDto { ErrorCode = StatusCodes.Status404NotFound, ErrorMessage = "Questionnaire Question Group record Not Created" };


            }
            catch (Exception ex)
            {
                _questionnaireLogger.LogError(ex, "{className}.{methodName}: ERROR - msg : {msg}, for requestDto: {RequestDto}", className, methodName, ex.Message, requestDto.ToJson());
                return new BaseResponseDto { ErrorCode = StatusCodes.Status500InternalServerError, ErrorMessage = "Questionnaire Question Group Not Created" };

            }
        }
    }
}
