using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Infrastructure.Helpers.Interface;
using SunnyRewards.Helios.Task.Infrastructure.Mappings;
using SunnyRewards.Helios.Task.Infrastructure.Mappings.MappingProfile;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SunnyRewards.Helios.Task.Infrastructure.Services.Helpers
{
    public class RowToDtoMapper : IRowToDtoMapper
    {
        private readonly IMappingProfileProvider _mappingProfileProvider;

        public RowToDtoMapper(IMappingProfileProvider mappingProfileProvider)
        {
            _mappingProfileProvider = mappingProfileProvider;
        }

        public TDto MapToDto<TDto>(IDictionary<string, object> row) where TDto : new()
        {
            var propertyToColumnMap = _mappingProfileProvider.GetPropertyToColumnMap<TDto>();
            return MapToDtoInternal<TDto>(row, propertyToColumnMap);
        }

        private static TDto MapToDtoInternal<TDto>(IDictionary<string, object> row, IDictionary<string, string> propertyToColumnMap)
            where TDto : new()
        {
            var dto = new TDto();
            var dtoType = typeof(TDto);

            foreach (var prop in dtoType.GetProperties())
            {
                if (propertyToColumnMap.TryGetValue(prop.Name, out var columnName))
                {
                    if (row.TryGetValue(columnName, out var value) && value != null)
                    {
                        try
                        {
                            var targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                            var safeValue = Convert.ChangeType(value, targetType);
                            prop.SetValue(dto, safeValue);
                        }
                        catch
                        {
                            // Optionally log or handle conversion errors
                        }
                    }
                }
            }

            return dto;
        }
    }
}
