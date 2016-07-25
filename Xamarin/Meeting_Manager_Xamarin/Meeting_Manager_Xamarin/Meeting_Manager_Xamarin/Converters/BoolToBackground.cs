//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using System;
using System.Globalization;
using System.Linq;
using Xamarin.Forms;

namespace Meeting_Manager_Xamarin.Converters
{
    class BoolToBackground : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool && parameter != null)
            {
                var colorNames = parameter.ToString().Split(":".ToCharArray());

                if (colorNames.Count() > 1)
                {
                    return ColorParser.Parse(colorNames[(bool)value ? 0 : 1]);
                }
            }

            return Color.Pink;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
