using MeetingManager.Models;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Windows.AppModel;
using Prism.Windows.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;

namespace MeetingManager.ViewModels
{
    public class RecurrencePageViewModel : ViewModel
    {
        private const string TypeDaily = "daily";
        private const string TypeWeekly = "weekly";
        private const string TypeMonthly = "monthly";
        private const string TypeYearly = "yearly";

        private Visibility _dailyVisibility;
        private Visibility _weeklyVisibility;
        private Visibility _monthlyVisibility;
        private Visibility _yearlyVisibility;

        private string _type;
        private Meeting.EventRecurrence _recurrence;

        private bool _everyNDays;
        private bool _everyWeekDay;
        private bool _absoluteMonthly;
        private bool _relativeeMonthly;
        private bool _absoluteYearly;
        private bool _relativeYearly;

        private bool _isNoEnd;
        private bool _isNumbered;
        private bool _isEndBy;

        private bool[] _weekDays = new bool[7];

        private int _dailyInterval;
        private int _weeklyInterval;
        private int _monthlyInterval;
        private int _yearlyInterval;

        private int _dayOfMonth;
        private int _firstLastIndex;
        private int _dayOfWeekIndex;
        private int _monthIndex;

        private DateTimeOffset _startDate = DateTimeOffset.Now;
        private DateTimeOffset _endDate = DateTimeOffset.Now;

        private static string[] _dayOfWeekNames =
        {
            OData.Sunday, OData.Monday, OData.Tuesday, OData.Wednesday,
            OData.Thursday, OData.Friday, OData.Saturday
        };

        private static string[] _indexNames =
        {
            OData.First, OData.Second, OData.Third, OData.Fourth, OData.Last
        };

        public RecurrencePageViewModel()
        {
            this.SaveCommand = new DelegateCommand(SaveRecurrence);
        }

        public DelegateCommand SaveCommand { get; private set; }

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

        public IEnumerable<string> MonthNames
        {
            get { return CultureInfo.CurrentCulture.DateTimeFormat.MonthNames; }
        }

        public IEnumerable<string> DayOfWeekNames
        {
            get { return DateTimeUtils.GetDaysOfWeek(); }
        }

        public IEnumerable<string> Ordinals
        {
            get { return GetOrdinals(); }
        }

        public int FirstLastIndex
        {
            get { return _firstLastIndex; }
            set { SetProperty(ref _firstLastIndex, value); }
        }

        public DateTimeOffset StartDate
        {
            get { return _startDate; }
            set { SetProperty(ref _startDate, value); }
        }

        public DateTimeOffset EndDate
        {
            get { return _endDate; }
            set { SetProperty(ref _endDate, value); }
        }

        public bool IsNoEnd
        {
            get { return _isNoEnd; }
            set
            {
                if (value)
                {
                    _recurrence.Range.Type = OData.NoEnd;
                }
                SetProperty(ref _isNoEnd, value);
            }
        }

        public bool IsNumbered
        {
            get { return _isNumbered; }
            set
            {
                if (value)
                {
                    _recurrence.Range.Type = OData.Numbered;
                }
                SetProperty(ref _isNumbered, value);
            }
        }

        public bool IsEndBy
        {
            get { return _isEndBy; }
            set
            {
                if (value)
                {
                    _recurrence.Range.Type = OData.EndDate;
                }
                SetProperty(ref _isEndBy, value);
            }
        }

        public bool[] WeekDays
        {
            get { return _weekDays; }
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

        public string Type
        {
            get { return _type; }
            set
            {
                DailyVisibility = Visibility.Collapsed;
                WeeklyVisibility = Visibility.Collapsed;
                MonthlyVisibility = Visibility.Collapsed;
                YearlyVisibility = Visibility.Collapsed;

                switch (value.ToLower())
                {
                    case TypeDaily:
                        DailyVisibility = Visibility.Visible;
                        break;
                    case TypeWeekly:
                        WeeklyVisibility = Visibility.Visible;
                        break;
                    case TypeMonthly:
                        MonthlyVisibility = Visibility.Visible;
//                        OnPropertyChanged(() => DayOfWeekIndex);
                        break;
                    case TypeYearly:
                        YearlyVisibility = Visibility.Visible;
//                        OnPropertyChanged(() => DayOfWeekIndex);
                        break;
                }

                SetProperty(ref _type, value);
            }
        }

        public Visibility DailyVisibility
        {
            get { return _dailyVisibility; }
            set { SetProperty(ref _dailyVisibility, value); }
        }

        public Visibility WeeklyVisibility
        {
            get { return _weeklyVisibility; }
            set { SetProperty(ref _weeklyVisibility, value); }
        }

        public Visibility MonthlyVisibility
        {
            get { return _monthlyVisibility; }
            set { SetProperty(ref _monthlyVisibility, value); }
        }

        public Visibility YearlyVisibility
        {
            get { return _yearlyVisibility; }
            set { SetProperty(ref _yearlyVisibility, value); }
        }

        public override void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            base.OnNavigatedTo(e, viewModelState);

            _recurrence = JsonConvert.DeserializeObject<Meeting.EventRecurrence>((string)e.Parameter);

            PopulatePatternViews();

            switch (_recurrence.Range.Type.ToLower())
            {
                case OData.NoEnd:    IsNoEnd = true; break;
                case OData.Numbered: IsNumbered = true; break;
                case OData.EndDate:  IsEndBy = true; break;
            }

            StartDate = DateTimeOffset.Parse(_recurrence.Range.StartDate);
            EndDate = DateTime.Parse(_recurrence.Range.EndDate);
        }

        private IEnumerable<string> GetOrdinals()
        {
            var loader = new ResourceLoaderAdapter(new ResourceLoader());
            yield return loader.GetString("First");
            yield return loader.GetString("Second");
            yield return loader.GetString("Third");
            yield return loader.GetString("Fourth");
            yield return loader.GetString("Last");
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
//                    DailyInterval = pattern.Interval;
                    break;
                case OData.Weekly:
                    Type = TypeWeekly;
//                    WeeklyInterval = pattern.Interval;

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

//                    MonthlyInterval = pattern.Interval;
                    //if (pattern.Index != null)
                    //{
                    //    FirstLastIndex = Array.IndexOf(_indexNames, pattern.Index.ToLower());
                    //}

                    //if (pattern.DaysOfWeek.Any())
                    //{
                    //    DayOfWeekIndex = Array.IndexOf(_dayOfWeekNames, pattern.DaysOfWeek[0].ToLower());
                    //}
//                    DayOfMonth = pattern.DayOfMonth;
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

//                    YearlyInterval = pattern.Interval;
                    //if (pattern.Index != null)
                    //{
                    //    FirstLastIndex = Array.IndexOf(_indexNames, pattern.Index.ToLower());
                    //}
                    //if (pattern.DaysOfWeek.Any())
                    //{
                    //    DayOfWeekIndex = Array.IndexOf(_dayOfWeekNames, pattern.DaysOfWeek[0].ToLower());
                    //}
                    MonthIndex = pattern.Month - 1;
//                    DayOfMonth = pattern.DayOfMonth;
                    break;
            }
        }

        private void SaveRecurrence()
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

            //            _recurrence.Range.StartDate = string.Format("{0:yyyy-MM-dd}", StartDate);
            _recurrence.Range.StartDate = StartDate.DateTime.DateToApiString();

            //            if (_recurrence.Range.Type.EqualsCaseInsensitive(OData.EndDate))
            {
                _recurrence.Range.EndDate = string.Format("{0:yyyy-MM-dd}", EndDate);
            }

            GetEvent<MeetingRecurrenceUpdatedEvent>().Publish(_recurrence);

            GoBack();
        }
    }
}
