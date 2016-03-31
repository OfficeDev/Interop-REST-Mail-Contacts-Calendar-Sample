using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeetingManager.Models
{
    public class ADUser
    {
        public string userPrincipalName { get; set; }
        public string displayName { get; set; }
        public string givenName { get; set; }
        public string surName { get; set; }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(displayName))
            {
                return displayName;
            }
            return userPrincipalName;
        }
    }
}
