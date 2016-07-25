//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using Meeting_Manager_Xamarin.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Xamarin.Forms;

namespace Meeting_Manager_Xamarin
{
    public static class Extensions
    {
        public static bool ContainsCaseInsensitive(this string input, string match)
        {
            return input.IndexOfCaseInsensitive(match) >= 0;
        }

        public static int IndexOfCaseInsensitive(this string input, string match)
        {
            return input.IndexOf(match, StringComparison.OrdinalIgnoreCase);
        }

        public static bool EqualsCaseInsensitive(this string input, string match)
        {
            return input.Equals(match, StringComparison.OrdinalIgnoreCase);
        }

        public static void Notify(this INotifyPropertyChanged sender, PropertyChangedEventHandler eventHandler, string propertyName)
        {
            eventHandler?.Invoke(sender, new PropertyChangedEventArgs(propertyName));
        }

        public static int IndexOf<T>(this IEnumerable<T> source, Func<T, bool> comparer)
        {
            if (source == null) return -1;

            int index = 0;

            foreach (T item in source)
            {
                if (comparer(item))
                {
                    return index;
                }
                index++;
            }
            return -1;
        }

        public static void ForEach<T>(this IEnumerable<T> value, Action<T> action)
        {
            foreach (T item in value)
            {
                action(item);
            }
        }

        public static bool IsEqualTo(this EmailAddress input, EmailAddress match)
        {
            return input.Address.EqualsCaseInsensitive(match.Address);
        }

        public static TDst ConvertObject<TDst>(this object src)
        {
            string json = JsonConvert.SerializeObject(src, Formatting.Indented);

            return JsonConvert.DeserializeObject<TDst>(json);
        }

        internal static void Subscribe<T>(this object receiver, Action<object, T> action)
        {
            MessagingCenter.Subscribe<object, T>(receiver, typeof(T).Name, action);
        }

        internal static void Publish<T>(this object sender, T data)
        {
            MessagingCenter.Send<object, T>(sender, typeof(T).Name, data);
        }

    }
}
