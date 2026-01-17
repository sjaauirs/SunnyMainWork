using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Task.Infrastructure.Helpers.Interface
{
    public interface IRowToDtoMapper
    {
        TDto MapToDto<TDto>(IDictionary<string, object> row) where TDto : new();
    }
}
