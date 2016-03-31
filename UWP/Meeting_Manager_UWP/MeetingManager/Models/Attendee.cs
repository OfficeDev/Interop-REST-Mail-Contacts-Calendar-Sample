using Newtonsoft.Json;
using Prism.Windows.AppModel;
using Windows.ApplicationModel.Resources;

namespace MeetingManager.Models
{
    public class Attendee
    {
        public EmailAddress EmailAddress { get; set; }
        public ResponseStatus Status { get; set; }
        public string Type { get; set; }

        [JsonIgnore]
        public string ResponseStatus
        {
            get
            {
                if (IsOrganizer)
                {
                    return ResMan.GetString("OrganizerResponse");
                }
                else
                {
                    if (Status != null)
                    {
                        switch (Status.Response.ToLower())
                        {
                            case OData.Accepted:
                                return ResMan.GetString("AcceptedResponse");
                            case OData.TentativelyAccepted:
                                return ResMan.GetString("TentativeResponse");
                            case OData.Declined:
                                return ResMan.GetString("DeclinedResponse");
                        }
                    }

                    return ResMan.GetString("NoResponse");
                }
            }
        }

        [JsonIgnore]
        public bool IsOrganizer { get; set; }

        public override string ToString()
        {
            return EmailAddress.ToString();
        }
    }
}
