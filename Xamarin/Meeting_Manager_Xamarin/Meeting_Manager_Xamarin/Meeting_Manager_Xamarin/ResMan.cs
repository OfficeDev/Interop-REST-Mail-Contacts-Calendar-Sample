//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using System;

namespace Meeting_Manager_Xamarin
{
    public static class ResMan
    {
        public static string GetString(string id)
        {
            var text = ResourceStrings.ResourceManager.GetString(id, ResourceStrings.Culture);

            if (text == null)
            {
                throw new ArgumentException(String.Format($"Key '{id}' was not found in resources"));
            }

            return text;
        }
    }
}
