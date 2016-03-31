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
using System.Diagnostics;
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
        private bool inExceptionHandler = false;

        private IDictionary<Type, object> _vmCache = new Dictionary<Type, object>();

        public App()
        {
            UnhandledException += OnUnhandledException;
        }

        private CoreDispatcher _mainDispatcher;

        public CoreDispatcher MainDispatcher
        {
            get
            {
                return _mainDispatcher;
            }
        }


        public IEventAggregator EventAggregator { get; private set; }

        public new INavigationService NavigationService
        {
            get { return base.NavigationService; }
        }

        public IGraphService OfficeService { get; private set; }
        public IAuthenticationService AuthenticationService { get; private set; }

        private async void OnUnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            if (!inExceptionHandler)
            {
                inExceptionHandler = true;

                StackTrace stackTrace = new StackTrace(args.Exception, true);
                string stackTraceString = args.Exception.StackTrace == null ? stackTrace.ToString() : args.Exception.StackTrace;

                string errText = string.Format("An unhandled exception occurred: {0}\r\nStack Trace: {1}", args.Exception.Message, stackTraceString);

                Debug.WriteLine(errText);

                args.Handled = true;
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
            _mainDispatcher = Window.Current.Dispatcher;

            return Task.FromResult<object>(null);
        }

        protected override Task OnInitializeAsync(IActivatedEventArgs args)
        {
            EventAggregator = new EventAggregator();
            var logger = new Logger(EventAggregator);

            AuthenticationService = new AuthenticationHelper(SessionStateService, logger);

            Container.RegisterInstance<IAuthenticationService>(AuthenticationService);


            OfficeService = new HttpGraphService(AuthenticationService, logger);

            Container.RegisterInstance<INavigationService>(NavigationService);

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
    }
}

