//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using Newtonsoft.Json;
using System;
using Windows.UI.Core;

namespace MeetingManager.ViewModels
{
    class JsonPageViewModel : BaseViewModel
    {
        private CoreDispatcher _dispatcher;

        public JsonPageViewModel()
        {
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled) return;

            _dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;

            UI.Subscribe<string>(ShowJson);
        }

        public string Text { get; private set; }

        private async void ShowJson(string text)
        {
            var json = Prettify(text);

            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                Text = json;
                OnPropertyChanged(() => Text);
            });
        }

        private string Prettify(string text)
        {
            try
            {
                var instance = JSON.Deserialize<object>(text);
                return JSON.Serialize(instance, Formatting.Indented);
            }
            catch
            {
                return text;
            }
        }
    }
}
