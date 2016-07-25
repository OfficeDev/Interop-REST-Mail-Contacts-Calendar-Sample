//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using Meeting_Manager_Xamarin.Models;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Meeting_Manager_Xamarin.ViewModels
{
    class UsersPageViewModel : BaseViewModel, ITransientViewModel
    {
        private const int PageSize = 10;
        private ObservableCollection<User> _users;
        private IUserPager _userPager;
        private bool _hasNext;
        private bool _hasPrev;
        private User _selectedUser;
        private string _filter;
        private bool _getHumans;

        public Command FilterCommand => new Command(FilterUsers);
        public Command NextCommand => new Command(NextPage);
        public Command PrevCommand => new Command(PrevPage);
        public Command ItemTappedCommand => new Command(ItemTapped);

        public ObservableCollection<User> Users
        {
            get { return _users; }
            private set { SetProperty(ref _users, value); }
        }

        public User SelectedUser
        {
            get { return _selectedUser; }
            set
            {
                SetProperty(ref _selectedUser, value);
                OnPropertyChanged(() => HasSelected);
            }
        }

        public string Title => GetString(_getHumans ? "SelectPerson" : "SelectRoom");

        public string Filter
        {
            get { return _filter; }
            set { SetProperty(ref _filter, value); }
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

        public bool HasSelected => SelectedUser != null;

        public override async void OnAppearing(object data)
        {
            _getHumans = (bool)data;
            OnPropertyChanged(() => Title);

            await GetFirstUsersPage();
        }

        private async Task GetFirstUsersPage()
        {
            _userPager = GraphService.GetUserPager(PageSize, Filter, _getHumans);

            await GetNextUsersPage(true);
        }

        private async void FilterUsers()
        {
            await GetFirstUsersPage();
        }

        private async void NextPage()
        {
            await GetNextUsersPage(true);
        }

        private async void PrevPage()
        {
            await GetNextUsersPage(false);
        }

        private async void ItemTapped()
        {
            OnOk();
            await UI.GoBack();
        }

        private async Task GetNextUsersPage(bool next)
        {
            using (new Loading(this))
            {
                var items = await _userPager.GetNextPage(next);

                Users = UI.UpdateObservableCollection<User>(ref _users, items);
                NotifyPropertyChanged(() => Users);
            }

            HasNext = _userPager.HasNextPage;
            HasPrev = _userPager.HasPrevPage;
        }

        private void OnOk()
        {
            if (_getHumans)
            {
                Publish<User>(SelectedUser);
            }
            else
            {
                Publish<Room>(SelectedUser.ConvertObject<Room>());
            }
        }
    }
}
