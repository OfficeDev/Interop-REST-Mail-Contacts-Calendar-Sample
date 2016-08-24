//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Meeting_Manager_Xamarin
{
    class HttpHelperBase
    {
        private HttpClient _httpClient = GetHttpClient();

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

        internal async Task DeleteItemAsync(string uri)
        {
            await DoHttpAsync<EmptyBody, EmptyBody>(HttpMethod.Delete, uri, null);
        }

        internal async Task<T> PatchItemAsync<T>(string uri, T item)
        {
            return await DoHttpAsync<T, T>(new HttpMethod("PATCH"), uri, item);
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

            return default(TResult);
        }

        protected async Task<TResult> Deserialize<TResult>(HttpResponseMessage response)
        {
            if (typeof(TResult) == typeof(byte[]))
            {
                var bytes = await response.Content.ReadAsByteArrayAsync();

                var result = Convert.ChangeType(bytes, typeof(TResult));
                return (TResult)result;
            }
            else   // assume string payload
            {
                string jsonResponse = await response.Content.ReadAsStringAsync();

                return JSON.Deserialize<TResult>(jsonResponse);
            }
        }

        protected async Task<string> GetErrorMessage(HttpResponseMessage response)
        {
            string jsonResponse = await response.Content.ReadAsStringAsync();

            try
            {
                var errorDetail = string.IsNullOrEmpty(jsonResponse) ? null : JSON.Deserialize<ODataError>(jsonResponse);

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
                    request.Content = new StringContent(body as string, UnicodeEncoding.UTF8, "application/x-www-form-urlencoded");
                }
                else
                {
                    var bodyJson = JSON.Serialize(body);
                    request.Content = new StringContent(bodyJson, UnicodeEncoding.UTF8, "application/json");
                }
            }
            return request;
        }

        protected async Task<HttpResponseMessage> ExecuteRequestAsync(HttpRequestMessage request)
        {
            try
            {
                return await _httpClient.SendAsync(request);
            }
            catch (Exception ex)
            {
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent(ex.Message)
                };
            }
        }

        protected virtual string BuildUri(string uri)
        {
            uri = uri.Replace(" ", "%20");
            uri = uri.Replace("#", "%23");

            return uri;
        }

        private static HttpClient GetHttpClient()
        {
            return new HttpClient();
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