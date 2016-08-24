//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using MeetingManager.Models;

namespace MeetingManager.ViewModels
{
    class LoginPageViewModel : BaseViewModel
    {
        private string RedirectUri => App.Current.Resources["ida:RedirectUri"].ToString();
        private string ClientID => App.Current.Resources["ida:ClientID"].ToString();
        private string AADInstance => App.Current.Resources["ida:AADInstance"].ToString();

        protected override void OnNavigatedTo(object parameter)
        {
            LoginUrl = $"{AADInstance + "common/oauth2/authorize"}?" +
                                    "response_type=code" +
                                    "&prompt=login" +
                                    $"&client_id={ClientID}" +
                                    $"&redirect_uri={RedirectUri}";
            OnPropertyChanged(() => LoginUrl);
        }

        public Command<NavigationArgs> NavigationCommand => new Command<NavigationArgs>(NavigationStarting);

        public string LoginUrl { get; private set; }

        private void NavigationStarting(NavigationArgs args)
        {
            string authCode = string.Empty;

            if (args.Uri.IndexOfCaseInsensitive(RedirectUri) == 0)
            {
                string codeKey = "code=";
                int codeStartIndex = args.Uri.IndexOfCaseInsensitive(codeKey);

                if (codeStartIndex > 0)
                {
                    codeStartIndex += codeKey.Length;
                    var segments = args.Uri.Substring(codeStartIndex).Split('&');
                    authCode = segments[0];

                    IsLoading = false;
                    args.Cancel = true;

                    InitializeApp(authCode);
                }
            }
        }

        private void InitializeApp(string authCode)
        {
            UI.Publish(new LoginEventData
            {
                Url = LoginUrl,
                AuthCode = authCode
            });

            App.Me.InitializeApp(authCode);
        }
    }
}
