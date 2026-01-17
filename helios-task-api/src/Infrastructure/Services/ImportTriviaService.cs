using Amazon.CloudWatchLogs.Model;
using AutoMapper;
using FirebaseAdmin;
using Microsoft.AspNetCore.Http;
using Microsoft.ClearScript;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Task.Core.Domain.Constants;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Models;
using SunnyRewards.Helios.Task.Infrastructure.Mappings;
using SunnyRewards.Helios.Task.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Task.Infrastructure.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Task.Infrastructure.Services
{
    public class ImportTriviaService : IImportTriviaService
    {
        private readonly ILogger<ImportTriviaService> _importLogger;
        private readonly IMapper _mapper;
        private readonly ITriviaRepo _triviaRepo;
        private readonly ITriviaQuestionGroupRepo _TriviaQuestionGroupRepo;
        private readonly ITriviaQuestionRepo _TriviaQuestionRepo;
        private readonly ITaskService _taskService;
        private readonly ITriviaQuestionGroupService _triviaQuesGroupService;
        private readonly ITriviaQuestionService _triviaQuesService;
        private readonly ITriviaService _triviaService;
        private readonly ITaskRewardRepo _taskRewardRepo;
        private readonly ImportTriviaQuestionMappingDto triviaQuestionCodeMappingDto;
        private readonly List<ImportTriviaQuestionDto> triviaCodeMapping;
        private readonly List<ImportTriviaQuestionDto> questionCodeMapping;

        const string className = nameof(ImportTriviaService);

        public ImportTriviaService(
        ILogger<ImportTriviaService> importLogger,
        IMapper mapper,
        ITriviaRepo triviaRepo,
        ITriviaQuestionGroupRepo TriviaQuestionGroupRepo,
        ITriviaQuestionRepo TriviaQuestionRepo,
        ITaskService taskService,
        ITriviaQuestionGroupService triviaQuesGroupService,
        ITriviaQuestionService triviaQuesService,
        ITriviaService triviaService,
        ITaskRewardRepo taskRewardRepo)
        {
            _importLogger = importLogger;
            _mapper = mapper;
            _triviaRepo = triviaRepo;
            _TriviaQuestionGroupRepo = TriviaQuestionGroupRepo;
            _TriviaQuestionRepo = TriviaQuestionRepo;
            _taskService = taskService;
            _triviaQuesGroupService = triviaQuesGroupService;
            _triviaQuesService = triviaQuesService;
            _taskRewardRepo = taskRewardRepo;
            _triviaService = triviaService;
            triviaQuestionCodeMappingDto = new ImportTriviaQuestionMappingDto();
            triviaCodeMapping = new List<ImportTriviaQuestionDto>();
            questionCodeMapping = new List<ImportTriviaQuestionDto>();

        }
        public async Task<BaseResponseDto> ImportTrivia(ImportTriviaRequestDto triviaRequestDto)
        {
            const string methodName = nameof(ImportTrivia);
            int count = 0;
            StringBuilder sb = new StringBuilder();
            try
            {
                if (triviaRequestDto.TriviaDetailDto == null || triviaRequestDto.TriviaDetailDto.Trivia == null
                    || triviaRequestDto.TriviaDetailDto.Trivia?.Count <= 0 || string.IsNullOrEmpty(triviaRequestDto.TenantCode) ||
                    triviaRequestDto.TriviaDetailDto?.TriviaQuestionGroup == null || triviaRequestDto.TriviaDetailDto?.Trivia == null ||
                    triviaRequestDto.TriviaDetailDto?.TriviaQuestion == null)
                {
                    _importLogger.LogError("{className}.{methodName}: no record found for trivia Question group {Trivia}", className, methodName, triviaRequestDto.ToJson());
                    sb.AppendLine("trivia Question group Error: No record found");
                    return new BaseResponseDto { ErrorCode = StatusCodes.Status404NotFound, ErrorMessage = "No record to import" };

                }
                BaseResponseDto response = new BaseResponseDto();
                response = await ImportTriviaRecords(triviaRequestDto);
                if (response != null && response.ErrorCode != null)
                {
                    sb.AppendLine(response.ErrorMessage);
                    //return if trivia fails
                    return new BaseResponseDto { ErrorMessage = sb.Length > 0 ? sb.ToString() : null, ErrorCode = sb.Length > 0 ? response.ErrorCode : null };

                }
                triviaQuestionCodeMappingDto.TriviaCodeMapping = triviaCodeMapping;
                response = await ImportTriviaQuestionRecords(triviaRequestDto);
                if (response != null && response.ErrorCode != null)
                {
                    sb.AppendLine(response.ErrorMessage);
                }

                triviaQuestionCodeMappingDto.TriviaQuestionMapping = questionCodeMapping;
                response = await ImportTriviaQuestionGroupRecords(triviaRequestDto);
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
        public async Task<BaseResponseDto> ImportTriviaRecords(ImportTriviaRequestDto triviaRequestDto)
        {
            const string methodName = nameof(ImportTriviaRecords);
            StringBuilder sb = new StringBuilder();

            _importLogger.LogInformation("{className}.{methodName}:Beginning trivia import", className, methodName);


            foreach (var triviaDto in triviaRequestDto.TriviaDetailDto.Trivia)
            {
                try
                {
                    ImportTriviaQuestionDto triviaMapp = new ImportTriviaQuestionDto();

                    if (triviaDto?.Trivia == null && triviaDto?.TaskExternalCode == null)
                    {
                        _importLogger.LogError("{className}.{methodName}: Request doesn't contain Trivia data for import {TaskRequestDto}", className, methodName, triviaDto?.Trivia?.ToJson());
                        continue;
                    }
                    var taskRewardModel = await _taskRewardRepo.FindOneAsync(x => x.TaskExternalCode == triviaDto.TaskExternalCode && x.TenantCode == triviaRequestDto.TenantCode && x.DeleteNbr == 0);
                    triviaMapp.Id = triviaDto?.Trivia?.TriviaId;
                    if (taskRewardModel == null)
                    {
                        _importLogger.LogError("{className}.{methodName}:  Task reward record not found for request: {requestDto}", className, methodName, triviaDto?.ToJson());

                        continue;
                    }
                    else
                    {
                        var triviaModel = await _triviaRepo.FindOneAsync(x => x.TaskRewardId == taskRewardModel.TaskRewardId && x.DeleteNbr == 0);
                        BaseResponseDto baseResponseDto = new BaseResponseDto();
                        Trivia TriviaDto = _mapper.Map<Trivia>(triviaDto.Trivia);
                        TriviaDto.TaskRewardId = taskRewardModel.TaskRewardId;
                        if (triviaModel != null)
                        {

                            TriviaDto.TriviaCode = triviaModel.TriviaCode;
                            TriviaDto.UpdateUser = Constant.ImportUser;
                            triviaMapp.Code = triviaModel.TriviaCode;

                            TriviaRequestDto trivia = new TriviaRequestDto { TaskRewardCode = taskRewardModel.TaskRewardCode, trivia = TriviaDto };
                            baseResponseDto = await _triviaService.UpdateTrivia(trivia);
                            if (baseResponseDto.ErrorCode != null)
                            {
                                _importLogger.LogError("{className}.{methodName}: Error occurred while Updating trivia  {trivia}", className, methodName, triviaDto.Trivia?.ToJson());
                            }
                            else
                                _importLogger.LogInformation("{className}.{methodName}:successfully Updated trivia  {trivia}", className, methodName, triviaDto.Trivia?.ToJson());

                        }
                        else
                        {

                            TriviaDto.CreateUser = Constant.ImportUser;
                            TriviaDto.TriviaCode = "trv-" + Guid.NewGuid().ToString("N");
                            triviaMapp.Code = TriviaDto.TriviaCode;

                            TriviaRequestDto trivia = new TriviaRequestDto { TaskRewardCode = taskRewardModel.TaskRewardCode, trivia = TriviaDto };
                            baseResponseDto = await _triviaService.CreateTrivia(trivia);
                            if (baseResponseDto.ErrorCode != null)
                            {
                                _importLogger.LogError("{className}.{methodName}: Error occurred while Creating trivia  {trivia}", className, methodName, triviaDto.Trivia?.ToJson());
                            }
                            else
                                _importLogger.LogInformation("{className}.{methodName}:successfully Created trivia  {trivia}", className, methodName, triviaDto.Trivia?.ToJson());
                        }
                        triviaCodeMapping.Add(triviaMapp);
                    }
                }
                catch (Exception ex)
                {

                    _importLogger.LogError(ex, "{ClassName}.{MethodName}: Error Message: {Message}, Error Code: {ErrorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                    sb.AppendLine($"Trivia Error:  exception occurred {ex.Message}");
                }
            }
            return new BaseResponseDto { ErrorCode = sb.Length > 0 ? StatusCodes.Status406NotAcceptable : null, ErrorMessage = sb.Length > 0 ? sb.ToString() : null };

        }
        public async Task<BaseResponseDto> ImportTriviaQuestionRecords(ImportTriviaRequestDto triviaRequestDto)
        {
            const string methodName = nameof(ImportTriviaQuestionRecords);
            _importLogger.LogInformation("{className}.{methodName}:Beginning trivia Question import", className, methodName);
            StringBuilder sb = new StringBuilder();


            foreach (var TriviaQuestionDto in triviaRequestDto.TriviaDetailDto.TriviaQuestion)
            {
                try
                {
                    ImportTriviaQuestionDto triviaQuesMapp = new ImportTriviaQuestionDto();

                    if (TriviaQuestionDto == null)
                    {

                        _importLogger.LogError("{className}.{methodName}: Request doesn't contain Trivia data for import {TaskRequestDto}", className, methodName, TriviaQuestionDto?.ToJson());
                        continue;
                    }
                    var trivaQuestionModel = await _TriviaQuestionRepo.FindOneAsync(x => x.QuestionExternalCode == TriviaQuestionDto.QuestionExternalCode && x.DeleteNbr == 0);
                    BaseResponseDto baseResponseDto = new BaseResponseDto();
                    triviaQuesMapp.Id = TriviaQuestionDto.TriviaQuestionId;

                    if (trivaQuestionModel == null)
                    {
                        TriviaQuestionRequestDto TriviaDto = _mapper.Map<TriviaQuestionRequestDto>(TriviaQuestionDto);
                        TriviaDto.CreateUser = Constant.ImportUser;
                        TriviaDto.TriviaQuestionCode = "trq-" + Guid.NewGuid().ToString("N");
                        triviaQuesMapp.Code = TriviaDto.TriviaQuestionCode;

                        baseResponseDto = await _triviaService.CreateTriviaQuestion(TriviaDto);
                        if (baseResponseDto.ErrorCode != null)
                        {
                            _importLogger.LogError("{className}.{methodName}: Error occurred while Creating TriviaQuestion  {TriviaQuestion}", className, methodName, TriviaQuestionDto.ToJson());
                        }
                        else
                            _importLogger.LogInformation("{className}.{methodName}:successfully Created trivia Question {TriviaQuestion}", className, methodName, TriviaQuestionDto.ToJson());
                    }
                    else
                    {
                        TriviaQuestionData TriviaQuestion = _mapper.Map<TriviaQuestionData>(TriviaQuestionDto);
                        TriviaQuestion.TriviaQuestionCode = trivaQuestionModel.TriviaQuestionCode;
                        triviaQuesMapp.Code = trivaQuestionModel.TriviaQuestionCode;

                        baseResponseDto = await _triviaQuesService.UpdateTriviaQuestion(trivaQuestionModel.TriviaQuestionCode, TriviaQuestion);
                        if (baseResponseDto.ErrorCode != null)
                        {
                            _importLogger.LogError("{className}.{methodName}: Error occurred while Updating TriviaQuestion  {TriviaQuestion}", className, methodName, TriviaQuestionDto.ToJson());

                        }
                        else
                            _importLogger.LogInformation("{className}.{methodName}:successfully Updated TriviaQuestion   {TriviaQuestion}", className, methodName, TriviaQuestionDto.ToJson());

                    }
                    questionCodeMapping.Add(triviaQuesMapp);


                }
                catch (Exception ex)
                {

                    _importLogger.LogError(ex, "{ClassName}.{MethodName}: Error Message: {Message}, Error Code: {ErrorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                    sb.AppendLine($"Trivia Question Error:  exception occurred {ex.Message}");
                }
            }
            return new BaseResponseDto { ErrorCode = sb.Length > 0 ? StatusCodes.Status406NotAcceptable : null, ErrorMessage = sb.Length > 0 ? sb.ToString() : null };
        }
        public async Task<BaseResponseDto> ImportTriviaQuestionGroupRecords(ImportTriviaRequestDto triviaRequestDto)
        {
            const string methodName = nameof(ImportTriviaQuestionGroupRecords);
            _importLogger.LogInformation("{className}.{methodName}:Beginning trivia Question group import", className, methodName);
            StringBuilder sb = new StringBuilder();

            foreach (var TriviaQuestionGroup in triviaRequestDto.TriviaDetailDto.TriviaQuestionGroup)
            {
                try
                {

                    var triviacode = triviaQuestionCodeMappingDto?.TriviaCodeMapping?.Where(x => x.Id == TriviaQuestionGroup.TriviaId).FirstOrDefault()?.Code;
                    var TriviaQuestioncode = triviaQuestionCodeMappingDto?.TriviaQuestionMapping?.Where(x => x.Id == TriviaQuestionGroup.TriviaQuestionId).FirstOrDefault()?.Code;
                    var trivia = await _triviaRepo.FindOneAsync(x => x.TriviaCode == triviacode && x.DeleteNbr == 0);
                    var TriviaQuestion = await _TriviaQuestionRepo.FindOneAsync(x => x.TriviaQuestionCode == TriviaQuestioncode && x.DeleteNbr == 0);
                    if (TriviaQuestion != null && trivia != null)
                    {
                        TriviaQuestionGroup.TriviaId = trivia.TriviaId;
                        TriviaQuestionGroup.TriviaQuestionId = TriviaQuestion.TriviaQuestionId;
                        var TriviaQuestiongroup = await _TriviaQuestionGroupRepo.FindOneAsync(x => x.TriviaId == trivia.TriviaId && x.TriviaQuestionId == TriviaQuestion.TriviaQuestionId && x.DeleteNbr == 0);
                        if (TriviaQuestiongroup != null)
                        {

                            TriviaQuestionGroup.TriviaQuestionGroupId = TriviaQuestiongroup.TriviaQuestionGroupId;

                            var updateBaseResponseDto = await _triviaQuesGroupService.UpdateTriviaQuestionGroup(TriviaQuestiongroup.TriviaQuestionGroupId, TriviaQuestionGroup);
                            if (updateBaseResponseDto.ErrorCode != null)
                            {
                                _importLogger.LogError("{className}.{methodName}: Error occurred while Updating TriviaQuestionGroup  {Task}", className, methodName, TriviaQuestionGroup.ToJson());

                            }
                            else
                                _importLogger.LogInformation("{className}.{methodName}:successfully Updated TriviaQuestion group  {TriviaQuestion}", className, methodName, TriviaQuestionGroup.ToJson());

                        }
                        else
                        {
                            TriviaQuestionGroupRequestDto TriviaQuestionGroupRequestDto = new TriviaQuestionGroupRequestDto
                            {
                                TriviaCode = trivia.TriviaCode,
                                TriviaQuestionCode = TriviaQuestion.TriviaQuestionCode,
                                TriviaQuestionGroup = _mapper.Map<TriviaQuestionGroupPostRequestDto>(TriviaQuestionGroup),
                            };
                            TriviaQuestionGroupRequestDto.TriviaQuestionGroup.CreateUser = Constant.ImportUser;
                            var baseResponseDto = await _triviaService.CreateTriviaQuestionGroup(TriviaQuestionGroupRequestDto);
                            if (baseResponseDto.ErrorCode != null)
                            {
                                _importLogger.LogError("{className}.{methodName}: Error occurred while Creating TriviaQuestionGroup  {Task}", className, methodName, TriviaQuestionGroup.ToJson());

                            }
                            else
                                _importLogger.LogInformation("{className}.{methodName}:successfully Updated TriviaQuestion group  {TriviaQuestion}", className, methodName, TriviaQuestionGroup.ToJson());

                        }
                    }
                    else
                    {
                        //sb.AppendLine("Trivia Question Group Error: Request doesn't contain Trivia and trivia Question data Record");
                        _importLogger.LogError("{className}.{methodName}: Error occurred while Importing TriviaQuestionGroup  {group}", className, methodName, TriviaQuestionGroup.ToJson());

                    }
                }
                catch (Exception ex)
                {

                    _importLogger.LogError(ex, "{ClassName}.{MethodName}: Error Message: {Message}, Error Code: {ErrorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                    sb.AppendLine($"Trivia Question Error:  exception occurred {ex.Message}");
                }
            }
            return new BaseResponseDto { ErrorCode = sb.Length > 0 ? StatusCodes.Status406NotAcceptable : null, ErrorMessage = sb.Length > 0 ? sb.ToString() : null };

        }
    }
}
