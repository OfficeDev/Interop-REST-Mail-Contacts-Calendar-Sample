//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Xamarin.Forms;

namespace Meeting_Manager_Xamarin.Models
{
    public class Contact : INotifyPropertyChanged
    {
        public string Id { get; set; }
        public List<EmailAddress> EmailAddresses {get;set;}
        public string DisplayName { get; set; }

        [JsonIgnore]
        public ImageSource Photo { get; set; }

        [JsonIgnore]
        public System.IO.Stream Data { get; set; }

        [JsonIgnore]
        public string Name => ToString();

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propertyName)
        {
            this.Notify(PropertyChanged, propertyName);
        }

        public override string ToString()
        {
            return EmailAddresses.Any() ?
                    EmailAddresses[0].ToString() : 
                    DisplayName;
        }
    }
}
