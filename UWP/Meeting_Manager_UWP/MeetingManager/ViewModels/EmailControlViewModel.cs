using MeetingManager.Models;
using Prism.Commands;
using Prism.Windows.AppModel;
using Prism.Windows.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Navigation;

namespace MeetingManager.ViewModels
{
    class EmailControlViewModel : ViewModel
    {
        private EventMessage _message;
        private string _action;
        private string _comment;
        private bool _mailHasBeenSent;
        private ObservableCollection<Message.Recipient> _recipients;

        public EmailControlViewModel()
        {
            SendCommand = new DelegateCommand(SendMail);
            AddUserRecipientCommand = new DelegateCommand(AddUserRecipient);
            AddContactRecipientCommand = new DelegateCommand(AddContactRecipient);
            DeleteRecipientCommand = new DelegateCommand<Message.Recipient>(DeleteRecipient);

            GetEvent<UserSelectedEvent>().Subscribe(UserSelected);
            GetEvent<ContactSelectedEvent>().Subscribe(ContactSelected);
        }

        public DelegateCommand SendCommand { get; private set; }
        public DelegateCommand AddUserRecipientCommand { get; private set; }
        public DelegateCommand AddContactRecipientCommand { get; private set; }
        public DelegateCommand<Message.Recipient> DeleteRecipientCommand { get; private set; }

        [RestorableState]
        public EventMessage Message
        {
            get { return _message; }
            private set { SetProperty(ref _message, value); }
        }

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

        public bool IsEmailAction
        {
            get
            {
                switch (_action)
                {
                    case OData.Reply:
                    case OData.ReplyAll:
                    case OData.Forward:
                        return true;
                }
                return false;
            }
        }

        public bool IsContentText { get; set; }

        public override void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            base.OnNavigatedTo(e, viewModelState);

            if (e.NavigationMode == NavigationMode.New)
            {
                var tuple = Deserialize<Tuple<EventMessage, string, string>>((string)e.Parameter);

                Message = tuple.Item1;
                _action = tuple.Item2.ToLower();
                Comment = tuple.Item3;

                _mailHasBeenSent = false;

                IsContentText = Message.Body.ContentType.EqualsCaseInsensitive("text");

                PopulateRecipients();
            }
        }

        public override async void OnNavigatingFrom(NavigatingFromEventArgs e, Dictionary<string, object> viewModelState, bool suspending)
        {
            if (IsEmailAction && e.NavigationMode == NavigationMode.Back && !_mailHasBeenSent)
            {
                await OfficeService.DeleteDraftMessage(Message.Id);
            }
        }

        private void PopulateRecipients()
        {
            Recipients = new ObservableCollection<Message.Recipient>(Message.ToRecipients);
        }

        private void SendMail()
        {
            if (IsEmailAction)
            {
                DoSendMail();
            }
            else
            {
                PublishComment();
            }
        }

        private async void DoSendMail()
        {
            using (new Loading(this))
            {
                _mailHasBeenSent = await OfficeService.UpdateAndSendMessage(Message, Comment, Recipients);
            }
            GoBack();
        }

        private void PublishComment()
        {
            var response = new InvitationResponsePayload
            {
                Comment = Comment,
                Action = _action
            };

            GetEvent<AcceptDeclineEvent>().Publish(response);

            GoBack();
        }

        private void AddUserRecipient()
        {
            Navigate("Users", true);
        }

        private void AddContactRecipient()
        {
            Navigate("Contacts");
        }

        private void DeleteRecipient(Message.Recipient recipient)
        {
            int pos = Recipients.IndexOf(x => x.EmailAddress.IsEqualTo(recipient.EmailAddress));
            Recipients.RemoveAt(pos);
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
