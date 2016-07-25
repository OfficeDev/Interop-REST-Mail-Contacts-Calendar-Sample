//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using System;
using System.IO;

namespace Meeting_Manager_Xamarin.Models
{
    public class DriveItem
    {
        public string Id { get; set; }
        public Stream Content { get; set; }
        public string Name { get; set; }
        public Int64? Size { get; set; }
        public string WebUrl { get; set; }
        public FileDef File { get; set; }
        public FolderDef Folder { get; set; }

        public class FileDef
        {
        }

        public class FolderDef
        {
        }
    }
}
