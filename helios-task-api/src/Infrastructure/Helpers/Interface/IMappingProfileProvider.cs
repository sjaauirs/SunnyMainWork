using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Task.Infrastructure.Helpers.Interface
{
    public interface IMappingProfileProvider
    {
        IDictionary<string, string> GetPropertyToColumnMap<TDto>();
    }
}
