using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeetingManager
{
    public interface IAuthenticationService
    {
        string UserId { get; set; }
        string AuthorizationCode { get; set; }
        string LoginUrl { get; }
        string RedirectUri { get; }
        Task<string> GetTokenAsync(string resourceId, bool isRefresh);
    }
}
