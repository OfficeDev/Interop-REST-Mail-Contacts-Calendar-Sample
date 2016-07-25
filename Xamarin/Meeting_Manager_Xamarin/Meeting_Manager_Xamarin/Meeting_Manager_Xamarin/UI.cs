//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Meeting_Manager_Xamarin
{
    internal static class UI
    {
        internal static async Task NavigateTo(string pageToken, object data = null)
        {
            await App.Me.PushAsync(pageToken, SerializeParameter(data));
        }

        internal static async Task GoBack()
        {
            await App.Me.PopAsync();
        }

        internal static ObservableCollection<T> UpdateObservableCollection<T>(ref ObservableCollection<T> collection, IEnumerable<T> items)
        {
            if (items.Any())
            {
                collection = new ObservableCollection<T>(items);
            }
            else if (collection != null)
            {
                while (collection.Any()) collection.RemoveAt(0);
            }
            else
            {
                collection = new ObservableCollection<T>();
            }

            return collection;
        }

        internal static async Task<bool> YesNoDialog(string message)
        {
            return await App.Me.DisplayAlert(ResMan.GetString("AlertTitle"), message, ResMan.GetString("YesCaption"), ResMan.GetString("NoCaption"));
        }

        internal static async Task<bool> Alert(string title, string message)
        {
            return await App.Me.DisplayAlert(title, message, ResMan.GetString("OKCaption"), null);
        }

        internal static async Task DisplayAndExecuteAction(string title, IDictionary<string, Action> options)
        {
            var captions = options.Keys.ToArray();
            var actionTag = await DisplayActions(title, captions);

            if (actionTag != null)
            {
                options[actionTag].Invoke();
            }
        }

        internal static async Task<string> DisplayActions(string title, params string[] actions)
        {
            return await App.Me.DisplayActions(title, actions);
        }

        internal static async Task DisplayAlert(string title, string message, string cancelCaption)
        {
            await App.Me.DisplayAlert(title, message, cancelCaption);
        }

        private static object SerializeParameter(object parameter = null)
        {
            if (parameter != null)
            {
                if (!(parameter is string) && !(parameter is bool))
                {
                    parameter = JSON.Serialize(parameter);
                }
            }
            return parameter;
        }
    }
}
