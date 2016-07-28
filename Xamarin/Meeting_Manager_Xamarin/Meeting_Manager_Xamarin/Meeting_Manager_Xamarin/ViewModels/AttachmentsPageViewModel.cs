//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using Meeting_Manager_Xamarin.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xamarin.Forms;

namespace Meeting_Manager_Xamarin.ViewModels
{
    class AttachmentsPageViewModel : BaseViewModel, ITransientViewModel
    {
        private string _eventId;
        private bool _isOrganizer;

        public Command<FileAttachment> DeleteAttachmentCommand => new Command<FileAttachment>(DeleteAttachment, CanDelete);
        public Command<FileAttachment> OpenAttachmentCommand => new Command<FileAttachment>(OpenAttachment);

        public ObservableCollection<FileAttachment> Items { get; private set; }

        public override void OnAppearing(object data)
        {
            var tuple = JSON.Deserialize<Tuple<List<FileAttachment>, string, bool>>(data);

            Items = new ObservableCollection<FileAttachment>(tuple.Item1);
            OnPropertyChanged(() => Items);
            _eventId = tuple.Item2;
            _isOrganizer = tuple.Item3;
        }

        private void OpenAttachment(FileAttachment item)
        {
            DependencyService.Get<IAttachmentOpener>().Open(item);
        }

        private bool CanDelete(FileAttachment item)
        {
            return _isOrganizer;
        }

        private async void DeleteAttachment(FileAttachment item)
        {
            if (!string.IsNullOrEmpty(item.Id))
            {
                using (new Loading(this))
                {
                    await GraphService.DeleteEventAttachment(_eventId, item.Id);
                }
            }

            Items.Remove(item);
            Publish(item);
        }
    }
}
