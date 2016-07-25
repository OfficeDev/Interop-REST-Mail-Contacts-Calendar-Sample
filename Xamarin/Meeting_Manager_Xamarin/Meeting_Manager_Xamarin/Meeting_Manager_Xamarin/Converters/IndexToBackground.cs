//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using System;
using System.Globalization;
using System.Linq;
using Xamarin.Forms;

namespace Meeting_Manager_Xamarin.Converters
{
    class IndexToBackground : Xamarin.Forms.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int && parameter != null)
            {
                var colorNames = parameter.ToString().Split(":".ToCharArray());

                if (colorNames.Count() > 0)
                {
                    return ColorParser.Parse(colorNames[(int)value % colorNames.Count()]);
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
