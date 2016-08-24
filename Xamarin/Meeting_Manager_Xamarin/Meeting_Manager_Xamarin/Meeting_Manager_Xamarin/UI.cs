//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using Meeting_Manager_Xamarin.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Meeting_Manager_Xamarin
{
    internal static class UI
    {
        internal static async void NavigateTo(string pageToken, object data = null)
        {
            await App.Me.PushAsync(pageToken, SerializeParameter(data));
        }

        internal static async void GoBack()
        {
            await App.Me.PopAsync();
        }

        internal static void Publish<T>(T data)
        {
            MessagingCenter.Send<object, T>(App.Me, typeof(T).Name, data);
        }

        internal static void Subscribe<T>(Action<T> action)
        {
            Action<object, T> superDelegate = (object that, T arg) => action.Invoke(arg);

            MessagingCenter.Subscribe(App.Me, typeof(T).Name, superDelegate);
        }

        internal static ObservableCollection<T> UpdateObservableCollection<T>(ref ObservableCollection<T> collection, IEnumerable<T> items)
        {
            if (items?.Any() == true)
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

        internal static async Task MessageDialog(string message)
        {
            await App.Me.DisplayAlert(ResMan.GetString("AlertTitle"), message, ResMan.GetString("OKCaption"));
        }

        internal static async Task OpenAttachment(FileAttachment item)
        {
            DependencyService.Get<IAttachmentOpener>().Open(item);
            await Task.FromResult(0);
        }

        internal static Task<ImageSource> BytesToPhoto(byte[] data)
        {
            ImageSource result;

            if (data == null)
            {
                result = ImageSource.FromResource("Meeting_Manager_Xamarin.Images.outlook_small.png");
            }
            else
            {
                var ms = new System.IO.MemoryStream(data);

                result = ImageSource.FromStream(() => ms);
            }

            return Task.FromResult(result);
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
