using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sunny.Benefits.Bff.Infrastructure.Helpers.Interface
{
   public interface ICommonHelper
    {
        Task<string> GetLanguageCode();
        string? GetUserConsumerCodeFromHttpContext();

    }
}
