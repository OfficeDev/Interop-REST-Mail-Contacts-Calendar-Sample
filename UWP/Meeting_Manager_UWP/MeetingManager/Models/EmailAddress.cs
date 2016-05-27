//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

namespace MeetingManager.Models
{
    public class EmailAddress
    {
        public string Address { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(Name))
            {
                return Name;
            }
            return Address;
        }
    }
}
