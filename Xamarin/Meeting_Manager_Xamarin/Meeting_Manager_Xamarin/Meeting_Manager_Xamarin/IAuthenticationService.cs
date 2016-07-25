//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using System.Threading.Tasks;

namespace Meeting_Manager_Xamarin
{
    public interface IAuthenticationService
    {
        Task<string> GetTokenAsync(string resourceId);
    }
}
