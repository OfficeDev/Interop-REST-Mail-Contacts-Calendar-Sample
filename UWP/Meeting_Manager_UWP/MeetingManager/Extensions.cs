using MeetingManager.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeetingManager
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
            var handler = eventHandler;

            if (null != handler)
            {
                handler(sender, new PropertyChangedEventArgs(propertyName));
            }
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
    }
}
