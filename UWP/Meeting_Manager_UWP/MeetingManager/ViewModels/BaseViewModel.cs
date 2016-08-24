//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using MeetingManager.Models;
using Prism.Windows.Mvvm;
using Prism.Windows.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.UI.Xaml.Navigation;

namespace MeetingManager.ViewModels
{
    public abstract class BaseViewModel : ViewModelBase
    {
        private bool _isLoading;

        protected IGraphService GraphService => App.Me.GraphService;

        public bool IsLoading
        {
            get { return _isLoading; }
            set { SetProperty(ref _isLoading, value); }
        }

        // This only exists for unity with Xamarin version of view models
        protected bool SetCollectionProperty<T>(
            ref ObservableCollection<T> storage,
            ObservableCollection<T> value,
            [CallerMemberName] string propertyName = null
            )
        {
            return SetProperty(ref storage, value, propertyName);
        }

        public override void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            base.OnNavigatedTo(e, viewModelState);

            if (e.NavigationMode == NavigationMode.New)
            {
                OnNavigatedTo(e.Parameter);
            }

            UI.Publish<HttpEventData>(null);
        }

        public override void OnNavigatingFrom(NavigatingFromEventArgs e, Dictionary<string, object> viewModelState, bool suspending)
        {
            OnNavigatingFrom();
        }

        protected virtual void OnNavigatedTo(object data) { }

        protected virtual void OnNavigatingFrom() { }

        protected virtual void GoBack()
        {
            UI.GoBack();
        }

        protected async void NavigateToEmail(Meeting meeting, string action, string comment = null)
        {
            using (new Loading(this))
            {
                var response = await GraphService.CreateInvitationResponse(meeting, action);

                if (response != null)
                {
                    var parameter = Tuple.Create(response, action, comment);
                    UI.NavigateTo("Email", parameter);
                }
                else
                {
                    await UI.MessageDialog(GetString("CantReply"));
                }
            }
        }

        protected void SendRunningLate(Meeting meeting)
        {
            NavigateToEmail(meeting, OData.ReplyAll, GetString("RunningLate"));
        }

        protected void NavigateToContacts()
        {
            UI.NavigateTo("Contacts");
        }

        protected void NavigateToUsers(bool getHumans)
        {
            UI.NavigateTo("Users", getHumans);
        }

        protected void NavigateToAttachments(IEnumerable<FileAttachment> attachments, Meeting meeting)
        {
            UI.NavigateTo("Attachments", Tuple.Create(attachments, meeting.Id, meeting.IsOrganizer));
        }

        protected static string GetString(string id)
        {
            return ResMan.GetString(id);
        }

        protected async Task<IEnumerable<MeetingTimeCandidate>> GetAllTimeCandidates(Meeting meeting)
        {
            using (new Loading(this))
            {
                return await GraphService.FindMeetingTimes(meeting);
            }
        }
    }
}
