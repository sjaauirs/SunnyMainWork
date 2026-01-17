using AutoMapper;
using FluentNHibernate.Conventions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Common.Core.Services;
using SunnyRewards.Helios.Task.Core.Domain.Constants;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Models;
using SunnyRewards.Helios.Task.Infrastructure.Repositories;
using SunnyRewards.Helios.Task.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Task.Infrastructure.Services.Interface;
using Newtonsoft.Json.Linq;

namespace SunnyRewards.Helios.Task.Infrastructure.Services
{
    public class TriviaService : BaseService, ITriviaService
    {
        private readonly ILogger<TriviaService> _triviaLogger;
        private readonly IMapper _mapper;
        private readonly ITriviaQuestionRepo _triviaQuestionRepo;
        private readonly ITriviaRepo _triviaRepo;
        private readonly ITriviaQuestionGroupRepo _triviaQuestionGroupRepo;
        private readonly ITaskRewardRepo _taskRewardRepo;
        private readonly ITaskRewardService _taskRewardService;
        private readonly IConsumerTaskRepo _consumerTaskRepo;
        private readonly NHibernate.ISession _session;
        const string className = nameof(TriviaService);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="triviaLogger"></param>
        /// <param name="mapper"></param>
        /// <param name="triviaQuestionRepo"></param>
        /// <param name="triviaRepo"></param>
        /// <param name="triviaQuestionGroupRepo"></param>
        /// <param name="taskRewardRepo"></param>
        /// <param name="taskRewardService"></param>
        /// <param name="consumerTaskRepo"></param>
        /// <param name="session"></param>
        public TriviaService(ILogger<TriviaService> triviaLogger,
        IMapper mapper, ITriviaQuestionRepo triviaQuestionRepo, ITriviaRepo triviaRepo,
        ITriviaQuestionGroupRepo triviaQuestionGroupRepo, ITaskRewardRepo taskRewardRepo,
        ITaskRewardService taskRewardService, IConsumerTaskRepo consumerTaskRepo, NHibernate.ISession session)
        {
            _triviaLogger = triviaLogger;
            _mapper = mapper;
            _triviaQuestionRepo = triviaQuestionRepo;
            _triviaRepo = triviaRepo;
            _triviaQuestionGroupRepo = triviaQuestionGroupRepo;
            _taskRewardRepo = taskRewardRepo;
            _taskRewardService = taskRewardService;
            _consumerTaskRepo = consumerTaskRepo;
            _session = session;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskRewardId"></param>
        /// <param name="consumerCode"></param>
        /// <returns></returns>
        public async Task<GetTriviaResponseDto> GetTrivia(long taskRewardId, string consumerCode , string? languageCode)
        {
            const string methodName = nameof(GetTrivia);
            var response = new GetTriviaResponseDto();
            DateTime tsNow = DateTime.UtcNow;
            try
            {
                var trivia = await _triviaRepo.FindOneAsync(x => x.TaskRewardId == taskRewardId && x.DeleteNbr == 0);

                if (trivia == null)
                {
                    _triviaLogger.LogError("{className}.{methodName}: Trivia not found for given taskRewardId: {taskRewardId},Error Code:{errorCode}", className, methodName, taskRewardId, StatusCodes.Status404NotFound);
                    return new GetTriviaResponseDto()
                    {
                        ErrorCode = StatusCodes.Status404NotFound
                    };
                }
                var triviaQuestionGroup = await _triviaQuestionGroupRepo.FindAsync(x => x.TriviaId == trivia.TriviaId && x.DeleteNbr == 0 && DateTime.UtcNow >= x.ValidStartTs && DateTime.UtcNow < x.ValidEndTs);
                var triviaQuestionGroupSequence = triviaQuestionGroup.OrderBy(x => x.SequenceNbr);
                var res = new List<TriviaQuestionDto>();
                foreach (var questionId in triviaQuestionGroupSequence)
                {
                    var triviaQuestions = await _triviaQuestionRepo.FindAsync(x => x.TriviaQuestionId == questionId.TriviaQuestionId);
                    var questionsDto = _mapper.Map<List<TriviaQuestionDto>>(triviaQuestions);
                    res.AddRange(questionsDto);
                }

                foreach (var triviaQuestion in res)
                {
                    triviaQuestion.TriviaJson = FilterTriviaJsonByLanguage(triviaQuestion.TriviaJson, languageCode);
                }

                _triviaLogger.LogInformation("{className}.{methodName}: successfully retrieved data from  GetTrivia API for taskRewardId: {taskRewardId}", className, methodName, taskRewardId);

                TriviaQuestionDto[] array = res.ToArray();
                response.Questions = array;

                var triviaDto = _mapper.Map<TriviaDto>(trivia);

                TaskRewardModel? ctaTaskreward = null;
                ConsumerTaskModel? ctaConsumerTaskResponse = null;

                if (trivia.CtaTaskExternalCode != null)
                {
                    var triviaTaskReward = await _taskRewardRepo.FindOneAsync(x => x.TaskRewardId == trivia.TaskRewardId && x.DeleteNbr == 0);

                    ctaTaskreward = await _taskRewardRepo.FindOneAsync(x => x.TenantCode == triviaTaskReward.TenantCode &&
                        x.TaskExternalCode == trivia.CtaTaskExternalCode && x.DeleteNbr == 0);

                    if (ctaTaskreward != null)
                    {
                        var consumerTasks = await _consumerTaskRepo.FindAsync(x => x.TaskId == ctaTaskreward.TaskId &&
                            x.ConsumerCode == consumerCode && x.DeleteNbr == 0);
                        // Filtering latest consumertask
                        ctaConsumerTaskResponse = consumerTasks.OrderByDescending(x => x.ConsumerTaskId).FirstOrDefault();
                    }
                    else
                    {
                        _triviaLogger.LogError("{className}.{methodName}: Consumer: {consumerCode}, " +
                            "CTA task external code is not null: {ctaTaskExternalCode} but unable to find the TaskReward for it, Error Code:{errorCode}", className, methodName,
                            consumerCode, trivia.CtaTaskExternalCode, StatusCodes.Status404NotFound);
                    }
                }

                response.Trivia = triviaDto;

                if (ctaConsumerTaskResponse == null && ctaTaskreward != null)
                {
                    await GetTaskRewarddetails(response, ctaTaskreward, languageCode);
                }
                else if (ctaConsumerTaskResponse != null && ctaConsumerTaskResponse.TaskStatus != "COMPLETED")
                {
                    await GetTaskRewarddetails(response, ctaTaskreward , languageCode);
                }
                return response;
            }
            catch (Exception ex)
            {
                _triviaLogger.LogError(ex, "{className}.{methodName}: ERROR Msg:{msg},Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return new GetTriviaResponseDto()
                {
                    ErrorCode = StatusCodes.Status500InternalServerError
                };
            }
        }

        public string? FilterTriviaJsonByLanguage(string? triviaJson, string? language)
        {
            if (string.IsNullOrWhiteSpace(triviaJson))
            {
                return triviaJson;
            }

            if (string.IsNullOrWhiteSpace(language))
            {
                language = Constant.LanguageCode.ToLower();
            }

            try
            {
                var jObject = JObject.Parse(triviaJson);

                var dict = jObject.Properties()
                          .ToDictionary(p => p.Name.ToLower(), p => p.Value);

                if (dict.TryGetValue(language.ToLower(), out var localizedContent))
                {
                    return localizedContent.ToString(Formatting.None);
                }
                // if not return , return english
                if (dict.TryGetValue(Constant.LanguageCode.ToLower(), out var fallbackContent))
                {
                    return fallbackContent.ToString(Formatting.None);
                }

                return triviaJson;
            }
            catch
            {
                return triviaJson;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="postTaskProgressUpdateRequestDto"></param>
        /// <returns></returns>
        public async Task<PostTaskProgressUpdateResponseDto> TaskProgressUpdate(PostTaskProgressUpdateRequestDto postTaskProgressUpdateRequestDto)
        {
            const string methodName = nameof(TaskProgressUpdate);
            var response = new PostTaskProgressUpdateResponseDto();
            try
            {
                var consumerTasks = await _consumerTaskRepo.FindAsync(x => x.ConsumerCode == postTaskProgressUpdateRequestDto.ConsumerCode && x.TaskId == postTaskProgressUpdateRequestDto.TaskId && x.DeleteNbr == 0);
                // Filtering latest consumertask
                var consumerTaskModel = consumerTasks.OrderByDescending(x => x.ConsumerTaskId).FirstOrDefault();
                if (consumerTaskModel == null)
                {
                    _triviaLogger.LogError("{className}.{methodName}: consumer Task Not Found. Error Code:{errorCode}", className, methodName, StatusCodes.Status404NotFound);
                    response.ErrorCode = StatusCodes.Status404NotFound;
                    return response;
                };
                using var transaction = _session.BeginTransaction();
                try
                {
                    consumerTaskModel.ProgressDetail = postTaskProgressUpdateRequestDto.ProgressDetail;
                    await _session.UpdateAsync(consumerTaskModel);

                    _triviaLogger.LogInformation("{className}.{methodName}: Updated record in ConsumerTask table for ConsumerCode: {ConsumerCode}", className, methodName, postTaskProgressUpdateRequestDto.ConsumerCode);
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _triviaLogger.LogError(ex, "{className}.{methodName}: Not able to persist some objects in DB, skipping current record Error Msg:{msg}", className, methodName, ex.Message);
                }
                return response;
            }
            catch (Exception ex)
            {
                _triviaLogger.LogError(ex, "{className}.{methodName}: ERROR Msg:{msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return new PostTaskProgressUpdateResponseDto();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="response"></param>
        /// <param name="ctaTaskreward"></param>
        /// <returns></returns>
        private async System.Threading.Tasks.Task GetTaskRewarddetails(GetTriviaResponseDto response, TaskRewardModel? ctaTaskreward , string? languageCode)
        {
            var taskRewardRequestDto = new GetTaskRewardByCodeRequestDto();
            taskRewardRequestDto.TaskRewardCode = ctaTaskreward?.TaskRewardCode ?? string.Empty;
            taskRewardRequestDto.LanguageCode = languageCode;

            var taskRewardResponseDto = new GetTaskRewardByCodeResponseDto();
            if (ctaTaskreward?.TaskExternalCode != null)
            {
                _triviaLogger.LogInformation("{className}.GetTaskRewarddetails: successfully retrieved data from  GetTaskRewardByCode API for CtaTaskExternalCode: {CtaTaskExternalCode}", className, ctaTaskreward?.TaskExternalCode);
                taskRewardResponseDto = await _taskRewardService.GetTaskRewardByCode(taskRewardRequestDto);
            }
            response.Trivia.taskRewardDetail = taskRewardResponseDto.TaskRewardDetail;
        }

        public async Task<BaseResponseDto> CreateTrivia(TriviaRequestDto requestDto)
        {
            const string methodName = nameof(CreateTrivia);
            try
            {
                if (requestDto == null)
                {
                    _triviaLogger.LogError("{className}.{methodName}: Failed to Saved data for Trivia for request: {requestDto}", className, methodName, requestDto?.ToJson());
                    return new BaseResponseDto { ErrorCode = StatusCodes.Status404NotFound, ErrorMessage = "Trivia record Not Found" };
                }
                var taskRewardModel = await _taskRewardRepo.FindOneAsync(x => x.TaskRewardCode == requestDto.TaskRewardCode && x.DeleteNbr == 0);

                if (taskRewardModel == null)
                {
                    _triviaLogger.LogInformation("{className}.{methodName}:  Task reward record not found for request: {requestDto}", className, methodName, requestDto?.ToJson());
                    return new BaseResponseDto { ErrorCode = StatusCodes.Status404NotFound, ErrorMessage = "Task reward record not found" };

                }
                var triviaModel = await _triviaRepo.FindOneAsync(x => x.TaskRewardId == taskRewardModel.TaskRewardId && x.DeleteNbr == 0);
                if (triviaModel != null)
                {
                    _triviaLogger.LogInformation("{className}.{methodName}: record exists for request: {requestDto}", className, methodName, requestDto.ToJson());
                    return new BaseResponseDto { ErrorCode = StatusCodes.Status409Conflict, ErrorMessage = "Task External Mapping already exists" };
                }

                TriviaModel trivia = new TriviaModel();
                trivia = _mapper.Map<TriviaModel>(requestDto.trivia);
                trivia.CreateTs = DateTime.Now;
                trivia.TaskRewardId= taskRewardModel.TaskRewardId;
                trivia.ConfigJson = JsonConvert.SerializeObject(requestDto?.trivia.ConfigJson, new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                });
                trivia.CreateUser = requestDto?.trivia.CreateUser ?? Constant.SystemUser;
                trivia = await _triviaRepo.CreateAsync(trivia);
                if (trivia.TriviaId > 0)
                {
                    _triviaLogger.LogInformation("{className}.{methodName}: Successfully Saved data for trivia request: {requestDto}", className, methodName, requestDto.ToJson());
                    return new BaseResponseDto();
                }
                else
                {
                    _triviaLogger.LogError("{className}.{methodName}: Failed to Saved data for Trivia request: {requestDto}", className, methodName, requestDto.ToJson());
                    return new BaseResponseDto { ErrorCode = StatusCodes.Status404NotFound, ErrorMessage = "Trivia record Not Created" };
                }

            }
            catch (Exception ex)
            {
                _triviaLogger.LogError(ex, "{className}.{methodName}: ERROR - msg : {msg}, for requestDto: {RequestDto}", className, methodName, ex.Message, requestDto.ToJson());
                return new BaseResponseDto { ErrorCode = StatusCodes.Status500InternalServerError, ErrorMessage = "Task External Mapping Not Created" };

            }
        }
        public async Task<BaseResponseDto> CreateTriviaQuestion(TriviaQuestionRequestDto requestDto)
        {
            const string methodName = nameof(CreateTriviaQuestion);
            try
            {
                if (requestDto == null)
                {
                    _triviaLogger.LogError("{className}.{methodName}: Failed to Saved data for Trivia Question  for request: {requestDto}", className, methodName, requestDto?.ToJson());
                    return new BaseResponseDto { ErrorCode = StatusCodes.Status404NotFound, ErrorMessage = "Trivia Question record Not Found" };
                }


                var triviaModel = await _triviaQuestionRepo.FindOneAsync(x => x.QuestionExternalCode == requestDto.QuestionExternalCode && x.DeleteNbr == 0);
                if (triviaModel!=null)
                {
                    _triviaLogger.LogInformation("{className}.{methodName}: record exists for request: {requestDto}", className, methodName, requestDto.ToJson());
                    return new BaseResponseDto { ErrorCode = StatusCodes.Status409Conflict, ErrorMessage = "Trivia Question already exists" };
                }

                TriviaQuestionModel triviaQuestion = new TriviaQuestionModel();
                triviaQuestion = _mapper.Map<TriviaQuestionModel>(requestDto);
                triviaQuestion.CreateTs = DateTime.Now;
                triviaQuestion.TriviaJson = JsonConvert.SerializeObject(requestDto?.TriviaJson, new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                });
                triviaQuestion.CreateUser = requestDto?.CreateUser ?? Constant.SystemUser;
                triviaQuestion = await _triviaQuestionRepo.CreateAsync(triviaQuestion);
                if (triviaQuestion.TriviaQuestionId <= 0)
                {
                    _triviaLogger.LogError("{className}.{methodName}: Failed to Saved data for Trivia Question  request: {requestDto}", className, methodName, requestDto.ToJson());
                    return new BaseResponseDto { ErrorCode = StatusCodes.Status404NotFound, ErrorMessage = "Trivia Question record Not Created" };
                }
                _triviaLogger.LogInformation("{className}.{methodName}: Successfully Saved data for Trivia Question  request: {requestDto}", className, methodName, requestDto.ToJson());
                return new BaseResponseDto();


            }
            catch (Exception ex)
            {
                _triviaLogger.LogError(ex, "{className}.{methodName}: ERROR - msg : {msg}, for requestDto: {RequestDto}", className, methodName, ex.Message, requestDto.ToJson());
                return new BaseResponseDto { ErrorCode = StatusCodes.Status500InternalServerError, ErrorMessage = "Trivia Question Not Created" };

            }
        }
        public async Task<BaseResponseDto> CreateTriviaQuestionGroup(TriviaQuestionGroupRequestDto requestDto)
        {
            const string methodName = nameof(CreateTriviaQuestionGroup);
            try
            {
                if (requestDto == null)
                {
                    _triviaLogger.LogError("{className}.{methodName}: Failed to Saved data for Trivia Question group for request: {requestDto}", className, methodName, requestDto?.ToJson());
                    return new BaseResponseDto { ErrorCode = StatusCodes.Status404NotFound, ErrorMessage = "Trivia Question group record Not Found" };
                }


                var triviaQuestionModel = await _triviaQuestionRepo.FindOneAsync(x => x.TriviaQuestionCode == requestDto.TriviaQuestionCode && x.DeleteNbr == 0);
                var triviaModel = await _triviaRepo.FindOneAsync(x => x.TriviaCode == requestDto.TriviaCode && x.DeleteNbr == 0);
                if (triviaQuestionModel == null || triviaModel == null)
                {
                    _triviaLogger.LogInformation("{className}.{methodName}: Trivia Question and Trivia record not found for request: {requestDto}", className, methodName, requestDto.ToJson());
                    return new BaseResponseDto { ErrorCode = StatusCodes.Status404NotFound, ErrorMessage = " Trivia Question and Trivia record not found" };

                }
                var triviaQuestionGroupModel = await _triviaQuestionGroupRepo.FindOneAsync(x => x.TriviaId == triviaModel.TriviaId && x.TriviaQuestionId == triviaQuestionModel.TriviaQuestionId && x.DeleteNbr == 0);
                if (triviaQuestionGroupModel != null)
                {
                    _triviaLogger.LogInformation("{className}.{methodName}: record exists for request: {requestDto}", className, methodName, requestDto.ToJson());
                    return new BaseResponseDto { ErrorCode = StatusCodes.Status409Conflict, ErrorMessage = "Trivia Question group already exists" };
                }

                TriviaQuestionGroupModel triviaQuestionGroup = new TriviaQuestionGroupModel();
                triviaQuestionGroup = _mapper.Map<TriviaQuestionGroupModel>(requestDto.TriviaQuestionGroup);
                triviaQuestionGroup.CreateTs = DateTime.Now;
                triviaQuestionGroup.TriviaQuestionId = triviaQuestionModel.TriviaQuestionId;
                triviaQuestionGroup.TriviaId = triviaModel.TriviaId;
                triviaQuestionGroup.CreateUser = requestDto.TriviaQuestionGroup.CreateUser ?? Constant.ImportUser;
                triviaQuestionGroup = await _triviaQuestionGroupRepo.CreateAsync(triviaQuestionGroup);
                if (triviaQuestionGroup.TriviaQuestionGroupId > 0)
                {
                    _triviaLogger.LogInformation("{className}.{methodName}: Successfully Saved data for Trivia Question Group  request: {requestDto}", className, methodName, requestDto.ToJson());
                    return new BaseResponseDto();
                }
                _triviaLogger.LogError("{className}.{methodName}: Failed to Saved data for Trivia Question Group request: {requestDto}", className, methodName, requestDto.ToJson());
                return new BaseResponseDto { ErrorCode = StatusCodes.Status404NotFound, ErrorMessage = "Trivia Question Group record Not Created" };


            }
            catch (Exception ex)
            {
                _triviaLogger.LogError(ex, "{className}.{methodName}: ERROR - msg : {msg}, for requestDto: {RequestDto}", className, methodName, ex.Message, requestDto.ToJson());
                return new BaseResponseDto { ErrorCode = StatusCodes.Status500InternalServerError, ErrorMessage = "Trivia Question Group Not Created" };

            }
        }

        /// <summary>
        /// Gets all trivia.
        /// </summary>
        /// <returns></returns>
        public async Task<TriviaResponseDto> GetAllTrivia()
        {
            var triviaResponseDto = new TriviaResponseDto();
            var triviaModelList = await _triviaRepo.FindAsync(x => x.DeleteNbr == 0);
            triviaResponseDto.TriviaList = _mapper.Map<IList<TriviaDataDto>>(triviaModelList).ToList();
            return triviaResponseDto;
        }
        public async Task<BaseResponseDto> UpdateTrivia(TriviaRequestDto requestDto)
        {
            const string methodName = nameof(UpdateTrivia);
            try
            {
                if (requestDto == null)
                {
                    _triviaLogger.LogError("{className}.{methodName}: Failed to Saved data for Trivia for request: {requestDto}", className, methodName, requestDto?.ToJson());
                    return new BaseResponseDto { ErrorCode = StatusCodes.Status404NotFound, ErrorMessage = "Trivia record Not Found" };
                }
                var taskRewardModel = await _taskRewardRepo.FindOneAsync(x => x.TaskRewardCode == requestDto.TaskRewardCode && x.DeleteNbr == 0);

                if (taskRewardModel == null)
                {
                    _triviaLogger.LogInformation("{className}.{methodName}:  Task reward record not found for request: {requestDto}", className, methodName, requestDto?.ToJson());
                    return new BaseResponseDto { ErrorCode = StatusCodes.Status404NotFound, ErrorMessage = "Task reward record not found" };

                }
                var trivia = await _triviaRepo.FindOneAsync(x => x.TriviaCode == requestDto.trivia.TriviaCode && x.DeleteNbr == 0);
                if (trivia == null)
                {
                    _triviaLogger.LogInformation("{className}.{methodName}: record  does not exists  for request: {requestDto}", className, methodName, requestDto.ToJson());
                    return new BaseResponseDto { ErrorCode = StatusCodes.Status409Conflict, ErrorMessage = "Trivia does not exists" };
                }

                //trivia = _mapper.Map<TriviaModel>(requestDto.trivia);
                trivia.UpdateTs = DateTime.Now;
                trivia.CtaTaskExternalCode= requestDto.trivia.CtaTaskExternalCode;
                trivia.TaskRewardId = taskRewardModel.TaskRewardId;
                trivia.ConfigJson = JsonConvert.SerializeObject(requestDto?.trivia.ConfigJson, new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                });
                trivia.UpdateUser = requestDto?.trivia.UpdateUser ?? Constant.SystemUser;
                trivia = await _triviaRepo.UpdateAsync(trivia);
                if (trivia.TriviaId > 0)
                {
                    _triviaLogger.LogInformation("{className}.{methodName}: Successfully Updated data for trivia request: {requestDto}", className, methodName, requestDto.ToJson());
                    return new BaseResponseDto();
                }
                else
                {
                    _triviaLogger.LogError("{className}.{methodName}: Failed to Update data for Trivia request: {requestDto}", className, methodName, requestDto.ToJson());
                    return new BaseResponseDto { ErrorCode = StatusCodes.Status404NotFound, ErrorMessage = "Trivia record Not Updated" };
                }

            }
            catch (Exception ex)
            {
                _triviaLogger.LogError(ex, "{className}.{methodName}: ERROR - msg : {msg}, for requestDto: {RequestDto}", className, methodName, ex.Message, requestDto.ToJson());
                return new BaseResponseDto { ErrorCode = StatusCodes.Status500InternalServerError, ErrorMessage = "Trivia record Not Updated" };

            }
        }

    }
}
