//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using Newtonsoft.Json;

namespace Meeting_Manager_Xamarin
{
    class JSON
    {
        internal static string Serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Utc });
        }

        internal static string Serialize(object obj, Formatting format)
        {
            return JsonConvert.SerializeObject(obj, format, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Utc });
        }

        internal static T Deserialize<T>(object parameter)
        {
            return JsonConvert.DeserializeObject<T>((string)parameter, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Utc });
        }
    }
}
