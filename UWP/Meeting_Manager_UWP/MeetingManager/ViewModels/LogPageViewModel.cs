using MeetingManager.Models;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace MeetingManager.ViewModels
{
    class LogPageViewModel : ViewModel
    {
        private CoreDispatcher _dispatcher;

        public LogPageViewModel()
        {
            var _eventAggregator = (Application.Current as App).EventAggregator;
            _eventAggregator.GetEvent<HttpEvent>().Subscribe(LogUpdated);

            LogEntries = new ObservableCollection<EventData>();

            if (!Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                _dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
            }

            ItemToggleCommand = new DelegateCommand(ToggleItem);
        }

        public DelegateCommand ItemToggleCommand { get; private set; }

        public ObservableCollection<EventData> LogEntries { get; private set; }
        public EventData SelectedItem { get; set; }

        private async void LogUpdated(HttpEventData data)
        {
            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                LogEntries.Add(new EventData { Data = data });
            });
        }

        private void ToggleItem()
        {
            var item = SelectedItem;

            if (item != null)
            {
                item.IsOpened = !item.IsOpened;
                item.NotifyPropertyChanged("Text");
            }
        }

        public class EventData : INotifyPropertyChanged
        {
            public bool IsOpened { get; set; }

            public string Text
            {
                get
                {
                    string request = IsOpened ? GetOpenedRequest() : GetClosedRequest();

                    string response = IsOpened ? GetOpenedResponse() : GetClosedResponse();

                    return string.Format("{0}\n{1}", request, response);
                }
            }

            public HttpEventData Data { get; set; }

            public event PropertyChangedEventHandler PropertyChanged;

            public void NotifyPropertyChanged(String propertyName)
            {
                this.Notify(PropertyChanged, propertyName);
            }

            private string GetOpenedRequest()
            {
                var body = string.Format("Body = {0}", Data.RequestBody);

                return string.Format("{0}\n\t{1}", GetClosedRequest(), body);
            }

            private string GetClosedRequest()
            {
                return string.Format("{0:HH:mm:ss}  {1} {2}",
                        Data.TimeStamp, Data.Method, Data.Uri);
            }

            private string GetOpenedResponse()
            {
                var body = string.Format("Body = {0}", Data.ResponseBody);

                return string.Format("{0}\n\t{1}", GetClosedResponse(), body);
            }

            private string GetClosedResponse()
            {
                return Data.StatusCode;
            }
        }
    }
}
