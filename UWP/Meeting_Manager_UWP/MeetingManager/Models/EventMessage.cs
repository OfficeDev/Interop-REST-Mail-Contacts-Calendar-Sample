using Newtonsoft.Json;

namespace MeetingManager.Models
{
    public class EventMessage : Message
    {
        [JsonProperty("@odata.type")]
        public string Type { get; set; }
    }
}
