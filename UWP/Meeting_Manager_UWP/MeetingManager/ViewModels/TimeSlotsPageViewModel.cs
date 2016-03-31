using MeetingManager.Models;
using Newtonsoft.Json;
using Prism.Commands;
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
    class TimeSlotsPageViewModel : ViewModel
    {
        private MeetingTimeCandidate _selectedMeetingTimeCandidate;
        private ObservableCollection<MeetingTimeCandidate> _meetingTimeCandidates;
        private Meeting _meeting;

        public TimeSlotsPageViewModel()
        {
            DoubleTappedCommand = new DelegateCommand<DoubleTappedRoutedEventArgs>(DoubleTapped);
        }

        public DelegateCommand<DoubleTappedRoutedEventArgs> DoubleTappedCommand { get; private set; }

        public ObservableCollection<MeetingTimeCandidate> MeetingTimeCandidates
        {
            get { return _meetingTimeCandidates; }
            private set { SetProperty(ref _meetingTimeCandidates, value); }
        }

        public MeetingTimeCandidate SelectedMeetingTimeCandidate
        {
            get { return _selectedMeetingTimeCandidate; }
            set { SetProperty(ref _selectedMeetingTimeCandidate, value); }
        }

        public override async void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            base.OnNavigatedTo(e, viewModelState);

            _meeting = JsonConvert.DeserializeObject<Meeting>((string)e.Parameter);

            MeetingTimeCandidates = new ObservableCollection<MeetingTimeCandidate>();

            using (new Loading(this))
            {
                var items = await GetTimeCandidates("8:00:00", "11:00:00");

                items = items.Union(await GetTimeCandidates("11:00:00", "15:00:00"));
                items = items.Union(await GetTimeCandidates("15:00:00", "18:00:00"));

                items.ForEach(x => MeetingTimeCandidates.Add(x));
            }
        }

        private void DoubleTapped(DoubleTappedRoutedEventArgs args)
        {
            GetEvent<MeetingTimeCandidateSelectedEvent>().Publish(SelectedMeetingTimeCandidate);
            GoBack();
        }

        private async Task<IEnumerable<MeetingTimeCandidate>> GetTimeCandidates(string startTime, string endTime)
        {
            var items = await OfficeService.GetMeetingTimeCandidates(_meeting, startTime, endTime);

            foreach (var x in items)
            {
#if true
                var start = DateTime.Parse(x.MeetingTimeSlot.Start.Time);
                var end = DateTime.Parse(x.MeetingTimeSlot.End.Time);
#else
                var start = x.MeetingTimeSlot.Start.Time;
                var end = x.MeetingTimeSlot.End.Time;
#endif
                x.TimeSlot = string.Format("{0:hh:mm tt} - {1:hh:mm tt}", start, end);
                x.TimeZone = x.MeetingTimeSlot.Start.TimeZone;
            }

            return items;
        }
    }
}
