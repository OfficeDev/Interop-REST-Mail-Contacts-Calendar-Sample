//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using Microsoft.Identity.Client;
using System;
using System.Threading.Tasks;

namespace Meeting_Manager_Xamarin
{
    class AuthenticationHelperMSAL : IAuthenticationService
    {
        private PublicClientApplication _identityClient;

        private string TokenForUser { get; set; }
        private DateTimeOffset Expiration { get; set; }

        public AuthenticationHelperMSAL(IPlatformParameters platformParameters)
        {
            _identityClient = new PublicClientApplication(App.Me.ClientId);
            _identityClient.PlatformParameters = platformParameters;
        }

        public async Task<string> GetTokenAsync(string resourceId)
        {
            if (TokenForUser == null || Expiration <= DateTimeOffset.UtcNow.AddMinutes(5))
            {
                var scopes = new string[]
                        {
                        "https://graph.microsoft.com/User.Read",
                        "https://graph.microsoft.com/User.ReadWrite",
                        "https://graph.microsoft.com/User.ReadBasic.All",
                        "https://graph.microsoft.com/Mail.Send",
                        "https://graph.microsoft.com/Calendars.ReadWrite",
                        "https://graph.microsoft.com/Mail.ReadWrite",
                        "https://graph.microsoft.com/Files.ReadWrite",
                        "https://graph.microsoft.com/Contacts.Read",

                        // Admin-only scopes. Uncomment these if you're running the sample with an admin work account.
                        // You won't be able to sign in with a non-admin work account if you request these scopes.
                        // These scopes will be ignored if you leave them uncommented and run the sample with a consumer account.
                        //"https://graph.microsoft.com/Directory.AccessAsUser.All",
                        //"https://graph.microsoft.com/User.ReadWrite.All",
                        //"https://graph.microsoft.com/Group.ReadWrite.All"
                    };

                try
                {
                    var authResult = await _identityClient.AcquireTokenAsync(scopes);

                    TokenForUser = authResult.Token;
                    Expiration = authResult.ExpiresOn;
                }
                catch
                {
                }
            }

            return TokenForUser;
        }
    }
}
