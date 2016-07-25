//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using Meeting_Manager_Xamarin.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Meeting_Manager_Xamarin.ViewModels
{
    class AttachmentsPageViewModel : BaseViewModel, ITransientViewModel
    {
        private ObservableCollection<FileAttachment> _items;
        private FileAttachment _selectedItem;
        private string _eventId;

        public Command<FileAttachment> DeleteAttachmentCommand => new Command<FileAttachment>(DeleteAttachment);
        public Command<FileAttachment> OpenAttachmentCommand => new Command<FileAttachment>(OpenAttachment);

        public FileAttachment SelectedItem
        {
            get { return _selectedItem; }
            set { SetProperty(ref _selectedItem, value); }
        }

        public ObservableCollection<FileAttachment> Items
        {
            get { return _items; }
            private set { SetProperty(ref _items, value); }
        }

        public override void OnAppearing(object data)
        {
            var tuple = JSON.Deserialize<Tuple<List<FileAttachment>, string>>(data);

            Items = new ObservableCollection<FileAttachment>(tuple.Item1);
            _eventId = tuple.Item2;
        }

        private void OpenAttachment(FileAttachment item)
        {
            DependencyService.Get<IAttachmentOpener>().Open(item);
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
