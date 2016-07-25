//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using Meeting_Manager_Xamarin.Models;
using System;
using System.Collections.Generic;
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

        protected async Task NavigateTo(string pageToken, object data = null)
        {
            using (new Loading(this))
            {
                await UI.NavigateTo(pageToken, data);
            }
        }

        protected static async void NavigateToContacts()
        {
            await UI.NavigateTo("Contacts");
        }

        private static async void NavigateToHumans()
        {
            await NavigateToUsers(true);
        }

        protected static async Task NavigateToUsers(bool getHumans)
        {
            await UI.NavigateTo("Users", getHumans);
        }

        protected async Task SendRunningLate(Meeting meeting)
        {
            await NavigateToEmail(meeting, OData.ReplyAll, GetString("RunningLate"));
        }

        protected static string GetString(string id)
        {
            return ResMan.GetString(id);
        }

        protected void Publish<T>(T data)
        {
            MessagingCenter.Send<object, T>(this, typeof(T).Name, data);
        }

        protected void Subscribe<T>(Action<object, T> action)
        {
            MessagingCenter.Subscribe<object, T>(this, typeof(T).Name, action);
        }

        protected async Task NavigateToEmail(Meeting meeting, string action, string comment = null)
        {
            using (new Loading(this))
            {
                var response = await GraphService.CreateInvitationResponse(meeting, action);

                if (response != null)
                {
                    var parameter = Tuple.Create(response, action, comment);
                    await UI.NavigateTo("Email", parameter);
                }
                else
                {
                    await UI.DisplayAlert("Alert", GetString("CantReply"), GetString("OKCaption"));
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

        protected static async void AddUserOrContact()
        {
            await UI.DisplayAndExecuteAction(GetString("UserOrContactCaption"), new Dictionary<string, Action>
            {
                [GetString("AddressListOption")] = NavigateToHumans,
                [GetString("ContactsOption")] = NavigateToContacts,
            });
        }

        public virtual void OnAppearing(object data)
        {
        }

        public virtual void OnDisappearing()
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

        public virtual bool SetProperty<T>(
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
