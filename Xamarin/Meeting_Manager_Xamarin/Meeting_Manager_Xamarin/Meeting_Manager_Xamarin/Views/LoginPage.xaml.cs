//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using Meeting_Manager_Xamarin.Models;
using System;
using System.Net;
using Xamarin.Forms;

namespace Meeting_Manager_Xamarin.Views
{
    public partial class LoginPage : ContentPage
    {
        private string LoginUrl => $"{App.Me.AuthorityOAuth2 + "authorize"}" +
                        "?response_type=code" + 
                        "&prompt=login" +
//                        "&scope=openid%20offline_access%20https%3A%2F%2Fgraph.microsoft.com%2FUser.Read" +
//                        "&response_mode=query" +
                        $"&client_id={App.Me.ClientId}" +
                        $"&redirect_uri={WebUtility.UrlEncode(App.Me.RedirectUri)}";

        public LoginPage()
        {
            InitializeComponent();

            webView.Source = LoginUrl;
        }

        public void OnNavigated(object sender, EventArgs args)
        {
        }

        public void OnNavigating(object sender, EventArgs args)
        {
            string authCode = string.Empty;
            var webArgs = args as WebNavigatingEventArgs;
            string argsUri = webArgs.Url;

            if (argsUri.IndexOfCaseInsensitive(App.Me.RedirectUri) == 0)
            {
                string codeKey = "code=";
                int codeStartIndex = argsUri.IndexOfCaseInsensitive(codeKey);

                if (codeStartIndex > 0)
                {
                    webArgs.Cancel = true;

                    codeStartIndex += codeKey.Length;
                    var segments = argsUri.Substring(codeStartIndex).Split('&');
                    authCode = segments[0];

                    InitializeApp(authCode);
                }
            }
        }

        private async void InitializeApp(string authCode)
        {
            App.Me.AuthorizationCode = authCode;

            await App.Me.InitializeApp();

            // Do this after navigating because we need LogWindow to be instantiated
            this.Publish(
                    new LoginEventData
                    {
                        Url = LoginUrl,
                        AuthCode = authCode
                    });
        }
    }
}
