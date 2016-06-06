//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using System.Threading.Tasks;

namespace MeetingManager
{
    public interface IAuthenticationService
    {
        string UserId { get; set; }
        string AuthorizationCode { get; set; }
        string LoginUrl { get; }
        string RedirectUri { get; }
        Task<string> GetTokenAsync(string resourceId);
    }
}
