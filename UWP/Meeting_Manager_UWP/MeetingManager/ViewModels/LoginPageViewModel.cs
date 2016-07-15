//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using MeetingManager.Models;
using Prism.Commands;
using Prism.Windows.Navigation;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;

namespace MeetingManager.ViewModels
{
    class LoginPageViewModel : ViewModel
    {
        private string _loginUrl;

        public LoginPageViewModel()
        {
        }

        public override void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            base.OnNavigatedTo(e, viewModelState);

            LoginUrl = App.Me.AuthenticationService.LoginUrl;
        }

        public DelegateCommand<WebViewNavigationStartingEventArgs> NavigationStartingCommand =>
                            new DelegateCommand<WebViewNavigationStartingEventArgs>(NavigationStarting);

        public string LoginUrl
        {
            get { return _loginUrl; }
            private set { SetProperty(ref _loginUrl, value); }
        }

        private void NavigationStarting(WebViewNavigationStartingEventArgs args)
        {
            string authCode = string.Empty;
            string argsUri = args.Uri.AbsoluteUri;

            if (argsUri.IndexOfCaseInsensitive(App.Me.AuthenticationService.RedirectUri) == 0)
            {
                string codeKey = "code=";
                int codeStartIndex = argsUri.IndexOfCaseInsensitive(codeKey);

                if (codeStartIndex > 0)
                {
                    args.Cancel = true;

                    codeStartIndex += codeKey.Length;
                    var segments = argsUri.Substring(codeStartIndex).Split('&');
                    authCode = segments[0];

                    IsLoading = false;

                    InitializeApp(authCode);
                }
            }
        }

        private async void InitializeApp(string authCode)
        {
            App.Me.AuthenticationService.AuthorizationCode = authCode;

            UI.Publish(new LoginEventData
            {
                Url = LoginUrl,
                AuthCode = authCode
            });

            var user = await OfficeService.GetUser();
            App.Me.AuthenticationService.UserId = user.UserPrincipalName;

            await UI.NavigateTo("Calendar");
        }
    }
}
