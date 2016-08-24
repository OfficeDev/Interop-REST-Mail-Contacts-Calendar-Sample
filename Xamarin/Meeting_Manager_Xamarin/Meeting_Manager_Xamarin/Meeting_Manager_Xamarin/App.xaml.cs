//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using Meeting_Manager_Xamarin.ViewModels;
using Meeting_Manager_Xamarin.Views;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace Meeting_Manager_Xamarin
{
    public partial class App : Application
    {
        internal bool InTransient { get; private set; }

        private IDictionary<Type, object> _vmCache = new Dictionary<Type, object>();

        private Logger _logger;

        public App()
        {
            InitializeComponent();

            _logger = new Logger();

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

        internal async void InitializeApp(string authCode, Microsoft.Identity.Client.IPlatformParameters platformParameters=null)
        {
            AuthorizationCode = authCode;

            CreateServices(platformParameters);

            var user = await App.Me.GraphService.GetUser();

            if (user != null)
            {
                UserId = user.UserPrincipalName;

                await PushAsync("Calendar");
            }
        }

        internal async Task PushAsync(string pageToken, object data=null)
        {
            Type viewModelType;
            Type viewType;

            if (!GetTypes(pageToken, "Page", out viewModelType, out viewType) &&
                !GetTypes(pageToken, "Dialog", out viewModelType, out viewType))
            {
                throw new ArgumentException($"Cannot get view[model] type for {pageToken}!");
            }

            var vm = GetViewModelInstance(viewModelType) as BaseViewModel;
            var view = Activator.CreateInstance(viewType) as ContentPage;

            vm.NavigateTo(data);
            view.BindingContext = vm;
            vm.OnPropertyChanged();

            await CurrentPage.Navigation.PushAsync(view);
        }

        private bool GetTypes(string pageToken, string name,
                                out Type viewModelType, out Type viewType)
        {
            var viewModelNameSpace = GetType().Namespace + ".ViewModels";
            var viewNameSpace = GetType().Namespace + ".Views";

            var viewModelTypeName = viewModelNameSpace + "." + pageToken + name + "ViewModel";
            viewModelType = Type.GetType(viewModelTypeName);

            var viewTypeName = viewNameSpace + "." + pageToken + name;
            viewType = Type.GetType(viewTypeName);

            return viewModelType != null && viewType != null;
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

        private object GetViewModelInstance(Type viewModelType)
        {
            bool transient = typeof(ITransientViewModel)
                                .GetTypeInfo()
                                .IsAssignableFrom(viewModelType.GetTypeInfo());

            InTransient = transient;

            if (transient || !_vmCache.ContainsKey(viewModelType))
            {
                _vmCache[viewModelType] = Activator.CreateInstance(viewModelType);
            }

            return _vmCache[viewModelType];
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

        private void CreateServices(Microsoft.Identity.Client.IPlatformParameters platformParameters)
        {
            AuthenticationService = UseMSAL ? new AuthenticationHelperMSAL(platformParameters) as IAuthenticationService
                                            : new AuthenticationHelper(_logger, AuthorizationCode);

            GraphService = UseHttp ? new HttpGraphService(AuthenticationService, _logger) as IGraphService
                           : new SDKGraphService(AuthenticationService, _logger);
        }
    }
}
