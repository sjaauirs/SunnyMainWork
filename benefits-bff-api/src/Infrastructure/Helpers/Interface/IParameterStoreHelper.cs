using System.Threading.Tasks;

namespace Sunny.Benefits.Bff.Infrastructure.Helpers.Interface
{
    /// <summary>
    /// Helper interface for interacting with AWS Systems Manager Parameter Store
    /// </summary>
    public interface IParameterStoreHelper
    {
        /// <summary>
        /// Retrieves the raw string value of a parameter from Parameter Store.
        /// </summary>
        /// <param name="parameterName">Full parameter path.</param>
        /// <param name="withDecryption">Whether to decrypt secure string parameters.</param>
        Task<string?> GetRawValueAsync(string parameterName, bool withDecryption = true);

        /// <summary>
        /// Retrieves and deserializes a parameter value into the specified type.
        /// </summary>
        /// <typeparam name="T">Type to deserialize into.</typeparam>
        /// <param name="parameterName">Full parameter path.</param>
        /// <param name="withDecryption">Whether to decrypt secure string parameters.</param>
        Task<T?> GetDeserializedValueAsync<T>(string parameterName, bool withDecryption = true);

        /// <summary>
        /// Saves a value to Parameter Store as a JSON string.
        /// </summary>
        /// <typeparam name="T">Type of the value to serialize.</typeparam>
        /// <param name="parameterName">Full parameter path.</param>
        /// <param name="value">Value to serialize and store.</param>
        Task<bool> SaveValueAsync<T>(string parameterName, T value);
    }
}

