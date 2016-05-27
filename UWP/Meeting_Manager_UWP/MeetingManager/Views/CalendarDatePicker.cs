//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace MeetingManager.Views
{
    class CalendarDatePicker : Windows.UI.Xaml.Controls.CalendarDatePicker
    {
        public DateTimeOffset SelectedDate
        {
            get { return(DateTimeOffset)GetValue(SelectedDateProperty); }
            set { SetValue(SelectedDateProperty, value); }
        }

        public static readonly DependencyProperty SelectedDateProperty =
                DependencyProperty.Register("SelectedDate",
                typeof(DateTimeOffset), typeof(CalendarDatePicker), new
                PropertyMetadata(null, (sender, e) =>
                {
                    if (e.NewValue != null)
                    {
                        ((CalendarDatePicker)sender).Date = (DateTimeOffset)e.NewValue;
                    }
                    else
                    {
                        ((CalendarDatePicker)sender).Date = null;
                    }
                }));

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.DateChanged += CalendarDatePicker_DateChanged;
        }

        private void CalendarDatePicker_DateChanged(
            Windows.UI.Xaml.Controls.CalendarDatePicker sender,
            Windows.UI.Xaml.Controls.CalendarDatePickerDateChangedEventArgs args)
        {
            if (args.NewDate != args.OldDate)
            {
                if (args.NewDate != null && args.NewDate.HasValue)
                {
                    SelectedDate = args.NewDate.Value;
                }
                else if (args.OldDate != null && args.OldDate.HasValue)
                {
                    SelectedDate = args.OldDate.Value;
                    Date = args.OldDate;
                }
                else
                {
                    SelectedDate = DateTimeOffset.Now;
                }
            }
        }
    }
}
