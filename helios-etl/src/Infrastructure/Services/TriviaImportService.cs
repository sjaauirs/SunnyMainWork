using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NHibernate.Linq;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using SunnyRewards.Helios.Etl.Core.Domain.Dtos;
using SunnyRewards.Helios.Etl.Infrastructure.Helpers;
using SunnyRewards.Helios.ETL.Common.CustomException;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces;
using System.Text;
using ISession = NHibernate.ISession;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services
{
    public class TriviaImportService : ITriviaImportService
    {
        private readonly ILogger<TriviaImportService> _logger;
        private readonly IVault _vault;
        private const string className = nameof(TriviaImportService);

        private readonly ITaskRewardRepo _taskRewardRepo;
        private readonly ITriviaRepo _triviaRepo;
        private readonly ITriviaQuestionRepo _triviaQuestionRepo;

        private readonly IdGenerator _idGenerator = new IdGenerator(10, 4);
        private readonly ISession _session;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="awsQueueService"></param>
        public TriviaImportService(ILogger<TriviaImportService> logger, IVault vault, ISession session,
            ITaskRewardRepo taskRewardRepo, ITriviaRepo triviaRepo, ITriviaQuestionRepo triviaQuestionRepo)
        {
            _logger = logger;
            _vault = vault;
            _taskRewardRepo = taskRewardRepo;
            _session = session;
            _triviaRepo = triviaRepo;
            _triviaQuestionRepo = triviaQuestionRepo;
        }

        /// <summary>
        /// Imports trivia files for given tenant code
        /// </summary>
        /// <param name="etlExecutionContext">The etl execution context.</param>
        public async Task Import(EtlExecutionContext etlExecutionContext)
        {
            const string methodName = nameof(Import);
            var tenantCode = etlExecutionContext.TenantCode;
            var triviaFilePath = etlExecutionContext.TriviaImportFilePath;
            try
            {
                if (string.IsNullOrEmpty(triviaFilePath))
                {
                    _logger.LogError("{ClassName}.{MethodName} - triviaFilePath is not valid", className, methodName);
                    throw new ETLException(ETLExceptionCodes.NullValue, "Trivia file path is not valid");
                }

                // retrieve environment from vault
                var environment = await _vault.GetSecret("env");
                if (string.IsNullOrEmpty(environment))
                {
                    _logger.LogError("{ClassName}.{MethodName} - environment is not valid", className, methodName);
                    throw new ETLException(ETLExceptionCodes.AWSSecretNotFound, "Secret is not configured AWS secret manager for 'env'");
                }

                _logger.LogInformation("{ClassName}.{MethodName} - processing trivia File: {FilePath}, Env: {Env}", className, methodName, triviaFilePath, environment);
                var triviaImportFileContents = etlExecutionContext.TriviaImportFileContents;
                var triviaText = triviaImportFileContents?.Length > 0 ? Encoding.UTF8.GetString(triviaImportFileContents!) : File.ReadAllText(triviaFilePath);
                var trivia = JsonConvert.DeserializeObject<TriviaImportDto>(triviaText);

                if (trivia == null)
                {
                    _logger.LogError("{ClassName}.{MethodName} - error deserializing trivia File: {FilePath}", className, methodName, triviaFilePath);
                    throw new ETLException(ETLExceptionCodes.NullValue, $"Error deserializing trivia File: {triviaFilePath}");
                }

                // check if current tenant already has this trivia - abort if already exists
                var tenantTaskReward = await _taskRewardRepo.FindOneAsync(x => x.TenantCode == tenantCode &&
                    x.TaskExternalCode == trivia.Trivia.TriviaTaskExternalCode && x.DeleteNbr == 0);

                if (tenantTaskReward == null)
                {
                    _logger.LogError("{ClassName}.{MethodName} - Skipping trivia task import because task reward is not found with tenant: {TenantCode}, TaskExternalCode: {TriviaTaskExternalCode}", className, methodName, tenantCode, trivia.Trivia.TriviaTaskExternalCode);
                    throw new ETLException(ETLExceptionCodes.NullValue, $"Error deserializing trivia File: {triviaFilePath}");
                }

                var triviaModel = await _triviaRepo.FindOneAsync(x => x.TaskRewardId == tenantTaskReward.TaskRewardId && x.DeleteNbr == 0);
                if (triviaModel != null)
                {
                    _logger.LogInformation("{ClassName}.{MethodName} - Tenant: {Tenant} already contains trivia task: {TaskCode}, proceeding for update",
                         className, methodName, tenantCode, trivia.Trivia.TriviaTaskExternalCode);
                }

                await ImportTrivia(tenantTaskReward, trivia, triviaModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Error occured while trivia import,ErrorCode:{Code}, ERROR:{Msg}", className, methodName, StatusCodes.Status500InternalServerError, ex.Message);
                throw;
            }
        }

        private async Task ImportTrivia(ETLTaskRewardModel tenantTaskReward, TriviaImportDto trivia, TriviaModel? existingTrivia)
        {
            const string methodName = nameof(ImportTrivia);
            var now = DateTime.UtcNow;

            var transaction = _session.BeginTransaction();
            try
            {
                List<TriviaQuestionGroupDataDto> questionGroupData = new();
                foreach (var question in trivia.TriviaQuestions)
                {
                    // check for question
                    var questionModel = await _triviaQuestionRepo.FindOneAsync(x => x.DeleteNbr == 0 && x.QuestionExternalCode == question.QuestionExternalCode);

                    var outputQuestion = new Dictionary<string, TriviaQuestionDto>();
                    foreach (var item in question.LocalizedInfo)
                    {
                        outputQuestion.Add(item.Key, new TriviaQuestionDto
                        { 
                            QuestionText = item.Value.QuestionText,
                            AnswerText = item.Value.AnswerText,
                            CorrectAnswer = item.Value.CorrectAnswer,
                            Learning = item.Value.Learning,
                            AnswerType = question.AnswerType,
                            LayoutType = question.LayoutType,
                            QuestionExternalCode = question.QuestionExternalCode,
                            ValidStartTs = question.ValidStartTs,
                            ValidEndTs = question.ValidEndTs
                        });
                    }
                    if (questionModel != null)
                    {
                        _logger.LogInformation("{ClassName}.{MethodName} - Trivia question already exists in the database with question external code: {QuestionExternalCode}. Proceeding to update", 
                            className, methodName, question.QuestionExternalCode);

                        questionModel.TriviaJson = JsonConvert.SerializeObject(outputQuestion, new JsonSerializerSettings
                        {
                            ContractResolver = new CamelCasePropertyNamesContractResolver()
                        });
                        questionModel.UpdateTs = now;
                        questionModel.UpdateUser = "SYSTEM";
                        await _session.UpdateAsync(questionModel);

                        // question already exists
                        questionGroupData.Add(new TriviaQuestionGroupDataDto()
                        {
                            Id = questionModel.TriviaQuestionId,
                            ValidEndTs = question.ValidEndTs,
                            ValidStartTs = question.ValidStartTs
                        });
                    }
                    else
                    {
                        _logger.LogInformation("{ClassName}.{MethodName} - Trivia question doesn't exists in the database with question external code: {QuestionExternalCode}. Proceeding to insert",
                            className, methodName, question.QuestionExternalCode);
                        // insert new question
                        questionModel = new TriviaQuestionModel
                        {
                            TriviaQuestionCode = CreateCode("trq"),
                            TriviaJson = JsonConvert.SerializeObject(outputQuestion, new JsonSerializerSettings
                            {
                                ContractResolver = new CamelCasePropertyNamesContractResolver()
                            }),
                            QuestionExternalCode = question.QuestionExternalCode,
                            CreateTs = now,
                            CreateUser = "SYSTEM",
                            DeleteNbr = 0
                        };
                        var questionId = await _session.SaveAsync(questionModel);
                        questionGroupData.Add(new TriviaQuestionGroupDataDto()
                        {
                            Id = (long)questionId,
                            ValidEndTs = question.ValidEndTs,
                            ValidStartTs = question.ValidStartTs
                        });
                    }
                }
                if (existingTrivia == null)
                {
                    _logger.LogInformation("{ClassName}.{MethodName} - Trivia doesn't exists in the database for the task reward id: {TaskRewardId}, proceeding to insert",
                            className, methodName, tenantTaskReward.TaskRewardId);
                    // insert trivia
                    var triviaModel = new TriviaModel
                    {
                        TriviaCode = CreateCode("trv"),
                        TaskRewardId = tenantTaskReward.TaskRewardId,
                        CtaTaskExternalCode = trivia.Trivia.CtaTaskExternalCode,
                        ConfigJson = JsonConvert.SerializeObject(trivia.Trivia.Config, new JsonSerializerSettings
                        {
                            ContractResolver = new CamelCasePropertyNamesContractResolver()
                        }),
                        CreateTs = now,
                        CreateUser = "SYSTEM",
                        DeleteNbr = 0
                    };
                    long triviaId = (long)await _session.SaveAsync(triviaModel);
                    existingTrivia = new TriviaModel { TriviaId = triviaId };
                }

                // insert question group
                int seq = 0;
                foreach (var questionGroup in questionGroupData)
                {
                    var questionGroupModel = await _session.Query<TriviaQuestionGroupModel>().FirstOrDefaultAsync(x =>
                        x.DeleteNbr == 0 && x.TriviaId == existingTrivia.TriviaId && x.TriviaQuestionId == questionGroup.Id);

                    if (questionGroupModel != null)
                    {
                        _logger.LogInformation("{ClassName}.{MethodName} - Trivia question group already exists in the database for the trivia id: {TriviaId} and question id:{QuestionId}, proceeding to update",
                            className, methodName, existingTrivia.TriviaId, questionGroup.Id);

                        questionGroupModel.UpdateUser = "SYSTEM";
                        questionGroupModel.UpdateTs = now;
                        questionGroupModel.ValidStartTs = questionGroup.ValidStartTs ?? DateTime.UtcNow.AddMonths(-1);
                        questionGroupModel.ValidEndTs = questionGroup.ValidEndTs ?? DateTime.UtcNow.AddMonths(1);
                        questionGroupModel.SequenceNbr = seq++;
                        await _session.UpdateAsync(questionGroupModel);
                    }
                    else
                    {
                        _logger.LogInformation("{ClassName}.{MethodName} - Trivia question group doesn't exists in the database for the trivia id: {TriviaId} and question id:{QuestionId}, proceeding to insert",
                            className, methodName, existingTrivia.TriviaId, questionGroup.Id);

                        var qgModel = new TriviaQuestionGroupModel
                        {
                            TriviaId = existingTrivia.TriviaId,
                            TriviaQuestionId = questionGroup.Id,
                            SequenceNbr = seq++,
                            CreateTs = now,
                            CreateUser = "SYSTEM",
                            DeleteNbr = 0,
                            ValidStartTs = questionGroup.ValidStartTs.HasValue ? questionGroup.ValidStartTs.Value : DateTime.UtcNow.AddMonths(-1),
                            ValidEndTs = questionGroup.ValidEndTs.HasValue ? questionGroup.ValidEndTs.Value : DateTime.UtcNow.AddMonths(1)
                        };
                        await _session.SaveAsync(qgModel);
                    }
                }

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Error importing trivia: {TaskHeader},ErrorCode:{Code},ERROR: {Msg}", className, methodName, trivia.Trivia.TriviaTaskExternalCode, StatusCodes.Status500InternalServerError, ex.Message);
                await transaction.RollbackAsync();
                throw;
            }
        }

        private string CreateCode(string prefix)
        {
            return prefix + "-" + Guid.NewGuid().ToString("N");
        }
    }
}
