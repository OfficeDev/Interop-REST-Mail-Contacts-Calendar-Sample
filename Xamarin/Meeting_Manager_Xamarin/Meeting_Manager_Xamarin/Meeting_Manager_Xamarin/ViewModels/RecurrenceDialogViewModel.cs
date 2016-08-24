//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using Meeting_Manager_Xamarin.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Meeting_Manager_Xamarin.ViewModels
{
    class RecurrenceDialogViewModel : BaseViewModel, ITransientViewModel
    {
        private const int TypeDaily = 0;
        private const int TypeWeekly = 1;
        private const int TypeMonthly = 2;
        private const int TypeYearly = 3;

        private const int OptionNoEnd = 0;
        private const int OptionNumbered = 1;
        private const int OptionEndDate = 2;

        private bool _dailyIsVisible;
        private bool _weeklyIsVisible;
        private bool _monthlyIsVisible;
        private bool _yearlyIsVisible;

        private int _selectedEndOption;

        private int _type;
        private Meeting.EventRecurrence _recurrence;

        private bool _everyNDays;
        private bool _everyWeekDay;
        private bool _absoluteMonthly;
        private bool _relativeeMonthly;
        private bool _absoluteYearly;
        private bool _relativeYearly;

        private bool[] _weekDays = new bool[7];

        private int _dailyInterval;
        private int _weeklyInterval;
        private int _monthlyInterval;
        private int _yearlyInterval;

        private int _dayOfMonth;
        private int _firstLastIndex;
        private int _dayOfWeekIndex;
        private int _monthIndex;

        private DateTime _startDate = DateTime.Now;
        private DateTime _endDate = DateTime.Now;

        private static string[] _dayOfWeekNames =
        {
            OData.Sunday, OData.Monday, OData.Tuesday, OData.Wednesday,
            OData.Thursday, OData.Friday, OData.Saturday
        };

        private static string[] _indexNames =
        {
            OData.First, OData.Second, OData.Third, OData.Fourth, OData.Last
        };

        public Command SubmitCommand => new Command(SubmitRecurrence);

        public bool CanSubmit => (this.Type != TypeWeekly) || _weekDays.Where(x => x).Any();

        public int OccurencesNumber
        {
            get { return _recurrence.Range.NumberOfOccurrences; }
            set
            {
                _recurrence.Range.NumberOfOccurrences = value;
                OnPropertyChanged();
            }
        }

        public int MonthIndex
        {
            get { return _monthIndex; }
            set { SetProperty(ref _monthIndex, value); }
        }

        public int FirstLastIndex
        {
            get { return _firstLastIndex; }
            set { SetProperty(ref _firstLastIndex, value); }
        }

        public DateTime StartDate
        {
            get { return _startDate; }
            set { SetProperty(ref _startDate, value); }
        }

        public DateTime EndDate
        {
            get { return _endDate; }
            set { SetProperty(ref _endDate, value); }
        }

        public int SelectedEndOption
        {
            get { return _selectedEndOption; }

            set
            {
                switch (value)
                {
                    case OptionNoEnd:
                        _recurrence.Range.Type = OData.NoEnd;
                        break;
                    case OptionNumbered:
                        _recurrence.Range.Type = OData.Numbered;
                        break;
                    case OptionEndDate:
                        _recurrence.Range.Type = OData.EndDate;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                SetProperty(ref _selectedEndOption, value);

                OnPropertyChanged(() => IsNoEnd);
                OnPropertyChanged(() => IsNumbered);
                OnPropertyChanged(() => IsEndBy);
            }
        }

        public bool IsNoEnd => SelectedEndOption == OptionNoEnd;

        public bool IsNumbered => SelectedEndOption == OptionNumbered;

        public bool IsEndBy => SelectedEndOption == OptionEndDate;

        public bool Sun_Toggle
        {
            get { return _weekDays[0]; }
            set { SetDayToggle(ref _weekDays[0], value); }
        }

        public bool Mon_Toggle
        {
            get { return _weekDays[1]; }
            set { SetDayToggle(ref _weekDays[1], value); }
        }

        public bool Tue_Toggle
        {
            get { return _weekDays[2]; }
            set { SetDayToggle(ref _weekDays[2], value); }
        }

        public bool Wed_Toggle
        {
            get { return _weekDays[3]; }
            set { SetDayToggle(ref _weekDays[3], value); }
        }

        public bool Thu_Toggle
        {
            get { return _weekDays[4]; }
            set { SetDayToggle(ref _weekDays[4], value); }
        }

        public bool Fri_Toggle
        {
            get { return _weekDays[5]; }
            set { SetDayToggle(ref _weekDays[5], value); }
        }

        public bool Sat_Toggle
        {
            get { return _weekDays[6]; }
            set { SetDayToggle(ref _weekDays[6], value); }
        }

        public int DayOfWeekIndex
        {
            get { return _dayOfWeekIndex; }
            set { SetProperty(ref _dayOfWeekIndex, value); }
        }

        public int DayOfMonth
        {
            get { return _dayOfMonth; }
            set { SetProperty(ref _dayOfMonth, value); }
        }

        public int DailyInterval
        {
            get { return _dailyInterval; }
            set { SetProperty(ref _dailyInterval, value); }
        }

        public int WeeklyInterval
        {
            get { return _weeklyInterval; }
            set { SetProperty(ref _weeklyInterval, value); }
        }

        public int MonthlyInterval
        {
            get { return _monthlyInterval; }
            set { SetProperty(ref _monthlyInterval, value); }
        }

        public int YearlyInterval
        {
            get { return _yearlyInterval; }
            set { SetProperty(ref _yearlyInterval, value); }
        }

        public bool EveryNDays
        {
            get { return _everyNDays; }
            set
            {
                if (value)
                {
                    _recurrence.Pattern.Type = OData.Daily;
                }
                SetProperty(ref _everyNDays, value);
            }
        }

        public bool EveryWeekDay
        {
            get { return _everyWeekDay; }
            set
            {
                if (value)
                {
                    _recurrence.Pattern.Type = OData.Weekly;
                    _type = TypeWeekly; // i.e. no notification

                    for (int i = 0; i < _weekDays.Length; ++i)
                    {
                        _weekDays[i] = true;
                    }
                }

                SetProperty(ref _everyWeekDay, value);
            }
        }

        public bool AbsoluteMonthly
        {
            get { return _absoluteMonthly; }
            set
            {
                if (value)
                {
                    _recurrence.Pattern.Type = OData.AbsoluteMonthly;
                }
                SetProperty(ref _absoluteMonthly, value);
            }
        }

        public bool RelativeMonthly
        {
            get { return _relativeeMonthly; }
            set
            {
                if (value)
                {
                    _recurrence.Pattern.Type = OData.RelativeMonthly;
                }
                SetProperty(ref _relativeeMonthly, value);
            }
        }

        public bool AbsoluteYearly
        {
            get { return _absoluteYearly; }
            set
            {
                if (value)
                {
                    _recurrence.Pattern.Type = OData.AbsoluteYearly;
                }
                SetProperty(ref _absoluteYearly, value);
            }
        }

        public bool RelativeYearly
        {
            get { return _relativeYearly; }
            set
            {
                if (value)
                {
                    _recurrence.Pattern.Type = OData.RelativeYearly;
                }
                SetProperty(ref _relativeYearly, value);
            }
        }

        public int Type
        {
            get { return _type; }
            set
            {
                DailyIsVisible = false;
                WeeklyIsVisible = false;
                MonthlyIsVisible = false;
                YearlyIsVisible = false;

                switch (value)
                {
                    case TypeDaily:
                        DailyIsVisible = true;
                        break;
                    case TypeWeekly:
                        WeeklyIsVisible = true;
                        break;
                    case TypeMonthly:
                        MonthlyIsVisible = true;
                        break;
                    case TypeYearly:
                        YearlyIsVisible = true;
                        break;
                }

                SetProperty(ref _type, value);
                OnPropertyChanged(() => CanSubmit);
            }
        }

        public bool DailyIsVisible
        {
            get { return _dailyIsVisible; }
            set { SetProperty(ref _dailyIsVisible, value); }
        }

        public bool WeeklyIsVisible
        {
            get { return _weeklyIsVisible; }
            set { SetProperty(ref _weeklyIsVisible, value); }
        }

        public bool MonthlyIsVisible
        {
            get { return _monthlyIsVisible; }
            set { SetProperty(ref _monthlyIsVisible, value); }
        }

        public bool YearlyIsVisible
        {
            get { return _yearlyIsVisible; }
            set { SetProperty(ref _yearlyIsVisible, value); }
        }

        protected override void OnNavigatedTo(object parameter)
        {
            _recurrence = JSON.Deserialize<Meeting.EventRecurrence>(parameter);
            PopulatePatternViews();

            switch (_recurrence.Range.Type.ToLower())
            {
                case OData.NoEnd:   SelectedEndOption = OptionNoEnd; break;
                case OData.Numbered:SelectedEndOption = OptionNumbered; break;
                case OData.EndDate: SelectedEndOption = OptionEndDate; break;
            }

            StartDate = DateTime.Parse(_recurrence.Range.StartDate);
            EndDate = DateTime.Parse(_recurrence.Range.EndDate);
        }

        private void PopulatePatternViews()
        {
            var pattern = _recurrence.Pattern;
            string patternType = pattern.Type;
            pattern.DaysOfWeek = pattern.DaysOfWeek ?? new List<string>();

            if (pattern.DaysOfWeek.Any())
            {
                DayOfWeekIndex = Array.IndexOf(_dayOfWeekNames, pattern.DaysOfWeek[0].ToLower());
            }
            else
            {
                DayOfWeekIndex = 0;
            }

            if (pattern.Index != null)
            {
                FirstLastIndex = Array.IndexOf(_indexNames, pattern.Index.ToLower());
            }
            else
            {
                FirstLastIndex = 0;
            }

            MonthIndex = 0;
            DayOfMonth = Math.Max(pattern.DayOfMonth, 1);
            DailyInterval = WeeklyInterval =
            MonthlyInterval = YearlyInterval = Math.Max(pattern.Interval, 1);

            _everyNDays = true;
            _absoluteMonthly = true;
            _absoluteYearly = true;

            switch (patternType.ToLower())
            {
                case OData.Daily:
                    Type = TypeDaily;
                    EveryNDays = true;
                    break;
                case OData.Weekly:
                    Type = TypeWeekly;

                    foreach (var x in pattern.DaysOfWeek)
                    {
                        _weekDays[Array.IndexOf(_dayOfWeekNames, x.ToLower())] = true;
                    }

                    break;
                case OData.RelativeMonthly:
                case OData.AbsoluteMonthly:
                    Type = TypeMonthly;

                    if (patternType.EqualsCaseInsensitive(OData.RelativeMonthly))
                    {
                        RelativeMonthly = true;
                    }
                    else
                    {
                        AbsoluteMonthly = true;
                    }
                    break;
                case OData.RelativeYearly:
                case OData.AbsoluteYearly:
                    Type = TypeYearly;

                    if (patternType.EqualsCaseInsensitive(OData.RelativeYearly))
                    {
                        RelativeYearly = true;
                    }
                    else
                    {
                        AbsoluteYearly = true;
                    }

                    MonthIndex = pattern.Month - 1;
                    break;
            }
        }

        private void SetDayToggle(ref bool toggle, bool value)
        {
            SetProperty(ref toggle, value);
            OnPropertyChanged(() => CanSubmit);
        }

        private void SubmitRecurrence()
        {
            var pattern = _recurrence.Pattern;
            pattern.DaysOfWeek = new List<string>();

            switch (Type)
            {
                case TypeDaily:
                    pattern.Type = OData.Daily;
                    pattern.Interval = _dailyInterval;
                    break;
                case TypeWeekly:
                    pattern.Type = OData.Weekly;
                    pattern.Interval = _weeklyInterval;

                    for (int i = 0; i < 7; ++i)
                    {
                        if (_weekDays[i])
                        {
                            pattern.DaysOfWeek.Add(_dayOfWeekNames[i]);
                        }
                    }
                    break;
                case TypeMonthly:
                    if (AbsoluteMonthly)
                    {
                        pattern.Type = OData.AbsoluteMonthly;
                        pattern.DayOfMonth = _dayOfMonth;
                    }
                    else
                    {
                        pattern.Type = OData.RelativeMonthly;
                        pattern.Index = _indexNames[_firstLastIndex];
                        pattern.DaysOfWeek.Add(_dayOfWeekNames[_dayOfWeekIndex]);
                    }
                    pattern.Interval = _monthlyInterval;
                    break;

                case TypeYearly:
                    if (AbsoluteYearly)
                    {
                        pattern.Type = OData.AbsoluteYearly;
                        pattern.DayOfMonth = _dayOfMonth;
                    }
                    else
                    {
                        pattern.Type = OData.RelativeYearly;
                        pattern.Index = _indexNames[_firstLastIndex];
                        pattern.DaysOfWeek.Add(_dayOfWeekNames[_dayOfWeekIndex]);
                    }
                    pattern.Month = MonthIndex + 1;
                    pattern.Interval = _yearlyInterval;
                    break;
            }

            _recurrence.Range.StartDate = StartDate.DateToApiString();
            _recurrence.Range.EndDate = EndDate.DateToApiString();

            UI.Publish(_recurrence);
            GoBack();
        }
    }
}
