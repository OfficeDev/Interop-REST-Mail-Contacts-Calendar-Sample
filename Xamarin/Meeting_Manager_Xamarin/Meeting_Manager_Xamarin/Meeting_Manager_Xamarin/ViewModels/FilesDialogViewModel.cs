//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using Meeting_Manager_Xamarin.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Meeting_Manager_Xamarin.ViewModels
{
    class FilesDialogViewModel : DialogViewModel
    {
        private Stack<string> _folders = new Stack<string>();
        private DriveItem _selectedItem;
        private ObservableCollection<DriveItem> _items;

        public Command<DriveItem> ItemSelectedCommand => new Command<DriveItem>(ItemSelected);
        public Command<DriveItem> DeleteCommand => new Command<DriveItem>(DeleteFile);
        public Command<DriveItem> ViewCommand => new Command<DriveItem>(ViewFile);
        public Command UpCommand => new Command(GoUp);

        public bool IsRootFolder => !_folders.Any();

        public ObservableCollection<DriveItem> Items
        {
            get { return _items; }
            private set { SetCollectionProperty(ref _items, value); }
        }

        public DriveItem SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                SetProperty(ref _selectedItem, value);
            }
        }

        protected override async void OnNavigatedTo(object parameter)
        {
            await GetDriveItems();
        }

        private async Task GetDriveItems()
        {
            using (new Loading(this))
            {
                var folderId = !IsRootFolder ? _folders.Peek() : string.Empty;

                var items = await GraphService.GetDriveItems(folderId, 0, 100);
                Items = new ObservableCollection<DriveItem>(items);
                OnPropertyChanged(() => Items);
            }
        }

        private async void GoUp()
        {
            _folders.Pop();
            await GetDriveItems();
            OnPropertyChanged(() => IsRootFolder);
        }

        private void ItemSelected(DriveItem item)
        {
            HandleItem(item);

            if (!IsFolder(item))
            {
                GoBack();
            }
        }

        private async void HandleItem(DriveItem item)
        {
            if (!IsFolder(item))
            {
                UI.Publish(item);
            }
            else
            {
                _folders.Push(item.Id);
                await GetDriveItems();
                OnPropertyChanged(() => IsRootFolder);
            }
        }

        private async void ViewFile(DriveItem item)
        {
            using (new Loading(this))
            {
                var data = await GraphService.GetDriveItemContent(item.Id);

                if (data != null)
                {
                    await UI.OpenAttachment(new FileAttachment
                    {
                        Name = item.Name,
                        ContentBytes = data,
                    });
                }
            }
        }

        private async void DeleteFile(DriveItem item)
        {
            using (new Loading(this))
            {
                await GraphService.DeleteDriveItem(item.Id);
                Items.Remove(item);
            }
        }

        private bool IsFolder(DriveItem item)
        {
            return item?.Folder != null;
        }
    }
}
