//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using MeetingManager.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace MeetingManager.ViewModels
{
    class ContactsDialogViewModel : DialogViewModel
    {
        private const int PageSize = 10;

        private Contact _selectedContact;
        private int _curPageIndex;
        private int _contactsCount;

        public Command NextCommand => new Command(NextPage);
        public Command PrevCommand => new Command(PrevPage);
        public Command ItemSelectedCommand => new Command(ItemSelected);
        public Command OkCommand => new Command(OnOk);

        public ObservableCollection<Contact> Contacts { get; private set; }

        public Contact SelectedContact
        {
            get { return _selectedContact; }
            set
            {
                SetProperty(ref _selectedContact, value);
                OnPropertyChanged(nameof(HasSelected));
            }
        }

        public bool HasSelected => SelectedContact != null;

        public bool HasNext { get; private set; }

        public bool HasPrev { get; private set; }

        protected override async void OnNavigatedTo(object parameter)
        {
            using (new Loading(this))
            {
                _contactsCount = await GraphService.GetContactsCount();
            }

            await GetFirstPage();
        }

        private async Task GetFirstPage()
        {
            await GetContacts();
        }

        private async void NextPage()
        {
            ++_curPageIndex;
            await GetContacts();
        }

        private async void PrevPage()
        {
            --_curPageIndex;
            await GetContacts();
        }

        private async Task GetContacts()
        {
            using (new Loading(this))
            {
                var items = await GraphService.GetContacts(_curPageIndex, PageSize);
                Contacts = new ObservableCollection<Contact>(items);
                OnPropertyChanged(() => Contacts);

                var tasks = Contacts.Select(x => SetContactPhoto(x));
                await Task.WhenAll(tasks);
            }

            HasPrev = _curPageIndex > 0;
            HasNext = PageSize * (_curPageIndex + 1) < _contactsCount;

            OnPropertyChanged(() => HasPrev);
            OnPropertyChanged(() => HasNext);
        }

        private async Task SetContactPhoto(Contact contact)
        {
            var photoData = await GraphService.GetContactPhoto(contact.Id);

            var photo = await UI.BytesToPhoto(photoData);

            if (photo != null)
            {
                contact.Photo = photo;
                contact.NotifyPropertyChanged("Photo");
            }
        }

        private void ItemSelected()
        {
            OnOk();
            GoBack();
        }

        private void OnOk()
        {
            if (SelectedContact.EmailAddresses.Any())
            {
                UI.Publish(SelectedContact);
            }
        }
    }
}
