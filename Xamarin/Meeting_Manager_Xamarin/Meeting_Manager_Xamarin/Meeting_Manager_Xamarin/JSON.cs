//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using Newtonsoft.Json;

namespace Meeting_Manager_Xamarin
{
    static class JSON
    {
        internal static string Serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj, Settings());
        }

        internal static string Serialize(object obj, Formatting format)
        {
            return JsonConvert.SerializeObject(obj, format, Settings());
        }

        internal static T Deserialize<T>(object parameter)
        {
            return JsonConvert.DeserializeObject<T>((string)parameter, Settings());
        }

        private static JsonSerializerSettings Settings()
        {
            return new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Utc };
        }
    }
}
