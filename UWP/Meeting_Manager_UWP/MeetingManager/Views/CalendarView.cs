//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using System;
using System.Linq;
using Windows.UI.Xaml;

namespace MeetingManager.Views
{
    class CalendarView : Windows.UI.Xaml.Controls.CalendarView
    {
        public DateTimeOffset SelectedDate
        {
            get { return (DateTimeOffset)GetValue(SelectedDateProperty); }
            set { SetValue(SelectedDateProperty, value); }
        }

        public static readonly DependencyProperty SelectedDateProperty =
                DependencyProperty.Register("SelectedDate",
                typeof(DateTimeOffset), typeof(CalendarView), new
                PropertyMetadata(null, (sender, e) =>
                {
                    // We always use Single Selection
                    ((CalendarView)sender).SelectedDates.Clear();
                    ((CalendarView)sender).SelectedDates.Add((DateTimeOffset)e.NewValue);
                }));

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.SelectedDatesChanged += CalendarView_DatesChanged;
        }

        private void CalendarView_DatesChanged(
            Windows.UI.Xaml.Controls.CalendarView sender,
            Windows.UI.Xaml.Controls.CalendarViewSelectedDatesChangedEventArgs args)
        {
            if (args.AddedDates.Any())
            {
                SelectedDate = args.AddedDates[0].Date;
            }
        }
    }
}
