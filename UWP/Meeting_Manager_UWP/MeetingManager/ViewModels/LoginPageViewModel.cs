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
            NavigationStartingCommand = new DelegateCommand<WebViewNavigationStartingEventArgs>(NavigationStarting);
        }

        public override void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            base.OnNavigatedTo(e, viewModelState);

            LoginUrl = _authenticationService.LoginUrl;
        }

        public DelegateCommand<WebViewNavigationStartingEventArgs> NavigationStartingCommand { get; }

        public string LoginUrl
        {
            get { return _loginUrl; }
            private set { SetProperty(ref _loginUrl, value); }
        }

        private void NavigationStarting(WebViewNavigationStartingEventArgs args)
        {
            string authCode = string.Empty;
            string argsUri = args.Uri.AbsoluteUri;

            if (argsUri.IndexOfCaseInsensitive(_authenticationService.RedirectUri) == 0)
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
            _authenticationService.AuthorizationCode = authCode;

            GetEvent<LoginEvent>().Publish(new LoginEventData
            {
                Url = LoginUrl,
                AuthCode = authCode
            });

            var user = await OfficeService.GetUser();
            _authenticationService.UserId = user.UserPrincipalName;

            await NavigateTo("Calendar");
        }
    }
}
