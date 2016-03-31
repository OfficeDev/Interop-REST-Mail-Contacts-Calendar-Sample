using MeetingManager.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace MeetingManager.Views
{
    public sealed partial class LogPage : Page
    {
        private IList<EventData> _items = new List<EventData>();

        public LogPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var _eventAggregator = (Application.Current as App).EventAggregator;

//            _eventAggregator.GetEvent<HttpEvent>().Subscribe(LogUpdated);
        }

        private async void LogUpdated(HttpEventData data)
        {
            string request = string.Format("{0:HH:mm:ss}  {1} {2}",
                    data.TimeStamp, data.Method, data.Uri);
            string response = data.StatusCode;

            await LogEntries.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    //                    ContentPresenter contentPresenter = FindVisualChild<ContentPresenter>(yourListBoxItem);
                    var template = LogEntries.Template;

                    LogEntries.Items.Add(request);
                    LogEntries.Items.Add(response);

                    var eventData = new EventData { Data = data };
                    _items.Add(eventData);
                    _items.Add(eventData);
                });
        }

        private void LogEntries_ItemToggled(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            var listView = sender as ListView;
            int index = listView.SelectedIndex;

            _items[index].IsOpened = !_items[index].IsOpened;
            //            var item = LogEntries.Items[index] as ListViewItem;
            //            LogEntries.Items[index] = string.Empty;
//            listView.Items.RemoveAt(index);
            string item;

            if (_items[index].IsOpened)
            {
                item = (index % 2 == 0) ? GetOpenedRequest(index) : GetOpenedResponse(index);
            }
            else
            {
                 item = (index % 2 == 0) ? GetClosedRequest(index) : GetClosedResponse(index);
            }
            //            listView.UpdateLayout();
//            LogEntries.Items.Insert(index, item);
            LogEntries.Items[index] = item;
        }

        private string GetOpenedRequest(int index)
        {
            var data = _items[index].Data;

            var body = string.Format("Body = {0}", data.RequestBody);

            return string.Format("{0}\n\t{1}", GetClosedRequest(index), body);
        }

        private string GetClosedRequest(int index)
        {
            var data = _items[index].Data;

            return string.Format("{0:HH:mm:ss}  {1} {2}",
                    data.TimeStamp, data.Method, data.Uri);
        }

        private string GetOpenedResponse(int index)
        {
            var data = _items[index].Data;

            var body = string.Format("Body = {0}", data.ResponseBody);

            return string.Format("{0}\n\t{1}", GetClosedResponse(index), body);
        }

        private string GetClosedResponse(int index)
        {
            var data = _items[index].Data;

            return data.StatusCode;
        }

        private class EventData
        {
            public bool IsOpened { get; set; }
            public HttpEventData Data { get; set; }
        }
    }
}
