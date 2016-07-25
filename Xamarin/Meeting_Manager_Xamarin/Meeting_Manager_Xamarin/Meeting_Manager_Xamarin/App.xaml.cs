//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using Meeting_Manager_Xamarin.ViewModels;
using Meeting_Manager_Xamarin.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace Meeting_Manager_Xamarin
{
    public partial class App : Application
    {
        internal string ClientId => UseMSAL ? 
                            "7d94fafd-5ad3-40e0-bc4e-914920af2226" :
                            "6669f82f-61ac-46ea-a346-7c3444d45819";

        internal string RedirectUri => UseMSAL ?
                            "urn:ietf:wg:oauth:2.0:oob" :
                            "MS-APPX-WEB://MICROSOFT.AAD.BROKERPLUGIN/S-1-15-2-2615261976-468863395-2165854654-1528528134-2599808767-1175204876-2313817458";

        private const string CommonAuth = "https://login.microsoftonline.com/common/oauth2/";

        internal string AuthorityOAuth2 => UseMSAL ?
                            CommonAuth + "v2.0/" :
                            CommonAuth;

        private IDictionary<Type, object> _vmCache = new Dictionary<Type, object>();

        public App()
        {
            InitializeComponent();

            Logger = new Logger();

            // The root page of your application
            MainPage = new NavigationPage(new ConnectPage());
        }

        internal static App Me => Application.Current as App;

        internal IGraphService GraphService { get; private set; }

        private IAuthenticationService AuthenticationService { get; set; }

        internal string AuthorizationCode { get; set; }

        internal string UserId { get; set; }

        internal bool UseHttp { get; set; }

        internal bool UseMSAL { get; set; }

        private Page CurrentPage => (MainPage as IPageContainer<Page>).CurrentPage;
        private Logger Logger { get; set; }

        internal async Task InitializeApp()
        {
            var user = await App.Me.GraphService.GetUser();

            if (user != null)
            {
                App.Me.UserId = user.UserPrincipalName;

                await App.Me.PushAsync("Calendar");
            }
        }

        internal void CreateServices(Microsoft.Identity.Client.IPlatformParameters platformParameters)
        {
            AuthenticationService = UseMSAL ? new AuthenticationHelperMSAL(platformParameters) as IAuthenticationService
                                            : new AuthenticationHelper(Logger);

            GraphService = UseHttp ? new HttpGraphService(AuthenticationService, Logger) as IGraphService
                           : new SDKGraphService(AuthenticationService, Logger);
        }

        internal async Task PushAsync(string pageToken, object data=null)
        {
            var viewModelNameSpace = GetType().Namespace + ".ViewModels";
            var viewNameSpace = GetType().Namespace + ".Views";

            var viewModelTypeName = viewModelNameSpace + "." + pageToken + "PageViewModel";
            var viewModelType = Type.GetType(viewModelTypeName);

            var vm = GetViewModelInstance(viewModelType) as BaseViewModel;

            var viewTypeName = viewNameSpace + "." + pageToken + "Page";
            var viewType = Type.GetType(viewTypeName);

            var view = Activator.CreateInstance(viewType) as ContentPage;

            vm.OnAppearing(data);
            view.BindingContext = vm;
            vm.OnPropertyChanged();

            await CurrentPage.Navigation.PushAsync(view);
        }

        internal async Task PushAsync(Page page)
        {
            await CurrentPage.Navigation.PushAsync(page);
        }

        internal async Task PopAsync()
        {
            await CurrentPage.Navigation.PopAsync();
        }

        internal async Task DisplayAlert(string title, string message, string cancelCaption)
        {
            await CurrentPage.DisplayAlert(title, message, cancelCaption);
        }

        internal async Task<bool> DisplayAlert(string title, string message, string okCaption, string cancelCaption)
        {
            if (string.IsNullOrEmpty(cancelCaption))
            {
                await CurrentPage.DisplayAlert(title, message, okCaption);
                return false;
            }
            else
            {
                return await CurrentPage.DisplayAlert(title, message, okCaption, cancelCaption);
            }
        }

        internal async Task<string> DisplayActions(string title, params string[] actions)
        {
            return await CurrentPage.DisplayActionSheet(title, null/*"Cancel"*/, null, actions);
        }

        private object GetViewModelInstance(Type viewModelType)
        {
            bool transient = typeof(ITransientViewModel)
                                .GetTypeInfo()
                                .IsAssignableFrom(viewModelType.GetTypeInfo());

            if (transient || !_vmCache.ContainsKey(viewModelType))
            {
                _vmCache[viewModelType] = Activator.CreateInstance(viewModelType);
            }

            return _vmCache[viewModelType];
        }

        internal BaseViewModel ViewModelFromType(Type viewModelType)
        {
            return _vmCache[viewModelType] as BaseViewModel;
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
