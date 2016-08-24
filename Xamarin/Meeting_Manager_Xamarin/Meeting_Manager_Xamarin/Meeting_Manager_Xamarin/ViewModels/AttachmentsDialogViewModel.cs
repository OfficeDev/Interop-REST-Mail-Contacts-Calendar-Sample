//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using Meeting_Manager_Xamarin.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xamarin.Forms;

namespace Meeting_Manager_Xamarin.ViewModels
{
    class AttachmentsDialogViewModel : DialogViewModel
    {
        private string _eventId;
        private bool _isOrganizer;

        public Command<FileAttachment> DeleteCommand => new Command<FileAttachment>(DeleteAttachment, CanDelete);
        public Command<FileAttachment> ViewCommand => new Command<FileAttachment>(OpenAttachment);

        public ObservableCollection<FileAttachment> Items { get; private set; }

        protected override void OnNavigatedTo(object parameter)
        {
            var tuple = JSON.Deserialize<Tuple<List<FileAttachment>, string, bool>>(parameter);

            Items = new ObservableCollection<FileAttachment>(tuple.Item1);
            _eventId = tuple.Item2;
            _isOrganizer = string.IsNullOrEmpty(_eventId) || tuple.Item3;

            OnPropertyChanged(() => Items);
        }

        private async void OpenAttachment(FileAttachment item)
        {
            using (new Loading(this))
            {
                await UI.OpenAttachment(item);
            }
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
            UI.Publish(item);
        }
    }
}
