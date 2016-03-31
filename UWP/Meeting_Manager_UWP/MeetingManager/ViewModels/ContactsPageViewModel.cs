using MeetingManager.Models;
using Prism.Commands;
using Prism.Windows.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace MeetingManager.ViewModels
{
    class ContactsPageViewModel : ViewModel
    {
        private const int PageSize = 10;

        private Contact _selectedContact;
        private ObservableCollection<Contact> _contacts;
        private bool _hasNext;
        private bool _hasPrev;
        private int _curPageIndex;
        private int _contactsCount;

        public ContactsPageViewModel()
        {
            NextCommand = new DelegateCommand(NextPage);
            PrevCommand = new DelegateCommand(PrevPage);
            DoubleTappedCommand = new DelegateCommand(DoubleTapped);
        }

        public DelegateCommand NextCommand { get; private set; }
        public DelegateCommand PrevCommand { get; private set; }
        public DelegateCommand DoubleTappedCommand { get; private set; }

        public ObservableCollection<Contact> Contacts
        {
            get { return _contacts; }
            private set { SetProperty(ref _contacts, value); }
        }

        public Contact SelectedContact
        {
            get { return _selectedContact; }
            set { SetProperty(ref _selectedContact, value); }
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

        public override async void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            base.OnNavigatedTo(e, viewModelState);

            Contacts = null;
            _contactsCount = await OfficeService.GetContactsCount();
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
                var items = await OfficeService.GetContacts(_curPageIndex, PageSize);
                Contacts = new ObservableCollection<Contact>(items);

                var tasks = Contacts.Select(x => SetContactPhoto(x));
                await Task.WhenAll(tasks);
            }

            HasPrev = _curPageIndex > 0;
            HasNext = PageSize * (_curPageIndex + 1) < _contactsCount;
            IsLoading = false;
        }

        private async Task SetContactPhoto(Contact contact)
        {
            var photoData = await OfficeService.GetContactPhoto(contact.Id);

            if (photoData != null)
            {
                contact.Photo = await GetImage(photoData);
                contact.NotifyPropertyChanged("Photo");
            }
        }

        private async Task<BitmapImage> GetImage(byte[] data)
        {
            using (InMemoryRandomAccessStream ms = new InMemoryRandomAccessStream())
            {
                using (DataWriter writer = new DataWriter(ms.GetOutputStreamAt(0)))
                {
                    writer.WriteBytes(data);
                    await writer.StoreAsync();
                }

                var image = new BitmapImage();

                await image.SetSourceAsync(ms);

                return image;
            }
        }

        private void DoubleTapped()
        {
            if (SelectedContact.EmailAddresses.Any())
            {
                GetEvent<ContactSelectedEvent>().Publish(SelectedContact);
                GoBack();
            }
        }
    }
}
