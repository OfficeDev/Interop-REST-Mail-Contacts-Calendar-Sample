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
    class FilesPageViewModel : BaseViewModel, ITransientViewModel
    {
        private ObservableCollection<DriveItem> _files;
        private Stack<string> _folders = new Stack<string>();

        public bool IsRootFolder => !_folders.Any();

        public Command<DriveItem> ItemTappedCommand => new Command<DriveItem>(ItemTapped);
        public Command<DriveItem> ViewFileCommand => new Command<DriveItem>(ViewFile);
        public Command<DriveItem> DeleteFileCommand => new Command<DriveItem>(DeleteFile);
        public Command UpCommand => new Command(GoUp);

        public ObservableCollection<DriveItem> Files
        {
            get { return _files; }
            private set { SetProperty(ref _files, value); }
        }

        public override async void OnAppearing(object data)
        {
            await GetDriveItems();
        }

        private async Task GetDriveItems()
        {
            using (new Loading(this))
            {
                var folderId = !IsRootFolder ? _folders.Peek() : string.Empty;
                var files = await GraphService.GetDriveItems(folderId, 0, 100);

                Files = new ObservableCollection<DriveItem>(files);
            }
        }

        private async void GoUp()
        {
            _folders.Pop();
            await GetDriveItems();
            OnPropertyChanged(() => IsRootFolder);
        }

        private async void ItemTapped(DriveItem item)
        {
            if (item.File != null)
            {
                await UI.GoBack();
                Publish(item);
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
                    DependencyService.Get<IAttachmentOpener>().Open(new FileAttachment
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
                Files.Remove(item);
            }
        }
    }
}
