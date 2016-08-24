//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using Meeting_Manager_Xamarin.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Meeting_Manager_Xamarin.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        private bool _isLoading;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsLoading
        {
            get { return _isLoading; }
            set { SetProperty(ref _isLoading, value); }
        }

        protected IGraphService GraphService => App.Me.GraphService;

        protected static void NavigateToContacts()
        {
            UI.NavigateTo("Contacts");
        }

        protected static void NavigateToUsers(bool getHumans)
        {
            UI.NavigateTo("Users", getHumans);
        }

        protected void SendRunningLate(Meeting meeting)
        {
            NavigateToEmail(meeting, OData.ReplyAll, GetString("RunningLate"));
        }

        protected void NavigateToAttachments(IEnumerable<FileAttachment> attachments, Meeting meeting)
        {
            UI.NavigateTo("Attachments", Tuple.Create(attachments, meeting.Id, meeting.IsOrganizer));
        }

        protected static string GetString(string id)
        {
            return ResMan.GetString(id);
        }

        protected static void GoBack()
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

        protected async Task<IEnumerable<MeetingTimeCandidate>> GetAllTimeCandidates(Meeting meeting)
        {
            using (new Loading(this))
            {
                return await GraphService.FindMeetingTimes(meeting);
            }
        }

        public void NavigateTo(object parameter)
        {
            OnNavigatedTo(parameter);
        }

        protected virtual void OnNavigatedTo(object parameter)
        {
        }

        public void NavigateFrom()
        {
            OnNavigatingFrom();
        }

        protected virtual void OnNavigatingFrom()
        {
        }

        public void NotifyPropertyChanged(string propertyName)
        {
            var handler = this.PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
            else
            {
//                Debug.WriteLine("No PropertyChanged handler for " + propertyName);
            }
        }

        protected bool SetCollectionProperty<T>(
            ref ObservableCollection<T> storage,
            ObservableCollection<T> value,
            [CallerMemberName] string propertyName = null
            )
        {
            if (object.Equals(storage, value))
            {
                return false;
            }

            // Work around Xamarin bug
            UI.UpdateObservableCollection(ref storage, value);
            this.NotifyPropertyChanged(propertyName);

            return true;
        }

        protected bool SetProperty<T>(
            ref T storage,
            T value,
            [CallerMemberName] string propertyName = null)
        {
            if (object.Equals(storage, value))
            {
                return false;
            }

            storage = value;
            this.NotifyPropertyChanged(propertyName);

            return true;
        }

        protected static string DeriveMemberName(Expression<Func<object>> propertyExpression)
        {
            if (propertyExpression != null)
            {
                var body = propertyExpression.Body;

                var unaryExpression = body as UnaryExpression;

                if (unaryExpression != null)
                {
                    body = unaryExpression.Operand;
                }

                var memberExpression = body as MemberExpression;

                if (memberExpression != null)
                {
                    return memberExpression.Member.Name;
                }

                var constantExpression = body as ConstantExpression;

                if (constantExpression != null)
                {
                    return null;
                }
            }

            throw new FormatException("The specified expression is not a property expression: () => this.PropertyName");
        }

        protected void NotifyPropertyChanged(Expression<Func<object>> propertyExpression)
        {
            var propertyName = DeriveMemberName(propertyExpression);

            this.NotifyPropertyChanged(propertyName);
        }

        protected void OnPropertyChanged(Expression<Func<object>> propertyExpression)
        {
            NotifyPropertyChanged(propertyExpression);
        }

        public void OnPropertyChanged(string propertyName="")
        {
            NotifyPropertyChanged(propertyName);
        }
    }
}
