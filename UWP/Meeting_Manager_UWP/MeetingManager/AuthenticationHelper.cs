using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Web.Http;
using System.Net;
using Windows.UI.Xaml.Controls;
using Prism.Windows.AppModel;

namespace MeetingManager
{
    class AuthenticationHelper : IAuthenticationService
    {
        // The Client ID is used by the application to uniquely identify itself to Microsoft Azure Active Directory (AD).
        private static readonly string ClientID = App.Current.Resources["ida:ClientID"].ToString();

        // You'll create your tenant-specific authority from the tenant domain and AADInstance URI
        private static string tenant = App.Current.Resources["ida:Domain"].ToString();
        private static string AADInstance = App.Current.Resources["ida:AADInstance"].ToString();

        //Use the domain-specific authority when you're authenticating users from a single tenant only.
        private static string _authority = AADInstance + tenant;

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
        }

        private IDictionary<string, AccessTokenResponse> _tokenResponses = new Dictionary<string, AccessTokenResponse>();

        private readonly ISessionStateService _sessionStateService;
        private readonly Logger _logger;

        private const string AuthCodeKey = "AuthCode";
        private const string UserIdKey = "UserId";

        public AuthenticationHelper(ISessionStateService sessionStateService, Logger logger)
        {
            _sessionStateService = sessionStateService;
            _logger = logger;
        }

        private static string TokenUri
        {
            get
            {
                return _authority + "/oauth2/" + "token";
            }
        }

        public string UserId
        {
            get
            {
                if (_sessionStateService.SessionState.ContainsKey(UserIdKey))
                {
                    return _sessionStateService.SessionState[UserIdKey].ToString();
                }
                return null;
            }

            set
            {
                _sessionStateService.SessionState[UserIdKey] = value;
            }
        }

        public string AuthorizationCode
        {
            get
            {
                if (_sessionStateService.SessionState.ContainsKey(AuthCodeKey))
                {
                    return _sessionStateService.SessionState[AuthCodeKey].ToString();
                }
                return null;
            }

            set
            {
                _sessionStateService.SessionState[AuthCodeKey] = value;
            }
        }

        public string LoginUrl
        {
            get
            {
                return $"{_authority + "/oauth2/authorize"}?" +
                        "response_type=code" +
                        $"&client_id={ClientID}" +
                        $"&redirect_uri={RedirectUri}";
            }
        }

        public string RedirectUri
        {
            get
            {
                return App.Current.Resources["ida:RedirectUri"].ToString();
            }
        }

        private async Task<string> GetTokenHelperHttp(string resourceId, bool isRefresh)
        {
            if (isRefresh)
            {
                var authResponse = _tokenResponses[resourceId];

                if (authResponse == null)
                {
                    return null;
                }

                string body = "grant_type=refresh_token"+
                                $"&refresh_token={WebUtility.UrlEncode(authResponse.refresh_token)}"+
                                $"&client_id={WebUtility.UrlEncode(ClientID)}"+
                                $"&resource={WebUtility.UrlEncode(resourceId)}";

                var newAuthResponse = await DoTokenHttp(body);

                _tokenResponses[resourceId] = newAuthResponse;
            }

            if (_tokenResponses.ContainsKey(resourceId) == false)
            {
                _tokenResponses[resourceId] = await QueryTokenResponse(resourceId);
            }

            return _tokenResponses[resourceId]?.access_token;
        }

        private async Task<AccessTokenResponse> QueryTokenResponse(string resourceId)
        {
            var body = "grant_type=authorization_code" +
                        $"&code={AuthorizationCode}" +
                        $"&resource={WebUtility.UrlEncode(resourceId)}" +
                        $"&client_id={WebUtility.UrlEncode(ClientID)}" +
                        $"&redirect_uri={WebUtility.UrlEncode(RedirectUri)}";

            return await DoTokenHttp(body);
        }

        private async Task<AccessTokenResponse> DoTokenHttp(string body)
        {
            return await new HttpHelper(this, _logger).PostItemAsync<string, AccessTokenResponse>(TokenUri, body);
        }

        public async Task<string> GetTokenAsync(string resourceId, bool isRefresh)
        {
            return await GetTokenHelperHttp(resourceId, isRefresh);
        }
    }
}
