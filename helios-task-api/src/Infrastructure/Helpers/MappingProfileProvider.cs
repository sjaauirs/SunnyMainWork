using FluentNHibernate;
using FluentNHibernate.Mapping;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Infrastructure.Helpers.Interface;
using SunnyRewards.Helios.Task.Infrastructure.Mappings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SunnyRewards.Helios.Task.Infrastructure.Helpers
{
    public class MappingProfileProvider : IMappingProfileProvider
    {
        private readonly ILogger<MappingProfileProvider> _logger;

        public MappingProfileProvider(ILogger<MappingProfileProvider> logger)
        {
            _logger = logger;
        }

        public IDictionary<string, string> GetPropertyToColumnMap<TDto>()
        {
            // Create the mapping instance (returns object)
            var mappingInstance = CreateMappingInstanceForDto<TDto>();
            return ExtractPropertyToColumnMap(mappingInstance);
        }

        private object CreateMappingInstanceForDto<TDto>()
        {
            var dtoType = typeof(TDto);
            if (dtoType.Name == nameof(TaskDto)) return new TaskMap();
            if (dtoType.Name == nameof(TaskRewardDto)) return new TaskRewardMap();
            if (dtoType.Name == nameof(TaskDetailDto)) return new TaskDetailMap();
            if (dtoType.Name == nameof(ConsumerTaskDto)) return new ConsumerTaskMap();
            throw new NotSupportedException($"No mapping defined for DTO type {dtoType.Name}");
        }

        private IDictionary<string, string> ExtractPropertyToColumnMap(object mappingInstance)
        {
            var propertyToColumnMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (mappingInstance == null) return propertyToColumnMap;
            if (!(mappingInstance is IMappingProvider provider)) return propertyToColumnMap;

            var classMapping = provider.GetClassMapping();

            // --- Extract Id property ---
            if (classMapping.Id != null)
            {
                var idMapping = classMapping.Id;
                var idType = idMapping.GetType();

                // Name property via reflection
                var propertyName = idType.GetProperty("Name")?.GetValue(idMapping)?.ToString();

                // Columns property via reflection
                var columnsProp = idType.GetProperty("Columns")?.GetValue(idMapping) as IEnumerable<object>;
                var columnName = columnsProp?.Cast<dynamic>().FirstOrDefault()?.Name;

                if (!string.IsNullOrEmpty(propertyName) && !string.IsNullOrEmpty(columnName))
                    propertyToColumnMap[propertyName] = columnName;
            }

            // --- Extract regular properties ---
            foreach (var prop in classMapping.Properties)
            {
                var propType = prop.GetType();
                var propName = propType.GetProperty("Name")?.GetValue(prop)?.ToString();
                var columnsProp = propType.GetProperty("Columns")?.GetValue(prop) as IEnumerable<object>;
                var columnName = columnsProp?.Cast<dynamic>().FirstOrDefault()?.Name;

                if (!string.IsNullOrEmpty(propName) && !string.IsNullOrEmpty(columnName))
                    propertyToColumnMap[propName] = columnName;
            }

            return propertyToColumnMap;
        }


    }
}
