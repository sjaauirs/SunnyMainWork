using AutoMapper;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Common.Core.Domain;
using SunnyRewards.Helios.Common.Core.Services;
using SunnyRewards.Helios.User.Core.Domain.Constant;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Models;
using SunnyRewards.Helios.User.Infrastructure.ReadReplica;
using SunnyRewards.Helios.User.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.User.Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Http;

namespace SunnyRewards.Helios.User.Infrastructure.Services
{
    public class ConsumerFlowProgressService : BaseService, IConsumerFlowProgressService
    {
        private readonly ILogger<ConsumerFlowProgressService> _consumerLogger;
        private readonly IMapper _mapper;
        private readonly IConsumerFlowProgressRepo _consumerFlowProgressRepo;
        private readonly IConsumerOnboardingProgressHistoryRepo _consumerOnboardingProgressHistoryRepo;
        private readonly NHibernate.ISession _session;
        private readonly IReadOnlySession? _readOnlySession;
        private const string _className = nameof(ConsumerFlowProgressService);

        private NHibernate.ISession ReadSession => _readOnlySession?.Session ?? _session;

        public ConsumerFlowProgressService(
            ILogger<ConsumerFlowProgressService> consumerLogger,
            IMapper mapper,
            IConsumerFlowProgressRepo consumerFlowProgressRepo,
            IConsumerOnboardingProgressHistoryRepo consumerOnboardingProgressHistoryRepo,
            NHibernate.ISession session,
            IReadOnlySession? readOnlySession = null)
        {
            _consumerLogger = consumerLogger;
            _mapper = mapper;
            _consumerFlowProgressRepo = consumerFlowProgressRepo;
            _consumerOnboardingProgressHistoryRepo = consumerOnboardingProgressHistoryRepo;
            _session = session;
            _readOnlySession = readOnlySession;
        }

        /// <summary>
        /// Retrieves the consumer flow progress based on the given request details.
        /// </summary>
        /// <param name="consumerFlowProgressRequestDto">The request containing consumer, tenant, and cohort information.</param>
        /// <returns>
        /// A response DTO containing the consumer flow progress.  
        /// Returns <c>NotStarted</c> status if no progress is found.
        /// </returns>

        public async Task<ConsumerFlowProgressResponseDto> GetConsumerFlowProgressAsync(ConsumerFlowProgressRequestDto consumerFlowProgressRequestDto)
        {
            const string MethodName = nameof(GetConsumerFlowProgressAsync);

            try
            {
               _consumerLogger.LogInformation("{ClassName}.{MethodName}: Request received for ConsumerCode: {ConsumerCode}, TenantCode: {TenantCode}",
                    _className, MethodName, consumerFlowProgressRequestDto.ConsumerCode, consumerFlowProgressRequestDto.TenantCode);

                var consumerFlowProgressModel = await _consumerFlowProgressRepo.FindOneAsync(x =>
                    x.ConsumerCode == consumerFlowProgressRequestDto.ConsumerCode &&
                    x.TenantCode == consumerFlowProgressRequestDto.TenantCode &&
                    (x.CohortCode == null || consumerFlowProgressRequestDto.CohortCodes.Count ==0  ||
                    consumerFlowProgressRequestDto.CohortCodes.Contains(x.CohortCode!)) &&
                    x.DeleteNbr == 0);

                if (consumerFlowProgressModel == null || consumerFlowProgressModel.Pk <= 0)
                {
                    _consumerLogger.LogInformation("{ClassName}.{MethodName}: No consumer flow progress found. Returning NotStarted status for ConsumerCode: {ConsumerCode}",
                        _className, MethodName, consumerFlowProgressRequestDto.ConsumerCode);

                    return new ConsumerFlowProgressResponseDto
                    {
                        ConsumerFlowProgress = new ConsumerFlowProgressDto
                        {
                            Status = Constant.NotStarted
                        }
                    };
                }

                var responseDto = _mapper.Map<ConsumerFlowProgressDto>(consumerFlowProgressModel);

                _consumerLogger.LogInformation("{ClassName}.{MethodName}: Consumer flow progress found. ConsumerCode: {ConsumerCode}",
                    _className, MethodName, consumerFlowProgressModel.ConsumerCode);

                return new ConsumerFlowProgressResponseDto
                {
                    ConsumerFlowProgress = responseDto
                };
            }
            catch (Exception ex)
            {
                _consumerLogger.LogError(ex, "{ClassName}.{MethodName}: Error occurred while fetching consumer flow progress for ConsumerCode: {ConsumerCode}, TenantCode: {TenantCode}",
                    _className, MethodName, consumerFlowProgressRequestDto?.ConsumerCode, consumerFlowProgressRequestDto?.TenantCode);
                throw;
            }
        }

        public async Task<ConsumerFlowProgressResponseDto> UpdateOnboardingStatusFlow(UpdateFlowStatusRequestDto updateOnboardingStatusDto)
        {
            const string methodName = nameof(UpdateOnboardingStatusFlow);
            try
            {
                var consumerFlowProgress = await _consumerFlowProgressRepo.FindOneAsync(x =>
                    x.ConsumerCode == updateOnboardingStatusDto.ConsumerCode &&
                    x.TenantCode == updateOnboardingStatusDto.TenantCode &&
                    x.DeleteNbr == 0 &&
                    x.FlowFk == updateOnboardingStatusDto.FlowId &&
                    x.VersionNbr == updateOnboardingStatusDto.VersionId);

                var status = updateOnboardingStatusDto.ToFlowStepId == null && string.Equals(updateOnboardingStatusDto.Status, Constant.Success, StringComparison.OrdinalIgnoreCase)
                    ? Constant.Completed : updateOnboardingStatusDto.Status;
                long? nextStep = updateOnboardingStatusDto.ToFlowStepId;
                ConsumerFlowProgressModel updatedConsumerFlowProgress;

                if (status.ToLower() == Constant.Completed)
                {
                    nextStep = null;
                }

                if (consumerFlowProgress != null)
                {
                    if (consumerFlowProgress.Status == updateOnboardingStatusDto.Status && consumerFlowProgress.FlowStepPk == updateOnboardingStatusDto.ToFlowStepId)
                    {
                        return new ConsumerFlowProgressResponseDto
                        {
                            ConsumerFlowProgress = _mapper.Map<ConsumerFlowProgressDto>(consumerFlowProgress)
                        };
                    }

                    consumerFlowProgress.Status = status;
                    consumerFlowProgress.UpdateTs = DateTime.UtcNow;
                    consumerFlowProgress.UpdateUser = Constants.CreateUser;
                    consumerFlowProgress.FlowStepPk = nextStep.HasValue && nextStep != 0 ? nextStep.Value : consumerFlowProgress.FlowStepPk;

                    updatedConsumerFlowProgress = await _consumerFlowProgressRepo.UpdateAsync(consumerFlowProgress);
                }
                else
                {
                    consumerFlowProgress = new ConsumerFlowProgressModel()
                    {
                        Status = status,
                        CreateTs = DateTime.UtcNow,
                        CreateUser = Constants.CreateUser,
                        CohortCode = updateOnboardingStatusDto.CohortCode,
                        ConsumerCode = updateOnboardingStatusDto.ConsumerCode,
                        TenantCode = updateOnboardingStatusDto.TenantCode,
                        FlowFk = updateOnboardingStatusDto.FlowId,
                        VersionNbr = updateOnboardingStatusDto.VersionId,
                        FlowStepPk = nextStep,
                        DeleteNbr = 0,
                        ContextJson = "{}"
                    };

                    await _session.SaveAsync(consumerFlowProgress);

                    updatedConsumerFlowProgress = consumerFlowProgress;
                }

                var consumerOnboardingProgressHistory = await _consumerOnboardingProgressHistoryRepo.FindOneAsync(
                    x => x.FlowFk == updateOnboardingStatusDto.FlowId &&
                        x.ToFlowStepPk == updateOnboardingStatusDto.ToFlowStepId &&
                        x.ConsumerFlowProgressFk == updatedConsumerFlowProgress.Pk &&
                        x.VersionNbr == updateOnboardingStatusDto.VersionId &&
                        x.ConsumerCode == updateOnboardingStatusDto.ConsumerCode &&
                        x.TenantCode == updateOnboardingStatusDto.TenantCode &&
                        x.DeleteNbr == 0 &&
                        x.Outcome == status &&
                        x.FromFlowStepPk == updateOnboardingStatusDto.FromFlowStepId
                    );

                if (consumerOnboardingProgressHistory == null)
                {
                    consumerOnboardingProgressHistory = new ConsumerOnboardingProgressHistoryModel
                    {
                        ConsumerCode = updatedConsumerFlowProgress.ConsumerCode,
                        TenantCode = updatedConsumerFlowProgress.TenantCode,
                        CohortCode = updatedConsumerFlowProgress.CohortCode,
                        FlowFk = updatedConsumerFlowProgress.FlowFk,
                        VersionNbr = updatedConsumerFlowProgress.VersionNbr,
                        FromFlowStepPk = updateOnboardingStatusDto.FromFlowStepId,
                        ToFlowStepPk = updateOnboardingStatusDto.ToFlowStepId,
                        Outcome = status,
                        ConsumerFlowProgressFk = updatedConsumerFlowProgress.Pk,
                        CreateTs = DateTime.UtcNow,
                        CreateUser = Constants.CreateUser,
                        DeleteNbr = 0
                    };

                    await _session.SaveAsync(consumerOnboardingProgressHistory);
                }

                return new ConsumerFlowProgressResponseDto
                {
                    ConsumerFlowProgress = _mapper.Map<ConsumerFlowProgressDto>(updatedConsumerFlowProgress)
                };

            }
            catch (Exception ex)
            {
                _consumerLogger.LogError(ex, "{className}.{methodName}: ERROR - msg: {msg}, Error Code:{errorCode}", _className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return new ConsumerFlowProgressResponseDto() { ErrorMessage = ex.Message, ErrorDescription = ex.InnerException?.Message };
            }
        }
    }
}