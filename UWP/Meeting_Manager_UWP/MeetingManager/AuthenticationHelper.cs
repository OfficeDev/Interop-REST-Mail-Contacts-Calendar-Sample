﻿//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace MeetingManager
{
    sealed class AuthenticationHelper : IAuthenticationService
    {
        private readonly IDictionary<string, AccessTokenResponse> _tokenResponses = new Dictionary<string, AccessTokenResponse>();

        private readonly Logger _logger;
        private readonly string _authCode;

        public AuthenticationHelper(Logger logger, string authCode)
        {
            _logger = logger;
            _authCode = authCode;
        }

        public string RedirectUri => App.Current.Resources["ida:RedirectUri"].ToString();

        private string ClientID => App.Current.Resources["ida:ClientID"].ToString();

        private string AADInstance => App.Current.Resources["ida:AADInstance"].ToString();

        public async Task<string> GetTokenAsync(string resourceId)
        {
            AccessTokenResponse atr;

            if (_tokenResponses.TryGetValue(resourceId, out atr))
            {
                if (atr != null &&
                    atr.expiration <= DateTimeOffset.Now.UtcDateTime.AddMinutes(5) &&
                    !string.IsNullOrEmpty(atr.refresh_token))
                {
                    atr = await RefreshAccessToken(resourceId, atr.refresh_token);
                }
            }
            else
            {
                atr = await QueryAccessToken(resourceId);
            }

            _tokenResponses[resourceId] = atr;
            return atr?.access_token;
        }

        private async Task<AccessTokenResponse> QueryAccessToken(string resourceId)
        {
            var body = "grant_type=authorization_code" +
                        $"&code={_authCode}" +
                        $"&redirect_uri={WebUtility.UrlEncode(RedirectUri)}" +
                        GraphId(resourceId);

            return await DoTokenHttp(body);
        }

        private async Task<AccessTokenResponse> RefreshAccessToken(string resourceId, string refreshToken)
        {
            var body = "grant_type=refresh_token" +
                        $"&refresh_token={WebUtility.UrlEncode(refreshToken)}" +
                        GraphId(resourceId);

            return await DoTokenHttp(body);
        }

        private string GraphId(string resourceId)
        {
            return $"&client_id={WebUtility.UrlEncode(ClientID)}" +
                   $"&resource={WebUtility.UrlEncode(resourceId)}";
        }

        private async Task<AccessTokenResponse> DoTokenHttp(string body)
        {
            var tokenUri = AADInstance + "common/oauth2/token";
            var atr = await new HttpHelper(this, _logger).PostItemAsync<string, AccessTokenResponse>(tokenUri, body);

            if (atr != null)
            {
                atr.expiration = DateTimeOffset.UtcNow.Add(new TimeSpan(0, 0, atr.expires_in));
            }

            return atr;
        }

        private class AccessTokenResponse
        {
            public string access_token { get; set; }
            public int expires_in { get; set; }
            public int expires_on { get; set; }
            public string id_token { get; set; }
            public string refresh_token { get; set; }
            public string resource { get; set; }
            public string scope { get; set; }
            public string token_type { get; set; }

            [JsonIgnore]
            public DateTimeOffset expiration;
        }
    }
}