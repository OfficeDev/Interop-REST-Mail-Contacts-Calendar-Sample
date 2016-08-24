//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using MeetingManager.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MeetingManager.ViewModels
{
    class TimeSlotsDialogViewModel : DialogViewModel
    {
        private MeetingTimeCandidate _selectedMeetingTimeCandidate;
        private Meeting _meeting;

        public Command<MeetingTimeCandidate> ItemSelectedCommand => new Command<MeetingTimeCandidate>(ItemSelected);
        public Command OkCommand => new Command(OnOk);

        public ObservableCollection<MeetingTimeCandidate> Items { get; set; }

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

        protected override async void OnNavigatedTo(object parameter)
        {
            _meeting = JSON.Deserialize<Meeting>(parameter);

            var items = await GetAllTimeCandidates(_meeting);
            SetTimeSlotProperties(items);

            Items = new ObservableCollection<MeetingTimeCandidate>(items);
            OnPropertyChanged(() => Items);
        }

        private void ItemSelected(MeetingTimeCandidate item)
        {
            OnOk();
            GoBack();
        }

        private void OnOk()
        {
            UI.Publish(SelectedMeetingTimeCandidate);
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
