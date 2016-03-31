using Newtonsoft.Json;
using System;
using System.Dynamic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Web.Http;
using Windows.Web.Http.Filters;
using Windows.Web.Http.Headers;

namespace MeetingManager
{
    class HttpHelperBase
    {
        internal delegate Task<string> TokenFunc(bool isRefresh);

        internal async Task<T> GetItemAsync<T>(string uri)
        {
            return await DoHttpAsync<EmptyBody, T>(HttpMethod.Get, uri, null);
        }

        internal async Task<T> PostItemAsync<T>(string uri, T item)
        {
            return await DoHttpAsync<T, T>(HttpMethod.Post, uri, item);
        }

        internal async Task<TResult> PostItemAsync<TBody, TResult>(string uri, TBody item)
        {
            return await DoHttpAsync<TBody, TResult>(HttpMethod.Post, uri, item);
        }

        internal async Task<T> PostItemAsync<T>(string uri)
        {
            return await DoHttpAsync<T, T>(HttpMethod.Post, uri, default(T));
        }

        internal async Task PostItemVoidAsync<T>(string uri)
        {
            await DoHttpAsync<T, T>(HttpMethod.Post, uri, default(T));
        }

        internal async Task<T> PostItemDynamicAsync<T>(string uri, dynamic body)
        {
            return await DoHttpAsync<ExpandoObject, T>(HttpMethod.Post, uri, body);
        }

        internal async Task DeleteItemAsync(string uri)
        {
            await DoHttpAsync<EmptyBody, EmptyBody>(HttpMethod.Delete, uri, null);
        }

        internal async Task<T> PatchItemAsync<T>(string uri, T item)
        {
            return await DoHttpAsync<T, T>(HttpMethod.Patch, uri, item);
        }

        protected virtual async Task<TResult> DoHttpAsync<TBody, TResult>(HttpMethod method, string uri, TBody body)
        {
            var request = CreateRequest(method, uri, body);
            var response = await ExecuteRequestAsync(request);

            return await GetResultAsync<TResult>(response);
        }

        protected async Task<TResult> GetResultAsync<TResult>(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                return await Deserialize<TResult>(response);
            }

            HandleFailure(await GetErrorMessage(response), response);
            return default(TResult);
        }

        private async Task<TResult> Deserialize<TResult>(HttpResponseMessage response)
        {
            if (typeof(TResult) == typeof(byte[]))
            {
                var buf = await response.Content.ReadAsBufferAsync();

                var bytes = buf.ToArray();
                LogResponse(response, typeof(byte[]));

                var result = Convert.ChangeType(bytes, typeof(TResult));
                return (TResult)result;
            }
            else   // assume string payload
            {
                string jsonResponse = await response.Content.ReadAsStringAsync();
                LogResponse(response, typeof(string));

                return JsonConvert.DeserializeObject<TResult>(jsonResponse);
            }
        }

        private async Task<string> GetErrorMessage(HttpResponseMessage response)
        {
            string jsonResponse = await response.Content.ReadAsStringAsync();

            try
            {
                var errorDetail = string.IsNullOrEmpty(jsonResponse) ? null : JsonConvert.DeserializeObject<ODataError>(jsonResponse);

                if (errorDetail != null)
                {
                    return errorDetail.error.message;
                }
            }
            catch
            {
                return jsonResponse;
            }

            return string.Empty;
        }

        protected HttpRequestMessage CreateRequest<TBody>(HttpMethod method, string uri, TBody body)
        {
            var request = new HttpRequestMessage(method, new Uri(BuildUri(uri)));

            if (body != null)
            {
                if (body is string)
                {
                    request.Content = new HttpStringContent(body as string, Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/x-www-form-urlencoded");
                }
                else
                {
                    string serializedBody = JsonConvert.SerializeObject(body,
                                    new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

                    request.Content = new HttpStringContent(serializedBody, Windows.Storage.Streams.UnicodeEncoding.Utf8);
                    request.Content.Headers.ContentType = new HttpMediaTypeHeaderValue("application/json");
                }
            }
            return request;
        }

        protected async Task<HttpResponseMessage> ExecuteRequestAsync(HttpRequestMessage request)
        {
            using (HttpClient client = GetHttpClient())
            {
                return await client.SendRequestAsync(request);
            }
        }

        protected virtual void LogResponse(HttpResponseMessage response, Type type)
        {
        }

        protected virtual void HandleFailure(string errorMessage, HttpResponseMessage response)
        {
        }

        protected virtual string BuildUri(string uri)
        {
            uri = uri.Replace(" ", "%20");
            uri = uri.Replace("#", "%23");

            return uri;
        }

        private HttpClient GetHttpClient()
        {
            var rootFilter = new HttpBaseProtocolFilter();
            rootFilter.CacheControl.ReadBehavior = HttpCacheReadBehavior.MostRecent;
            rootFilter.CacheControl.WriteBehavior = HttpCacheWriteBehavior.NoCache;

            return new HttpClient(rootFilter);
        }
   
        private class ODataError
        {
            public class Error
            {
                public string code { get; set; }
                public string message { get; set; }
            }
            public Error error { get; set; }
        }

        private class EmptyBody { }
    }
}
