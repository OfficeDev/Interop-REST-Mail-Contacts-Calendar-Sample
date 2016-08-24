//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using MeetingManager.Models;
using System;

namespace MeetingManager.ViewModels
{
    class AcceptDeclineDialogViewModel : DialogViewModel
    {
        private string _action;
        private string _meetingId;

        public Command SendCommand => new Command(Send);

        public string Title { get; private set; }

        public string Comment { get; set; }

        protected override void OnNavigatedTo(object parameter)
        {
            var payload = JSON.Deserialize<Tuple<string, string>>(parameter);

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
            using (new Loading(this))
            {
                await GraphService.AcceptOrDecline(_meetingId, _action, Comment);
            }
        }
    }
}
