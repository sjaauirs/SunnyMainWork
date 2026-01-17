using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SunnyRewards.Helios.ETL.Common.Helpers.Interfaces;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;

namespace SunnyRewards.Helios.ETL.Common.Helpers
{
    /// <summary>
    /// 
    /// </summary>
    public class BaseClient : HttpClient, IBaseClient
    {
        private const string ApiKey = "X-API-KEY";
        private const string XApiSessionKey = "X-API-SESSION-KEY";
        private readonly ILogger<BaseClient> _logger;
        const string className = nameof(BaseClient);
        public BaseClient(ILogger<BaseClient> logger)
        {
            _logger = logger;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="uri"></param>
        public BaseClient(string uri, ILogger<BaseClient> logger): this(logger)
        {
            if (!string.IsNullOrEmpty(uri))
            {
                BaseAddress = new Uri(uri);
                DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }
        }

        public async Task<T> Get<T>(string url, IDictionary<string, long> parameters, Dictionary<string, string>? headers = null)
        {
            if (headers != null)
            {
                foreach (var item in headers)
                {
                    this.DefaultRequestHeaders.Add(item.Key, item.Value);
                }
            }

            var queryString = new StringBuilder();
            if (parameters?.Count > 0)
            {
                queryString.Append('?');
                foreach (var param in parameters)
                {
                    queryString.Append($"{param.Key}={param.Value}&");
                }
                queryString.Length--; // Remove the trailing "&" character
            }

            var result = (parameters?.Count > 0) ? (await GetAsync(url + queryString.ToString())) : (await GetAsync(url));
            var response = await result.Content.ReadAsStringAsync();
            if (!result.IsSuccessStatusCode)
            {
                _logger.LogError($"GET Request Failed. Url: {url}, Status Code: {result.StatusCode}, Response: {response}");
            }
            var deserializedObject = JsonConvert.DeserializeObject<T>(response);

            return deserializedObject != null ? deserializedObject : throw new Exception("Deserialization Failed");
        }

        public async Task<T> GetById<T>(string url, long parameters)
        {
            var result = await GetAsync(url + parameters);
            var response = await result.Content.ReadAsStringAsync();
            if (!result.IsSuccessStatusCode)
            {
                _logger.LogError($"GET by ID Request Failed. Url: {url}, Status Code: {result.StatusCode}, Response: {response}");
            }
            var deserializedObject = JsonConvert.DeserializeObject<T>(response);

            return deserializedObject != null ? deserializedObject : throw new Exception("Deserialization Failed");
        }

        public async Task<T> Post<T>(string url, object data, Dictionary<string, string>? headers = null)
        {
            if (headers != null)
            {
                foreach (var item in headers)
                {
                    if ((item.Key == ApiKey || item.Key == XApiSessionKey) && this.DefaultRequestHeaders.Contains(item.Key))
                    {
                        this.DefaultRequestHeaders.Remove(item.Key);
                    }
                    this.DefaultRequestHeaders.Add(item.Key, item.Value);
                }
            }
            var requestJson = JsonConvert.SerializeObject(data);
            var result = await PostAsync(url, new StringContent(requestJson, Encoding.UTF8, "application/json"));
            var response = await result.Content.ReadAsStringAsync();
            
            if (!result.IsSuccessStatusCode)
            {
                _logger.LogError($"POST Request Failed. Url: {url}, Status Code: {result.StatusCode}, Response: {response}");
                _logger.LogError($"POST Request Failed. Url: {url}, Request: {requestJson}");
            }
            var deserializedObject = JsonConvert.DeserializeObject<T>(response);

            return deserializedObject != null ? deserializedObject : throw new Exception($"Deserialization Failed, Response: {response}, StatusCode: {result.StatusCode}, Request Url: {url}");
        }

        /// <summary>
        /// Posts the form data.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url">The URL.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
        public async Task<T> PostFormData<T>(string url, object data)
        {
            const string methodName = nameof(PostFormData);
            try
            {
                string modelName = typeof(T).Name;
                using (var content = new MultipartFormDataContent())
                {
                    foreach (PropertyInfo prop in data.GetType().GetProperties())
                    {
                        object value = prop.GetValue(data);
                        if (value != null)
                        {
                            if (prop.PropertyType == typeof(IFormFile))
                            {
                                IFormFile formFile = value as IFormFile;
                                if (formFile != null)
                                {
                                    using (var ms = new MemoryStream())
                                    {
                                        await formFile.CopyToAsync(ms);
                                        var fileContent = new ByteArrayContent(ms.ToArray());
                                        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(formFile.ContentType);
                                        content.Add(fileContent, prop.Name, formFile.FileName);
                                    }
                                }
                            }
                            else if (prop.PropertyType == typeof(DateTime) || prop.PropertyType == typeof(DateTime?))
                            {
                                var dateValue = Convert.ToDateTime(value);
                                string formattedDate = dateValue.ToString("yyyy-MM-ddTHH:mm:ss"); // ISO 8601
                                content.Add(new StringContent(formattedDate), prop.Name);
                            }
                            else
                            {
                                content.Add(new StringContent(value.ToString()), prop.Name);
                            }
                        }
                    }
                    _logger.LogInformation("{ClassName}.{MethodName}: Sending POST form data request. URL: {Url}", className, methodName, url);
                    var response = await PostAsync(url, content);
                    return await HandleResponse<T>(url, data, methodName, modelName, response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: POST Form Data request failed for URL: {Url}, Request Data: {RequestData}", className, methodName, url, JsonConvert.SerializeObject(data));
                throw new Exception($"{className}.{methodName}: Failed to process POST Form Data request for URL: {url}.", ex);
            }

        }

        public async Task<T> Put<T>(string url, object data)
        {
            var requestJson = JsonConvert.SerializeObject(data);
            var result = await PutAsync(url, new StringContent(requestJson, Encoding.UTF8, "application/json"));
            var response = await result.Content.ReadAsStringAsync();
            var deserializedObject = JsonConvert.DeserializeObject<T>(response);
            if (!result.IsSuccessStatusCode)
            {
                _logger.LogError($"PUT Request Failed. Url: {url}, Status Code: {result.StatusCode}, Response: {response}");
                _logger.LogError($"PUT Request Failed. Url: {url}, Request: {requestJson}");
            }

            return deserializedObject != null ? deserializedObject : throw new Exception("Deserialization Failed");
        }

        public async Task<T> Patch<T>(string url, object data) where T : class
        {
            const string methodName = nameof(Patch);
            try
            {
                string modelName = typeof(T).Name;
                _logger.LogInformation("{ClassName}.{MethodName}: Sending PATCH request. URL: {Url}", className, methodName, url);
                var response = await PatchAsync(url, new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json"));
                return await HandleResponse<T>(url, data, methodName, modelName, response);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: PATCH request failed for URL: {Url}, Request Data: {RequestData}", className, methodName, url, JsonConvert.SerializeObject(data));
                throw new Exception($"{className}.{methodName}: Failed to process PATCH request for URL: {url}.", ex);
            }
        }

        public async Task<HttpResponseMessage> Delete(string url, IDictionary<string, string> parameters)
        {
            var requestJson = JsonConvert.SerializeObject(parameters);
            var result = await PostAsync(url, new StringContent(requestJson, Encoding.UTF8, "application/json"));
            var response = await result.Content.ReadAsStringAsync();
            if (!result.IsSuccessStatusCode)
            {
                _logger.LogError($"DELETE Request Failed. Url: {url}, Status Code: {result.StatusCode}, Response: {response}");
                _logger.LogError($"DELETE Request Failed. Url: {url}, Request: {requestJson}");
            }
            var deserializedObject = JsonConvert.DeserializeObject<HttpResponseMessage>(response);

            return deserializedObject != null ? deserializedObject : throw new Exception("Deserialization Failed");
        }
        private async Task<T> HandleResponse<T>(string url, object? data, string methodName, string modelName, HttpResponseMessage response)
        {
            string responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("{ClassName}.{MethodName}: Received response for {ModelName}, Status Code: {StatusCode}, URL: {Url}", className, methodName, modelName, response.StatusCode, url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("{ClassName}.{MethodName}: Request failed. Status Code: {StatusCode}, URL: {Url}, Request Data: {RequestData}, Response: {ResponseContent}", className, methodName, response.StatusCode, url, JsonConvert.SerializeObject(data), responseContent);
            }

            return CommonMethodToDeserialize<T>(url, data, methodName, modelName, responseContent);
        }

        private T CommonMethodToDeserialize<T>(string url, object? data, string methodName, string modelName, string responseContent)
        {
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName}: Deserializing response for {ModelName}", className, methodName, modelName);
                var deserializedObject = JsonConvert.DeserializeObject<T>(responseContent);

                if (EqualityComparer<T>.Default.Equals(deserializedObject, default(T)))
                    throw new Exception("Deserialization resulted in null");

                _logger.LogInformation("{ClassName}.{MethodName}: Successfully deserialized response for {ModelName}", className, methodName, modelName);
                return deserializedObject;
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "{ClassName}.{MethodName}: JSON deserialization failed for {ModelName}. URL: {Url}, Request Data: {RequestData}, Response: {ResponseContent}", className, methodName, modelName, url, JsonConvert.SerializeObject(data), responseContent);
                throw new Exception("Failed to deserialize response into the expected object type.", jsonEx);
            }
        }
    }
}
