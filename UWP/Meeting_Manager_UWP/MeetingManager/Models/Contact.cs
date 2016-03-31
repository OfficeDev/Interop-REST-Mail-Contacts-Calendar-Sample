using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Windows.UI.Xaml.Media.Imaging;

namespace MeetingManager.Models
{
    public class Contact : INotifyPropertyChanged
    {
        public string Id { get; set; }
        public List<EmailAddress> EmailAddresses {get;set;}
        public string DisplayName { get; set; }

        [JsonIgnore]
        public BitmapImage Photo { get; set; }

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
