//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using Prism.Unity.Windows;
using Prism.Windows.Navigation;
using Prism.Events;
using Prism.Mvvm;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using MeetingManager.Models;
using MeetingManager.ViewModels;
using Windows.UI.Core;
using System.Reflection;

namespace MeetingManager
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : PrismUnityApplication
    {
        private const string AuthCodeKey = "AuthCode";
        private const string UserIdKey = "UserId";

        private IDictionary<Type, object> _vmCache = new Dictionary<Type, object>();

        private Logger _logger;

        public CoreDispatcher MainDispatcher => Window.Current.Dispatcher;

        internal bool UseHttp { get; set; }
        internal bool UseMSAL { get; set; }

        internal IEventAggregator EventAggregator { get; private set; }

        internal new INavigationService NavigationService => base.NavigationService;

        internal static App Me => Application.Current as App;

        internal IGraphService GraphService { get; private set; }

        internal IAuthenticationService AuthenticationService { get; private set; }

        internal string UserId
        {
            get
            {
                if (SessionStateService.SessionState.ContainsKey(UserIdKey))
                {
                    return SessionStateService.SessionState[UserIdKey].ToString();
                }
                return null;
            }

            set
            {
                SessionStateService.SessionState[UserIdKey] = value;
            }
        }

        private string AuthorizationCode
        {
            get
            {
                if (SessionStateService.SessionState.ContainsKey(AuthCodeKey))
                {
                    return SessionStateService.SessionState[AuthCodeKey].ToString();
                }
                return null;
            }

            set
            {
                SessionStateService.SessionState[AuthCodeKey] = value;
            }
        }

        protected override void OnRegisterKnownTypesForSerialization()
        {
            // Set up the list of known types for the SuspensionManager
            SessionStateService.RegisterKnownType(typeof(Meeting));
            SessionStateService.RegisterKnownType(typeof(Message));
            SessionStateService.RegisterKnownType(typeof(EventMessage));
        }

        protected override Task OnLaunchApplicationAsync(LaunchActivatedEventArgs args)
        {
            NavigationService.Navigate("Connect", null);

            Window.Current.Activate();

            return Task.FromResult<object>(null);
        }

        protected override Task OnInitializeAsync(IActivatedEventArgs args)
        {
            EventAggregator = new EventAggregator();
            _logger = new Logger();

            ViewModelLocationProvider.SetDefaultViewModelFactory(CachingFactory);

            return base.OnInitializeAsync(args);
        }

        private object CachingFactory(Type type)
        {
            if (typeof(ITransientViewModel).IsAssignableFrom(type) || !_vmCache.ContainsKey(type))
            {
                _vmCache[type] = Container.Resolve(type);
            }

            return _vmCache[type];
        }

        internal async void InitializeApp(string authCode)
        {
            AuthorizationCode = authCode;

            CreateServices();

            var user = await GraphService.GetUser();

            if (user != null)
            {
                App.Me.UserId = user.UserPrincipalName;
                UI.NavigateTo("Calendar");
            }
        }

        private void CreateServices()
        {
            AuthenticationService = UseMSAL ? new AuthenticationHelperMSAL() as IAuthenticationService
                                            : new AuthenticationHelper(_logger, AuthorizationCode);

            GraphService = UseHttp ? new HttpGraphService(AuthenticationService, _logger) as IGraphService
                           : new SDKGraphService(AuthenticationService, _logger);
        }
    }
}

