//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using Meeting_Manager_Xamarin.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms;

namespace Meeting_Manager_Xamarin.ViewModels
{
    class EmailPageViewModel : BaseViewModel
    {
        private bool _mailHasBeenSent;

        public EmailPageViewModel()
        {
            UI.Subscribe<User>(UserSelected);
            UI.Subscribe<Contact>(ContactSelected);
        }

        public Command SendCommand => new Command(SendMail, () => Recipients.Any());
        public Command AddUserCommand => new Command(() => NavigateToUsers(true));
        public Command AddContactCommand => new Command(() => NavigateToContacts());
        public Command<Message.Recipient> DeleteRecipientCommand => new Command<Message.Recipient>(DeleteRecipient);

        public EventMessage Message { get; private set; }

        public string Title { get; private set; }

        public string Comment { get; set; }

        public ObservableCollection<Message.Recipient> Recipients { get; private set; }

        public bool IsContentText { get; private set; }

        protected override void OnNavigatedTo(object parameter)
        {
            var tuple = JSON.Deserialize<Tuple<EventMessage, string, string>>(parameter);

            Message = tuple.Item1;
            var action = tuple.Item2.ToLower();
            Comment = tuple.Item3;

            _mailHasBeenSent = false;

            IsContentText = Message.Body.ContentType.EqualsCaseInsensitive("text");

            SetTitle(action);
            Recipients = new ObservableCollection<Message.Recipient>(Message.ToRecipients);

            OnPropertyChanged(() => Message);
            OnPropertyChanged(() => Recipients);
        }

        protected override async void OnNavigatingFrom()
        {
            if (!_mailHasBeenSent)
            {
                using (new Loading(this))
                {
                    await GraphService.DeleteDraftMessage(Message.Id);
                }
            }
        }

        private void SetTitle(string action)
        {
            switch (action)
            {
                case OData.Reply:
                    Title = GetString("ReplyTitle");
                    break;
                case OData.ReplyAll:
                    Title = GetString("ReplyAllTitle");
                    break;
                case OData.Forward:
                    Title = GetString("ForwardTitle");
                    break;
            }
            OnPropertyChanged(() => Title);
        }

        private async void SendMail()
        {
            Message.Body.Content = Comment + Message.Body.Content;

            if (Recipients != null)
            {
                Message.ToRecipients = new List<Message.Recipient>(Recipients);
            }

            using (new Loading(this))
            {
                _mailHasBeenSent = await GraphService.UpdateAndSendMessage(Message);
            }
            GoBack();
        }

        private void DeleteRecipient(Message.Recipient recipient)
        {
            if (recipient == null) return;

            int pos = Recipients.IndexOf(x => x.EmailAddress.IsEqualTo(recipient.EmailAddress));
            Recipients.RemoveAt(pos);
            OnPropertyChanged(() => SendCommand);
        }

        private void UserSelected(User user)
        {
            var recipient = new Message.Recipient
            {
                EmailAddress = new EmailAddress
                {
                    Address = user.UserPrincipalName,
                    Name = user.DisplayName
                }
            };

            AddRecipient(recipient);
        }

        private void ContactSelected(Contact contact)
        {
            AddRecipient(new Message.Recipient
            {
                EmailAddress = contact.EmailAddresses[0]
            });
        }

        private void AddRecipient(Message.Recipient recipient)
        {
            if (!Recipients.Any(x => x.EmailAddress.IsEqualTo(recipient.EmailAddress)))
            {
                Recipients.Add(recipient);
                OnPropertyChanged(() => SendCommand);
            }
        }
    }
}
