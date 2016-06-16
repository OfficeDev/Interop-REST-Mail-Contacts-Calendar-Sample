//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.Web.Http;
using Windows.Web.Http.Headers;

namespace MeetingManager
{
    class HttpHelper : HttpHelperBase
    {
        private const string BaseGraphUri = @"https://graph.microsoft.com/v1.0/Me/";
        private readonly IAuthenticationService _authService;
        private readonly Logger _logger;

        public HttpHelper(IAuthenticationService authService, Logger logger)
        {
            _authService = authService;
            _logger = logger;
        }

        public async Task<IEnumerable<T>> GetItemsAsync<T>(string uri)
        {
            var list = await GetItemAsync<ODataList<T>>(uri);

            return list == null ? new List<T>() : list.value;
        }

        protected override async Task<TResult> DoHttpAsync<TBody, TResult>(HttpMethod method, string uri, TBody body)
        {
            var response = await CreateAndExecuteRequestAsync(method, uri, body);

            return await base.GetResultAsync<TResult>(response);
        }

        protected async override void LogResponse(HttpResponseMessage response)
        {
            var request = response.RequestMessage;

            var method = request.Method.Method;
            var uri = request.RequestUri.ToString();
            string requestBody = string.Empty;

            if (request.Content != null)
            {
                requestBody = await request.Content.ReadAsStringAsync();
            }

            var statusCode = $"{(int) response.StatusCode} ({response.StatusCode.ToString()})";
            string responseBody = string.Empty;

            var mediaType = response.Content?.Headers?.ContentType.MediaType;

            if (mediaType != null && !mediaType.Contains("image"))
            {
                responseBody = await response.Content.ReadAsStringAsync();
            }

            _logger.LogHttp(method, uri, requestBody, request.Headers.ToString(),
                        statusCode, responseBody, response.Headers.ToString());
        }

        private async Task<HttpResponseMessage> CreateAndExecuteRequestAsync<TBody>(
            HttpMethod method, string uri, TBody body)
        {
            var request = base.CreateRequest(method, uri, body);
            await SetHeaders(request);

            return await base.ExecuteRequestAsync(request);
        }

        private async Task SetHeaders(HttpRequestMessage request)
        {
            if (!string.IsNullOrEmpty(_authService.UserId))
            {
                request.Headers.Add("AnchorMailbox", _authService.UserId);
            }

            await SetAuthorizationHeaderAsync(request);
        }

        protected override string BuildUri(string uri)
        {
            uri = base.BuildUri(uri);

            if (uri.StartsWith("http"))
            {
                return uri;
            }

            return BaseGraphUri + uri;
        }

        protected async override void HandleFailure(string errorMessage, HttpResponseMessage response)
        {
            LogResponse(response);

            var message = string.IsNullOrEmpty(errorMessage) ?
                    string.Format("Failed with {0}", response.StatusCode) :
                    errorMessage;

            var messageDialog = new MessageDialog(message);

            await messageDialog.ShowAsync();
        }

        private async Task SetAuthorizationHeaderAsync(HttpRequestMessage request)
        {
            string uri = request.RequestUri.ToString();
            string resourceId = ResourceIdFromUri(uri);

            if (!string.IsNullOrEmpty(resourceId))
            {
                string token = await _authService.GetTokenAsync(resourceId);

                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new HttpCredentialsHeaderValue("Bearer", token);
                }
            }
        }

        private string ResourceIdFromUri(string uri)
        {
            if (uri.Contains("office"))
            {
                return "https://outlook.office365.com/";
            }
            else if (uri.Contains("graph.microsoft"))
            {
                return "https://graph.microsoft.com/";
            }
            return null;
        }

        public class ODataList<T>
        {
            [JsonProperty("@odata.nextLink")]
            public string NextLink { get; set; }

            public List<T> value { get; set; }
        }
    }
}
