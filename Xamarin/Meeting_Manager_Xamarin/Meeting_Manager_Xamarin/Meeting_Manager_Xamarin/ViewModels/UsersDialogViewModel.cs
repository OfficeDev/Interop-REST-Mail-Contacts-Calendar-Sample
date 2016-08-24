//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using Meeting_Manager_Xamarin.Models;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Meeting_Manager_Xamarin.ViewModels
{
    class UsersDialogViewModel : DialogViewModel
    {
        private const int PageSize = 10;
        private IUserPager _userPager;
        private User _selectedUser;
        private bool _getHumans;

        public Command FilterCommand => new Command(() => GetFirstUsersPage());
        public Command NextCommand => new Command(() => GetNextUsersPage(true));
        public Command PrevCommand => new Command(() => GetNextUsersPage(false));
        public Command ItemSelectedCommand => new Command(ItemSelected);

        public ObservableCollection<User> Users { get; private set; }

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

        public string Filter { get; set; }

        public bool HasNext { get; private set; }

        public bool HasPrev { get; private set; }

        public bool HasSelected => SelectedUser != null;

        protected override void OnNavigatedTo(object parameter)
        {
            _getHumans = (bool)parameter;
            OnPropertyChanged(() => Title);

            GetFirstUsersPage();
        }

        private void GetFirstUsersPage()
        {
            _userPager = GraphService.GetUserPager(PageSize, Filter, _getHumans);

            GetNextUsersPage(true);
        }

        private async void GetNextUsersPage(bool next)
        {
            using (new Loading(this))
            {
                var items = await _userPager.GetNextPage(next);
                Users = new ObservableCollection<User>(items);
            }

            HasNext = _userPager.HasNextPage;
            HasPrev = _userPager.HasPrevPage;

            OnPropertyChanged(() => Users);
            OnPropertyChanged(() => HasNext);
            OnPropertyChanged(() => HasPrev);
        }

        private void ItemSelected()
        {
            OnOk();
            GoBack();
        }

        private void OnOk()
        {
            if (_getHumans)
            {
                UI.Publish<User>(SelectedUser);
            }
            else
            {
                UI.Publish<Room>(SelectedUser.ConvertObject<Room>());
            }
        }
    }
}
