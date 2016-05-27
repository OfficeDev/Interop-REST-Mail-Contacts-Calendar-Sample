//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using System;
using System.Linq;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace MeetingManager.Converters
{
    class IndexToBackground : IValueConverter
    {
        private Brush _defaultBrush = new SolidColorBrush(Colors.Magenta);

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is int && parameter != null)
            {
                var colorNames = parameter.ToString().Split(":".ToArray());

                if (colorNames.Count() > 0)
                {
                    var brush = ColorParser.Parse(colorNames[(int)value % colorNames.Count()]);

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
