using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SunnyRewards.Helios.ETL.Common.Extensions;
using SunnyRewards.Helios.ETL.Common.Repositories.Interfaces;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using SunnyRewards.Helios.ETL.Infrastructure.Logs;
using SunnyRewards.Helios.ETL.Infrastructure.Logs.Interface;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.RuleEngine;
using SunnyRewards.Helios.ETL.Infrastructure.RuleEngine.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces;
using System.Linq.Dynamic.Core;
using SunnyRewards.Helios.ETL.Common.Constants;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services
{
    public class CohortService : ICohortService
    {
        private readonly ILogger<EnrollmentService> _logger;
        private readonly NHibernate.ISession _session;
        private readonly IRuleExecutor _ruleExecutor;
        private readonly IAuditTrailRepo _auditTrailRepo;

        private readonly IS3FileLogger _s3FileLogger;
        private readonly IConsumerQuery _consumerQuery;
        private readonly ITaskCompletionCheckerService _taskCompletionCheckerService;
        private readonly IConsumerRepo _consumerRepo;
        private readonly IAwsS3Service _awsS3Service;

        private const string className = nameof(CohortService);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="session"></param>
        public CohortService(ILogger<EnrollmentService> logger,
            IAuditTrailRepo auditTrailRepo, IS3FileLogger s3FileLogger,
            NHibernate.ISession session, ITaskCompletionCheckerService taskCompletionCheckerService,
            IRuleExecutor ruleExecutor, IConsumerQuery consumerQuery, IConsumerRepo consumerRepo, IAwsS3Service awsS3Service)

        {
            _ruleExecutor = ruleExecutor;
            _logger = logger;
            _session = session;
            _auditTrailRepo = auditTrailRepo;
            _taskCompletionCheckerService = taskCompletionCheckerService;
            _s3FileLogger = s3FileLogger;
            _consumerQuery = consumerQuery;
            _consumerRepo = consumerRepo;
            _awsS3Service = awsS3Service;
        }

        #region RuleExecutor - For demo only
        /// <summary>
        /// RuleExecutor Test 
        /// </summary>
        public async Task<CohortRuleExecutionDto> TestRuleExecutor()
        {
            var consumer = new ETLConsumerModel();
            consumer.MemberNbr = "123";
            consumer.ConsumerAttribute = @"{'Den_for_Bre_Can_Scr_BCS': '1', 'Num_for_Bre_Can_Scr_BCS': '5', 'Den_for_Bre_Can_Scr_BCS': '15'}";

            var person = new ETLPersonModel()
            {
                DOB = DateTime.UtcNow.AddYears(-19),
                Gender = "FEMALE"
            };
            _consumerQuery.ConsumerCode = "AVCSDDSD";
            var inputs = new Dictionary<string, object>()
            {
                {"consumer", consumer },
                {"person",person },
                {"util",new Util()},
                {"consumerQuery",_consumerQuery}
            };

            //string ruleJson = @"{
            //  ""ruleExpr"": ""person.Gender == \""FEMALE\"" && person.Age >= 40 && consumerQuery.IsInCohort(\""ABC\"")"",
            //  ""successExpr"": ""util.ToSuccess(\""age=\"" + person.Age.ToString() + \"", gender=\"" + person.Gender + \"", denom=\"" + consumer.Attr[\""Den_for_Bre_Can_Scr_BCS\""] + \"", num=\"" + consumer.Attr[\""Num_for_Bre_Can_Scr_BCS\""])""

            //}";
            string ruleJson = @"{
              ""ruleExpr"": ""person.Age > 18"",
              ""successExpr"": ""\""age=\"" + person.Age.ToString()""

            }";
            var result = await _ruleExecutor.Execute(ruleJson, inputs);
            return result;
        }
        #endregion

        /// <summary>
        /// Process Cohorts for the given consumers and persons
        /// </summary>
        /// <param name="etlConsumers"></param>
        /// <param name="etlPersons"></param>
        /// <param name="cohortCodesList"></param>
        /// <returns></returns>
        public async Task ProcessCohorts(List<ETLConsumerModel> etlConsumers, List<ETLPersonModel> etlPersons, List<string>? cohortCodesList = null)
        {
            const string methodName = nameof(ProcessCohorts);
            // skip the “everyone” cohort 
            // – we don’t create relnships as all consumers belong to this 
            // cohort by definition
            var cohortsQuery = _session.Query<ETLCohortModel>().Where(ch => ch.CohortEnabled == true
            && ch.DeleteNbr == 0 && ch.IncludeInCohortingJob);

            // if cohortCodesList is provided, filter cohorts based on the list
            if (cohortCodesList != null && cohortCodesList.Count > 0)
            {
                cohortsQuery = cohortsQuery.Where(x => cohortCodesList.Contains(x.CohortCode!));
            }
            var cohorts = cohortsQuery.OrderBy(x => x.CohortId).ToList();
            if (cohorts.Count == 0)
            {
                _logger.LogWarning("{ClassName}.{MethodName} - No cohorts found to process", className, methodName);
                return;
            }

            foreach (var consumer in etlConsumers)
            {
                foreach (var cohort in cohorts)
                {
                    _logger.LogInformation("{ClassName}.{MethodName} - Started processing cohorts with Consumer={ConsumerCode}, Cohort={CohortName}", className, methodName, consumer.ConsumerCode, cohort.CohortName);

                    using var transaction = _session.BeginTransaction();
                    try
                    {
                        string? cohortRuleJson = cohort.CohortRule;
                        if (string.IsNullOrEmpty(cohortRuleJson))
                        {
                            _logger.LogWarning("{ClassName}.{MethodName} - Warning no cohort rule attaced to cohort: {CohortName}", className, methodName, cohort.CohortName);
                            continue;
                        }

                        // Need to add person as wel bcoz dob is in person model
                        var person = etlPersons.FirstOrDefault(x => x.PersonId == consumer.PersonId);

                        _consumerQuery.ConsumerCode = consumer.ConsumerCode;
                        _taskCompletionCheckerService.CurrentConsumerCode = consumer.ConsumerCode;
                        var ruleResult = await _ruleExecutor.Execute(cohortRuleJson,
                            contextList: new Dictionary<string, object>() {
                                { "consumer", consumer },
                                { "person", person },
                                { "consumerQuery", _consumerQuery},
                                { "taskCompletionChecker", _taskCompletionCheckerService}
                            });
                        if (ruleResult.IsSuccess)
                        {
                            var cohortRuleJsonObject = JsonConvert.DeserializeObject<CohortRule>(cohortRuleJson);
                            _logger.LogInformation("{ClassName}.{MethodName} - ruleExecutionResultSuccessExpression={SuccessExpr}, consumer={ConsumerCode}", className, methodName, cohortRuleJsonObject?.SuccessExpr, consumer.ConsumerCode);

                        }
                        var cohortConsumer = _session.Query<ETLCohortConsumerModel>().Where(chr => chr.DeleteNbr == 0 && chr.CohortId == cohort.CohortId &&
                            chr.ConsumerCode == consumer.ConsumerCode).FirstOrDefault();

                        _logger.LogInformation("{ClassName}.{MethodName} - ruleExecutionResult={IsSuccess}, consumer={ConsumerCode}", className, methodName, ruleResult.IsSuccess, consumer.ConsumerCode);

                        // Cohort rule met with success and no cohort consumer found
                        if (ruleResult.IsSuccess && cohortConsumer == null)
                        {
                            var etlCohortConsumer = new ETLCohortConsumerModel()
                            {
                                CohortDetectDescription = ruleResult.CohortRuleSuccessResult,
                                CohortId = cohort.CohortId,
                                ConsumerCode = consumer.ConsumerCode,
                                CreateTs = DateTime.UtcNow,
                                CreateUser = "ETL",
                                DeleteNbr = 0,
                                TenantCode = consumer.TenantCode,
                            };
                            await _session.SaveAsync(etlCohortConsumer);

                            await _session.SaveAsync(new Common.Domain.Models.AuditTrailModel()
                            {
                                SourceModule = "ETL",
                                SourceContext = "CohortService.ProcessCohorts",
                                AuditName = "Cohort",
                                AuditMessage = $"Consumer added thru etl : {etlCohortConsumer.ConsumerCode}",
                                CreateUser = "SYSTEM",
                                CreateTs = DateTime.UtcNow,
                                AuditJsonData = etlCohortConsumer.ToJson()
                            });
                        }
                        // Cohort rule met with success and cohort consumer found
                        else if (ruleResult.IsSuccess && cohortConsumer != null)
                        {
                            cohortConsumer.CohortDetectDescription = ruleResult.CohortRuleSuccessResult;
                            cohortConsumer.UpdateUser = "SYSTEM";
                            cohortConsumer.UpdateTs = DateTime.UtcNow;
                            await _session.SaveAsync(cohortConsumer);
                        }
                        // Cohort rule met with failure
                        else if (!ruleResult.IsSuccess && cohortConsumer != null && cohortConsumer.CohortDetectDescription != "ADMIN_FORCED")
                        {
                            _logger.LogError("{ClassName}.{MethodName} - Cohort={CohortName} failed for consumer={ConsumerCode}", className, methodName, cohort.CohortName, consumer.ConsumerCode);
                            // delete the cohortConsumer relnship if it has not been forced by ADMIN
                            cohortConsumer.DeleteNbr = cohortConsumer.CohortConsumerId;
                            cohortConsumer.UpdateUser = "SYSTEM";
                            cohortConsumer.UpdateTs = DateTime.UtcNow;
                            await _session.SaveAsync(cohortConsumer);

                            await _session.SaveAsync(new Common.Domain.Models.AuditTrailModel()
                            {
                                SourceModule = "ETL",
                                SourceContext = "CohortService.ProcessCohorts",
                                AuditName = "Cohort",
                                AuditMessage = $"Consumer updated thru etl : {cohortConsumer.ConsumerCode}",
                                CreateUser = "SYSTEM",
                                CreateTs = DateTime.UtcNow,
                                AuditJsonData = cohortConsumer.ToJson()
                            });
                        }
                        await transaction.CommitAsync();
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogError(ex, "{ClassName}.{MethodName} - Error occured while proccesing cohorts,ErrorCode:{Code}, ERROR: {ex.Message}", className, methodName, StatusCodes.Status500InternalServerError, ex.Message);

                        // this log using for s3FileLogger
                        await _s3FileLogger.AddErrorLogs(new S3LogContext()
                        {
                            Message = ex.Message,
                            TenantCode = consumer.TenantCode,
                            ConsumerCode = consumer.ConsumerCode,
                            MemberNbr = consumer.MemberNbr,
                            Ex = ex
                        });
                    }
                }
            }
        }

        /// <summary>
        /// Process Cohorts
        /// </summary>
        /// <param name="etlExecutionContext"></param>
        /// <returns></returns>
        public async Task ExecuteCohortingAsync(EtlExecutionContext etlExecutionContext)
        {
            const string methodName = nameof(ExecuteCohortingAsync);

            try
            {
                _logger.LogInformation("{ClassName}.{MethodName} : Start processing... Request Payload: Tenant Code: {TenantCode}", className, methodName, etlExecutionContext.TenantCode);
                int skip = 0;
                int batchSize = etlExecutionContext.BatchSize;
                IQueryable<ETLConsumerAndPersonModel> batch;

                //Read consumer list from the file if exists and execute cohorting rules only for the listed consumers
                var consumerCodesList = etlExecutionContext.ConsumerListFile.ToUpper()==Constants.ALL? null:await _awsS3Service.GetConsumerListFromFile(etlExecutionContext.ConsumerListFile);
                //Read cohort codes from the list of cohort codes from execution context or from the file if exists and execute cohorting rules only for the listed cohorts
                List<string>? cohortCodesList = null;
                if (etlExecutionContext.CohortListFile.ToUpper() == Constants.ALL)
                {
                    cohortCodesList = null;
                }
                else if (etlExecutionContext.CohortListFile.Contains(',') || 
                    (etlExecutionContext.CohortListFile.StartsWith("coh", StringComparison.OrdinalIgnoreCase) && 
                    !etlExecutionContext.CohortListFile.Contains('.')))
                {
                    cohortCodesList = etlExecutionContext.CohortListFile.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(code => code.Trim())
                        .Where(code => !string.IsNullOrWhiteSpace(code))
                        .ToList();
                }
                else
                {
                    cohortCodesList = await _awsS3Service.GetCohortListFromFile(etlExecutionContext.CohortListFile);
                }
                do
                {
                    // Fetch the batch of records
                    batch = _consumerRepo.GetConsumersAndPersonsByTenantCode(etlExecutionContext.TenantCode, skip, batchSize, consumerCodesList);

                    var consumers = batch.Select(x => x.Consumer).ToList();
                    var persons = batch.Select(x => x.Person).ToList();
                   
                    if (consumers.Count > 0 && persons.Count > 0)
                    {
                        await ProcessCohorts(consumers, persons, cohortCodesList);
                    }

                    // Increment the skip value by batch size
                    skip += batchSize;

                } while (batch.Any());

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Error occured while processing... Request Payload: Tenant Code: {TenantCode},ErrorCode:{Code},ERROR:{Msg}",
                    className, methodName, etlExecutionContext.TenantCode, StatusCodes.Status500InternalServerError, ex.Message);
                throw;
            }
        }
    }
}


