//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using MeetingManager.Models;
using Newtonsoft.Json;
using Prism.Events;
using System;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams;

namespace MeetingManager
{
    internal static class UI
    {
        private static ContentDialog CurrentDialogInstance;

        internal static void GoBack()
        {
            App.Me.NavigationService.GoBack();
        }

        internal static void Subscribe<T>(Action<T> action)
        {
            App.Me.EventAggregator.GetEvent<PubSubEvent<T>>().Subscribe(action);
        }

        internal static void Unsubscribe<T>(Action<T> action)
        {
            App.Me.EventAggregator.GetEvent<PubSubEvent<T>>().Unsubscribe(action);
        }

        internal static void Publish<T>(T data)
        {
            App.Me.EventAggregator.GetEvent<PubSubEvent<T>>().Publish(data);
        }

        internal static async Task<bool> YesNoDialog(string message)
        {
            var messageDialog = new MessageDialog(message);

            messageDialog.Commands.Add(new UICommand
            {
                Label = ResMan.GetString("YesCaption"),
                Id = 0
            });

            messageDialog.Commands.Add(new UICommand
            {
                Label = ResMan.GetString("NoCaption"),
                Id = 1
            });

            messageDialog.DefaultCommandIndex = 0;
            messageDialog.CancelCommandIndex = 1;

            var result = await messageDialog.ShowAsync();

            return (int)result.Id == 0;
        }

        internal static async Task MessageDialog(string message)
        {
            var messageDialog = new MessageDialog(message);

            messageDialog.Commands.Add(new UICommand
            {
                Label = ResMan.GetString("OKCaption"),
                Id = 0
            });

            messageDialog.DefaultCommandIndex = 0;

            await messageDialog.ShowAsync();
        }

        internal static async void NavigateTo(string pageToken, object parameter = null)
        {
            var viewNameSpace = App.Me.GetType().Namespace + ".Views";
            var dialogTypeName = viewNameSpace + "." + pageToken + "Dialog";

            var dialogType = Type.GetType(dialogTypeName);

            if (dialogType != null)
            {
                var dlg = Activator.CreateInstance(dialogType) as ContentDialog;
                CurrentDialogInstance = dlg;

                Publish(new InitDialog { Payload = UI.SerializeParameter(parameter) });
                await dlg.ShowAsync();
            }
            else 
            {
                UI.NavigateToPage(pageToken, parameter);
            }
        }

        internal static void CloseDialog()
        {
            CurrentDialogInstance?.Hide();
            CurrentDialogInstance = null;
        }

        internal static async Task OpenAttachment(FileAttachment attachment)
        {
            var storageFolder = ApplicationData.Current.LocalFolder;

            try
            {
                var newFile = await storageFolder.CreateFileAsync(attachment.Name, CreationCollisionOption.ReplaceExisting);

                await FileIO.WriteBytesAsync(newFile, attachment.ContentBytes);

                if (!await Windows.System.Launcher.LaunchFileAsync(newFile))
                {
                    await UI.MessageDialog($"Couldn't start application for {attachment.Name}");
                }
            }
            catch (Exception ex)
            {
                await UI.MessageDialog($"Failure: {ex.Message}");
            }
        }

        internal static async Task<BitmapImage> BytesToPhoto(byte[] data)
        {
            if (data == null) return null;

            using (var ms = new InMemoryRandomAccessStream())
            {
                using (var writer = new DataWriter(ms.GetOutputStreamAt(0)))
                {
                    writer.WriteBytes(data);
                    await writer.StoreAsync();
                }

                var image = new BitmapImage();

                try
                {
                    await image.SetSourceAsync(ms);
                    return image;
                }
                catch
                {
                    // in case we received invalid data
                }

                return null;
            }
        }

        private static void NavigateToPage(string pageToken, object parameter = null)
        {
            App.Me.NavigationService.Navigate(pageToken, SerializeParameter(parameter));
        }

        private static object SerializeParameter(object parameter)
        {
            if (parameter != null)
            {
                if (!(parameter is string) && !(parameter is bool))
                {
                    parameter = JsonConvert.SerializeObject(parameter);
                }
            }
            return parameter;
        }
    }
}
