//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using Newtonsoft.Json;

namespace MeetingManager.Models
{
    public class EventMessage : Message
    {
        [JsonProperty("@odata.type")]
        public string Type { get; set; }
    }
}
