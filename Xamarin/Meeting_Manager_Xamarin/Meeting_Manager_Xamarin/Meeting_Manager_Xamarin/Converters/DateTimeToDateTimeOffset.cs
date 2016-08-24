//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using System;
using System.Globalization;
using Xamarin.Forms;

namespace Meeting_Manager_Xamarin.Converters
{
    class DateTimeToDateTimeOffset : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((DateTimeOffset)value).DateTime;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new DateTimeOffset((DateTime) value);
        }
    }
}
