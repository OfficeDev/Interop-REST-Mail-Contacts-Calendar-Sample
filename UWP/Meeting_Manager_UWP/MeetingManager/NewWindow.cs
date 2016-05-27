//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MeetingManager
{
    static class NewWindow
    {
        internal static async Task<int> Create(Type viewType, string title, int windowId)
        {
            if (windowId == 0)
            {
                var newView = CoreApplication.CreateNewView();

                await newView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    var frame = new Frame();
                    var n = frame.Navigate(viewType);
                    Window.Current.Content = frame;
                    Window.Current.Activate();  // !!!

                    ApplicationView.GetForCurrentView().Title = title;

                    windowId = ApplicationView.GetForCurrentView().Id;
                });
            }

            await ApplicationViewSwitcher.TryShowAsStandaloneAsync(windowId,
                ViewSizePreference.Default, ApplicationView.GetForCurrentView().Id, ViewSizePreference.Default);

            return windowId;
        }
    }
}
