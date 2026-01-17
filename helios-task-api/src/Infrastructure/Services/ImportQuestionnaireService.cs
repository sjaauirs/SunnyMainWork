using Microsoft.AspNetCore.Http;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Infrastructure.Services.Interface;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using SunnyRewards.Helios.Task.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Task.Infrastructure.Repositories;
using SunnyRewards.Helios.Task.Core.Domain.Constants;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;

namespace SunnyRewards.Helios.Task.Infrastructure.Services
{
    public class ImportQuestionnaireService : IImportQuestionnaireService
    {
        private readonly ILogger<ImportQuestionnaireService> _importLogger;
        private readonly IMapper _mapper;
        private readonly IQuestionnaireRepo _questionnaireRepo;
        private readonly IQuestionnaireQuestionGroupRepo _questionnaireQuestionGroupRepo;
        private readonly IQuestionnaireQuestionRepo _questionnaireQuestionRepo;
        private readonly ITaskService _taskService;
        private readonly IQuestionnaireQuestionGroupService _questionnaireQuesGroupService;
        private readonly IQuestionnaireQuestionService _questionnaireQuesService;
        private readonly IQuestionnaireService _questionnaireService;
        private readonly ITaskRewardRepo _taskRewardRepo;
        private readonly ImportQuestionnaireQuestionMappingDto questionnaireQuestionCodeMappingDto;
        private readonly List<ImportQuestionnaireQuestionDto> questionnaireCodeMapping;
        private readonly List<ImportQuestionnaireQuestionDto> questionCodeMapping;
        public string className = nameof(ImportQuestionnaireService);

        public ImportQuestionnaireService(ILogger<ImportQuestionnaireService> importLogger, IMapper mapper,
            IQuestionnaireRepo questionnaireRepo, IQuestionnaireQuestionGroupRepo questionnaireQuestionGroupRepo,
            IQuestionnaireQuestionRepo questionnaireQuestionRepo, ITaskService taskService,
            IQuestionnaireQuestionGroupService questionnaireQuesGroupService, IQuestionnaireQuestionService questionnaireQuesService,
            IQuestionnaireService questionnaireService, ITaskRewardRepo taskRewardRepo)
        {
            _importLogger = importLogger;
            _mapper = mapper;
            _questionnaireRepo = questionnaireRepo;
            _questionnaireQuestionGroupRepo = questionnaireQuestionGroupRepo;
            _questionnaireQuestionRepo = questionnaireQuestionRepo;
            _taskService = taskService;
            _questionnaireQuesGroupService = questionnaireQuesGroupService;
            _questionnaireQuesService = questionnaireQuesService;
            _questionnaireService = questionnaireService;
            _taskRewardRepo = taskRewardRepo;
            questionnaireQuestionCodeMappingDto = new ImportQuestionnaireQuestionMappingDto();
             questionnaireCodeMapping = new List<ImportQuestionnaireQuestionDto>();
             questionCodeMapping = new List<ImportQuestionnaireQuestionDto>();
        }

        public async Task<BaseResponseDto> ImportQuestionnaire(ImportQuestionnaireRequestDto questionnaireRequestDto)
        {
            const string methodName = nameof(ImportQuestionnaire);
            int count = 0;
            StringBuilder sb = new StringBuilder();
            try
            {
                if (questionnaireRequestDto.QuestionnaireDetailDto == null || questionnaireRequestDto.QuestionnaireDetailDto.Questionnaire == null
                    || questionnaireRequestDto.QuestionnaireDetailDto.Questionnaire?.Count <= 0 || string.IsNullOrEmpty(questionnaireRequestDto.TenantCode) ||
                    questionnaireRequestDto.QuestionnaireDetailDto?.QuestionnaireQuestionGroup == null || questionnaireRequestDto.QuestionnaireDetailDto?.Questionnaire == null ||
                    questionnaireRequestDto.QuestionnaireDetailDto?.QuestionnaireQuestion == null)
                {
                    _importLogger.LogError("{className}.{methodName}: no record found for questionnaire Question group {Questionnaire}", className, methodName, questionnaireRequestDto.ToJson());
                    sb.AppendLine("questionnaire Question group Error: No record found");
                    return new BaseResponseDto { ErrorCode = StatusCodes.Status404NotFound, ErrorMessage = "No record to import" };

                }
                BaseResponseDto response = new BaseResponseDto();
                response = await ImportQuestionnaireRecords(questionnaireRequestDto);
                if (response != null && response.ErrorCode != null)
                {
                    sb.AppendLine(response.ErrorMessage);
                    //return if questionnaire fails
                    return new BaseResponseDto { ErrorMessage = sb.Length > 0 ? sb.ToString() : null, ErrorCode = sb.Length > 0 ? response.ErrorCode : null };

                }
                questionnaireQuestionCodeMappingDto.QuestionnaireCodeMapping = questionnaireCodeMapping;
                response = await ImportQuestionnaireQuestionRecords(questionnaireRequestDto);
                if (response != null && response.ErrorCode != null)
                {
                    sb.AppendLine(response.ErrorMessage);
                }

                questionnaireQuestionCodeMappingDto.QuestionnaireQuestionMapping = questionCodeMapping;
                response = await ImportQuestionnaireQuestionGroupRecords(questionnaireRequestDto);
                if (response != null && response.ErrorCode != null)
                {
                    sb.AppendLine(response.ErrorMessage);
                }
                return new BaseResponseDto { ErrorMessage = sb.Length > 0 ? sb.ToString() : null, ErrorCode = sb.Length > 0 ? StatusCodes.Status406NotAcceptable : null };
            }

            catch (Exception ex)
            {

                _importLogger.LogError(ex, "{ClassName}.{MethodName}: Error Message: {Message}, Error Code: {ErrorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);

                return new TasksResponseDto
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = ex.Message
                };
            }
        }
        /// <summary>
        /// Import Questionnaire Records
        /// </summary>
        /// <param name="questionnaireRequestDto"></param>
        /// <returns></returns>
        public async Task<BaseResponseDto> ImportQuestionnaireRecords(ImportQuestionnaireRequestDto questionnaireRequestDto)
        {
            const string methodName = nameof(ImportQuestionnaireRecords);
            StringBuilder sb = new StringBuilder();

            _importLogger.LogInformation("{className}.{methodName}:Beginning questionnaire import", className, methodName);


            foreach (var questionnaireDto in questionnaireRequestDto.QuestionnaireDetailDto.Questionnaire)
            {
                try
                {
                    ImportQuestionnaireQuestionDto questionnaireMapp = new ImportQuestionnaireQuestionDto();

                    if (questionnaireDto?.Questionnaire == null && questionnaireDto?.TaskExternalCode == null)
                    {
                        _importLogger.LogError("{className}.{methodName}: Request doesn't contain Questionnaire data for import {TaskRequestDto}", className, methodName, questionnaireDto?.Questionnaire?.ToJson());
                        continue;
                    }
                    var taskRewardModel = await _taskRewardRepo.FindOneAsync(x => x.TaskExternalCode == questionnaireDto.TaskExternalCode && x.TenantCode == questionnaireRequestDto.TenantCode && x.DeleteNbr == 0);
                    questionnaireMapp.Id = questionnaireDto?.Questionnaire?.QuestionnaireId;
                    if (taskRewardModel == null)
                    {
                        _importLogger.LogError("{className}.{methodName}:  Task reward record not found for request: {requestDto}", className, methodName, questionnaireDto?.ToJson());

                        continue;
                    }
                    else
                    {
                        var questionnaireModel = await _questionnaireRepo.FindOneAsync(x => x.TaskRewardId == taskRewardModel.TaskRewardId && x.DeleteNbr == 0);
                        BaseResponseDto baseResponseDto = new BaseResponseDto();
                        Questionnaire QuestionnaireDto = _mapper.Map<Questionnaire>(questionnaireDto.Questionnaire);
                        QuestionnaireDto.TaskRewardId = taskRewardModel.TaskRewardId;
                        if (questionnaireModel != null)
                        {

                            QuestionnaireDto.QuestionnaireCode = questionnaireModel.QuestionnaireCode;
                            QuestionnaireDto.UpdateUser = Constant.ImportUser;
                            questionnaireMapp.Code = questionnaireModel.QuestionnaireCode;

                            QuestionnaireRequestDto questionnaire = new QuestionnaireRequestDto { TaskRewardCode = taskRewardModel.TaskRewardCode, questionnaire = QuestionnaireDto };
                            baseResponseDto = await _questionnaireService.UpdateQuestionnaire(questionnaire);
                            if (baseResponseDto.ErrorCode != null)
                            {
                                _importLogger.LogError("{className}.{methodName}: Error occurred while Updating questionnaire  {questionnaire}", className, methodName, questionnaireDto.Questionnaire?.ToJson());
                            }
                            else
                                _importLogger.LogInformation("{className}.{methodName}:successfully Updated questionnaire  {questionnaire}", className, methodName, questionnaireDto.Questionnaire?.ToJson());

                        }
                        else
                        {

                            QuestionnaireDto.CreateUser = Constant.ImportUser;
                            QuestionnaireDto.QuestionnaireCode = "qsr-" + Guid.NewGuid().ToString("N");
                            questionnaireMapp.Code = QuestionnaireDto.QuestionnaireCode;

                            QuestionnaireRequestDto questionnaire = new QuestionnaireRequestDto { TaskRewardCode = taskRewardModel.TaskRewardCode, questionnaire = QuestionnaireDto };
                            baseResponseDto = await _questionnaireService.CreateQuestionnaire(questionnaire);
                            if (baseResponseDto.ErrorCode != null)
                            {
                                _importLogger.LogError("{className}.{methodName}: Error occurred while Creating questionnaire  {questionnaire}", className, methodName, questionnaireDto.Questionnaire?.ToJson());
                            }
                            else
                                _importLogger.LogInformation("{className}.{methodName}:successfully Created questionnaire  {questionnaire}", className, methodName, questionnaireDto.Questionnaire?.ToJson());
                        }
                        questionnaireCodeMapping.Add(questionnaireMapp);
                    }
                }
                catch (Exception ex)
                {

                    _importLogger.LogError(ex, "{ClassName}.{MethodName}: Error Message: {Message}, Error Code: {ErrorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                    sb.AppendLine($"Questionnaire Error:  exception occurred {ex.Message}");
                }
            }
            return new BaseResponseDto { ErrorCode = sb.Length > 0 ? StatusCodes.Status406NotAcceptable : null, ErrorMessage = sb.Length > 0 ? sb.ToString() : null };

        }


        /// <summary>
        /// Import Questionnaire Question Records
        /// </summary>
        /// <param name="questionnaireRequestDto"></param>
        /// <returns></returns>
        public async Task<BaseResponseDto> ImportQuestionnaireQuestionRecords(ImportQuestionnaireRequestDto questionnaireRequestDto)
        {
            const string methodName = nameof(ImportQuestionnaireQuestionRecords);
            _importLogger.LogInformation("{className}.{methodName}:Beginning questionnaire Question import", className, methodName);
            StringBuilder sb = new StringBuilder();


            foreach (var QuestionnaireQuestionDto in questionnaireRequestDto.QuestionnaireDetailDto.QuestionnaireQuestion)
            {
                try
                {
                    ImportQuestionnaireQuestionDto questionnaireQuesMapp = new ImportQuestionnaireQuestionDto();

                    if (QuestionnaireQuestionDto == null)
                    {

                        _importLogger.LogError("{className}.{methodName}: Request doesn't contain Questionnaire data for import {TaskRequestDto}", className, methodName, QuestionnaireQuestionDto?.ToJson());
                        continue;
                    }
                    var questionnaireQuestionModel = await _questionnaireQuestionRepo.FindOneAsync(x => x.QuestionExternalCode == QuestionnaireQuestionDto.QuestionExternalCode && x.DeleteNbr == 0);
                    BaseResponseDto baseResponseDto = new BaseResponseDto();
                    questionnaireQuesMapp.Id = QuestionnaireQuestionDto.QuestionnaireQuestionId;

                    if (questionnaireQuestionModel == null)
                    {
                        QuestionnaireQuestionRequestDto QuestionnaireDto = _mapper.Map<QuestionnaireQuestionRequestDto>(QuestionnaireQuestionDto);
                        QuestionnaireDto.CreateUser = Constant.ImportUser;
                        QuestionnaireDto.QuestionnaireQuestionCode = "qsq-" + Guid.NewGuid().ToString("N");
                        questionnaireQuesMapp.Code = QuestionnaireDto.QuestionnaireQuestionCode;

                        baseResponseDto = await _questionnaireQuesService.CreateQuestionnaireQuestion(QuestionnaireDto);
                        if (baseResponseDto.ErrorCode != null)
                        {
                            _importLogger.LogError("{className}.{methodName}: Error occurred while Creating QuestionnaireQuestion  {QuestionnaireQuestion}", className, methodName, QuestionnaireQuestionDto.ToJson());
                        }
                        else
                            _importLogger.LogInformation("{className}.{methodName}:successfully Created questionnaire Question {QuestionnaireQuestion}", className, methodName, QuestionnaireQuestionDto.ToJson());
                    }
                    else
                    {
                        QuestionnaireQuestionData QuestionnaireQuestion = _mapper.Map<QuestionnaireQuestionData>(QuestionnaireQuestionDto);
                        QuestionnaireQuestion.QuestionnaireQuestionCode = questionnaireQuestionModel.QuestionnaireQuestionCode;
                        questionnaireQuesMapp.Code = questionnaireQuestionModel.QuestionnaireQuestionCode;

                        baseResponseDto = await _questionnaireQuesService.UpdateQuestionnaireQuestion(questionnaireQuestionModel.QuestionnaireQuestionCode, QuestionnaireQuestion);
                        if (baseResponseDto.ErrorCode != null)
                        {
                            _importLogger.LogError("{className}.{methodName}: Error occurred while Updating QuestionnaireQuestion  {QuestionnaireQuestion}", className, methodName, QuestionnaireQuestionDto.ToJson());

                        }
                        else
                            _importLogger.LogInformation("{className}.{methodName}:successfully Updated QuestionnaireQuestion   {QuestionnaireQuestion}", className, methodName, QuestionnaireQuestionDto.ToJson());

                    }
                    questionCodeMapping.Add(questionnaireQuesMapp);


                }
                catch (Exception ex)
                {

                    _importLogger.LogError(ex, "{ClassName}.{MethodName}: Error Message: {Message}, Error Code: {ErrorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                    sb.AppendLine($"Questionnaire Question Error:  exception occurred {ex.Message}");
                }
            }
            return new BaseResponseDto { ErrorCode = sb.Length > 0 ? StatusCodes.Status406NotAcceptable : null, ErrorMessage = sb.Length > 0 ? sb.ToString() : null };
        }

        /// <summary>
        /// Import Questionnaire Question Group Records
        /// </summary>
        /// <param name="questionnaireRequestDto"></param>
        /// <returns></returns>
        public async Task<BaseResponseDto> ImportQuestionnaireQuestionGroupRecords(ImportQuestionnaireRequestDto questionnaireRequestDto)
        {
            const string methodName = nameof(ImportQuestionnaireQuestionGroupRecords);
            _importLogger.LogInformation("{className}.{methodName}:Beginning questionnaire Question group import", className, methodName);
            StringBuilder sb = new StringBuilder();

            foreach (var QuestionnaireQuestionGroup in questionnaireRequestDto.QuestionnaireDetailDto.QuestionnaireQuestionGroup)
            {
                try
                {

                    var questionnairecode = questionnaireQuestionCodeMappingDto?.QuestionnaireCodeMapping?.Where(x => x.Id == QuestionnaireQuestionGroup.QuestionnaireId).FirstOrDefault()?.Code;
                    var questionnaireQuestioncode = questionnaireQuestionCodeMappingDto?.QuestionnaireQuestionMapping?.Where(x => x.Id == QuestionnaireQuestionGroup.QuestionnaireQuestionId).FirstOrDefault()?.Code;
                    var questionnaire = await _questionnaireRepo.FindOneAsync(x => x.QuestionnaireCode == questionnairecode && x.DeleteNbr == 0);
                    var questionnaireQuestion = await _questionnaireQuestionRepo.FindOneAsync(x => x.QuestionnaireQuestionCode == questionnaireQuestioncode && x.DeleteNbr == 0);
                    if (questionnaireQuestion != null && questionnaire != null)
                    {
                        QuestionnaireQuestionGroup.QuestionnaireId = questionnaire.QuestionnaireId;
                        QuestionnaireQuestionGroup.QuestionnaireQuestionId = questionnaireQuestion.QuestionnaireQuestionId;
                        var QuestionnaireQuestiongroup = await _questionnaireQuestionGroupRepo.FindOneAsync(x => x.QuestionnaireId == questionnaire.QuestionnaireId && x.QuestionnaireQuestionId == questionnaireQuestion.QuestionnaireQuestionId && x.DeleteNbr == 0);
                        if (QuestionnaireQuestiongroup != null)
                        {

                            QuestionnaireQuestionGroup.QuestionnaireQuestionGroupId = QuestionnaireQuestiongroup.QuestionnaireQuestionGroupId;

                            var updateBaseResponseDto = await _questionnaireQuesGroupService.UpdateQuestionnaireQuestionGroup(QuestionnaireQuestiongroup.QuestionnaireQuestionGroupId, QuestionnaireQuestionGroup);
                            if (updateBaseResponseDto.ErrorCode != null)
                            {
                                _importLogger.LogError("{className}.{methodName}: Error occurred while Updating QuestionnaireQuestionGroup  {Task}", className, methodName, QuestionnaireQuestionGroup.ToJson());

                            }
                            else
                                _importLogger.LogInformation("{className}.{methodName}:successfully Updated QuestionnaireQuestion group  {QuestionnaireQuestion}", className, methodName, QuestionnaireQuestionGroup.ToJson());

                        }
                        else
                        {
                            QuestionnaireQuestionGroupRequestDto QuestionnaireQuestionGroupRequestDto = new QuestionnaireQuestionGroupRequestDto
                            {
                                QuestionnaireCode = questionnaire.QuestionnaireCode,
                                QuestionnaireQuestionCode = questionnaireQuestion.QuestionnaireQuestionCode,
                                QuestionnaireQuestionGroup = _mapper.Map<QuestionnaireQuestionGroupPostRequestDto>(QuestionnaireQuestionGroup),
                            };
                            QuestionnaireQuestionGroupRequestDto.QuestionnaireQuestionGroup.CreateUser = Constant.ImportUser;
                            var baseResponseDto = await _questionnaireService.CreateQuestionnaireQuestionGroup(QuestionnaireQuestionGroupRequestDto);
                            if (baseResponseDto.ErrorCode != null)
                            {
                                _importLogger.LogError("{className}.{methodName}: Error occurred while Creating QuestionnaireQuestionGroup  {Task}", className, methodName, QuestionnaireQuestionGroup.ToJson());

                            }
                            else
                                _importLogger.LogInformation("{className}.{methodName}:successfully Updated QuestionnaireQuestion group  {QuestionnaireQuestion}", className, methodName, QuestionnaireQuestionGroup.ToJson());

                        }
                    }
                    else
                    {
                        _importLogger.LogError("{className}.{methodName}: Error occurred while Importing QuestionnaireQuestionGroup  {group}", className, methodName, QuestionnaireQuestionGroup.ToJson());

                    }
                }
                catch (Exception ex)
                {

                    _importLogger.LogError(ex, "{ClassName}.{MethodName}: Error Message: {Message}, Error Code: {ErrorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                    sb.AppendLine($"Questionnaire Question Error:  exception occurred {ex.Message}");
                }
            }
            return new BaseResponseDto { ErrorCode = sb.Length > 0 ? StatusCodes.Status406NotAcceptable : null, ErrorMessage = sb.Length > 0 ? sb.ToString() : null };

        }
    }
}
