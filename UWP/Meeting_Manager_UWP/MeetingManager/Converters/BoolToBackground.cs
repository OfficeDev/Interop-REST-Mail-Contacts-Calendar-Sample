//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using System;
using System.Linq;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace MeetingManager.Converters
{
    class BoolToBackground : IValueConverter
    {
        private Brush _defaultBrush = new SolidColorBrush(Colors.Magenta);

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool && parameter != null)
            {
                var colorNames = parameter.ToString().Split(":".ToArray());

                if (colorNames.Count() > 1)
                {
                    var brush = ColorParser.Parse(colorNames[(bool) value ? 0 : 1]);

                    return brush ?? _defaultBrush;
                }
            }

            return _defaultBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
