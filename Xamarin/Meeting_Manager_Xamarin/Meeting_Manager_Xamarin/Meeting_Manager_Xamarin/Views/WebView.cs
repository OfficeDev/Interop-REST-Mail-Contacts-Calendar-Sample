//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using System;
using System.Windows.Input;
using Xamarin.Forms;

namespace Meeting_Manager_Xamarin.Views
{
    class WebView : Xamarin.Forms.WebView
    {
        public static readonly BindableProperty CommandProperty =
            BindableProperty.Create("Command", typeof(ICommand), typeof(WebView), null, BindingMode.TwoWay, null);

        public WebView()
        {
            Navigating += OnNavigating;
        }

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        private void OnNavigating(object sender, EventArgs eventArgs)
        {
            if (Command?.CanExecute(null) == true)
            {
                var e = eventArgs as WebNavigatingEventArgs;
                var args = new Models.NavigationArgs { Uri = e.Url };
                Command.Execute(args);
                e.Cancel = args.Cancel;
            }
        }
    }
}
