//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

namespace Meeting_Manager_Xamarin.Models
{
    public class User
    {
        public string UserPrincipalName { get; set; }
        public string DisplayName { get; set; }
        public string GivenName { get; set; }
        public string SurName { get; set; }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(DisplayName))
            {
                return DisplayName;
            }
            return UserPrincipalName;
        }

        [Newtonsoft.Json.JsonIgnore]
        public string Name => $"{DisplayName} ({UserPrincipalName})";
    }

    public class Room : User { }
}
