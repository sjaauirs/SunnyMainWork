using Sunny.Benefits.Bff.Core.Domain.Dtos;

namespace Sunny.Benefits.Bff.Infrastructure.Services.Interfaces
{
    public interface IDynamicQueryProcessor
    {
        object? GetFilterObject(DynamicFilterContext filterContext, string key);

        bool EvaluateConditionsForContext(object context, List<Condition> conditions);

        bool EvaluateConditionsForAllContexts(Dictionary<string, List<Condition>> contexts, DynamicFilterContext filterContext);
    }
}
