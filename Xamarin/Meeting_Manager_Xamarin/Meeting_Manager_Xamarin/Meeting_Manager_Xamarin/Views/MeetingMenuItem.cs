//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

namespace Meeting_Manager_Xamarin.Views
{
    class MeetingMenuItem : CascadeMenuItem
    {
        public MeetingMenuItem()
        {
            IsSubMenu = (parameter) =>
            {
                var meeting = parameter as Models.Meeting;

                return !meeting.IsSingle;
            };
        }
    }
}
