//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using Newtonsoft.Json;

namespace Meeting_Manager_Xamarin.Models
{
    public class User
    {
        public string UserPrincipalName { get; set; }
        public string DisplayName { get; set; }
        public string GivenName { get; set; }
        public string SurName { get; set; }

        [JsonIgnore]
        public string Name => ToString();

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(DisplayName))
            {
                return DisplayName;
            }
            return UserPrincipalName;
        }
    }

    public class Room : User { }
}
