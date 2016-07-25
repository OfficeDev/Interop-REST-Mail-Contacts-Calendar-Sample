//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Globalization;

namespace Meeting_Manager_Xamarin.Views
{
    public partial class RecurrencePage : Xamarin.Forms.ContentPage
    {
        public RecurrencePage()
        {
            InitializeComponent();
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            Populate();
        }

        private void Populate()
        {
            this.TypePicker.Items.Add(ResMan.GetString("DailyOption"));
            this.TypePicker.Items.Add(ResMan.GetString("WeeklyOption"));
            this.TypePicker.Items.Add(ResMan.GetString("MonthlyOption"));
            this.TypePicker.Items.Add(ResMan.GetString("YearlyOption"));

            this.DateModePicker.Items.Add(ResMan.GetString("RelativeDateOption"));
            this.DateModePicker.Items.Add(ResMan.GetString("AbsoluteDateOption"));
            this.DateModePicker2.Items.Add(ResMan.GetString("RelativeDateOption"));
            this.DateModePicker2.Items.Add(ResMan.GetString("AbsoluteDateOption"));

            foreach (var x in GetOrdinals())
            {
                this.Ordinals.Items.Add(x);
                this.Ordinals2.Items.Add(x);
            }

            foreach (var x in DateTimeUtils.GetDaysOfWeek())
            {
                this.DayOfWeekNames.Items.Add(x);
                this.DayOfWeekNames2.Items.Add(x);
            }

            foreach (var x in CultureInfo.CurrentCulture.DateTimeFormat.MonthNames)
            {
                this.MonthNames.Items.Add(x);
                this.MonthNames2.Items.Add(x);
            }

            this.EndOptions.Items.Add(ResMan.GetString("NoEndOption"));
            this.EndOptions.Items.Add(ResMan.GetString("EndAfterOption"));
            this.EndOptions.Items.Add(ResMan.GetString("EndByOption"));
        }

        private IEnumerable<string> GetOrdinals()
        {
            yield return ResMan.GetString("First");
            yield return ResMan.GetString("Second");
            yield return ResMan.GetString("Third");
            yield return ResMan.GetString("Fourth");
            yield return ResMan.GetString("Last");
        }
    }
}
