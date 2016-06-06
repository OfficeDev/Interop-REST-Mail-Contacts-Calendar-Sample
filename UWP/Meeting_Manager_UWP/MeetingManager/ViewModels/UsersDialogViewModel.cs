//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using MeetingManager.Models;
using Prism.Commands;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace MeetingManager.ViewModels
{
    class UsersDialogViewModel : ViewModel, ITransientViewModel
    {
        private const int PageSize = 10;
        private ObservableCollection<User> _users;
        private IUserPager _userPager;
        private bool _hasNext;
        private bool _hasPrev;
        private User _selectedUser;
        private string _filter;
        private bool _getHumans;

        public UsersDialogViewModel()
        {
            FilterCommand = new DelegateCommand(FilterUsers);
            NextCommand = new DelegateCommand(NextPage);
            PrevCommand = new DelegateCommand(PrevPage);
            DoubleTappedCommand = new DelegateCommand(DoubleTapped);
            OkCommand = new DelegateCommand(OnOk);

            GetEvent<InitDialogEvent>().Subscribe(OnInitialize);
        }

        public DelegateCommand FilterCommand { get; }
        public DelegateCommand NextCommand { get; }
        public DelegateCommand PrevCommand { get; }
        public DelegateCommand DoubleTappedCommand { get; }
        public DelegateCommand OkCommand { get; }

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

        private async void OnInitialize(object parameter)
        {
            GetEvent<InitDialogEvent>().Unsubscribe(OnInitialize);

            _getHumans = (bool) parameter;
            OnPropertyChanged(() => Title);

            await GetFirstUsersPage();
        }

        private async Task GetFirstUsersPage()
        {
            _userPager = OfficeService.GetUserPager(PageSize, Filter, _getHumans);

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

        private async Task GetNextUsersPage(bool next)
        {
            using (new Loading(this))
            {
                var items = await _userPager.GetNextPage(next);
                Users = new ObservableCollection<User>(items);
            }

            HasNext = _userPager.HasNextPage;
            HasPrev = _userPager.HasPrevPage;
        }


        private void DoubleTapped()
        {
            OnOk();
        }

        private void OnOk()
        {
            if (_getHumans)
            {
                GetEvent<UserSelectedEvent>().Publish(SelectedUser);
            }
            else
            {
                GetEvent<RoomSelectedEvent>().Publish(SelectedUser);
            }
        }
    }
}
