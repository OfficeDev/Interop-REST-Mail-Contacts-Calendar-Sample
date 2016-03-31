using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeetingManager.Models
{
    class HttpRequestEventData
    {
        public DateTimeOffset TimeStamp { get; set; }
        public string Method { get; set; }
        public string Uri { get; set; }
        public string Body { get; set; }
        public bool IsBodyJson { get; set; }
    }
}
