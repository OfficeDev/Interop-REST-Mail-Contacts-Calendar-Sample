//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using MeetingManager.Models;
using Prism.Events;
using Prism.Windows.Mvvm;
using Prism.Windows.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MeetingManager.ViewModels
{
    public class ViewModel : ViewModelBase
    {
        private static readonly Lazy<IGraphService> _officeService =
                            new Lazy<IGraphService>(() => App.Me.GetGraphService());
        private bool _isLoading;

        protected IGraphService OfficeService => _officeService.Value;

        public bool IsLoading
        {
            get { return _isLoading; }
            set { SetProperty(ref _isLoading, value); }
        }

        public override void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            base.OnNavigatedTo(e, viewModelState);

            GetEvent<HttpEvent>().Publish(null);
        }

        protected static TEvent GetEvent<TEvent>() where TEvent : EventBase, new()
        {
            return UI.GetEvent<TEvent>();
        }

        protected async Task NavigateToEmail(Meeting meeting, string action, string comment = null)
        {
            using (new Loading(this))
            {
                var response = await OfficeService.CreateInvitationResponse(meeting, action);

                if (response != null)
                {
                    var parameter = Tuple.Create(response, action, comment);
                    await UI.NavigateTo("Email", parameter);
                }
                else
                {
                    await UI.MessageDialog(GetString("CantReply"));
                }
            }
        }

        protected async Task SendRunningLate(Meeting meeting)
        {
            await NavigateToEmail(meeting, OData.ReplyAll, GetString("RunningLate"));
        }

        protected async Task NavigateToContacts()
        {
            await UI.NavigateTo("Contacts");
        }

        protected async Task NavigateToUsers(bool getHumans)
        {
            await UI.NavigateTo("Users", getHumans);
        }

        protected static string GetString(string id)
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
