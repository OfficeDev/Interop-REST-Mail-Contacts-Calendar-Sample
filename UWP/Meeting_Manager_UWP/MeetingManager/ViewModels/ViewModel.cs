using MeetingManager.Models;
using Newtonsoft.Json;
using Prism.Events;
using Prism.Windows.Mvvm;
using Prism.Windows.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MeetingManager.ViewModels
{
    public class ViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;
        private readonly IGraphService _officeService;
        private readonly IEventAggregator _eventAggregator;
        protected readonly IAuthenticationService _authenticationService;
        private bool _isLoading;

        protected IGraphService OfficeService
        {
            get  { return _officeService; } 
        }

        protected ViewModel()
        {
            _navigationService = (Application.Current as App).NavigationService;
            _eventAggregator = (Application.Current as App).EventAggregator;
            _officeService = (Application.Current as App).OfficeService;
            _authenticationService = (Application.Current as App).AuthenticationService;
        }

        public bool IsLoading
        {
            get { return _isLoading; }
            set { SetProperty(ref _isLoading, value); }
        }

        private void NavigateToPage(string pageToken, object parameter=null)
        {
            _navigationService.Navigate(pageToken, SerializeParameter(parameter));
        }

        public override void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            base.OnNavigatedTo(e, viewModelState);

            GetEvent<HttpEvent>().Publish(null);
        }

        protected void GoBack()
        {
            _navigationService.GoBack();
        }

        protected TEvent GetEvent<TEvent>() where TEvent : EventBase, new()
        {
            return _eventAggregator.GetEvent<TEvent>();
        }

        protected string Serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        protected T Deserialize<T>(object parameter)
        {
            return JsonConvert.DeserializeObject<T>((string) parameter);
        }

        protected async Task<bool> YesNoDialog(string message)
        {
            var messageDialog = new MessageDialog(message);

            messageDialog.Commands.Add(new UICommand
            {
                Label = GetString("YesCaption"),
                Id = 0
            });

            messageDialog.Commands.Add(new UICommand
            {
                Label = GetString("NoCaption"),
                Id = 1
            });

            messageDialog.DefaultCommandIndex = 0;
            messageDialog.CancelCommandIndex = 1;

            var result = await messageDialog.ShowAsync();

            return (int)result.Id == 0;
        }

        private async Task MessageDialog(string message)
        {
            var messageDialog = new MessageDialog(message);

            messageDialog.Commands.Add(new UICommand
            {
                Label = GetString("OKCaption"),
                Id = 0
            });

            messageDialog.DefaultCommandIndex = 0;

            await messageDialog.ShowAsync();
        }

        protected async Task NavigateToEmail(Meeting meeting, string action, string comment = null)
        {
            using (new Loading(this))
            {
                var response = await OfficeService.CreateInvitationResponse(meeting, action);

                if (response != null)
                {
                    var parameter = Tuple.Create(response, action, comment);
                    await NavigateTo("Email", parameter);
                }
                else
                {
                    await MessageDialog(GetString("CantReply"));
                }
            }
        }

        protected async Task SendRunningLate(Meeting meeting)
        {
            await NavigateToEmail(meeting, OData.ReplyAll, GetString("RunningLate"));
        }

        protected async Task NavigateTo(string pageToken, object parameter = null)
        {
            var typeName = GetType().Namespace;

            typeName = typeName.Replace("ViewModels", "Views");
            typeName += "." + pageToken + "Dialog";

            var type = Type.GetType(typeName);

            if (type == null)
            {
                NavigateToPage(pageToken, parameter);
            }
            else
            {
                var dlg = Activator.CreateInstance(type) as ContentDialog;

                GetEvent<InitDialogEvent>().Publish(SerializeParameter(parameter));
                await dlg.ShowAsync();
            }
        }

        private object SerializeParameter(object parameter)
        {
            if (parameter != null)
            {
                if (!(parameter is string) && !(parameter is bool))
                {
                    parameter = JsonConvert.SerializeObject(parameter);
                }
            }
            return parameter;
        }

        protected async Task NavigateToContacts()
        {
            await NavigateTo("Contacts");
        }

        protected async Task NavigateToUsers(bool getHumans)
        {
            await NavigateTo("Users", getHumans);
        }

        protected string GetString(string id)
        {
            return ResMan.GetString(id);
        }

        protected async Task<IEnumerable<MeetingTimeCandidate>> GetAllTimeCandidates(Meeting meeting)
        {
            using (new Loading(this))
            {
                return (await GetTimeCandidates(meeting, "8:00:00", "11:00:00"))
                    .Union(await GetTimeCandidates(meeting, "11:00:00", "15:00:00"))
                    .Union(await GetTimeCandidates(meeting, "15:00:00", "18:00:00"));
            }
        }

        private async Task<IEnumerable<MeetingTimeCandidate>> GetTimeCandidates(Meeting meeting, string startTime, string endTime)
        {
            return await OfficeService.GetMeetingTimeCandidates(meeting, startTime, endTime);
        }
    }
}
