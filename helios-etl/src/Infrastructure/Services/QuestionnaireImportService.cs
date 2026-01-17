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
    public class QuestionnaireImportService : IQuestionnaireImportService
    {
        private readonly ILogger<QuestionnaireImportService> _logger;
        private readonly IVault _vault;
        private const string className = nameof(QuestionnaireImportService);

        private readonly ITaskRewardRepo _taskRewardRepo;
        private readonly IQuestionnaireRepo _questionnaireRepo;
        private readonly IQuestionnaireQuestionRepo _questionnaireQuestionRepo;

        private readonly IdGenerator _idGenerator = new IdGenerator(10, 4);
        private readonly ISession _session;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="awsQueueService"></param>
        public QuestionnaireImportService(ILogger<QuestionnaireImportService> logger, IVault vault, ISession session,
            ITaskRewardRepo taskRewardRepo, IQuestionnaireRepo questionnaireRepo, IQuestionnaireQuestionRepo questionnaireQuestionRepo)
        {
            _logger = logger;
            _vault = vault;
            _taskRewardRepo = taskRewardRepo;
            _session = session;
            _questionnaireRepo = questionnaireRepo;
            _questionnaireQuestionRepo = questionnaireQuestionRepo;
        }

        /// <summary>
        /// Imports Questionnaire files for given tenant code
        /// </summary>
        /// <param name="etlExecutionContext">The etl execution context.</param>
        public async Task Import(EtlExecutionContext etlExecutionContext)
        {
            const string methodName = nameof(Import);
            var tenantCode = etlExecutionContext.TenantCode;
            var questionnaireFilePath = etlExecutionContext.QuestionnaireImportFilePath;
            try
            {
                if (string.IsNullOrEmpty(questionnaireFilePath))
                {
                    _logger.LogError("{ClassName}.{MethodName} - QuestionnaireFilePath is not valid", className, methodName);
                    throw new ETLException(ETLExceptionCodes.NullValue, "Questionnaire file path is not valid");
                }

                // retrieve environment from vault
                var environment = await _vault.GetSecret("env");
                if (string.IsNullOrEmpty(environment))
                {
                    _logger.LogError("{ClassName}.{MethodName} - environment is not valid", className, methodName);
                    throw new ETLException(ETLExceptionCodes.AWSSecretNotFound, "Secret is not configured AWS secret manager for 'env'");
                }

                _logger.LogInformation("{ClassName}.{MethodName} - processing Questionnaire File: {FilePath}, Env: {Env}", className, methodName, questionnaireFilePath, environment);
                var QuestionnaireImportFileContents = etlExecutionContext.QuestionnaireImportFileContents;
                var QuestionnaireText = QuestionnaireImportFileContents?.Length > 0 ? Encoding.UTF8.GetString(QuestionnaireImportFileContents!) : File.ReadAllText(questionnaireFilePath);
                var Questionnaire = JsonConvert.DeserializeObject<QuestionnaireImportDto>(QuestionnaireText);

                if (Questionnaire == null)
                {
                    _logger.LogError("{ClassName}.{MethodName} - error deserializing Questionnaire File: {FilePath}", className, methodName, questionnaireFilePath);
                    throw new ETLException(ETLExceptionCodes.NullValue, $"Error deserializing Questionnaire File: {questionnaireFilePath}");
                }

                // check if current tenant already has this Questionnaire - abort if already exists
                var tenantTaskReward = await _taskRewardRepo.FindOneAsync(x => x.TenantCode == tenantCode &&
                    x.TaskExternalCode == Questionnaire.Questionnaire.QuestionnaireTaskExternalCode && x.DeleteNbr == 0);

                if (tenantTaskReward == null)
                {
                    _logger.LogError("{ClassName}.{MethodName} - Skipping Questionnaire task import because task reward is not found with tenant: {TenantCode}, TaskExternalCode: {QuestionnaireTaskExternalCode}", className, methodName, tenantCode, Questionnaire.Questionnaire.QuestionnaireTaskExternalCode);
                    throw new ETLException(ETLExceptionCodes.NullValue, $"Error deserializing Questionnaire File: {questionnaireFilePath}");
                }

                var questionnaireModel = await _questionnaireRepo.FindOneAsync(x => x.TaskRewardId == tenantTaskReward.TaskRewardId && x.DeleteNbr == 0);
                if (questionnaireModel != null)
                {
                    _logger.LogInformation("{ClassName}.{MethodName} - Tenant: {Tenant} already contains Questionnaire task: {TaskCode}, proceeding for update",
                         className, methodName, tenantCode, Questionnaire.Questionnaire.QuestionnaireTaskExternalCode);
                }

                await ImportQuestionnaire(tenantTaskReward, Questionnaire, questionnaireModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Error occured while Questionnaire import,ErrorCode:{Code}, ERROR:{Msg}", className, methodName, StatusCodes.Status500InternalServerError, ex.Message);
                throw;
            }
        }

        private async Task ImportQuestionnaire(ETLTaskRewardModel tenantTaskReward, QuestionnaireImportDto questionnaire, ETLQuestionnaireModel? existingQuestionnaire)
        {
            const string methodName = nameof(ImportQuestionnaire);
            var now = DateTime.UtcNow;

            var transaction = _session.BeginTransaction();
            try
            {
                List<QuestionnaireQuestionGroupDataDto> questionGroupData = new();
                foreach (var question in questionnaire.QuestionnaireQuestions)
                {
                    // check for question
                    var questionModel = await _questionnaireQuestionRepo.FindOneAsync(x => x.DeleteNbr == 0 && x.QuestionExternalCode == question.QuestionExternalCode);

                    var outputQuestion = new Dictionary<string, QuestionnaireQuestionDto>();
                    foreach (var item in question.LocalizedInfo)
                    {
                        outputQuestion.Add(item.Key, new QuestionnaireQuestionDto
                        { 
                            QuestionText = item.Value.QuestionText,
                            AnswerText = item.Value.AnswerText,
                            CorrectAnswer = item.Value.CorrectAnswer,
                            Learning = item.Value.Learning,
                            AnswerType = question.AnswerType,
                            LayoutType = question.LayoutType,
                            QuestionExternalCode = question.QuestionExternalCode,
                            ValidStartTs = question.ValidStartTs,
                            ValidEndTs = question.ValidEndTs,
                            AnswerScale = item.Value.AnswerScale
                        });
                    }
                    if (questionModel != null)
                    {
                        _logger.LogInformation("{ClassName}.{MethodName} - Questionnaire question already exists in the database with question external code: {QuestionExternalCode}. Proceeding to update", 
                            className, methodName, question.QuestionExternalCode);

                        questionModel.QuestionnaireJson = JsonConvert.SerializeObject(outputQuestion, new JsonSerializerSettings
                        {
                            ContractResolver = new CamelCasePropertyNamesContractResolver()
                        });
                        questionModel.UpdateTs = now;
                        questionModel.UpdateUser = "SYSTEM";
                        await _session.UpdateAsync(questionModel);

                        // question already exists
                        questionGroupData.Add(new QuestionnaireQuestionGroupDataDto()
                        {
                            Id = questionModel.QuestionnaireQuestionId,
                            ValidEndTs = question.ValidEndTs,
                            ValidStartTs = question.ValidStartTs
                        });
                    }
                    else
                    {
                        _logger.LogInformation("{ClassName}.{MethodName} - Questionnaire question doesn't exists in the database with question external code: {QuestionExternalCode}. Proceeding to insert",
                            className, methodName, question.QuestionExternalCode);
                        // insert new question
                        questionModel = new ETLQuestionnaireQuestionModel
                        {
                            QuestionnaireQuestionCode = CreateCode("qsq"),
                            QuestionnaireJson = JsonConvert.SerializeObject(outputQuestion, new JsonSerializerSettings
                            {
                                ContractResolver = new CamelCasePropertyNamesContractResolver()
                            }),
                            QuestionExternalCode = question.QuestionExternalCode,
                            CreateTs = now,
                            CreateUser = "SYSTEM",
                            DeleteNbr = 0
                        };
                        var questionId = await _session.SaveAsync(questionModel);
                        questionGroupData.Add(new QuestionnaireQuestionGroupDataDto()
                        {
                            Id = (long)questionId,
                            ValidEndTs = question.ValidEndTs,
                            ValidStartTs = question.ValidStartTs
                        });
                    }
                }
                if (existingQuestionnaire == null)
                {
                    _logger.LogInformation("{ClassName}.{MethodName} - Questionnaire doesn't exists in the database for the task reward id: {TaskRewardId}, proceeding to insert",
                            className, methodName, tenantTaskReward.TaskRewardId);
                    // insert Questionnaire
                    var QuestionnaireModel = new ETLQuestionnaireModel
                    {
                        QuestionnaireCode = CreateCode("qsr"),
                        TaskRewardId = tenantTaskReward.TaskRewardId,
                        CtaTaskExternalCode = questionnaire.Questionnaire.CtaTaskExternalCode,
                        ConfigJson = JsonConvert.SerializeObject(questionnaire.Questionnaire.Config, new JsonSerializerSettings
                        {
                            ContractResolver = new CamelCasePropertyNamesContractResolver()
                        }),
                        CreateTs = now,
                        CreateUser = "SYSTEM",
                        DeleteNbr = 0
                    };
                    long QuestionnaireId = (long)await _session.SaveAsync(QuestionnaireModel);
                    existingQuestionnaire = new ETLQuestionnaireModel { QuestionnaireId = QuestionnaireId };
                }

                // insert question group
                int seq = 0;
                foreach (var questionGroup in questionGroupData)
                {
                    var questionGroupModel = await _session.Query<ETLQuestionnaireQuestionGroupModel>().FirstOrDefaultAsync(x =>
                        x.DeleteNbr == 0 && x.QuestionnaireId == existingQuestionnaire.QuestionnaireId && x.QuestionnaireQuestionId == questionGroup.Id);

                    if (questionGroupModel != null)
                    {
                        _logger.LogInformation("{ClassName}.{MethodName} - Questionnaire question group already exists in the database for the Questionnaire id: {QuestionnaireId} and question id:{QuestionId}, proceeding to update",
                            className, methodName, existingQuestionnaire.QuestionnaireId, questionGroup.Id);

                        questionGroupModel.UpdateUser = "SYSTEM";
                        questionGroupModel.UpdateTs = now;
                        questionGroupModel.ValidStartTs = questionGroup.ValidStartTs ?? DateTime.UtcNow.AddMonths(-1);
                        questionGroupModel.ValidEndTs = questionGroup.ValidEndTs ?? DateTime.UtcNow.AddMonths(1);
                        questionGroupModel.SequenceNbr = seq++;
                        await _session.UpdateAsync(questionGroupModel);
                    }
                    else
                    {
                        _logger.LogInformation("{ClassName}.{MethodName} - Questionnaire question group doesn't exists in the database for the Questionnaire id: {QuestionnaireId} and question id:{QuestionId}, proceeding to insert",
                            className, methodName, existingQuestionnaire.QuestionnaireId, questionGroup.Id);

                        var qgModel = new ETLQuestionnaireQuestionGroupModel
                        {
                            QuestionnaireId = existingQuestionnaire.QuestionnaireId,
                            QuestionnaireQuestionId = questionGroup.Id,
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
                _logger.LogError(ex, "{ClassName}.{MethodName} - Error importing Questionnaire: {TaskHeader},ErrorCode:{Code},ERROR: {Msg}", className, methodName, questionnaire.Questionnaire.QuestionnaireTaskExternalCode, StatusCodes.Status500InternalServerError, ex.Message);
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
