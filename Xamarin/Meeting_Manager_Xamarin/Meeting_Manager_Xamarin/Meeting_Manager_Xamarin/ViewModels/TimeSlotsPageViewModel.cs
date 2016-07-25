//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using Meeting_Manager_Xamarin.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Meeting_Manager_Xamarin.ViewModels
{
    class TimeSlotsPageViewModel : BaseViewModel, ITransientViewModel
    {
        private ObservableCollection<MeetingTimeCandidate> _items;
        private Meeting _meeting;

        public Command<MeetingTimeCandidate> ItemTappedCommand => new Command<MeetingTimeCandidate>(ItemTapped);

        public ObservableCollection<MeetingTimeCandidate> Items
        {
            get { return _items; }
            private set { SetProperty(ref _items, value); }
        }

        public override async void OnAppearing(object data)
        {
            _meeting = JSON.Deserialize<Meeting>(data);

            var items = await GetAllTimeCandidates(_meeting);
            SetTimeSlotProperties(items);

            Items = new ObservableCollection<MeetingTimeCandidate>(items);
        }

        private async void ItemTapped(MeetingTimeCandidate item)
        {
            await UI.GoBack();
            Publish(item);
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
