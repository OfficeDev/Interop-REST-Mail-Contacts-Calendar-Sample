using MeetingManager.Models;
using Prism.Commands;
using Prism.Windows.AppModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;

namespace MeetingManager.ViewModels
{
    class AcceptDeclineDialogViewModel : ViewModel, ITransientViewModel
    {
        private string _action;
        private string _meetingId;

        public AcceptDeclineDialogViewModel()
        {
            SendCommand = new DelegateCommand(Send);

            GetEvent<InitDialogEvent>().Subscribe(OnInitialize);
        }

        public DelegateCommand SendCommand { get; }

        public string Title { get; set; }

        public string Comment { get; set; }

        private void OnInitialize(object parameter)
        {
            GetEvent<InitDialogEvent>().Unsubscribe(OnInitialize);

            var payload = Deserialize<Tuple<string, string>>(parameter);

            _action = payload.Item1.ToLower();
            _meetingId = payload.Item2;

            switch (_action)
            {
                case OData.Accept:
                    Title = GetString("AcceptTitle");
                    break;
                case OData.TentativelyAccept:
                    Title = GetString("TentativeTitle");
                    break;
                case OData.Decline:
                    Title = GetString("DeclineTitle");
                    break;
            }

            OnPropertyChanged(() => Title);
        }

        private async void Send()
        {
            await OfficeService.AcceptOrDecline(_meetingId, _action, Comment);
        }
    }
}
