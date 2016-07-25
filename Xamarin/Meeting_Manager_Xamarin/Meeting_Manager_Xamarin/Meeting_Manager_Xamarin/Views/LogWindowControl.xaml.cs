//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using Meeting_Manager_Xamarin.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

using Xamarin.Forms;

namespace Meeting_Manager_Xamarin.Views
{
    public partial class LogWindowControl : Grid
    {
        private static ObservableEvents _items = new ObservableEvents();
        private static bool _firstInstance = true;

        private bool _initializing = true;

        public LogWindowControl()
        {
            InitializeComponent();

            SizeChanged += OnSizeChanged;

            if (_firstInstance)
            {
                _firstInstance = false;

                this.Subscribe<HttpEventData>(LogHttpEvent);
                this.Subscribe<LoginEventData>(LogLoginEvent);
            }

            LogEntries.ItemsSource = _items;
            NotifyLogWindow();
        }

        private void ToggleItem()
        {
            var item = LogEntries.SelectedItem;

            if (item != null)
            {
                var data = item as EventData;
                data.IsOpened = !data.IsOpened;

                data.NotifyPropertyChanged("Text");
                data.NotifyPropertyChanged(string.Empty);

//              It appears that it's not enough just to change the
//              ViewCell/Label Text property
                int pos = _items.IndexOf(data);

                _items.SetItemAt(pos, data);
            }
        }

        private void OnItemTapped(object sender, EventArgs e)
        {
            ToggleItem();
        }

        private async void OnShowJson(object sender, EventArgs e)
        {
            await App.Me.PushAsync(new JsonPage());

            var item = LogEntries.SelectedItem as EventData;

            if (item == null) return;

            if (item.Data != null)
            {
                this.Publish(
                        new JsonData { Payload = item.IsRequest ?
                                        item.Data.RequestBody : item.Data.ResponseBody });
            }
            else
            {
                this.Publish(new JsonData { Payload = item.Text });
            }
        }

        private async void OnMaximize(object sender, EventArgs e)
        {
            await App.Me.PushAsync(new LogWindowPage());
        }

        private void OnSizeChanged(object sender, EventArgs e)
        {
            if (Parent is ContentPage)
            {
                Maximize.IsEnabled = false;
            }
            else if (_initializing)
            {
                _initializing = false;
                Element parent = this;

                while (parent != null && !(parent is ContentPage))
                {
                    parent = parent.Parent;
                }

                if (parent != null)
                {
                    var mainGrid = parent.FindByName<Grid>("MainGrid");

                    if (mainGrid != null && mainGrid.RowDefinitions.Count == 2)
                    {
                        mainGrid.RowDefinitions[0].Height = new GridLength(3, GridUnitType.Star);

                        var logWindowHeight = (parent as VisualElement).Height < 700 ? 0 : 1;

                        mainGrid.RowDefinitions[1].Height = new GridLength(logWindowHeight, GridUnitType.Star);
                    }
                }
            }
        }

        private void LogLoginEvent(object sender, LoginEventData data)
        {
            if (data != null)
            {
                string loginNavigation = $"Navigated to {data.Url}";
                string authCode = $"Received authorization code: {data.AuthCode}";

                // These items should be the very first in the log
                _items.Insert(0, new EventData { StaticText = authCode, IsRequest = false });
                _items.Insert(0, new EventData { StaticText = loginNavigation, IsRequest = true });

                NotifyLogWindow();
            }
        }

        private void LogHttpEvent(object sender, HttpEventData data)
        {
            if (data != null)
            {
                _items.Add(new EventData { Data = data, IsRequest = true });
                _items.Add(new EventData { Data = data, IsRequest = false });

                NotifyLogWindow();
            }
        }

        private void NotifyLogWindow()
        {
            if (_items.Any())
            {
                LogEntries.ScrollTo(_items.Last(), ScrollToPosition.End, false);
            }
        }

        private class EventData : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            public bool IsOpened { get; set; }
            public bool IsRequest { get; set; }

            public string Text
            {
                get
                {
                    if (Data == null)
                    {
                        return StaticText;
                    }
                    else if (IsRequest)
                    {
                        return IsOpened ? GetOpenedRequest() : GetClosedRequest();
                    }
                    else
                    {
                        return IsOpened ? GetOpenedResponse() : GetClosedResponse();
                    }
                }
            }

            public string StaticText { get; set; }

            public HttpEventData Data { get; set; }

            public void NotifyPropertyChanged(string propertyName)
            {
                this.Notify(PropertyChanged, propertyName);
            }

            private string GetOpenedRequest()
            {
                var body = string.Format(" Body = <{0}>\r\n Headers = {1}", Data.RequestBody, Data.RequestHeaders);

                return string.Format("{0}\r\n{1}", GetClosedRequest(), body);
            }

            private string GetClosedRequest()
            {
                return string.Format("{0:HH:mm:ss}  {1} {2}", Data.TimeStamp, Data.Method, Data.Uri);
            }

            private string GetOpenedResponse()
            {
                var body = string.Format(" Body = <{0}>\r\n Headers = {1}", Data.ResponseBody, Data.ResponseHeaders);

                return string.Format("{0}\r\n{1}", GetClosedResponse(), body);
            }

            private string GetClosedResponse()
            {
                return Data.StatusCode;
            }
        }

        private class ObservableEvents : ObservableCollection<EventData>
        {
            public void SetItemAt(int index, EventData data)
            {
                base.SetItem(index, data);
            }
        }
    }
}
