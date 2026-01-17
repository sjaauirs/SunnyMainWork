using AutoMapper;
using Microsoft.Extensions.Logging;
using NHibernate;
using NHibernate.Transform;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Infrastructure.Helpers.Interface;
using SunnyRewards.Helios.Task.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Task.Infrastructure.Services.Helpers;
using SunnyRewards.Helios.Task.Infrastructure.Services.Interface;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Task.Infrastructure.Services
{


    public class DataQueryService : IDataQueryService
    {
        private readonly ISessionFactory _sessionFactory;
        private readonly IRowToDtoMapper _rowToDtoMapper;
        private readonly ILogger<DataQueryService> _logger;
        private readonly IQueryGeneratorService _dynamicQueryService;
        private const string ClassName = nameof(DataQueryService);

        public DataQueryService(
            ISessionFactory sessionFactory,
            ILogger<DataQueryService> logger,
            IRowToDtoMapper rowToDtoMapper,
            IQueryGeneratorService dynamicQueryService)
        {
            _sessionFactory = sessionFactory;
            _logger = logger;
            _rowToDtoMapper = rowToDtoMapper;
            _dynamicQueryService = dynamicQueryService;
        }

        public async Task<DataQueryResponseDto> GetConsumerTask(DataQueryRequestDto request)
        {
            const string methodName = nameof(GetConsumerTask);

            try
            {
                _logger.LogInformation("{Class}.{Method} - Started processing request for Tenant:{Tenant}, Consumer:{Consumer}",
                    ClassName, methodName, request.TenantCode, request.ConsumerCode);

                using var session = _sessionFactory.OpenSession();

                var baseSql = getQuery();

                var requiredParameters = new Dictionary<string, object>
                    {
                        { "consumer_code", request.ConsumerCode },
                        { "tenant_code", request.TenantCode },
                        { "language_code", request.LanguageCode }
                    };

                var (result, response) = await _dynamicQueryService.ExecuteDynamicQueryAsync(
                    baseSql,
                    request.SearchAttributes,
                    requiredParameters,
                    session);
                if (response.ErrorCode != null)
                {
                    _logger.LogError("{Class}.{Method} - Error in dynamic query execution for Tenant:{Tenant}, Consumer:{Consumer}. ErrorCode: {ErrorCode}, ErrorMessage: {ErrorMessage}",
                        ClassName, methodName, request.TenantCode, request.ConsumerCode, response.ErrorCode, response.ErrorMessage);
                    return new DataQueryResponseDto
                    {
                        ErrorCode = response.ErrorCode,
                        ErrorMessage = response.ErrorMessage
                    };
                }
                _logger.LogInformation("{Class}.{Method} - Retrieved {Count} records for Tenant:{Tenant}, Consumer:{Consumer}",
                    ClassName, methodName, result.Count, request.TenantCode, request.ConsumerCode);

                // Group by task_reward_code and map to TaskRewardDetailItemDto
                var grouped = await GetGroupedTaskRewardDetails(result);

                _logger.LogInformation("{Class}.{Method} - Query completed successfully for Tenant:{Tenant}, Consumer:{Consumer}",
                    ClassName, methodName, request.TenantCode, request.ConsumerCode);

                return new DataQueryResponseDto
                {
                    TaskRewardDetail = grouped
        .Select(kvp => new Dictionary<string, TaskRewardDetailItemDto>
        {
            { kvp.Key, kvp.Value }
        })
        .ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Class}.{Method} - Error while processing dynamic query for Tenant:{Tenant}, Consumer:{Consumer}",
                    ClassName, methodName, request.TenantCode, request.ConsumerCode);
                throw;
            }
        }
        private string getQuery()
        {
            return @"SELECT 
        tr.task_reward_code,
        t.*,
        tr.*,
        td.*,
    t.self_report AS self_report,
    tr.self_report AS task_reward_self_report,
        ct.*
    FROM task.task_reward tr
    INNER JOIN task.task t 
        ON tr.task_id = t.task_id 
    AND tr.tenant_code = :tenant_code
    LEFT JOIN task.task_detail td 
        ON td.task_id = t.task_id 
        AND td.language_code = :language_code
					And tr.tenant_code=td.tenant_code
    LEFT JOIN task.consumer_task ct 
        ON ct.task_id = t.task_id
        AND ct.tenant_code = tr.tenant_code
        AND ct.consumer_code = :consumer_code
        AND (ct.delete_nbr IS NULL OR ct.delete_nbr = 0)
    ";
        }

        private static IDictionary<string, object> RemapKeys(
        IDictionary<string, object> source,
        IReadOnlyDictionary<string, string> keyMap)
        {
            // Early return if no keys to remap
            if (keyMap == null || keyMap.Count == 0)
                return source;

            // Clone only if remapping needed
            var clone = new Dictionary<string, object>(source);

            foreach (var kv in keyMap)
            {
                // If old key exists, set its value to new key name
                if (clone.TryGetValue(kv.Key, out var val))
                {
                    clone[kv.Value] = val;
                }
            }

            return clone;
        }

        public async Task<Dictionary<string, TaskRewardDetailItemDto>> GetGroupedTaskRewardDetails(
            List<Dictionary<string, object>> result)
        {
            // Map of old → new keys for resolving overlaps
            var taskRewardRemap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        { "task_reward_self_report", "self_report" }
        // it can easily add more in future:
        
    };

            var grouped = result
                .GroupBy(r => r["task_reward_code"]?.ToString() ?? string.Empty)
                .ToDictionary(
                    g => g.Key,
                    g =>
                    {
                        var x = g.First();

                        // Remap overlapping columns only for TaskRewardDto
                        var remappedForReward = RemapKeys(x, taskRewardRemap);

                        return new TaskRewardDetailItemDto
                        {
                            Task = _rowToDtoMapper.MapToDto<TaskDto>(x),
                            TaskReward = _rowToDtoMapper.MapToDto<TaskRewardDto>(remappedForReward),
                            TaskDetail = _rowToDtoMapper.MapToDto<TaskDetailDto>(x),
                            ConsumerTask = x["consumer_task_id"] == null
                                ? new ConsumerTaskDto()
                                : _rowToDtoMapper.MapToDto<ConsumerTaskDto>(x)
                        };
                    });

            return grouped;
        }

    }
}

