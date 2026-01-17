namespace SunnyRewards.Helios.ETL.Common.Helpers.Interfaces
{
    public interface IBaseClient
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        Task<T> Get<T>(string url, IDictionary<string, long> parameters, Dictionary<string, string>? headers = null);

        /// <summary>
        /// rest get method
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        Task<T> GetById<T>(string url, long parameters);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        Task<T> Post<T>(string url, object data, Dictionary<string, string>? headers = null);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        Task<T> Put<T>(string url, object data);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        Task<HttpResponseMessage> Delete(string url, IDictionary<string, string> parameters);

        /// <summary>
        /// Posts the form data.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url">The URL.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        Task<T> PostFormData<T>(string url, object data);

        /// <summary>
        /// Patches the specified URL.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url">The URL.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        Task<T> Patch<T>(string url, object data) where T : class;
    }
}
