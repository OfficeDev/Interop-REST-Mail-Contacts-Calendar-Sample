using MeetingManager.Models;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Input;

namespace MeetingManager.ViewModels
{
    class TimeSlotsDialogViewModel : ViewModel, ITransientViewModel
    {
        private MeetingTimeCandidate _selectedMeetingTimeCandidate;
        private ObservableCollection<MeetingTimeCandidate> _meetingTimeCandidates;
        private Meeting _meeting;

        public TimeSlotsDialogViewModel()
        {
            DoubleTappedCommand = new DelegateCommand<DoubleTappedRoutedEventArgs>(DoubleTapped);
            OkCommand = new DelegateCommand(OnOk);

            GetEvent<InitDialogEvent>().Subscribe(OnInitialize);
        }

        public DelegateCommand<DoubleTappedRoutedEventArgs> DoubleTappedCommand { get; }
        public DelegateCommand OkCommand { get; }

        public ObservableCollection<MeetingTimeCandidate> MeetingTimeCandidates
        {
            get { return _meetingTimeCandidates; }
            private set { SetProperty(ref _meetingTimeCandidates, value); }
        }

        public MeetingTimeCandidate SelectedMeetingTimeCandidate
        {
            get { return _selectedMeetingTimeCandidate; }
            set
            {
                SetProperty(ref _selectedMeetingTimeCandidate, value);
                OnPropertyChanged(() => HasSelected);
            }
        }

        public bool HasSelected => SelectedMeetingTimeCandidate != null;

        private async void OnInitialize(object parameter)
        {
            GetEvent<InitDialogEvent>().Unsubscribe(OnInitialize);

            _meeting = Deserialize<Meeting>(parameter);

            var items = await GetAllTimeCandidates(_meeting);
            SetTimeSlotProperties(items);

            MeetingTimeCandidates = new ObservableCollection<MeetingTimeCandidate>(items);
        }

        private void DoubleTapped(DoubleTappedRoutedEventArgs args)
        {
            OnOk();
        }

        private void OnOk()
        {
            GetEvent<MeetingTimeCandidateSelectedEvent>().Publish(SelectedMeetingTimeCandidate);
        }

        private void SetTimeSlotProperties(IEnumerable<MeetingTimeCandidate> items)
        {
            foreach (var x in items)
            {
                var start = DateTime.Parse(x.MeetingTimeSlot.Start.Time);
                var end = DateTime.Parse(x.MeetingTimeSlot.End.Time);

                x.TimeSlot = string.Format("{0:hh:mm tt} - {1:hh:mm tt}", start, end);
                x.TimeZone = x.MeetingTimeSlot.Start.TimeZone;
            }
        }
    }
}
