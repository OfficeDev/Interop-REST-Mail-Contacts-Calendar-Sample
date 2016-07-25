//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using System;
using Xamarin.Forms;

namespace Meeting_Manager_Xamarin.ViewModels
{
    class AcceptOrDeclinePageViewModel : BaseViewModel, ITransientViewModel
    {
        private string _action;
        private string _meetingId;

        public Command SendCommand => new Command(Send);

        public string Title { get; set; }

        public string Comment { get; set; }

        public override void OnAppearing(object data)
        {
            var payload = JSON.Deserialize<Tuple<string, string>>(data);

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

            await UI.GoBack();
        }
    }
}
