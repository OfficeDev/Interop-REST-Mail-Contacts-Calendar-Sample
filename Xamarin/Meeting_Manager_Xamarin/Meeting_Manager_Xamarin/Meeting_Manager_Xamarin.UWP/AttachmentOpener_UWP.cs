//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using Meeting_Manager_Xamarin.Models;
using Meeting_Manager_Xamarin.UWP;
using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Popups;
using Xamarin.Forms;

[assembly: Dependency(typeof(AttachmentOpener_UWP))]

namespace Meeting_Manager_Xamarin.UWP
{
    public class AttachmentOpener_UWP : IAttachmentOpener
    {
        public async void Open(FileAttachment attachment)
        {
            var storageFolder = ApplicationData.Current.LocalFolder;

            try
            {
                var newFile = await storageFolder.CreateFileAsync(attachment.Name, CreationCollisionOption.ReplaceExisting);

                await FileIO.WriteBytesAsync(newFile, attachment.ContentBytes);

                if (!await Windows.System.Launcher.LaunchFileAsync(newFile))
                {
                    await MessageDialog($"Couldn't start application for {attachment.Name}");
                }
            }
            catch (Exception ex)
            {
                await MessageDialog($"Failure: {ex.Message}");
            }
        }

        private static async Task MessageDialog(string message)
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
    }
}
