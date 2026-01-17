using Microsoft.Extensions.Logging;
using NHibernate;
using SunnyRewards.Helios.Common.Core.Repositories;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos.Json;
using SunnyRewards.Helios.Tenant.Core.Domain.Models;
using SunnyRewards.Helios.Tenant.Infrastructure.Repositories.Interfaces;
using System.Text.Json;

namespace SunnyRewards.Helios.Tenant.Infrastructure.Repositories
{
    public class FlowStepRepo : BaseRepo<FlowStepModel>, IFlowStepRepo
    {

        private readonly ISession _session;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseLogger"></param>
        /// <param name="session"></param>
        public FlowStepRepo(ILogger<BaseRepo<FlowStepModel>> baseLogger, NHibernate.ISession session) : base(baseLogger, session)
        {
            _session = session;
        }

        /// <summary>
        /// Get Flow Steps
        /// </summary>
        /// <param name="flowRequestDto"></param>
        /// <returns></returns>
        public FlowResponseDto? GetFlowSteps(FlowRequestDto flowRequestDto)
        {

            var response = (from f in _session.Query<FlowModel>()
                            where f.DeleteNbr == 0
                                  && f.TenantCode == flowRequestDto.TenantCode
                                  && (flowRequestDto.FlowId == 0 || f.Pk == flowRequestDto.FlowId)
                                  && (flowRequestDto.FlowId != 0 || f.FlowName == flowRequestDto.FlowName)
                                  && (f.CohortCode == null || flowRequestDto.CohortCodes.Count == 0 || flowRequestDto.CohortCodes.Contains(f.CohortCode))
                                  && f.EffectiveStartTs <= flowRequestDto.EffectiveDate
                                  && (f.EffectiveEndTs == null || flowRequestDto.EffectiveDate < f.EffectiveEndTs)
                            orderby f.VersionNbr descending
                            select new FlowResponseDto
                            {
                                TenantCode = f.TenantCode,
                                CohortCode = f.CohortCode,
                                FlowId = f.Pk,
                                VersionNumber = f.VersionNbr
                            }).FirstOrDefault();

            if (response == null)
                return response;

            var steps = (from fs in _session.Query<FlowStepModel>()
                         join c in _session.Query<ComponentCatalogueModel>() on fs.CurrentComponentCatalogueFk equals c.Pk
                         join ct in _session.Query<ComponentTypeModel>() on c.ComponentTypeFk equals ct.Pk
                         // Join for the "success" flow steps (DefaultIfEmpty for left join)
                         join onSuccessStep in _session.Query<FlowStepModel>()
                            on new { FlowFk = (long?)fs.FlowFk, OnSuccessComponentCatalogueFk = (long?)fs.OnSuccessComponentCatalogueFk }
                            equals new { FlowFk = (long?)onSuccessStep.FlowFk, OnSuccessComponentCatalogueFk = (long?)onSuccessStep.CurrentComponentCatalogueFk } into successSteps
                         from onSuccess in successSteps.DefaultIfEmpty()
                             // Join for the "failure" flow steps (DefaultIfEmpty for left join)
                         join failureStep in _session.Query<FlowStepModel>()
                             on new { FlowFk = (long?)fs.FlowFk, OnFailureComponentCatalogueFk = (long?)fs.OnFailureComponentCatalogueFk }
                             equals new { FlowFk = (long?)failureStep.FlowFk, OnFailureComponentCatalogueFk = (long?)failureStep.CurrentComponentCatalogueFk } into failureSteps
                         from onFailure in failureSteps.DefaultIfEmpty()
                         where fs.DeleteNbr == 0
                               && c.DeleteNbr == 0
                               && ct.DeleteNbr == 0
                               && ct.IsActive
                               && fs.FlowFk == response.FlowId
                               && (onFailure == null || onFailure.FlowFk == response.FlowId)
                               && (onSuccess == null || onSuccess.FlowFk == response.FlowId)
                               && (onSuccess == null || onSuccess.DeleteNbr == 0)
                               && (onFailure == null || onFailure.DeleteNbr == 0)
                         orderby fs.StepIdx
                         select new FlowStepDto
                         {
                             StepId = fs.Pk,
                             StepIdx = fs.StepIdx,
                             ComponentType = ct.ComponentType,
                             ComponentName = c.ComponentName,
                             OnSuccessStepId = onSuccess != null ? onSuccess.Pk : null,
                             OnFailureStepId = onFailure != null ? onFailure.Pk : null,
                             StepConfigJson = fs.StepConfig
                         }).ToList();


            response.Steps = steps;
            return response;

        }
    }
}
