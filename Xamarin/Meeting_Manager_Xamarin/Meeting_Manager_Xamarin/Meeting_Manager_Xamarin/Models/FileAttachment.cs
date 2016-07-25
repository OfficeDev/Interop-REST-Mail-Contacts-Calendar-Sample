//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using System;

namespace Meeting_Manager_Xamarin.Models
{
    public class FileAttachment
    {
        [Newtonsoft.Json.JsonProperty("@odata.type")]
        public string Type => "#microsoft.graph.fileAttachment";

        public string Id { get; set; }
        public string Name { get; set; }
        public Int32? Size { get; set; }
        public string ContentType { get; set; }
        public string ContentLocation { get; set; }
        public byte[] ContentBytes { get; set; }
        public bool IsInline { get; set; }
    }
}
