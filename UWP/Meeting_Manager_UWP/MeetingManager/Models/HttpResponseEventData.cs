using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeetingManager.Models
{
    class HttpResponseEventData
    {
        public string StatusCode { get;set;}
        public string Body { get; set; }
        public bool IsBodyJson { get; set; }
    }
}
