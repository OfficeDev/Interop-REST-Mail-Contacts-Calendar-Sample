//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using Microsoft.Xaml.Interactivity;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MeetingManager.Views
{
    class WebNavigationBehavior : DependencyObject, IBehavior
    {
        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public static readonly DependencyProperty CommandProperty =
                DependencyProperty.Register("Command",
                typeof(ICommand), typeof(WebNavigationBehavior), null);

        public DependencyObject AssociatedObject {get; private set;}

        public void Attach(DependencyObject associatedObject)
        {
            AssociatedObject = associatedObject;

            var webView = AssociatedObject as WebView;

            webView.NavigationStarting += OnNavigationStarting;
        }

        public void Detach()
        {
            AssociatedObject = null;
        }

        private void OnNavigationStarting(object sender, WebViewNavigationStartingEventArgs e)
        {
            if (Command?.CanExecute(null) == true)
            {
                var args = new Models.NavigationArgs { Uri = e.Uri.AbsoluteUri };
                Command.Execute(args);
                e.Cancel = args.Cancel;
            }
        }
    }
}
