using System;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using Microsoft.Extensions.Logging;
using Sunny.Benefits.Bff.Infrastructure.Helpers.Interface;

namespace Sunny.Benefits.Bff.Infrastructure.Helpers
{
    /// <summary>
    /// Helper for interacting with AWS Systems Manager Parameter Store.
    /// </summary>
    public class ParameterStoreHelper : IParameterStoreHelper
    {
        private readonly IAmazonSimpleSystemsManagement _ssmClient;
        private readonly ILogger<ParameterStoreHelper> _logger;

        public ParameterStoreHelper(
            IAmazonSimpleSystemsManagement ssmClient,
            ILogger<ParameterStoreHelper> logger)
        {
            _ssmClient = ssmClient;
            _logger = logger;
        }

        public async Task<string?> GetRawValueAsync(string parameterName, bool withDecryption = true)
        {
            try
            {
                var request = new GetParameterRequest
                {
                    Name = parameterName,
                    WithDecryption = withDecryption
                };

                var response = await _ssmClient.GetParameterAsync(request);
                return response.Parameter?.Value;
            }
            catch (ParameterNotFoundException)
            {
                _logger.LogWarning("Parameter not found in SSM: {ParameterName}", parameterName);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving parameter from SSM: {ParameterName}", parameterName);
                throw;
            }
        }

        public async Task<T?> GetDeserializedValueAsync<T>(string parameterName, bool withDecryption = true)
        {
            var rawJson = await GetRawValueAsync(parameterName, withDecryption);

            if (string.IsNullOrWhiteSpace(rawJson))
            {
                _logger.LogWarning("Empty or null parameter value for: {ParameterName}", parameterName);
                return default;
            }

            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                return JsonSerializer.Deserialize<T>(rawJson, options);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize parameter: {ParameterName}", parameterName);
                throw;
            }
        }

        public async Task<bool> SaveValueAsync<T>(string parameterName, T value)
        {
            var json = JsonSerializer.Serialize(value, new JsonSerializerOptions { WriteIndented = false });

            var request = new PutParameterRequest
            {
                Name = parameterName,
                Value = json,
                Type = ParameterType.String,
                Overwrite = true
            };

            var response = await _ssmClient.PutParameterAsync(request);
            return response.HttpStatusCode == System.Net.HttpStatusCode.OK;
        }
    }
}

