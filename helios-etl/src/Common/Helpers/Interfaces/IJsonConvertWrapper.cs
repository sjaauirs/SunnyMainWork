using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.ETL.Common.Helpers.Interfaces
{
    /// <summary>
    /// Helper methods for serialization and deserialization of objects.
    /// </summary>
    public interface IJsonConvertWrapper
    {
        /// <summary>
        /// Serializes an object to a JSON string.
        /// </summary>
        /// <typeparam name="T">The type of the object to serialize.</typeparam>
        /// <param name="obj">The object to serialize.</param>
        /// <param name="setting">Optional settings for JSON serialization.</param>
        /// <returns>The JSON string representing the serialized object.</returns>
        string SerializeObject<T>(T obj, JsonSerializerSettings? setting = null);

        /// <summary>
        /// Deserializes a JSON string to an object of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of object to deserialize to.</typeparam>
        /// <param name="jsonString">The JSON string to deserialize.</param>
        /// <returns>The deserialized object.</returns>
        T DeserializeObject<T>(string jsonString);
    }
}
