using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SunnyRewards.Helios.ETL.Common.Helpers.Interfaces;

namespace SunnyRewards.Helios.ETL.Common.Helpers
{
    /// <summary>
    /// Helper methods for serialization and deserialization of objects.
    /// </summary>
    public class JsonConvertWrapper : IJsonConvertWrapper
    {
        private static readonly JsonSerializerSettings defaultSettings;

        static JsonConvertWrapper()
        {
            defaultSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Formatting = Formatting.Indented
            };
        }

        /// <summary>
        /// Gets the singleton instance of the default JSON serializer settings.
        /// </summary>
        public static JsonSerializerSettings DefaultSettings => defaultSettings;

        /// <summary>
        /// Serializes an object to a JSON string.
        /// </summary>
        /// <typeparam name="T">The type of the object to serialize.</typeparam>
        /// <param name="obj">The object to serialize.</param>
        /// <param name="setting">Optional settings for JSON serialization.</param>
        /// <returns>The JSON string representing the serialized object.</returns>
        public string SerializeObject<T>(T obj, JsonSerializerSettings? setting = null)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj), "Object to serialize cannot be null.");

            return JsonConvert.SerializeObject(obj, setting ?? DefaultSettings);
        }

        /// <summary>
        /// Deserializes a JSON string to an object of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of object to deserialize to.</typeparam>
        /// <param name="jsonString">The JSON string to deserialize.</param>
        /// <returns>The deserialized object.</returns>
        public T DeserializeObject<T>(string jsonString)
        {
            if (string.IsNullOrWhiteSpace(jsonString))
                throw new ArgumentException("JSON string cannot be null or empty.", nameof(jsonString));

            return JsonConvert.DeserializeObject<T>(jsonString, DefaultSettings);
        }
    }
}
