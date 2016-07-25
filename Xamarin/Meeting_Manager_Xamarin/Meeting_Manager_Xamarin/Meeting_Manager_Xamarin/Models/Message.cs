//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Meeting_Manager_Xamarin.Models
{
    public class Message
    {
        public string Id { get; set; }
        public string Subject { get; set; }
        public DateTimeOffset CreatedDateTime { get; set; }
        public List<Recipient> ToRecipients { get; set; }
        public Body Body { get; set; }

        public Message()
        {
            ToRecipients = new List<Recipient>();
        }

        public class Recipient
        {
            public EmailAddress EmailAddress { get; set; }

            [JsonIgnore]
            public string Name => ToString();

            public override string ToString()
            {
                return EmailAddress.ToString();
            }
        }
    }
}
