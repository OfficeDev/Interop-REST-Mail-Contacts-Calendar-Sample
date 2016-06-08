//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

namespace MeetingManager
{
    static class Utils
    {
        public static string StripToken(string uri, string token)
        {
            string skiptoken = GetToken(uri, token);

            if (skiptoken != null)
            {
                uri = uri.Replace(skiptoken, "");
            }
            return uri;
        }

        public static string GetToken(string uri, string token)
        {
            int start = uri.IndexOf(token);
            if (start < 0)
            {
                return null;
            }

            int end = uri.IndexOf("&", start + 1);
            if (end < 0)
            {
                end = uri.Length;
            }

            return uri.Substring(start, end - start);
        }
    }
}
