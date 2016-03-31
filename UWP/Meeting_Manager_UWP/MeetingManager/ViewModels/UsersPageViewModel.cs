using MeetingManager.Models;
using Prism.Commands;
using Prism.Events;
using Prism.Windows.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Input;

namespace MeetingManager.ViewModels
{
    class UsersPageViewModel : ViewModel
    {
        private const int PageSize = 10;
        private ObservableCollection<User> _users;
        private IUserPager _userPager;
        private bool _hasNext;
        private bool _hasPrev;
        private User _selectedUser;
        private string _filter;
        private bool _getHumans;

        public UsersPageViewModel()
        {
            FilterCommand = new DelegateCommand(FilterUsers);
            NextCommand = new DelegateCommand(NextPage);
            PrevCommand = new DelegateCommand(PrevPage);
            DoubleTappedCommand = new DelegateCommand(DoubleTapped);
        }

        public DelegateCommand FilterCommand { get; private set; }
        public DelegateCommand NextCommand { get; private set; }
        public DelegateCommand PrevCommand { get; private set; }
        public DelegateCommand DoubleTappedCommand { get; private set; }

        public ObservableCollection<User> Users
        {
            get { return _users; }
            private set { SetProperty(ref _users, value); }
        }

        public User SelectedUser
        {
            get { return _selectedUser; }
            set { SetProperty(ref _selectedUser, value); }
        }

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

        public override async void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            base.OnNavigatedTo(e, viewModelState);

            _getHumans = (bool)e.Parameter;
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

            HasNext = _userPager.HasNextPage();
            HasPrev = _userPager.HasPrevPage();
        }

        private void DoubleTapped()
        {
            if (_getHumans)
            {
                GetEvent<UserSelectedEvent>().Publish(SelectedUser);
            }
            else
            {
                GetEvent<RoomSelectedEvent>().Publish(SelectedUser);
            }

            GoBack();
        }
    }
}
