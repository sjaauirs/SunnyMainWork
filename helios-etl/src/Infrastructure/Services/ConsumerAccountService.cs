using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NHibernate.Criterion;
using SunnyRewards.Helios.ETL.Core.Domain.Constants;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services
{
    public class ConsumerAccountService : IConsumerAccountService
    {
        private readonly IConsumerAccountRepo _consumerAccountRepo;
        private readonly NHibernate.ISession _session;
        private readonly ILogger<ConsumerAccountService> _logger;
        private readonly IMapper _mapper;
        private readonly IJobReportService _jobReportService;
        private const  string className=nameof(ConsumerAccountService);

        public ConsumerAccountService(IConsumerAccountRepo consumerAccountRepo, NHibernate.ISession session,
            ILogger<ConsumerAccountService> logger, IMapper mapper, IJobReportService jobReportService)
        {
            _consumerAccountRepo = consumerAccountRepo;
            _session = session;
            _logger = logger;
            _mapper = mapper;
            _jobReportService = jobReportService;
        }

        public async Task<List<ETLConsumerAccountModel>> MergeConsumerAccountAsync(List<ETLConsumerAccountModel> requestDto)
        {

            const string methodName=nameof(MergeConsumerAccountAsync);
            _logger.LogInformation("{ClassName}.{MethodName} - Started processing with request ConsumerCodes:{Codes}", className, methodName, requestDto.Select(e=>e.ConsumerCode).ToList());
            using var transaction = _session.BeginTransaction();
            try
            {
                var response = await UpsertConsumerAccount(requestDto);

                await transaction.CommitAsync();
                _logger.LogInformation("MergeConsumerAccount: Consumer accounts created successfully");
                _logger.LogInformation("{ClassName}.{MethodName} - Consumer accounts created successfully request ConsumerCodes:{Codes}", className, methodName, requestDto.Select(e => e.ConsumerCode).ToList());
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Error occurred while creating consumer account. for CosnumerCode:{Codes},ErrorCode:{Code}, ERROR : {ErrorMessage}", className, methodName, requestDto.Select(e => e.ConsumerCode).ToList(),StatusCodes.Status500InternalServerError, ex.Message);
                await transaction.RollbackAsync();
                _session.Clear();
                return null;  // throw will exit the job
            }
        }

        private async Task<List<ETLConsumerAccountModel>> UpsertConsumerAccount(List<ETLConsumerAccountModel> consumerAccounts)
        {
            foreach (var entity in consumerAccounts)
            {
                // Check if an entity with the same TenantCode and ConsumerCode exists
                var existingEntity = _session.CreateCriteria<ETLConsumerAccountModel>()
                    .Add(Restrictions.Eq(nameof(ETLConsumerAccountModel.TenantCode), entity.TenantCode))
                    .Add(Restrictions.Eq(nameof(ETLConsumerAccountModel.ConsumerCode), entity.ConsumerCode))
                    .Add(Restrictions.Eq(nameof(ETLConsumerAccountModel.DeleteNbr), (long)0))
                    .UniqueResult<ETLConsumerAccountModel>();

                if (existingEntity != null)
                {
                    // Update existing entity
                    entity.ConsumerAccountId = existingEntity.ConsumerAccountId;
                    existingEntity.ProxyNumber = entity.ProxyNumber;
                    existingEntity.ProxyUpdateTs = entity.UpdateTs = DateTime.UtcNow;
                    if(!string.IsNullOrWhiteSpace(entity.ClientUniqueId))
                    {
                        existingEntity.ClientUniqueId = entity.ClientUniqueId;
                    }
                    if (!string.IsNullOrWhiteSpace(entity.CardIssueStatus)) {
                        existingEntity.CardIssueStatus = entity.CardIssueStatus;
                    }
                    try
                    {
                        await _session.MergeAsync(existingEntity);
                    }
                    catch(Exception ex)
                    {
                        _jobReportService.keyRecordErrorMap[existingEntity.ConsumerCode!].ErrorMessage = ex.Message;
                        throw;
                    }
                }
                else
                {
                    entity.ConsumerAccountCode = "cac-" + Guid.NewGuid().ToString("N");
                    entity.CreateTs = entity.UpdateTs = DateTime.UtcNow;
                    entity.DeleteNbr = 0;
                    if (string.IsNullOrWhiteSpace(entity.CardRequestStatus))
                    {
                        entity.CardRequestStatus = BenefitsConstants.RequestedCardRequestStatusNotApplicable;
                    }
                    if (string.IsNullOrWhiteSpace(entity.CardIssueStatus))
                    {
                        entity.CardIssueStatus = BenefitsConstants.Card30BatchSentStatus;
                    }
                    // Insert new entity
                    try
                    {
                        await _session.SaveAsync(entity);
                    }
                    catch (Exception ex)
                    {
                        _jobReportService.keyRecordErrorMap[entity.ConsumerCode!].ErrorMessage = ex.Message;
                        throw;
                    }
                   
                    existingEntity = entity;
                }
                var consumerAccountHistoryModel = _mapper.Map<ETLConsumerAccountHistoryModel>(existingEntity);
                await SaveConsumerAccountHistory(consumerAccountHistoryModel);
            }
            return consumerAccounts;
        }

        private async Task<ETLConsumerAccountHistoryModel> SaveConsumerAccountHistory(ETLConsumerAccountHistoryModel consumerAccountHistory)
        {
            const string methodName=nameof(SaveConsumerAccountHistory);
            try
            {
                consumerAccountHistory.CreateUser = consumerAccountHistory.UpdateUser ?? consumerAccountHistory.CreateUser;
                consumerAccountHistory.CreateTs = DateTime.UtcNow;
                await _session.SaveAsync(consumerAccountHistory);
                return consumerAccountHistory;
            }
            catch (Exception ex)
            {
                _jobReportService.keyRecordErrorMap[consumerAccountHistory.ConsumerCode!].ErrorMessage = ex.Message;
                _logger.LogError(ex, "{ClassName}.{MethodName} - Error occurred while creating consumer account history. ErrorCode:{Code},ERROR : {ErrorMessage}", className, methodName, StatusCodes.Status500InternalServerError,ex.Message);

                throw new InvalidDataException(ex.Message);
            }
        }
    }
}
