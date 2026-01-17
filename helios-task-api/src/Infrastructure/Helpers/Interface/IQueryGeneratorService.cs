using NHibernate;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Task.Infrastructure.Helpers.Interface
{
    public interface IQueryGeneratorService
    {
        Task<(List<Dictionary<string, object>>, BaseResponseDto)> ExecuteDynamicQueryAsync(
            string baseSql,
            List<SearchAttributeDto> filters,
            IDictionary<string, object> requiredParameters,
            ISession session);
    }
}
