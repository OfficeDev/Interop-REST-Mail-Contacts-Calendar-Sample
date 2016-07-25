//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using Meeting_Manager_Xamarin.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Meeting_Manager_Xamarin.ViewModels
{
    class EmailPageViewModel : BaseViewModel
    {
        private EventMessage _message;
        private string _comment;
        private bool _mailHasBeenSent;
        private ObservableCollection<Message.Recipient> _recipients;

        public EmailPageViewModel()
        {
            Subscribe<User>(UserSelected);
            Subscribe<Contact>(ContactSelected);
        }

        public Command SendCommand => new Command(SendMail);
        public Command AddRecipientCommand => new Command(AddRecipient);
        public Command<Message.Recipient> DeleteRecipientCommand => new Command<Message.Recipient>(DeleteRecipient);

        public EventMessage Message
        {
            get { return _message; }
            private set { SetProperty(ref _message, value); }
        }

        public string Title { get; private set; }

        public string Comment
        {
            get { return _comment; }
            set { SetProperty(ref _comment, value); }
        }

        public ObservableCollection<Message.Recipient> Recipients
        {
            get { return _recipients; }
            private set { SetProperty(ref _recipients, value); }
        }

        public bool IsContentText { get; set; }

        public HtmlWebViewSource HtmlSource
        {
            get
            {
                return new HtmlWebViewSource()
                {
                    Html = _message.Body.Content ?? String.Empty
                };
            }
        }

        public string Description => _message.Body.Content; 

        public override void OnAppearing(object data)
        {
            var tuple = JSON.Deserialize<Tuple<EventMessage, string, string>>(data);

            Message = tuple.Item1;
            var action = tuple.Item2.ToLower();
            Comment = tuple.Item3;

            _mailHasBeenSent = false;

            IsContentText = Message.Body.ContentType.EqualsCaseInsensitive("text");

            SetTitle(action);
            PopulateRecipients();

            OnPropertyChanged(() => HtmlSource);
            OnPropertyChanged(() => Description);
        }

        public override async void OnDisappearing()
        {
            if (!_mailHasBeenSent)
            {
                await GraphService.DeleteDraftMessage(Message.Id);
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

        private void PopulateRecipients()
        {
            Recipients = new ObservableCollection<Message.Recipient>(Message.ToRecipients);
        }

        private async void SendMail()
        {
            using (new Loading(this))
            {
                _mailHasBeenSent = await GraphService.UpdateAndSendMessage(Message, Comment, Recipients);
            }
            await UI.GoBack();
        }

        private void AddRecipient()
        {
            BaseViewModel.AddUserOrContact();
        }

        private void DeleteRecipient(Message.Recipient recipient)
        {
            int pos = Recipients.IndexOf(x => x.EmailAddress.IsEqualTo(recipient.EmailAddress));
            Recipients.RemoveAt(pos);
        }

        private void UserSelected(object sender, User user)
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

        private void ContactSelected(object sender, Contact contact)
        {
            var recipient = new Message.Recipient
            {
                EmailAddress = contact.EmailAddresses[0]
            };

            AddRecipient(recipient);
        }

        private void AddRecipient(Message.Recipient recipient)
        {
            if (Recipients.FirstOrDefault(x => x.EmailAddress.IsEqualTo(recipient.EmailAddress)) == null)
            {
                Recipients.Add(recipient);
            }
        }
    }
}
