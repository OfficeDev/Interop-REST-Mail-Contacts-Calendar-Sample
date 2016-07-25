//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using Meeting_Manager_Xamarin.Models;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Meeting_Manager_Xamarin.ViewModels
{
    public class ContactsPageViewModel : BaseViewModel, ITransientViewModel
    {
        private const int PageSize = 10;

        private Contact _selectedContact;
        private ObservableCollection<Contact> _contacts;
        private bool _hasNext;
        private bool _hasPrev;
        private int _curPageIndex;
        private int _contactsCount;

        public Command NextCommand => new Command(NextPage);
        public Command PrevCommand => new Command(PrevPage);
        public Command PickCommand => new Command(ItemPicked);

        public ObservableCollection<Contact> Contacts
        {
            get { return _contacts; }
            private set { SetProperty(ref _contacts, value); }
        }

        public Contact SelectedContact
        {
            get { return _selectedContact; }
            set
            {
                SetProperty(ref _selectedContact, value);
                OnPropertyChanged(nameof(HasSelected));
            }
        }

        public bool HasSelected
        {
            get { return SelectedContact != null; }
        }

        public bool HasNext
        {
            get { return _hasNext; }
            private set { SetProperty(ref _hasNext, value); }
        }

        public bool HasPrev
        {
            get { return _hasPrev; }
            private set { SetProperty(ref _hasPrev, value); }
        }

        public override async void OnAppearing(object data)
        {
            using (new Loading(this))
            {
                _contactsCount = await GraphService.GetContactsCount();
            }

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

                var tasks = Contacts.Select(x => SetContactPhoto(x));
                await Task.WhenAll(tasks);
            }

            HasPrev = _curPageIndex > 0;
            HasNext = PageSize * (_curPageIndex + 1) < _contactsCount;
        }

        private async Task SetContactPhoto(Contact contact)
        {
            var photoData = await GraphService.GetContactPhoto(contact.Id);

            if (photoData != null)
            {
                contact.Photo = GetImage(photoData);
            }
            else
            {
                contact.Photo = ImageSource.FromResource("Meeting_Manager_Xamarin.Images.outlook_small.png");
            }

            contact.NotifyPropertyChanged("Photo");
        }

        private ImageSource GetImage(byte[] data)
        {
            var ms = new MemoryStream(data);

            var imageSource = ImageSource.FromStream(() => ms);

            return imageSource;
        }

        private async void ItemPicked()
        {
            OnOk();
            await UI.GoBack();
        }

        private void OnOk()
        {
            if (SelectedContact.EmailAddresses.Any())
            {
                Publish(SelectedContact);
            }
        }
    }
}
