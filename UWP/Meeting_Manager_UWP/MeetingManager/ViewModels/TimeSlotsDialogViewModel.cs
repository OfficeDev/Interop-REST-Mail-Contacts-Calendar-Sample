//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using MeetingManager.Models;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            GetEvent<InitDialogEvent>().Subscribe(OnInitialize);
        }

        public DelegateCommand<DoubleTappedRoutedEventArgs> DoubleTappedCommand => new DelegateCommand<DoubleTappedRoutedEventArgs>(DoubleTapped);
        public DelegateCommand OkCommand => new DelegateCommand(OnOk);

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

            _meeting = UI.Deserialize<Meeting>(parameter);

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

                start = DateTime.SpecifyKind(start, DateTimeKind.Utc);
                end = DateTime.SpecifyKind(end, DateTimeKind.Utc);

                start = start.ToLocalTime();
                end = end.ToLocalTime();

                x.TimeSlot = string.Format("{0:hh:mm tt} - {1:hh:mm tt}", start, end);
            }
        }
    }
}
