using Newtonsoft.Json;

namespace SunnyRewards.Helios.ETL.Common.Extensions
{
    public static class JsonExtension
    {
        public static string? ToJson(this object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
    }
}